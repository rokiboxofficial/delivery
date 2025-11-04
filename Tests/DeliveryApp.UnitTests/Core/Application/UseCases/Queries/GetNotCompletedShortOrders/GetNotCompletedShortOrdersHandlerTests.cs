using System.Threading;
using System.Threading.Tasks;
using DeliveryApp.Core.Application.UseCases.Queries.GetNotCompletedShortOrders;
using DeliveryApp.Core.Ports.ReadModelProviders;
using DeliveryApp.TestsCommon;
using NSubstitute;
using Xunit;

namespace DeliveryApp.UnitTests.Core.Application.UseCases.Queries.GetNotCompletedShortOrders;

public class GetNotCompletedShortOrdersHandlerTests
{
    private readonly IOrderReadModelProvider _orderReadModelProviderMock = Substitute.For<IOrderReadModelProvider>();
    private readonly GetNotCompletedShortOrdersHandler _getNotCompletedShortOrdersHandler;

    public GetNotCompletedShortOrdersHandlerTests()
    {
        _getNotCompletedShortOrdersHandler = new GetNotCompletedShortOrdersHandler(_orderReadModelProviderMock);
    }

    [Fact]
    public async Task WhenHandling_ThenReadModelProviderShouldBeCalled()
    {
        // Arrange.
        var getNotCompletedShortOrdersQuery = Create.GetNotCompletedShortOrdersQuery();
        
        // Act.
        await _getNotCompletedShortOrdersHandler.Handle(getNotCompletedShortOrdersQuery, CancellationToken.None);

        // Assert.
        await _orderReadModelProviderMock.Received(1).GetNotCompletedShortOrdersAsync();
    }
}