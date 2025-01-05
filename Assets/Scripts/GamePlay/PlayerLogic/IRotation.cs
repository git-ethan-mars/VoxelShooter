using System;
using UnityEngine;

namespace GamePlay
{
	public interface IRotation : IDisposable
	{
		void Rotate(Vector2 direction);
	}
}