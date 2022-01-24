using MyBox;
using UnityEngine;

public struct Droplet
{
    public Vector2 position
    {
        get => positionValue;
        set
        {
            positionValue = value;
            cellPositionInt = new Vector2Int((int) positionValue.x, (int) positionValue.y);
            cellPosition = cellPositionInt.ToVector2();
            cellOffset = positionValue - cellPosition;
        }
    }
    private Vector2 positionValue;
    public Vector2 direction;
    public float speed;
    public float water;
    public float sediment;

    public Vector2Int cellPositionInt { get; private set; }
    public Vector2 cellPosition { get; private set; }
    public Vector2 cellOffset { get; private set; }

    public Droplet(float x, float y, float speed, float water) : this()
    {
        position = new Vector2(x, y);
        direction = Vector2.zero;
        this.speed = speed;
        this.water = water;
        sediment = 0;
    }

    public Droplet(Droplet droplet) : this()
    {
        position = droplet.position;
        direction = droplet.direction;
        speed = droplet.speed;
        water = droplet.water;
        sediment = droplet.sediment;
    }

    public int CalculateIndex(FloatField field)
    {
        return field.GetIndex(cellPositionInt.x, cellPositionInt.y);
    }
}
