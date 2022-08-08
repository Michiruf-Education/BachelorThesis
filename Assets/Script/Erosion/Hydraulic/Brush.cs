using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// TODO For any reason this brush has slightly different values than the old one.
//      The old one is for backup reasons in the same commit as this comment was introduced in a file names "OldBrush.cs".
//      And is deleted in the following cleanup commit.
public class Brush
{
    public ValueField<BrushPoint[]> brushMap;

    public Brush(int width, int height, int radius)
    {
        Initialize(width, height, radius);
    }

    private void Initialize(int width, int height, int radius)
    {
        brushMap = new ValueField<BrushPoint[]>(width, height);

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                brushMap[x, y] = CalculateBrushDataForPosition(x, y, radius);
            }
        }
    }

    private BrushPoint[] CalculateBrushDataForPosition(int x, int y, int radius)
    {
        var data = new List<BrushPoint>();

        var sqrRadius = radius * radius;
        for (var currentRadiusY = -radius; currentRadiusY <= radius; currentRadiusY++)
        {
            for (var currentRadiusX = -radius; currentRadiusX <= radius; currentRadiusX++)
            {
                // Ensure that the coordinates are inside the radius
                float sqrDst = currentRadiusX * currentRadiusX + currentRadiusY * currentRadiusY;
                if (sqrDst >= sqrRadius)
                    continue;

                // Calc real coords
                var coordX = x + currentRadiusX;
                var coordY = y + currentRadiusY;

                // Ensure in bounds
                if (coordX < 0 ||
                    coordX >= brushMap.width ||
                    coordY < 0 ||
                    coordY >= brushMap.height)
                    continue;

                // Add the data
                data.Add(new BrushPoint
                {
                    position = new Vector2Int(coordX, coordY),
                    index = brushMap.GetIndex(coordX, coordY),
                    weight = 1 - Mathf.Sqrt(sqrDst) / radius
                });
            }
        }

        // Normalize weights
        var weightSum = data.Sum(point => point.weight);
        for (var i = 0; i < data.Count; i++)
        {
            var d = data[i];
            d.weight /= weightSum;
            data[i] = d;
        }

        return data.ToArray();
    }

    public struct BrushPoint
    {
        public Vector2Int position;
        public int index;
        public float weight;

        public override string ToString()
        {
            return $"({position.x}, {position.y}) - i: {index} - w: {weight}";
        }
    }
}
