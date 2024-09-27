using UnityEngine;

public class GameCamera : MonoBehaviour {
    private static readonly float ZoomSpeed = 2f;
    private static readonly float OrthogonalSpeed = 20f;
    private static readonly float TimeToUpdateInSeconds = 0.1f;

    private Vector3 cameraPositionTarget;
    private Vector3 cameraVelocity;
    private bool isMoveableByPlayer;

    void Start() {
        cameraPositionTarget = transform.position;
        cameraVelocity = Vector3.zero;
        isMoveableByPlayer = true;
    }

    void Update() {
        var scrollWheelAxis = Input.GetAxis("Mouse ScrollWheel");

        if (isMoveableByPlayer) {
            if (Mathf.Abs(scrollWheelAxis) > 0)
                cameraPositionTarget += ZoomSpeed * Mathf.Sign(scrollWheelAxis) * Vector3.down;

            if (Input.GetKey(KeyCode.W))
                cameraPositionTarget += OrthogonalSpeed * Time.deltaTime * Vector3.forward;

            if (Input.GetKey(KeyCode.A))
                cameraPositionTarget += OrthogonalSpeed * Time.deltaTime * Vector3.left;

            if (Input.GetKey(KeyCode.S))
                cameraPositionTarget += OrthogonalSpeed * Time.deltaTime * Vector3.back;

            if (Input.GetKey(KeyCode.D))
                cameraPositionTarget += OrthogonalSpeed * Time.deltaTime * Vector3.right;
        }

        if (cameraPositionTarget != transform.position) {
            transform.position = Vector3.SmoothDamp(
                transform.position,
                cameraPositionTarget,
                ref cameraVelocity,
                TimeToUpdateInSeconds
            );
        }
    }

    /// <summary>
    /// Sets up camera to center onto the tile that triggered the event
    /// </summary>
    void Tile_OnTileClick(Tile tile) {
        var cameraGlobalOffset = transform.position - GetGlobalPointFromCenter();
        cameraPositionTarget = tile.transform.position + cameraGlobalOffset;
        isMoveableByPlayer = false;
    }

    /// <summary>
    /// Gets the point on the Y-plane directed from the camera to the center of the screen
    /// </summary>
    Vector3 GetGlobalPointFromCenter() {
        Ray cameraCenterRay = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0, 0.5f));
        Plane groundPlane = new(Vector3.up, Vector3.zero);
        groundPlane.Raycast(cameraCenterRay, out float distanceToGround);
        return cameraCenterRay.GetPoint(distanceToGround);
    }
}
