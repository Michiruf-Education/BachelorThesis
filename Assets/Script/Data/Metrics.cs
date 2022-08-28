using UnityEngine;

public static class Metrics
{
    public static HeightMetrics CalculateHeightMetrics(IReadableFloatField floatField)
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

        return new HeightMetrics
        {
            sum = sum,
            average = average,
            variance = variance
        };
    }

    public struct HeightMetrics
    {
        public float sum;
        public float average;
        public float variance;
    }

    public static float CalculateDerivationAbsolutes_OLD(IReadableFloatField floatField)
    {
        var result = 0f;
        for (var x = 0; x < floatField.width - 1; x++)
        {
            for (var y = 0; y < floatField.height - 1; y++)
            {
                var gradients = HeightAndGradient.Calculate(floatField, new Vector2Int(x, y), Vector2.one * 0.5f);
                result += Mathf.Abs(gradients.gradientX);
                result += Mathf.Abs(gradients.gradientY);
            }
        }
        return result;
    }

    public static float CalculateDerivationAbsolutes(IReadableFloatField floatField)
    {
        var result = 0f;
        for (var x = 0; x < floatField.width - 1; x++)
        {
            for (var y = 0; y < floatField.height; y++)
            {
                var h = floatField.GetValue(x, y);
                var hx = floatField.GetValue(x + 1, y);
                result += Mathf.Abs(h - hx);
            }
        }
        for (var x = 0; x < floatField.width; x++)
        {
            for (var y = 0; y < floatField.height - 1; y++)
            {
                var h = floatField.GetValue(x, y);
                var hy = floatField.GetValue(x, y + 1);
                result += Mathf.Abs(h - hy);
            }
        }
        return result;
    }
}
