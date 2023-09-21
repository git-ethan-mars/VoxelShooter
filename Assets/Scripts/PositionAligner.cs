﻿using UnityEngine;

[ExecuteInEditMode]
public class PositionAligner : MonoBehaviour
{
    private void Update()
    {
        transform.position = Vector3Int.FloorToInt(transform.position) + Constants.WorldOffset;
    }
}