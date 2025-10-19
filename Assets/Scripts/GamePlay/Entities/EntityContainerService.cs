using System.Collections.Generic;
using System.Linq;
namespace GamePlay
{
	public class EntityContainerService
	{
		private readonly HashSet<IEntity> _entities = new HashSet<IEntity>();

		public void Add(IEntity entity)
		{
			_entities.Add(entity);
		}

		public void Remove(IEntity entity)
		{
			_entities.Remove(entity);
		}

		public IEnumerable<T> GetEntitiesByType<T>()
		{
			return _entities.OfType<T>();
		}
	}
}