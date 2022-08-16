using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using Debug = UnityEngine.Debug;

// TODO For any reason this brush has slightly different values than the old one.
//      The old one is for backup reasons in the same commit as this comment was introduced in a file names "OldBrush.cs".
//      And is deleted in the following cleanup commit.
public class Brush
{
    public ValueField<BrushPoint[]> brushMap;

    public Brush(int width, int height, int radius)
    {
        var timer = new Stopwatch();
        timer.Start();
        
        Initialize(width, height, radius);
        
        timer.Stop();
        Debug.Log($"Brush initialization finished after {timer.ElapsedMilliseconds}ms");
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
                    weightNotNormalized = 1 - Mathf.Sqrt(sqrDst) / radius
                });
            }
        }

        // Normalize weights
        var weightSum = data.Sum(point => point.weightNotNormalized);
        for (var i = 0; i < data.Count; i++)
        {
            var d = data[i];
            d.weight = d.weightNotNormalized / weightSum;
            data[i] = d;
        }

        return data.ToArray();
    }

    [Obsolete("This is just to visualize the brush in a float field")]
    internal void Debug_VisualizeBrush(FloatField groundMap)
    {
        var brush = new Brush(groundMap.width, groundMap.height, 3);

        var brushMaxIndex = 0;
        var brushMaxCount = 0;
        var brushMax = int.MinValue;
        for (var i = 0; i < brush.brushMap.values.Length; i++)
        {
            var c = brush.brushMap.values[i];
            if (c.Length > brushMax)
            {
                brushMax = c.Length;
                brushMaxIndex = i;
                brushMaxCount = 1;
            } else if (c.Length == brushMax)
                brushMaxCount++;
        }
        Debug.Log($"max length {brushMax} with {brushMaxCount} elements and one index is {brushMaxIndex}");


        var index = brushMaxIndex;
        var brushPoint = brush.brushMap[index];
        var logNew = brushPoint
            .Select(p => string.Format("{0} ({1}{2}) => {3:N2}\n", p.index, p.position.x, p.position.y, p.weight))
            .Aggregate("", (r, t) => r + t);
        Debug.Log(logNew);
        for (var i = 0; i < groundMap.size; i++)
        {
            try
            {
                var d = brushPoint.First(point => point.index == i);
                groundMap[i] = d.weightNotNormalized;
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }

    public struct BrushPoint
    {
        public Vector2Int position;
        public int index;
        public float weight;
        public float weightNotNormalized;

        public override string ToString()
        {
            return $"({position.x}, {position.y}) - i: {index} - w: {weight}";
        }
    }
}
