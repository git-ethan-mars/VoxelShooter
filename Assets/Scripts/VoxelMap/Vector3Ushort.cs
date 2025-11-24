namespace VoxelMap
{
	using System;
	using UnityEngine;

	/// <summary>
	/// Представляет вектор с тремя компонентами типа ushort (0–65535).
	/// Аналог Vector3Int, но для ushort.
	/// </summary>
	[Serializable]
	public struct Vector3Ushort : IEquatable<Vector3Ushort>
	{
		public ushort x;
		public ushort y;
		public ushort z;

		public Vector3Ushort(ushort x, ushort y, ushort z)
		{
			this.x = x;
			this.y = y;
			this.z = z;
		}

		// Свойства для удобства
		public static Vector3Ushort zero => new Vector3Ushort(0, 0, 0);
		public static Vector3Ushort one => new Vector3Ushort(1, 1, 1);

		// Преобразование в Vector3
		public Vector3 ToVector3() => new Vector3(x, y, z);

		// Преобразование из Vector3 (округление вниз)
		public static Vector3Ushort FloorToUshort(Vector3 v)
		{
			return new Vector3Ushort(
				(ushort)Mathf.Clamp(Mathf.FloorToInt(v.x), 0, ushort.MaxValue),
				(ushort)Mathf.Clamp(Mathf.FloorToInt(v.y), 0, ushort.MaxValue),
				(ushort)Mathf.Clamp(Mathf.FloorToInt(v.z), 0, ushort.MaxValue)
			);
		}

		public static Vector3Ushort RoundToUshort(Vector3 v)
		{
			return new Vector3Ushort(
				(ushort)Mathf.Clamp(Mathf.RoundToInt(v.x), 0, ushort.MaxValue),
				(ushort)Mathf.Clamp(Mathf.RoundToInt(v.y), 0, ushort.MaxValue),
				(ushort)Mathf.Clamp(Mathf.RoundToInt(v.z), 0, ushort.MaxValue)
			);
		}

		// Арифметика (если нужна)
		public static Vector3Ushort operator +(Vector3Ushort a, Vector3Ushort b)
		{
			return new Vector3Ushort(
				(ushort)(a.x + b.x),
				(ushort)(a.y + b.y),
				(ushort)(a.z + b.z)
			);
		}

		public static Vector3Ushort operator -(Vector3Ushort a, Vector3Ushort b)
		{
			return new Vector3Ushort(
				(ushort)(a.x - b.x),
				(ushort)(a.y - b.y),
				(ushort)(a.z - b.z)
			);
		}
		
		public static Vector3Ushort operator *(Vector3Ushort a, int b)
		{
			return new Vector3Ushort(
				(ushort)(a.x * b),
				(ushort)(a.y * b),
				(ushort)(a.z * b)
			);
		}
		
		public static Vector3Ushort operator *(int a, Vector3Ushort b)
		{
			return new Vector3Ushort(
				(ushort)(a * b.x),
				(ushort)(a * b.y),
				(ushort)(a * b.z)
			);
		}
		
		public static implicit operator Vector3(Vector3Ushort v)
		{
			return new Vector3(v.x, v.y, v.z);
		} 

		// Сравнение
		public bool Equals(Vector3Ushort other)
		{
			return x == other.x && y == other.y && z == other.z;
		}

		public override bool Equals(object obj)
		{
			return obj is Vector3Ushort other && Equals(other);
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(x, y, z);
		}

		public static bool operator ==(Vector3Ushort a, Vector3Ushort b)
		{
			return a.Equals(b);
		}

		public static bool operator !=(Vector3Ushort a, Vector3Ushort b)
		{
			return !a.Equals(b);
		}

		public override string ToString()
		{
			return $"({x}, {y}, {z})";
		}
	}
}