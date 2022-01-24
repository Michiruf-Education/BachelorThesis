public struct HeightAndGradient
{
    public float height;
    public float gradientX;
    public float gradientY;

    public static HeightAndGradient Calculate(FloatField field, Droplet droplet)
    {
        // Get droplet's offset inside the cell (0,0) = at NW node, (1,1) = at SE node
        var offset = droplet.cellOffset;

        // Calculate heights of the four nodes of the droplet's cell
        var heightNW = field.GetValue(droplet.cellPositionInt.x, droplet.cellPositionInt.y);
        var heightNE = field.GetValue(droplet.cellPositionInt.x + 1, droplet.cellPositionInt.y);
        var heightSW = field.GetValue(droplet.cellPositionInt.x, droplet.cellPositionInt.y + 1);
        var heightSE = field.GetValue(droplet.cellPositionInt.x + 1, droplet.cellPositionInt.y + 1);

        // Calculate height with bilinear interpolation of the heights of the nodes of the cell
        var height =
            heightNW * (1 - offset.x) * (1 - offset.y) +
            heightNE * offset.x * (1 - offset.y) +
            heightSW * (1 - offset.x) * offset.y +
            heightSE * offset.x * offset.y;

        // Calculate droplet's direction of flow with bilinear interpolation of height difference along the edges
        var gradientX = (heightNE - heightNW) * (1 - offset.y) + (heightSE - heightSW) * offset.y;
        var gradientY = (heightSW - heightNW) * (1 - offset.x) + (heightSE - heightNE) * offset.x;

        return new HeightAndGradient
        {
            height = height,
            gradientX = gradientX,
            gradientY = gradientY
        };
    }
}
