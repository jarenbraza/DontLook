using UnityEngine;

public class CameraMovement : MonoBehaviour {
  private const float CameraSpeed = 10f;

  void Update() {
    if (Input.GetKey(KeyCode.W)) {
      transform.position += CameraSpeed * Time.deltaTime * Vector3.up;
    }
    if (Input.GetKey(KeyCode.A)) {
      transform.position += CameraSpeed * Time.deltaTime * Vector3.left;
    }
    if (Input.GetKey(KeyCode.S)) {
      transform.position += CameraSpeed * Time.deltaTime * Vector3.down;
    }
    if (Input.GetKey(KeyCode.D)) {
      transform.position += CameraSpeed * Time.deltaTime * Vector3.right;
    }
  }
}