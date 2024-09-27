using UnityEngine;

public class Util : MonoBehaviour {
    // TODO: Remove and actually serialize row/column one-by-one. This is to make things easy for getting stuff up and debugging.
    public static (int row, int column) GetRowCol(Transform transform) {
        var TileSize = 4;
        var row = Mathf.FloorToInt(transform.position.z) / TileSize;
        var column = Mathf.FloorToInt(transform.position.x) / TileSize;
        return (row, column);
    }

    // TODO: Base this off of Y-coordinate as our height instead of Z-coordinates.
    public static Vector3 EstimateScreenPointToWorld(Vector3 screenPoint) {
        var cameraToScreenPointRay = Camera.main.ScreenPointToRay(screenPoint);

        // This is a hack. We use a plane a bit past the camera to raycast to simulate the screen being in world space.
        Plane screenEstimatedPlane = new(Vector3.back, new Vector3(0, 0, Camera.main.transform.position.z / 2));
        screenEstimatedPlane.Raycast(cameraToScreenPointRay, out float distanceToGround);
        return cameraToScreenPointRay.GetPoint(distanceToGround);
    }
}