using DeliveryApp.Core.Domain.Model.CourierAggregate;
using DeliveryApp.Core.Domain.Model.SharedKernel;
using DeliveryApp.Infrastructure.Adapters.Postgres.Repositories;
using DeliveryApp.TestsCommon;
using FluentAssertions;
using Xunit;

namespace DeliveryApp.IntegrationTests.Repositories;

public class CourierRepositoryTests : RepositoryBaseTests<CourierRepository>
{
    [Fact]
    public async Task WhenGetting_AndCourierIsAlreadyAdded_ThenCourierShouldBeEquivalentToStoredCourier()
    {
        // Arrange.
        var storedCourier = Create.Courier();
        await Repository.AddAsync(storedCourier);
        await UnitOfWork.SaveChangesAsync();

        // Act.
        var courier = (await Repository.GetAsync(storedCourier.Id)).Value;
        
        // Assert.
        courier.Should().BeEquivalentTo(storedCourier, opt => opt.ComparingByMembers<Courier>());
    }
    
    [Fact]
    public async Task WhenGetting_AndCourierIsAddedButNotSaved_ThenMaybeCourierShouldHasNoValue()
    {
        // Arrange.
        var storedCourier = Create.Courier();
        await Repository.AddAsync(storedCourier);

        // Act.
        var maybeCourier = await Repository.GetAsync(storedCourier.Id);

        // Assert.
        maybeCourier.HasNoValue.Should().BeTrue();        
    }
    
    [Fact]
    public async Task WhenGetting_AndCourierWasAddedAndUpdatedInDifferentContexts_ThenCourierShouldBeEquivalentToUpdatedCourier()
    {
        // Arrange.
        Courier updatedCourier = null;
        await ExecuteInNewContext(async (repository, unitOfWork) =>
        {
            updatedCourier = Create.Courier();
            await repository.AddAsync(updatedCourier);
            await unitOfWork.SaveChangesAsync();
        });
        
        await ExecuteInNewContext(async (repository, unitOfWork) =>
        {
            updatedCourier.Move(Location.MaxLocation);
            repository.Update(updatedCourier);
            await unitOfWork.SaveChangesAsync();
        });
        
        // Act.
        var courier = (await Repository.GetAsync(updatedCourier.Id)).Value;

        // Assert.
        courier.Should().BeEquivalentTo(updatedCourier, opt => opt.ComparingByMembers<Courier>());

    }

    [Fact]
    public async Task WhenGetting_AndCourierIsNotStored_ThenMaybeCourierShouldHasNoValue()
    {
        // Arrange.
        var notStoredCourier = Create.Courier();
        
        // Act.
        var maybeCourier = await Repository.GetAsync(notStoredCourier.Id);
        
        // Assert.
        maybeCourier.HasNoValue.Should().BeTrue();
    }
    
    [Fact]
    public async Task WhenSavingChanges_AndCourierWasUpdatedButCourierWasNotStoredBeforeUpdate_ThenExceptionShouldBeThrown()
    {
        // Arrange.
        var notStoredCourier = Create.Courier();
        Repository.Update(notStoredCourier);

        // Act.
        Func<Task> act = async () => await UnitOfWork.SaveChangesAsync();

        // Assert.
        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task WhenGettingAllFreeCouriers_AndThereAreNoCouriers_ThenCouriersShouldBeEmpty()
    {
        // Arrange and Act.
        var couriers = await Repository.GetAllFreeCouriersAsync();

        // Assert.
        couriers.Should().BeEmpty();
    }

    [Fact]
    public async Task WhenGettingAllFreeCouriers_AndThereIsSingleFreeCourierAddedInAnotherContext_ThenCouriersShouldBeAddedCourier()
    {
        // Arrange.
        Courier addedCourier = null;
        await ExecuteInNewContext(async (repository, unitOfWork) =>
        {
            addedCourier = Create.Courier();
            await repository.AddAsync(addedCourier);
            await unitOfWork.SaveChangesAsync();
        });

        // Act.
        var couriers = await Repository.GetAllFreeCouriersAsync();

        // Assert.
        couriers.Should().BeEquivalentTo([addedCourier], opt => opt.ComparingByMembers<Courier>());
    }

    [Fact]
    public async Task WhenGettingAllFreeCouriers_AndThereAreFewCouriersButEachHasAtLeastOneOccupiedStoragePlace_ThenCouriersShouldBeEmpty()
    {
        // Arrange.
        var courier1 = Setup.BusyCourier();
        await Repository.AddAsync(courier1);
        
        var courier2 = Create.Courier();
        courier2.StoragePlaces.Add(Create.StoragePlace());
        courier2.TakeOrder(Create.Order());
        await Repository.AddAsync(courier2);

        await UnitOfWork.SaveChangesAsync();

        // Act.
        var couriers = await Repository.GetAllFreeCouriersAsync();

        // Assert.
        couriers.Should().BeEmpty();
    }

    [Fact]
    public async Task WhenGettingAllFreeCouriers_AndFewCouriersAreFreeButFewAreNot_ThenCouriersShouldBeEquivalentToFreeCouriers()
    {
        // Arrange.
        var freeCouriers = Enumerable.Range(0, 5).Select(_ => Create.Courier()).ToArray();
        foreach (var courier in freeCouriers)
            await Repository.AddAsync(courier);

        var busyCourier1 = Setup.BusyCourier();
        await Repository.AddAsync(busyCourier1);
        
        var busyCourier2 = Create.Courier();
        busyCourier2.StoragePlaces.Add(Create.StoragePlace());
        busyCourier2.TakeOrder(Create.Order());
        await Repository.AddAsync(busyCourier2);
        
        await UnitOfWork.SaveChangesAsync();

        // Act.
        var couriers = await Repository.GetAllFreeCouriersAsync();

        // Assert.
        couriers.Should().BeEquivalentTo(freeCouriers, opt => opt.ComparingByMembers<Courier>());
    }
}