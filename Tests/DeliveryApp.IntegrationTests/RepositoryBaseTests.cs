using DeliveryApp.Infrastructure.Adapters.Postgres;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Testcontainers.PostgreSql;
using Xunit;

namespace DeliveryApp.IntegrationTests;

public class RepositoryBaseTests<TRepository> : IAsyncLifetime
{
    private IMediator _mediatorMock = Substitute.For<IMediator>();
    private ApplicationDbContext _context;
    protected UnitOfWork UnitOfWork;
    protected TRepository Repository;
    
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

        _context = CreateContext();
        Repository = (TRepository) Activator.CreateInstance(typeof(TRepository), _context);
        UnitOfWork = new UnitOfWork(_context, _mediatorMock);
        
        await _context.Database.MigrateAsync();
    }
    
    public async Task DisposeAsync()
    {
        await _postgreSqlContainer.DisposeAsync().AsTask();
        await _context.DisposeAsync();
    }

    protected async Task ExecuteInNewContext(Func<TRepository, UnitOfWork, Task> action)
    {
        await using var context = CreateContext();
        
        var repository = (TRepository) Activator.CreateInstance(typeof(TRepository), context);
        var unitOfWork = new UnitOfWork(context, _mediatorMock);
        
        await action.Invoke(repository, unitOfWork)!;
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
}