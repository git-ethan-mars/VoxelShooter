using UnityEngine;

public class SimulationFrameData
{
    public Vector3 Position { get; set; }
    public Quaternion Rotation { get; set; }

    public SimulationFrameData(Vector3 position, Quaternion rotation)
    {
        Position = position;
        Rotation = rotation;
    }
}