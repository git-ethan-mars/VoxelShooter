using System;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using Networking.LagCompensation;
using UnityEngine;

public class LagCompensated : MonoBehaviour
{
    public SortedDictionary<int, SimulationFrameData> FrameData = new();
    
    private SimulationFrameData savedFrameData;

    void Start()
    {
        SimulationHelper.simulationObjects.Add(this);
    }


    public void AddFrame()
    {
        if (FrameData.Keys.Count >= SimulationHelper.BufferSize)
        {
            var key = FrameData.First().Key;
            FrameData.Remove(key);
        }

        FrameData.Add((int) Math.Floor(NetworkTime.time * NetworkServer.tickRate), new SimulationFrameData(transform.position, transform.rotation));
    }

    public void SetStateTransform(int frameId, float clientSubFrame)
    {
        savedFrameData.Position = transform.position;
        savedFrameData.Rotation = transform.rotation;
        transform.position = Vector3.Lerp(FrameData[frameId - 1].Position, FrameData[frameId].Position, clientSubFrame);
        transform.rotation = FrameData[frameId - 1].Rotation;
    }

    public void ResetStateTransform()
    {
        transform.position = savedFrameData.Position;
        transform.rotation = savedFrameData.Rotation;
    }

    void OnDestroy()
    {
        SimulationHelper.simulationObjects.Remove(this);
    }
}