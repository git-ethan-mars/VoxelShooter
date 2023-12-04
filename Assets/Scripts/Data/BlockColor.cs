using UnityEngine;

namespace Data
{
    public static class BlockColor
    {
        public static readonly Color32 empty = new(0, 0, 0, 0);

        public static Color32 UInt32ToColor(uint color)
        {
            var a = (byte) (color >> 24);
            var r = (byte) (color >> 16);
            var g = (byte) (color >> 8);
            var b = (byte) (color >> 0);
            return new Color32(r, g, b, a);
        }
    }
}