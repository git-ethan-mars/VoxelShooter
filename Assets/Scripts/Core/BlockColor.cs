using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    public static class BlockColor
    {
        private class ColorBoarder
        {
            public readonly Color StartColor;
            public readonly Color EndColor;

            public ColorBoarder(Color startColor, Color endColor)
            {
                StartColor = startColor;
                EndColor = endColor;
            }
        }

        public static readonly Color32 Empty = new(0,0,0,0);

        private static Dictionary<byte, Color> _colorById;
        public static Dictionary<byte, Color> ColorById => _colorById ??= CacheColorByIdDictionary();
        
        public static Color32 UIntToColor(uint color)
        {
            var a = (byte) (color >> 24);
            var r = (byte) (color >> 16);
            var g = (byte) (color >> 8);
            var b = (byte) (color >> 0);
            return new Color32(r, g, b, a);
        }
        

        private static Dictionary<byte, Color> CacheColorByIdDictionary()
        {
            var blockColorById = new Dictionary<byte, Color>();
            var colorBoarders = new ColorBoarder[]
            {
                new(Color.black, Color.white),
                new(new Color(72 / 256f, 0, 0), new Color(255 / 256f, 217 / 256f, 217 / 256f)),
                new(new Color(74 / 256f, 37 / 256f, 0), new Color(255 / 256f, 223 / 256f, 191 / 256f)),
                new(new Color(70 / 256f, 70 / 256f, 0), new Color(255 / 256f, 255 / 256f, 210 / 256f)),
                new(new Color(0, 50 / 256f, 0), new Color(201 / 256f, 185 / 256f, 136 / 256f)),
                new(new Color(3 / 256f, 82 / 256f, 86 / 256f),
                    new Color(193 / 256f, 250 / 256f, 253 / 256f)),
                new(new Color(0, 0, 70 / 256f), new Color(215 / 256f, 215 / 256f, 255 / 256f)),
                new(new Color(50 / 256f, 0, 100 / 256f), new Color(239 / 256f, 223 / 256f, 255 / 256f))
            };
            for (var i = 0; i < colorBoarders.Length; i++)
            {
                for (var j = 0; j < colorBoarders.Length; j++)
                {
                    blockColorById[(byte)(j + i * colorBoarders.Length+1)] =
                        CreateGradient(colorBoarders[i].StartColor, colorBoarders[i].EndColor, (float) j / 8);
                }
            }

            return blockColorById;
        }


        private static Color CreateGradient(Color startValue, Color endValue, float percent)
        {
            var (h1, s1, l1) = RGB2HSL(startValue);
            var (h2, s2, l2) = RGB2HSL(endValue);
            var newHue = (h2 - h1) * percent + h1;
            var newSaturation = (s2 - s1) * percent + s1;
            var newLightness = (l2 - l1) * percent + l1;
            return HSL2RGB(newHue, newSaturation, newLightness);
        }

        private static (float, float, float) RGB2HSL(Color rgb)
        {
            var max = Math.Max(Math.Max(rgb.r, rgb.g), rgb.b);
            var min = Math.Min(Math.Min(rgb.r, rgb.g), rgb.b);
            var lightness = (max + min) / 2;
            float saturation;
            if (lightness == 0 || Math.Abs(max - min) < 1e-7)
            {
                saturation = 0;
            }
            else
            {
                saturation = (max - min) / (1 - Math.Abs(1 - (max + min)));
            }

            if (Math.Abs(max - min) < 1e-7) return (0, saturation, lightness);
            if (Math.Abs(max - rgb.r) < 1e-7 && rgb.g >= rgb.b)
                return (60 * (rgb.g - rgb.b) / (max - min), saturation, lightness);
            if (Math.Abs(max - rgb.r) < 1e-7 && rgb.g < rgb.b)
                return (60 * (rgb.g - rgb.b) / (max - min) + 360, saturation, lightness);
            if (Math.Abs(max - rgb.g) < 1e-7) return (60 * (rgb.b - rgb.r) / (max - min) + 120, saturation, lightness);
            if (Math.Abs(max - rgb.b) < 1e-7) return (60 * (rgb.r - rgb.g) / (max - min) + 240, saturation, lightness);
            return (0, saturation, lightness);
        }

        private static Color HSL2RGB(float hue, float saturation, float lightness)
        {
            var c = (1 - Math.Abs(2 * lightness - 1)) * saturation;
            var x = c * (1 - Math.Abs(hue / 60 % 2 - 1));
            var m = lightness - c / 2;
            float r = 0;
            float g = 0;
            float b = 0;
            if (0 <= hue && hue < 60)
            {
                r = c;
                g = x;
            }

            if (60 <= hue && hue < 120)
            {
                r = x;
                g = c;
            }

            if (120 <= hue && hue < 180)
            {
                g = c;
                b = x;
            }

            if (180 <= hue && hue < 240)
            {
                g = x;
                b = c;
            }

            if (240 <= hue && hue < 300)
            {
                r = x;
                b = c;
            }

            if (300 <= hue && hue < 360)
            {
                r = c;
                b = x;
            }

            if (hue >= 360)
                Debug.Log("FUCK");
            return new Color(r + m, g + m, b + m);
        }
    }
}