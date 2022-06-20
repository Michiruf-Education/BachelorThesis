using MyBox;
using UnityEngine;

public struct WindParticleData
{
    public Vector2 position
    {
        get => positionValue;
        set
        {
            positionValue = value;
            positionOfCell = new Vector2Int((int) positionValue.x, (int) positionValue.y);
            positionOfCellAsFloat = positionOfCell.ToVector2();
            positionRelativeToCell = positionValue - positionOfCellAsFloat;
        }
    }
    private Vector2 positionValue;
    public float height;
    public Vector3 velocity;
    public float sediment;

    public Vector2Int positionOfCell { get; private set; }
    public Vector2 positionOfCellAsFloat { get; private set; }
    public Vector2 positionRelativeToCell { get; private set; }

    public WindParticleData(float x, float y) : this()
    {
        position = new Vector2(x, y);
    }

    public WindParticleData(WindParticleData data) : this()
    {
        position = data.position;
        height = data.height;
        velocity = data.velocity;
        sediment = data.sediment;
    }
}
