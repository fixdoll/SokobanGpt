using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;          // Usually the player
    public Vector3 offset = new Vector3(0, 0, -10); // Camera should stay behind
    public float smoothSpeed = 5f;    // How fast it follows (0 = no smoothing)

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
    }
}
