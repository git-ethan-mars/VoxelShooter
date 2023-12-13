using UnityEngine;

namespace CameraLogic
{
    public class ZoomService
    {
        public bool IsZoomed { get; private set; }
        private readonly Camera _camera;

        public ZoomService(Camera camera)
        {
            _camera = camera;
        }

        public void ZoomIn(float zoomMultiplier)
        {
            _camera.fieldOfView = Constants.DefaultFov / zoomMultiplier;
            IsZoomed = true;
        }

        public void ZoomOut()
        {
            _camera.fieldOfView = Constants.DefaultFov;
            IsZoomed = false;
        }
    }
}