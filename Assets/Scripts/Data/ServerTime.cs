using System;

namespace Data
{
    public readonly struct ServerTime : IEquatable<ServerTime>
    {
        public readonly int Minutes;
        public readonly int Seconds;

        public ServerTime(int minutes, int seconds)
        {
            Minutes = minutes;
            Seconds = seconds;
        }

        public ServerTime(int seconds)
        {
            Minutes = seconds / 60;
            Seconds = seconds % 60;
        }

        public ServerTime Subtract(ServerTime another)
        {
            var totalSeconds = TotalSecond - another.TotalSecond; 
            return new ServerTime(totalSeconds / 60, totalSeconds % 60);
        }

        public int TotalSecond => Seconds + Minutes * 60;

        public bool Equals(ServerTime other)
        {
            return Minutes == other.Minutes && Seconds == other.Seconds;
        }

        public override bool Equals(object obj)
        {
            return obj is ServerTime other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Minutes, Seconds);
        }
    }
}