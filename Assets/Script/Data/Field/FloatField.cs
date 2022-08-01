using System;
using System.Linq;
using UnityEngine;

public partial class FloatField : ValueField<float>
{
    public FloatField(int width, int height) : base(width, height)
    {
    }

    public void BlendValue(int x, int y, BlendMode mode, float value)
    {
        ChangeValue(x, y, f => Blend.Calc(mode, f, value));
    }

    public void BlendAll(BlendMode mode, float value)
    {
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
            ChangeValue(x, y, f => Blend.Calc(mode, f, value));
    }

    public void BlendAll(BlendMode mode, FloatField values)
    {
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
            ChangeValue(x, y, (fieldX, fieldY, f) => Blend.Calc(mode, f, values.GetValue(fieldX, fieldY)));
    }

    public void Remap(float min, float max)
    {
        // Maybe consider using one of these functions instead
        // https://github.com/Wasserwecken/glslLib/blob/master/lib/helper.glsl
        var vMin = values.Min();
        var vMax = values.Max();
        var breadth = vMax - vMin;
        for (var i = 0; i < values.Length; i++)
        {
            var percentage = (values[i] - vMin) / breadth;
            values[i] = Mathf.Lerp(min, max, percentage);
        }
    }
}
