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
