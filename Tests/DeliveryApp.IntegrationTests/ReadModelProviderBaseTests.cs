using DeliveryApp.Infrastructure;
using DeliveryApp.Infrastructure.Adapters.Postgres;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Testcontainers.PostgreSql;
using Xunit;

namespace DeliveryApp.IntegrationTests;

public class ReadModelProviderBaseTests<TReadModelProvider> : IAsyncLifetime
{
    protected ApplicationDbContext Context;
    protected TReadModelProvider ReadModelProvider;
    
    private readonly PostgreSqlContainer _postgreSqlContainer = new PostgreSqlBuilder()
        .WithImage("postgres:14.7")
        .WithDatabase("delivery")
        .WithUsername("username")
        .WithPassword("secret")
        .WithCleanUp(true)
        .WithCommand(
            "-c", "log_statement=all",
            "-c", "log_destination=stderr",
            "-c", "logging_collector=off")
        .Build();
    
    public async Task InitializeAsync()
    {
        await _postgreSqlContainer.StartAsync();

        Context = CreateContext();
        var options = GetOptions();
        ReadModelProvider = (TReadModelProvider) Activator.CreateInstance(typeof(TReadModelProvider), options);
        
        await Context.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await _postgreSqlContainer.DisposeAsync().AsTask();
        await Context.DisposeAsync();
    }
    
    private ApplicationDbContext CreateContext()
    {
        var contextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql( 
                _postgreSqlContainer.GetConnectionString(),
                sqlOptions => { sqlOptions.MigrationsAssembly("DeliveryApp.Infrastructure"); }
            ).Options;
        
        return new ApplicationDbContext(contextOptions);
    }

    private IOptions<Settings> GetOptions()
    {
        var settings = new Settings
        {
            ConnectionString = _postgreSqlContainer.GetConnectionString()
        };
        
        return Options.Create(settings);
    }
}