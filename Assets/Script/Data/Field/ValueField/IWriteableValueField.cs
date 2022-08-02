using System;

public interface IWriteableValueField<T>
{
    public void SetValue(int index, T value);

    public void SetValue(int x, int y, T value);

    public void ChangeValue(int x, int y, Func<T, T> changeFunction);

    public void ChangeValue(int x, int y, Func<int, int, T, T> changeFunction);

    public void ChangeAll(Func<T, T> changeFunction);

    public void ChangeAll(Func<int, int, T, T> changeFunction);
}
