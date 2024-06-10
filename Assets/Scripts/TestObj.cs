using System.Collections.Generic;
using Geometry;
using UnityEngine;

public class TestObj : MonoBehaviour
{
    [SerializeField]
    private int length;

    private List<GameObject> buffer = new();

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            DrawLine();
            Debug.DrawLine(transform.position, transform.forward * length, Color.black, 5, true);
        }
    }

    private void DrawLine()
    {
        foreach (var cube in buffer)
        {
            Destroy(cube);
        }

        buffer.Clear();

        var ray = new Ray(transform.position, transform.forward);
        foreach (var position in DDA.Calculate(transform.position, ray.GetPoint(length)))
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.position = position;
            buffer.Add(cube);
        }
        
        buffer[0].GetComponent<MeshRenderer>().material.color = Color.green;
        buffer[^1].GetComponent<MeshRenderer>().material.color = Color.red;
    }
}