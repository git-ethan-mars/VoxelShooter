using UnityEngine;

public class ZoomService
{
    private const float DefaultFov = 60;
    public bool IsZoomed { get; private set; }
    private readonly Camera _camera;

    public ZoomService(Camera camera)
    {
        _camera = camera;
    }

    public void ZoomIn(float zoomMultiplier)
    {
        _camera.fieldOfView = DefaultFov / zoomMultiplier;
        IsZoomed = true;
    }

    public void ZoomOut()
    {
        _camera.fieldOfView = DefaultFov;
        IsZoomed = false;
    }
}