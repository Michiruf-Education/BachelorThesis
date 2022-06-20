using System;

public class ValueField<T>
{
    public readonly int width;
    public readonly int height;
    public T[] values;

    public int size => width * height;

    public ValueField(int width, int height)
    {
        this.width = width;
        this.height = height;
        values = new T[width * height];
    }

    public int GetIndex(int x, int y)
    {
        return y * width + x;
    }

    public T GetValue(int x, int y)
    {
        return values[GetIndex(x, y)];
    }

    public void SetValue(int x, int y, T value)
    {
        values[GetIndex(x, y)] = value;
    }

    public void ChangeValue(int x, int y, Func<T, T> changeFunction)
    {
        SetValue(x, y, changeFunction(GetValue(x, y)));
    }

    public bool IsInBounds(int x, int y)
    {
        return x >= 0 && x < width && y >= 0 && y < height;
    }

    public T this[int index]
    {
        get => values[index];
        set => values[index] = value;
    }

    public T this[int x, int y]
    {
        get => GetValue(x, y);
        set => SetValue(x, y, value);
    }
}
