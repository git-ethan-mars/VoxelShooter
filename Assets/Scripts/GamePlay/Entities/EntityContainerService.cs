using System.Collections.Generic;
using System.Linq;
namespace GamePlay
{
	public class EntityContainerService
	{
		private readonly HashSet<Entity> _entities = new HashSet<Entity>();

		public void Add(Entity entity)
		{
			_entities.Add(entity);
		}

		public void Remove(Entity entity)
		{
			_entities.Remove(entity);
		}

		public IEnumerable<T> GetEntitiesByType<T>()
		{
			return _entities.OfType<T>();
		}
	}
}