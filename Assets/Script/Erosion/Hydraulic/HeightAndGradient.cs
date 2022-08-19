using UnityEngine;

public struct HeightAndGradient
{
    public float height;
    public float gradientX;
    public float gradientY;

    public static HeightAndGradient Calculate(IReadableFloatField field, Droplet droplet)
    {
        return Calculate(field, droplet.cellPositionInt, droplet.cellOffset);
    }

    public static HeightAndGradient Calculate(IReadableFloatField field, Vector2Int cellPosition, Vector2 offsetInCell)
    {
        // Calculate heights of the four nodes of the droplet's cell
        var heightNW = field.GetValue(cellPosition.x, cellPosition.y);
        var heightNE = field.GetValue(cellPosition.x + 1, cellPosition.y);
        var heightSW = field.GetValue(cellPosition.x, cellPosition.y + 1);
        var heightSE = field.GetValue(cellPosition.x + 1, cellPosition.y + 1);

        // Calculate height with bilinear interpolation of the heights of the nodes of the cell
        // Offset inside the cell (0,0) = at NW node, (1,1) = at SE node
        var height =
            heightNW * (1 - offsetInCell.x) * (1 - offsetInCell.y) +
            heightNE * offsetInCell.x * (1 - offsetInCell.y) +
            heightSW * (1 - offsetInCell.x) * offsetInCell.y +
            heightSE * offsetInCell.x * offsetInCell.y;

        // Calculate droplet's direction of flow with bilinear interpolation of height difference along the edges
        var gradientX = (heightNE - heightNW) * (1 - offsetInCell.y) + (heightSE - heightSW) * offsetInCell.y;
        var gradientY = (heightSW - heightNW) * (1 - offsetInCell.x) + (heightSE - heightNE) * offsetInCell.x;

        return new HeightAndGradient
        {
            height = height,
            gradientX = gradientX,
            gradientY = gradientY
        };
    }
}
