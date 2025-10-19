using UnityEngine;
namespace GamePlay
{
	public interface IMovement
	{
		void Initialize();
		void Move(Vector2 direction, float speed);
		void Jump(float jumpHeight);
		void Rotate(Vector2 direction);
		void Tick();
		Vector3 ForwardVector { get; }
	}
}