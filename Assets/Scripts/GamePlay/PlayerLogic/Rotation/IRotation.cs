using System;
using UnityEngine;

namespace PlayerLogic.Rotation
{
	public interface IRotation : IDisposable
	{
		void Rotate(Vector2 direction);
	}
}