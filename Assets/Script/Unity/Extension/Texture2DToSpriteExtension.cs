using UnityEngine;

public static class Texture2DToSpriteExtension
{
    public static Sprite ToSprite(this Texture2D texture, Vector2 pivot = default)
    {
        return Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), pivot);
    }
}
