using CameraLogic;
using UnityEngine;

public class CubeRenderer
{
    private readonly LineRenderer _lineRenderer;
    private readonly RayCaster _rayCaster;
    private readonly float _placeDistance;

    public CubeRenderer(LineRenderer lineRenderer, RayCaster rayCaster, float drawDistance)
    {
        _lineRenderer = lineRenderer;
        _rayCaster = rayCaster;
        _placeDistance = drawDistance;
    }

    public void EnableCube()
    {
        _lineRenderer.enabled = true;
        _lineRenderer.positionCount = 0;
    }

    public void DisableCube()
    {
        _lineRenderer.positionCount = 0;
        _lineRenderer.enabled = false;
    }

    public void UpdateCube()
    {
        var raycastResult = _rayCaster.GetRayCastHit(out var raycastHit, _placeDistance, Constants.buildMask);
        if (!raycastResult)
        {
            _lineRenderer.positionCount = 0;
            return;
        }

        var blockStartPosition =
            Vector3Int.FloorToInt(raycastHit.point - raycastHit.normal / 2);
        DrawCube(blockStartPosition);
    }

    public bool GetRayCastHit(out RaycastHit raycastHit) =>
        _rayCaster.GetRayCastHit(out raycastHit, _placeDistance, Constants.buildMask);

    private void DrawCube(Vector3Int startPosition)
    {
        _lineRenderer.positionCount = 17;
        _lineRenderer.SetPosition(0, startPosition);
        _lineRenderer.SetPosition(1, startPosition + new Vector3Int(1, 0, 0));
        _lineRenderer.SetPosition(2, startPosition + new Vector3Int(1, 1, 0));
        _lineRenderer.SetPosition(3, startPosition + new Vector3Int(0, 1, 0));
        _lineRenderer.SetPosition(4, startPosition);
        _lineRenderer.SetPosition(5, startPosition + new Vector3Int(0, 0, 1));
        _lineRenderer.SetPosition(6, startPosition + new Vector3Int(0, 1, 1));
        _lineRenderer.SetPosition(7, startPosition + new Vector3Int(0, 1, 0));
        _lineRenderer.SetPosition(8, startPosition + new Vector3Int(1, 1, 0));
        _lineRenderer.SetPosition(9, startPosition + new Vector3Int(1, 1, 1));
        _lineRenderer.SetPosition(10, startPosition + new Vector3Int(0, 1, 1));
        _lineRenderer.SetPosition(11, startPosition + new Vector3Int(0, 0, 1));
        _lineRenderer.SetPosition(12, startPosition + new Vector3Int(1, 0, 1));
        _lineRenderer.SetPosition(13, startPosition + new Vector3Int(1, 1, 1));
        _lineRenderer.SetPosition(14, startPosition + new Vector3Int(1, 1, 0));
        _lineRenderer.SetPosition(15, startPosition + new Vector3Int(1, 0, 0));
        _lineRenderer.SetPosition(16, startPosition + new Vector3Int(1, 0, 1));
    }
}