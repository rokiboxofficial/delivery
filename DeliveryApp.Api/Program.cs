using System.Reflection;
using CSharpFunctionalExtensions;
using DeliveryApp.Api;
using DeliveryApp.Core.Application.Services;
using DeliveryApp.Core.Application.UseCases.Commands.AssignOrder;
using DeliveryApp.Core.Application.UseCases.Commands.CreateOrder;
using DeliveryApp.Core.Application.UseCases.Commands.MoveCouriers;
using DeliveryApp.Core.Application.UseCases.Queries.GetAllShortCouriers;
using DeliveryApp.Core.Application.UseCases.Queries.GetNotCompletedShortOrders;
using DeliveryApp.Core.Domain.Services;
using DeliveryApp.Core.Ports;
using DeliveryApp.Core.Ports.ReadModelProviders;
using DeliveryApp.Core.Ports.Repositories;
using DeliveryApp.Infrastructure.Adapters.DotnetRandom;
using DeliveryApp.Infrastructure.Adapters.Postgres;
using DeliveryApp.Infrastructure.Adapters.Postgres.ReadModelProviders;
using DeliveryApp.Infrastructure.Adapters.Postgres.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Primitives;

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

// Random number port
builder.Services.AddScoped<IRandomNumberProvider, RandomNumberProvider>();

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

var app = builder.Build();

// -----------------------------------
// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
    app.UseDeveloperExceptionPage();
else
    app.UseHsts();

app.UseHealthChecks("/health");
app.UseRouting();

// Apply Migrations
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}

app.Run();