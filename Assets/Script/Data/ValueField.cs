public class ValueField<T>
{
    public readonly int width;
    public readonly int height;
    public readonly T[] values;

    public ValueField(int width, int height)
    {
        this.width = width;
        this.height = height;
        values = new T[width * height];
    }

    public T GetValue(int x, int y)
    {
        return values[y * width + x];
    }

    public void SetValue(int x, int y, T value)
    {
        values[y * width + x] = value;
    }
}
