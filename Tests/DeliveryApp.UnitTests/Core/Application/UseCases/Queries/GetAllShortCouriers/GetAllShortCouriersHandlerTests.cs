using System.Threading;
using System.Threading.Tasks;
using DeliveryApp.Core.Application.UseCases.Queries.GetAllShortCouriers;
using DeliveryApp.Core.Ports.ReadModelProviders;
using DeliveryApp.TestsCommon;
using NSubstitute;
using Xunit;

namespace DeliveryApp.UnitTests.Core.Application.UseCases.Queries.GetAllShortCouriers;

public class GetAllShortCouriersHandlerTests
{
    private readonly ICourierReadModelProvider _courierReadModelProviderMock = Substitute.For<ICourierReadModelProvider>();
    private readonly GetAllShortCouriersHandler _getAllShortCouriersHandler;
    
    public GetAllShortCouriersHandlerTests()
    {
        _getAllShortCouriersHandler = new GetAllShortCouriersHandler(_courierReadModelProviderMock);
    }
    
    [Fact]
    public async Task WhenHandling_ThenReadModelProviderShouldBeCalled()
    {
        // Arrange.
        var getAllShortCouriersQuery = Create.GetAllShortCouriersQuery();
        
        // Act.
        await _getAllShortCouriersHandler.Handle(getAllShortCouriersQuery, CancellationToken.None);

        // Assert.
        await _courierReadModelProviderMock.Received(1).GetAllShortCouriersAsync();
    }
}