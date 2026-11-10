using UnityEngine;
using UnityEngine.InputSystem; 


public class CameraMouseFollow : MonoBehaviour
{
    [SerializeField] private Transform target;

    [SerializeField] private float maxOffset = 2.0f;           
    [SerializeField, Range(0f, 0.5f)] private float deadZone = 0.05f; 
    [SerializeField, Range(0f, 2f)] private float edgePower = 0f;

    [SerializeField] private float smoothTime = 0.15f;         
    private Vector3 vel;                    

    private float zOffset = -10f;             

    Camera cam;

    void Awake() {
        cam = GetComponent<Camera>();
        if (!target) Debug.LogWarning("target Missing.");
    }

    void LateUpdate() {
        if (!target) return;

        Vector2 dir = GetMouseDirFromCenter();          
        float mag = dir.magnitude;

        if (mag < deadZone) dir = Vector2.zero;
        else {
            float t = Mathf.InverseLerp(deadZone, 0.5f, mag); 
            t = Mathf.Pow(t, edgePower);                      
            dir = dir.normalized * t;
        }

        Vector3 offset = (Vector3)(dir * maxOffset);
        Vector3 targetPos = target.position + offset;
        targetPos.z = zOffset;

        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref vel, smoothTime);
    }

    Vector2 GetMouseDirFromCenter() {
        Vector2 vp;
        Vector2 m = Mouse.current != null ? Mouse.current.position.ReadValue() : (Vector2)Input.mousePosition;
        vp = cam.ScreenToViewportPoint(m);

        Vector2 centered = vp - new Vector2(0.5f, 0.5f);

        if (centered.sqrMagnitude > 0.25f)
            centered = centered.normalized * 0.5f;

        return centered; 
    }
}