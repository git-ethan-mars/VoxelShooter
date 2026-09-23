using System;

namespace Data
{
    public readonly struct EntityId : IEquatable<EntityId>
    {
        private readonly uint _entityId;

        public EntityId(uint entityId)
        {
            _entityId = entityId;
        }

        public bool Equals(EntityId other)
        {
            return _entityId == other._entityId;
        }

        public override bool Equals(object obj)
        {
            return obj is EntityId other && Equals(other);
        }

        public override int GetHashCode()
        {
            return _entityId.GetHashCode();
        }

        public override string ToString()
        {
            return $"EntityId: {_entityId}";
        }
        
        public static bool operator ==(EntityId first, EntityId second) => first.Equals(second);
        public static bool operator !=(EntityId first, EntityId second) => !(first == second);
    }
}