using System;

namespace Data
{
	public struct PlayerId : IEquatable<PlayerId>
	{
		private readonly int _id;

		public PlayerId(int id)
		{
			_id = id;
		}

		public bool Equals(PlayerId other)
		{
			return _id == other._id;
		}

		public override bool Equals(object obj)
		{
			return obj is PlayerId other && Equals(other);
		}

		public override int GetHashCode()
		{
			return _id;
		}

		public static bool operator ==(PlayerId first, PlayerId second)
		{
			return first.Equals(second);
		}

		public static bool operator !=(PlayerId first, PlayerId second)
		{
			return !(first == second);
		}
	}
}
