using System;
using UnityEngine;

public interface IValueField<T>
{
    public int size { get; }

    public int GetIndex(int x, int y);

    public int GetIndex(Vector2Int v);

    public int GetXFromIndex(int index);

    public int GetYFromIndex(int index);

    public T GetValue(int index);

    public T GetValue(int x, int y);

    public void SetValue(int index, T value);

    public void SetValue(int x, int y, T value);

    public void ChangeValue(int x, int y, Func<T, T> changeFunction);

    public void ChangeValue(int x, int y, Func<int, int, T, T> changeFunction);

    public void ChangeAll(Func<T, T> changeFunction);

    public void ChangeAll(Func<int, int, T, T> changeFunction);

    public bool IsInBounds(int x, int y);
}
