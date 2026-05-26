using UnityEngine;

namespace Vertigo.Wheel.Views
{
    internal static class WheelSliceIconFitter
    {
        public static Vector2 FitWithinBounds(Vector2 sliceSize, Sprite sprite)
        {
            Vector2 maxSize = sliceSize * WheelSliceMotion.IconBoundsScale;
            if (sprite == null)
            {
                return maxSize;
            }

            float spriteWidth = sprite.rect.width;
            float spriteHeight = sprite.rect.height;
            if (spriteWidth <= 0f || spriteHeight <= 0f)
            {
                return maxSize;
            }

            float spriteAspect = spriteWidth / spriteHeight;
            float boundsAspect = maxSize.x / maxSize.y;
            if (spriteAspect > boundsAspect)
            {
                return new Vector2(maxSize.x, maxSize.x / spriteAspect);
            }

            return new Vector2(maxSize.y * spriteAspect, maxSize.y);
        }
    }
}
