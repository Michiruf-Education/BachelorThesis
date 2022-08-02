using UnityEngine;

public interface IReadableFloatField : IReadableValueField<float>
{
    public Texture2D ToTexture(
        string name = "ValueField Texture2D",
        TextureWrapMode wrapMode = TextureWrapMode.Clamp,
        FilterMode filterMode = FilterMode.Point
    );

    public void ApplyToTexture(Texture2D texture);
}
