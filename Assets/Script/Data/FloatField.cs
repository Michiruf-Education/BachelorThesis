using UnityEngine;

public class FloatField : ValueField<float>
{
    public FloatField(int width, int height) : base(width, height)
    {
    }

    public Texture2D ToTexture(
        string name = "ValueField Texture2D",
        TextureWrapMode wrapMode = TextureWrapMode.Clamp,
        FilterMode filterMode = FilterMode.Point
    )
    {
        var texture = new Texture2D(width, height, TextureFormat.RGBA64, false)
        {
            name = name,
            wrapMode = wrapMode,
            filterMode = filterMode
        };

        ApplyToTexture(texture);
        return texture;
    }

    public void ApplyToTexture(Texture2D texture)
    {
        var pixels = new Color[texture.width * texture.height];
        for (var y = 0; y < texture.height; y++)
        {
            for (var x = 0; x < texture.width; x++)
            {
                var value = GetValue(x, y);
                pixels[y * texture.width + x] = new Color(value, value, value);
            }
        }

        // Copy the pixel data to the texture and load it into the GPU
        texture.SetPixels(pixels);
        texture.Apply();
    }
}
