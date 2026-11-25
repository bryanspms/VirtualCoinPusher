using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PusherController : MonoBehaviour
{
    public static PusherController Instance { get; private set; }

    public float strokeDistance = 1.45f;
    public float speed = 1.4f;
    public bool IsPaused { get; private set; } = true;

    private Rigidbody rb;
    private Vector3 startPos;
    private float timer = -Mathf.PI * 0.5f;

    void Awake()
    {
        Instance = this;
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        startPos = transform.position;
    }

    public void SetPaused(bool paused)
    {
        IsPaused = paused;
        if (UIManager.Instance != null)
        {
            UIManager.Instance.RefreshHUD();
        }
    }

    public void TogglePaused()
    {
        SetPaused(!IsPaused);
    }

    void FixedUpdate()
    {
        if (IsPaused) return;

        timer += Time.fixedDeltaTime * speed;
        float zOffset = (Mathf.Sin(timer) * 0.5f + 0.5f) * strokeDistance;
        Vector3 targetPos = startPos + new Vector3(0f, 0f, -zOffset);
        rb.MovePosition(targetPos);
    }
}
