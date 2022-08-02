using System;
using System.Linq;
using UnityEngine;

public class CompoundFloatField : IReadableFloatField
{
    private readonly BlendMode mode;
    private readonly FloatField[] fields;
    private FloatField first => fields[0];

    public int width => first.width;
    public int height => first.height;
    public int size => first.size;

    public CompoundFloatField(BlendMode mode, params FloatField[] fields)
    {
        this.mode = mode;
        this.fields = fields;
        
        var width = first.width;
        var height = first.height;
        if (fields.Any(field => field.width != width || field.height != height))
            throw new ArgumentException("Fields are not of equal size");
    }

    public int GetIndex(int x, int y)
    {
        return first.GetIndex(x, y);
    }

    public int GetIndex(Vector2Int v)
    {
        return first.GetIndex(v);
    }

    public int GetXFromIndex(int index)
    {
        return first.GetXFromIndex(index);
    }

    public int GetYFromIndex(int index)
    {
        return first.GetYFromIndex(index);
    }

    public float GetValue(int index)
    {
        return fields.Aggregate(float.NaN, (f, field) => float.IsNaN(f) ? field.GetValue(index) : Blend.Calc(mode, f, field.GetValue(index)));
    }

    public float GetValue(int x, int y)
    {
        return fields.Aggregate(float.NaN, (f, field) => float.IsNaN(f) ? field.GetValue(x, y) : Blend.Calc(mode, f, field.GetValue(x, y)));
    }

    public bool IsInBounds(int x, int y)
    {
        return first.IsInBounds(x, y);
    }

    public Texture2D ToTexture(string name = "ValueField Texture2D", TextureWrapMode wrapMode = TextureWrapMode.Clamp,
        FilterMode filterMode = FilterMode.Point)
    {
        return first.ToTexture(name, wrapMode, filterMode);
    }

    public void ApplyToTexture(Texture2D texture)
    {
        first.ApplyToTexture(texture);
    }

    public float this[int index] => GetValue(index);

    public float this[int x, int y] => GetValue(x, y);
}
