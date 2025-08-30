using DeliveryApp.Core.Ports;

namespace DeliveryApp.Infrastructure.Adapters.DotnetRandom;

public sealed class RandomNumberProvider : IRandomNumberProvider
{
    private readonly Random _random = new ();

    public int GetRandomNumber(int minValue, int maxValue)
    {
        var randomNumber = _random.Next(minValue, maxValue);
        if (maxValue == int.MaxValue && randomNumber == int.MaxValue - 1)
        {
            var addOne = _random.NextDouble() >= 0.5;
            randomNumber += addOne ? 1 : 0;
        }

        return randomNumber;
    }
}