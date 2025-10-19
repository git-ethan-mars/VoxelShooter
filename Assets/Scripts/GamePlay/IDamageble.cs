using UnityEngine;
namespace GamePlay
{
	public interface IDamageaeble
	{
		void Damage(int damage);
		Transform transform { get; }
	}
}