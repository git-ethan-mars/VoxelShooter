using System;
using UnityEngine;

namespace VoxelMap.CustomAttributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class ReadOnlyAttribute : PropertyAttribute
    {
    }
}