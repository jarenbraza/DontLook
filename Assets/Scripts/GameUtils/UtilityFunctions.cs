using UnityEngine;

class Utility {
    public static (int, int) GetTwoUniqueInRange(int minInclusive, int maxExclusive) {
        if (minInclusive == maxExclusive)
            throw new System.Exception("Would infinite loop generating two unique numbers");

        int uniqueOne = Random.Range(minInclusive, maxExclusive);
        int uniqueTwo;

        do {
            uniqueTwo = Random.Range(minInclusive, maxExclusive);
        } while (uniqueOne == uniqueTwo);

        return (uniqueOne, uniqueTwo);
    }

    public static (int, int) GetTwoUniqueInRange(int maxExclusive) {
        if (0 == maxExclusive)
            throw new System.Exception("Would infinite loop generating two unique numbers");

        int uniqueOne = Random.Range(0, maxExclusive);
        int uniqueTwo;

        do {
            uniqueTwo = Random.Range(0, maxExclusive);
        } while (uniqueOne == uniqueTwo);

        return (uniqueOne, uniqueTwo);
    }

    public static Vector3 EstimateScreenPointToWorld(Vector3 screenPoint) {
        var cameraToScreenPointRay = Camera.main.ScreenPointToRay(screenPoint);

        // This is a hack. We use a plane a bit past the camera to raycast to simulate the screen being in world space.
        Plane screenEstimatedPlane = new(Vector3.back, new Vector3(0, 0, Camera.main.transform.position.z / 2));
        screenEstimatedPlane.Raycast(cameraToScreenPointRay, out float distanceToGround);
        return cameraToScreenPointRay.GetPoint(distanceToGround);
    }

    /// <summary>
    /// Updates the provided line renderer with constant values.
    /// Useful to make consistent lines between objects.
    /// </summary>
    /// <param name="lineRenderer"></param>
    public static void UpdateLineRenderer(LineRenderer lineRenderer) {
        lineRenderer.startWidth = LineRendererConstants.Width;
        lineRenderer.endWidth = LineRendererConstants.Width;
        lineRenderer.startColor = LineRendererConstants.Color;
        lineRenderer.endColor = LineRendererConstants.Color;
        lineRenderer.material = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply"));
    }
}
