using UnityEngine;

public class CameraMovement : MonoBehaviour {
    private Vector3 cameraPositionTarget;
    private Vector3 cameraVelocity;

    void Start() {
        cameraPositionTarget = transform.position;
        cameraVelocity = Vector3.zero;

        foreach (var tile in GameObject.Find("GameContainer").GetComponent<Game>().Tiles)
            if (tile != null)
                tile.ReachableTileClickEvent.AddListener(Tile_ReachableTileClickEvent);
    }

    void Update() {
        var scrollWheelAxis = Input.GetAxis("Mouse ScrollWheel");

        if (Mathf.Abs(scrollWheelAxis) > 0)
            cameraPositionTarget += CameraConstants.ZoomSpeed * Mathf.Sign(scrollWheelAxis) * Vector3.forward;

        if (Input.GetKey(KeyCode.W))
            cameraPositionTarget += CameraConstants.OrthogonalSpeed * Time.deltaTime * Vector3.up;

        if (Input.GetKey(KeyCode.A))
            cameraPositionTarget += CameraConstants.OrthogonalSpeed * Time.deltaTime * Vector3.left;

        if (Input.GetKey(KeyCode.S))
            cameraPositionTarget += CameraConstants.OrthogonalSpeed * Time.deltaTime * Vector3.down;

        if (Input.GetKey(KeyCode.D))
            cameraPositionTarget += CameraConstants.OrthogonalSpeed * Time.deltaTime * Vector3.right;

        if (cameraPositionTarget != transform.position) {
            cameraPositionTarget = new Vector3(
                Mathf.Clamp(cameraPositionTarget.x, CameraConstants.MinClamp.x, CameraConstants.MaxClamp.x),
                Mathf.Clamp(cameraPositionTarget.y, CameraConstants.MinClamp.y, CameraConstants.MaxClamp.y),
                Mathf.Clamp(cameraPositionTarget.z, CameraConstants.MinClamp.z, CameraConstants.MaxClamp.z)
            );

            transform.position = Vector3.SmoothDamp(
                transform.position,
                cameraPositionTarget,
                ref cameraVelocity,
                CameraConstants.TimeToUpdateInSeconds
            );
        }
    }

    /// <summary>
    /// Sets up camera to center onto the tile that triggered the event
    /// </summary>
    void Tile_ReachableTileClickEvent(Tile tile) {
        var cameraGlobalOffset = transform.position - GetGlobalPointFromCenter();
        cameraPositionTarget = tile.transform.position + cameraGlobalOffset;
    }

    /// <summary>
    /// Gets the point on the Z-plane directed from the camera to the center of the screen
    /// </summary>
    Vector3 GetGlobalPointFromCenter() {
        Ray cameraCenterRay = GetComponent<Camera>().ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        Plane groundPlane = new(Vector3.back, Vector3.zero);
        groundPlane.Raycast(cameraCenterRay, out float distanceToGround);
        return cameraCenterRay.GetPoint(distanceToGround);
    }
}
