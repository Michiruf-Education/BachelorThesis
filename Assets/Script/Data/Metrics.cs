using UnityEngine;

public struct Metrics
{
    public float sum;
    public float average;
    public float variance;

    public static Metrics Calculate(IReadableFloatField floatField)
    {
        var sum = 0f;
        for (var i = 0; i < floatField.size; i++)
        {
            sum += floatField.GetValue(i);
        }
        var average = sum / floatField.size;

        // Calculate variance
        var varianceSum = 0f;
        for (var i = 0; i < floatField.size; i++)
        {
            varianceSum += Mathf.Pow(floatField.GetValue(i) - average, 2);
        }
        var variance = varianceSum / floatField.size;

        return new Metrics
        {
            sum = sum,
            average = average,
            variance = variance
        };
    }
}
