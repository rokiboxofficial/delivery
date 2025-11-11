using System.Reflection;
using Clients.Geo;
using Confluent.Kafka;
using CSharpFunctionalExtensions;
using DeliveryApp.Api;
using DeliveryApp.Api.Adapters.BackgroundJobs;
using DeliveryApp.Api.Adapters.Kafka.BasketConfirmed;
using DeliveryApp.Core.Application.DomainEventHandlers;
using DeliveryApp.Core.Application.Services;
using DeliveryApp.Core.Application.UseCases.Commands.AssignOrder;
using DeliveryApp.Core.Application.UseCases.Commands.CreateOrder;
using DeliveryApp.Core.Application.UseCases.Commands.MoveCouriers;
using DeliveryApp.Core.Application.UseCases.Queries.GetAllShortCouriers;
using DeliveryApp.Core.Application.UseCases.Queries.GetNotCompletedShortOrders;
using DeliveryApp.Core.Domain.Model.OrderAggregate.DomainEvents;
using DeliveryApp.Core.Domain.Services;
using DeliveryApp.Core.Ports;
using DeliveryApp.Core.Ports.ReadModelProviders;
using DeliveryApp.Core.Ports.Repositories;
using DeliveryApp.Infrastructure;
using DeliveryApp.Infrastructure.Adapters.DotnetRandom;
using DeliveryApp.Infrastructure.Adapters.Grpc.GeoService;
using DeliveryApp.Infrastructure.Adapters.Kafka.Order;
using DeliveryApp.Infrastructure.Adapters.Postgres;
using DeliveryApp.Infrastructure.Adapters.Postgres.ReadModelProviders;
using DeliveryApp.Infrastructure.Adapters.Postgres.Repositories;
using Grpc.Core;
using Grpc.Net.Client.Configuration;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using OpenApi.Filters;
using OpenApi.Formatters;
using OpenApi.OpenApi;
using Primitives;
using Quartz;
using Error = Primitives.Error;

var builder = WebApplication.CreateBuilder(args);

// Health Checks
builder.Services.AddHealthChecks();

// Cors
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
        policy =>
        {
            policy.AllowAnyOrigin(); // Не делайте так в проде!
        });
});

// Configuration
builder.Services.ConfigureOptions<SettingsSetup>();
var connectionString = builder.Configuration["CONNECTION_STRING"];
var geoServiceGrpcHost = builder.Configuration["GEO_SERVICE_GRPC_HOST"];

// Domain Services
builder.Services.AddScoped<IDispatchService, DispatchService>();

// Services
builder.Services.AddScoped<IRandomLocationProvider, RandomLocationProvider>();

// БД, ORM 
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    {
        options.UseNpgsql(connectionString,
            sqlOptions => { sqlOptions.MigrationsAssembly("DeliveryApp.Infrastructure"); });
        options.EnableSensitiveDataLogging();
    }
);

// UnitOfWork
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Repositories
builder.Services.AddScoped<ICourierRepository, CourierRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();

// Ports
builder.Services.AddScoped<IRandomNumberProvider, RandomNumberProvider>();
builder.Services.AddScoped<IGeoClient, GeoClient>();

// ReadModel Providers
builder.Services.AddScoped<ICourierReadModelProvider, CourierReadModelProvider>();
builder.Services.AddScoped<IOrderReadModelProvider, OrderReadModelProvider>();

// Mediator
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

// Commands
builder.Services.AddScoped<IRequestHandler<CreateOrderCommand, UnitResult<Error>>, CreateOrderHandler>();
builder.Services.AddScoped<IRequestHandler<MoveCouriersCommand, UnitResult<Error>>, MoveCouriersHandler>();
builder.Services.AddScoped<IRequestHandler<AssignOrderCommand, UnitResult<Error>>, AssignOrderHandler>();

// Queries
builder.Services.AddScoped<IRequestHandler<GetNotCompletedShortOrdersQuery, GetNotCompletedShortOrdersResponse>, GetNotCompletedShortOrdersHandler>();
builder.Services.AddScoped<IRequestHandler<GetAllShortCouriersQuery, GetAllShortCouriersResponse>, GetAllShortCouriersHandler>();

// HTTP Handlers
builder.Services.AddControllers(options => { options.InputFormatters.Insert(0, new InputFormatterStream()); })
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
        options.SerializerSettings.Converters.Add(new StringEnumConverter
        {
            NamingStrategy = new CamelCaseNamingStrategy()
        });
    });

// Swagger
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("1.0.0", new OpenApiInfo
    {
        Title = "Delivery Service",
        Description = "Отвечает за диспетчеризацию доставки",
        Contact = new OpenApiContact
        {
            Name = "Kirill Vetchinkin",
            Url = new Uri("https://microarch.ru"),
            Email = "info@microarch.ru"
        }
    });
    options.CustomSchemaIds(type => type.FriendlyId(true));
    options.IncludeXmlComments(
        $"{AppContext.BaseDirectory}{Path.DirectorySeparatorChar}{Assembly.GetEntryAssembly()?.GetName().Name}.xml");
    options.DocumentFilter<BasePathFilter>("");
    options.OperationFilter<GeneratePathParamsValidationFilter>();
});
builder.Services.AddSwaggerGenNewtonsoftSupport();

// CRON Jobs
builder.Services.AddQuartz(configure =>
{
    var assignOrderJobKey = new JobKey(nameof(AssignOrderJob));
    var moveCouriersJobKey = new JobKey(nameof(MoveCouriersJob));
    configure
        .AddJob<AssignOrderJob>(assignOrderJobKey)
        .AddTrigger(
            trigger => trigger.ForJob(assignOrderJobKey)
                .WithSimpleSchedule(
                    schedule => schedule.WithIntervalInSeconds(1)
                        .RepeatForever()))
        .AddJob<MoveCouriersJob>(moveCouriersJobKey)
        .AddTrigger(
            trigger => trigger.ForJob(moveCouriersJobKey)
                .WithSimpleSchedule(
                    schedule => schedule.WithIntervalInSeconds(2)
                        .RepeatForever()));
});
builder.Services.AddQuartzHostedService();

// gRPC
builder.Services
    .AddGrpcClient<Geo.GeoClient>(o =>
    {
        o.Address = new Uri(geoServiceGrpcHost!);
    })
    .ConfigureChannel(o =>
    {
        o.HttpHandler = new SocketsHttpHandler
        {
            PooledConnectionIdleTimeout = Timeout.InfiniteTimeSpan,
            KeepAlivePingDelay = TimeSpan.FromSeconds(60),
            KeepAlivePingTimeout = TimeSpan.FromSeconds(30),
            EnableMultipleHttp2Connections = true
        };
        o.ServiceConfig = new ServiceConfig
        {
            MethodConfigs =
            {
                new MethodConfig
                {
                    Names = { MethodName.Default },
                    RetryPolicy = new RetryPolicy
                    {
                        MaxAttempts = 5,
                        InitialBackoff = TimeSpan.FromSeconds(1),
                        MaxBackoff = TimeSpan.FromSeconds(5),
                        BackoffMultiplier = 1.5,
                        RetryableStatusCodes = { StatusCode.Unavailable }
                    }
                }
            }
        };
    });

// Message Broker Consumer
builder.Services.Configure<HostOptions>(options =>
{
    options.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore;
    options.ShutdownTimeout = TimeSpan.FromSeconds(30);
});
builder.Services.AddSingleton(serviceProvider =>
{
    var settings = serviceProvider.GetRequiredService<IOptions<Settings>>();
    var consumerConfig = new ConsumerConfig
    {
        BootstrapServers = settings?.Value.MessageBrokerHost ?? throw new ArgumentNullException(nameof(settings)),
        GroupId = "DeliveryConsumerGroup",
        EnableAutoOffsetStore = false,
        EnableAutoCommit = true,
        AutoOffsetReset = AutoOffsetReset.Earliest,
        EnablePartitionEof = true
    };
    return new ConsumerBuilder<Ignore, string>(consumerConfig).Build();
});
builder.Services.AddHostedService<ConsumerService>();

// Domain Event Handlers
builder.Services.AddScoped<INotificationHandler<OrderCreatedDomainEvent>, OrderCreatedDomainEventHandler>();
builder.Services.AddScoped<INotificationHandler<OrderCompletedDomainEvent>, OrderCompletedDomainEventHandler>();

// Message Broker Producer
builder.Services.AddSingleton(serviceProvider =>
{
    var settings = serviceProvider.GetRequiredService<IOptions<Settings>>();
    var config = new ProducerConfig
    {
        BootstrapServers = settings?.Value.MessageBrokerHost ?? throw new ArgumentNullException(nameof(settings))
    };

    return new ProducerBuilder<string, string>(config).Build();
});
builder.Services.AddScoped<IMessageBusProducer, Producer>();

var app = builder.Build();

// -----------------------------------
// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
    app.UseDeveloperExceptionPage();
else
    app.UseHsts();

app.UseHealthChecks("/health");
app.UseRouting();

app.UseDefaultFiles();
app.UseStaticFiles();
app.UseSwagger(c => { c.RouteTemplate = "openapi/{documentName}/openapi.json"; })
    .UseSwaggerUI(options =>
    {
        options.RoutePrefix = "openapi";
        options.SwaggerEndpoint("/openapi/1.0.0/openapi.json", "Swagger Delivery Service");
        options.RoutePrefix = string.Empty;
        options.SwaggerEndpoint("/openapi-original.json", "Swagger Delivery Service");
    });

app.UseCors();
app.MapControllers();

// Apply Migrations
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}

app.Run();