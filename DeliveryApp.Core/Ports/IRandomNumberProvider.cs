namespace DeliveryApp.Core.Ports;

public interface IRandomNumberProvider
{
    /// <summary>
    /// Returns a random integer that is within a specified range.
    /// </summary>
    /// <param name="minValue">The inclusive lower bound of the random number returned.</param>
    /// <param name="maxValue">The inclusive upper bound of the random number returned. <paramref name="maxValue"/> must be greater than or equal to <paramref name="minValue"/>.</param>
    /// <returns></returns>
    public int GetRandomNumber(int minValue, int maxValue);
}