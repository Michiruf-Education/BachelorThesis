using UnityEngine;

public interface IReadableValueField<out T>
{
    public int width { get; }
    public int height { get; }
    public int size { get; }

    public int GetIndex(int x, int y);

    public int GetIndex(Vector2Int v);

    public int GetXFromIndex(int index);

    public int GetYFromIndex(int index);

    public T GetValue(int index);

    public T GetValue(int x, int y);

    public bool IsInBounds(int x, int y);
}
