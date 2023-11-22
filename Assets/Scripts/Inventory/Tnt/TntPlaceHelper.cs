using System;
using UnityEngine;

namespace Inventory
{
    public static class TntPlaceHelper
    {
        public static Vector3 GetTntOffsetPosition(Vector3 normal)
        {
            if (normal == Vector3.up)
            {
                return new Vector3(0.45f, 0, 0.43f);
            }

            if (normal == Vector3.down)
            {
                return new Vector3(0.45f, 1, 0.57f);
            }

            if (normal == Vector3.right)
            {
                return new Vector3(0, 0.57f, 0.57f);
            }

            if (normal == Vector3.left)
            {
                return new Vector3(1, 0.57f, 0.43f);
            }

            if (normal == Vector3.forward)
            {
                return new Vector3(0.43f, 0.57f, 0);
            }

            if (normal == Vector3.back)
            {
                return new Vector3(0.57f, 0.57f, 1);
            }

            throw new ArgumentException("Can't attach tnt to wrong face of block");
        }

        public static Quaternion GetTntRotation(Vector3 normal)
        {
            if (normal == Vector3.up)
            {
                return Quaternion.Euler(0, 0, 0);
            }

            if (normal == Vector3.down)
            {
                return Quaternion.Euler(180, 0, 0);
            }

            if (normal == Vector3.right)
            {
                return Quaternion.Euler(90, 0, -90);
            }

            if (normal == Vector3.left)
            {
                return Quaternion.Euler(90, 0, 90);
            }

            if (normal == Vector3.forward)
            {
                return Quaternion.Euler(90, 0, 0);
            }

            if (normal == Vector3.back)
            {
                return Quaternion.Euler(90, 0, 180);
            }

            throw new ArgumentException("Can't attach tnt to wrong face of block");
        }
    }
}