using DeliveryApp.Core.Domain.Model.OrderAggregate;
using DeliveryApp.Infrastructure.Adapters.Postgres.Repositories;
using DeliveryApp.TestsCommon;
using FluentAssertions;
using Xunit;

namespace DeliveryApp.IntegrationTests.Repositories;

public class OrderRepositoryTests : RepositoryBaseTests<OrderRepository>
{
    [Fact]
    public async Task WhenGetting_AndOrderIsAlreadyAdded_ThenOrderShouldBeEquivalentToStoredOrder()
    {
        // Arrange.
        var storedOrder = Create.Order();
        await Repository.AddAsync(storedOrder);
        await UnitOfWork.SaveChangesAsync();
        
        // Act.
        var order = (await Repository.GetAsync(storedOrder.Id)).Value;
        
        // Assert.
        order.Should().BeEquivalentTo(storedOrder, opt => opt.ComparingByMembers<Order>());
    }

    [Fact]
    public async Task WhenGetting_AndOrderIsAddedButNotSaved_ThenMaybeOrderShouldHasNoValue()
    {
        // Arrange.
        var notSavedOrder = Create.Order();
        await Repository.AddAsync(notSavedOrder);

        // Act.
        var maybeOrder = await Repository.GetAsync(notSavedOrder.Id);

        // Assert.
        maybeOrder.HasNoValue.Should().BeTrue();
    }

    [Fact]
    public async Task WhenGetting_AndOrderWasAddedAndUpdatedInDifferentContexts_ThenOrderShouldBeEquivalentToUpdatedOrder()
    {
        // Arrange.
        Order updatedOrder = null;
        await ExecuteInNewContext(async (repository, unitOfWork) =>
        {
            updatedOrder = Create.Order();
            await repository.AddAsync(updatedOrder);
            await unitOfWork.SaveChangesAsync();
        });
        
        await ExecuteInNewContext(async (repository, unitOfWork) =>
        {
            updatedOrder.Assign(Create.Courier());
            repository.Update(updatedOrder);
            await unitOfWork.SaveChangesAsync();
        });
        
        // Act.
        var order = (await Repository.GetAsync(updatedOrder.Id)).Value;

        // Assert.
        order.Should().BeEquivalentTo(updatedOrder, opt => opt.ComparingByMembers<Order>());
    }
    
    [Fact]
    public async Task WhenGetting_AndOrderIsNotStored_ThenMaybeOrderShouldHasNoValue()
    {
        // Arrange.
        var notStoredOrder = Create.Order();
        
        // Act.
        var maybeOrder = await Repository.GetAsync(notStoredOrder.Id);
        
        // Assert.
        maybeOrder.HasNoValue.Should().BeTrue();
    }
    
    [Fact]
    public async Task WhenSavingChanges_AndOrderWasUpdatedButOrderWasNotStoredBeforeUpdate_ThenExceptionShouldBeThrown()
    {
        // Arrange.
        var notStoredOrder = Create.Order();
        Repository.Update(notStoredOrder);

        // Act.
        Func<Task> act = async () => await UnitOfWork.SaveChangesAsync();

        // Assert.
        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task WhenGettingFirstInCreatedStatus_AndThereAreNoOrdersInCreatedStatus_ThenMaybeOrderShouldHasNoValue()
    {
        // Arrange and Act.
        var maybeOrder = await Repository.GetFirstInCreatedStatusAsync();

        // Assert.
        maybeOrder.HasNoValue.Should().BeTrue();
    }

    [Fact]
    public async Task WhenGettingFirstInCreatedStatus_AndThereIsSingleOrderStoredInCreatedStatus_ThenOrderShouldBeEquivalentToStoredOrderInCreatedStatus()
    {
        // Arrange.
        var storedOrdersInAssignedStatus = Enumerable.Range(0, 5).Select(_ => Setup.AssignedOrder()).ToArray();
        foreach (var storedOrder in storedOrdersInAssignedStatus)
            await Repository.AddAsync(storedOrder);
        
        var storedOrderInCreatedStatus = Create.Order();
        await Repository.AddAsync(storedOrderInCreatedStatus);
        
        await UnitOfWork.SaveChangesAsync();

        // Act.
        var order = (await Repository.GetFirstInCreatedStatusAsync()).Value;
        
        // Assert.
        order.Should().BeEquivalentTo(storedOrderInCreatedStatus, opt => opt.ComparingByMembers<Order>());
    }
    
    [Fact]
    public async Task WhenGettingFirstInCreatedStatus_AndThereAreFewOrdersStoredInCreatedStatus_ThenStoredOrdersInCreatedStatusShouldContainEquivalentOfOrder()
    {
        // Arrange.
        var storedOrdersInAssignedStatus = Enumerable.Range(0, 5).Select(_ => Setup.AssignedOrder()).ToArray();
        foreach (var storedOrder in storedOrdersInAssignedStatus)
            await Repository.AddAsync(storedOrder);

        var storedOrdersInCreatedStatus = Enumerable.Range(0, 5).Select(_ => Create.Order()).ToArray();
        foreach (var storedOrder in storedOrdersInCreatedStatus)
            await Repository.AddAsync(storedOrder);
        
        await UnitOfWork.SaveChangesAsync();
        
        // Act.
        var order = (await Repository.GetFirstInCreatedStatusAsync()).Value;
        
        // Assert.
        storedOrdersInCreatedStatus.Should().ContainEquivalentOf(order, opt => opt.ComparingByMembers<Order>());
    }

    [Fact]
    public async Task WhenGettingAllInAssignedStatus_AndThereAreNoOrdersInAssignedStatus_ThenOrdersShouldBeEmpty()
    {
        // Arrange and Act.
        var orders = await Repository.GetAllInAssignedStatusAsync();
        
        // Assert.
        orders.Should().BeEmpty();
    }

    [Fact]
    public async Task WhenGettingAllInAssignedStatus_AndThereIsSingleOrderStoredInAssignedStatus_ThenOrdersShouldBeEquivalentToStoredOrderInAssignedStatus()
    {
        // Arrange.
        var storedOrdersInCreatedStatus = Enumerable.Range(0, 5).Select(_ => Create.Order()).ToArray();
        foreach (var storedOrder in storedOrdersInCreatedStatus)
            await Repository.AddAsync(storedOrder);

        var storedOrderInAssignedStatus = Setup.AssignedOrder();
        await Repository.AddAsync(storedOrderInAssignedStatus);
        
        await UnitOfWork.SaveChangesAsync();

        // Act.
        var orders = await Repository.GetAllInAssignedStatusAsync();
        
        // Assert.
        orders.Should().BeEquivalentTo([storedOrderInAssignedStatus], opt => opt.ComparingByMembers<Order>());
    }

    [Fact]
    public async Task WhenGettingAllInAssignedStatus_AndThereAreFewOrdersStoredInAssignedStatus_ThenOrdersShouldBeEquivalentToStoredOrdersInAssignedStatus()
    {
        // Arrange.
        var storedOrdersInAssignedStatus = Enumerable.Range(0, 5).Select(_ => Setup.AssignedOrder()).ToArray();
        foreach (var storedOrder in storedOrdersInAssignedStatus)
            await Repository.AddAsync(storedOrder);

        var storedOrdersInCreatedStatus = Enumerable.Range(0, 5).Select(_ => Create.Order()).ToArray();
        foreach (var storedOrder in storedOrdersInCreatedStatus)
            await Repository.AddAsync(storedOrder);
        
        await UnitOfWork.SaveChangesAsync();

        // Act.
        var orders = await Repository.GetAllInAssignedStatusAsync();

        // Assert.
        orders.Should().BeEquivalentTo(storedOrdersInAssignedStatus, opt => opt.ComparingByMembers<Order>());
    }
}