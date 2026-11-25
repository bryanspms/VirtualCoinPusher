using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance { get; private set; }

    public Vector3 playfieldPos = new Vector3(0f, 7.6f, -9.2f);
    public Vector3 playfieldEuler = new Vector3(36f, 0f, 0f);

    public Vector3 hopperPos = new Vector3(0f, 2.4f, -7.0f);
    public Vector3 hopperEuler = new Vector3(50f, 0f, 0f);

    public float transitionSpeed = 6f;
    public bool IsHopperView { get; private set; } = false;

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            ToggleView();
        }

        Vector3 targetPos = IsHopperView ? hopperPos : playfieldPos;
        Quaternion targetRot = Quaternion.Euler(IsHopperView ? hopperEuler : playfieldEuler);

        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * transitionSpeed);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * transitionSpeed);
    }

    public void ToggleView()
    {
        IsHopperView = !IsHopperView;
        if (IsHopperView && PusherController.Instance != null)
        {
            // Stop the moving shelf when checking the hopper; stays stopped until user restarts it
            PusherController.Instance.SetPaused(true);
        }
        if (UIManager.Instance != null)
        {
            UIManager.Instance.SetHopperPanelVisible(IsHopperView);
        }
    }

    public void SetPlayfieldView()
    {
        IsHopperView = false;
        if (UIManager.Instance != null)
        {
            UIManager.Instance.SetHopperPanelVisible(false);
        }
    }
}
