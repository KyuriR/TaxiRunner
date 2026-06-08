using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float offsetY = 3.5f;

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 pos = transform.position;
        pos.y = target.position.y + offsetY;
        transform.position = pos;
    }
}