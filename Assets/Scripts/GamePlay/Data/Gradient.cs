using System;
using UnityEngine;

namespace GamePlay.Data
{
	[Serializable]
	public class Gradient
	{
		[SerializeField]
		private Color start;
		
		[SerializeField]
		public Color end;
		
		public Gradient(Color start, Color end)
		{
			this.start = start;
			this.end = end;
		}

		public Color CalculateGradient(float value)
		{
			var percent = Mathf.Clamp(value, 0, 1);
			var (h1, s1, l1) = RGB2HSL(start);
			var (h2, s2, l2) = RGB2HSL(end);
			var newHue = (h2 - h1) * percent + h1;
			var newSaturation = (s2 - s1) * percent + s1;
			var newLightness = (l2 - l1) * percent + l1;
			return HSL2RGB(newHue, newSaturation, newLightness);
		}

		private (float, float, float) RGB2HSL(Color rgb)
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

		private Color HSL2RGB(float hue, float saturation, float lightness)
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

			return new Color(r + m, g + m, b + m);
		}
	}
}