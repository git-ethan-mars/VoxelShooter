using UnityEngine;

namespace CameraLogic
{
    public class ZoomService
    {
        private readonly Camera _camera;
        private readonly float _zoomMultiplier;

        public ZoomService(Camera camera, float zoomMultiplier)
        {
            _camera = camera;
            _zoomMultiplier = zoomMultiplier;
        }

        public void ZoomIn()
        {
            _camera.fieldOfView = Constants.DefaultFov / _zoomMultiplier;
        }

        public void ZoomOut()
        {
            _camera.fieldOfView = Constants.DefaultFov;
        }
    }
}