using System;
using System.Collections.Generic;
using System.Linq;
using Data;

namespace GamePlay
{
	public class EntityContainer
	{
		private readonly Dictionary<EntityId, Entity> _entities = new Dictionary<EntityId, Entity>();

		public void Add(EntityId entityId, Entity entity)
		{
			_entities.Add(entityId, entity);
		}

		public void Remove(EntityId entityId)
		{
			_entities.Remove(entityId);
		}

		public TEntity GetById<TEntity>(EntityId id) where TEntity : Entity
		{
			var foundEntity = _entities.GetValueOrDefault(id);

			if (foundEntity == null)
			{
				throw new KeyNotFoundException($"Entity {id} not found");
			}

			if (foundEntity is not TEntity entity)
			{
				throw new InvalidCastException(nameof(TEntity));
			}
			
			return entity;
		}

		public IEnumerable<T> GetEntitiesByType<T>()
		{
			return _entities.Values.OfType<T>();
		}
	}
}