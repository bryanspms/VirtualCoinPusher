using UnityEngine;
using UnityEngine.EventSystems;

public class CoinDropper : MonoBehaviour
{
    public enum DropMode { Standard, Plinko }

    public DropMode currentDropMode = DropMode.Standard;
    public GameObject plinkoBoardObject;
    public Transform dropIndicator;

    public float minX = -2.35f;
    public float maxX = 2.35f;
    public float dropY = 5.4f;
    public float dropZ = 1.92f;
    public float rapidDropInterval = 0.18f;

    private float currentX = 0f;
    private float nextRapidDropTime = 0f;

    void Start()
    {
        ApplyDropMode(currentDropMode);
    }

    public void ApplyDropMode(DropMode mode)
    {
        currentDropMode = mode;
        if (plinkoBoardObject != null)
        {
            plinkoBoardObject.SetActive(mode == DropMode.Plinko);
        }
    }

    void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.isGameOver || GameManager.Instance.isWaitingForStart) return;
        if (CameraController.Instance != null && CameraController.Instance.IsHopperView) return;

        float keyInput = Input.GetAxis("Horizontal");
        if (Mathf.Abs(keyInput) > 0.05f)
        {
            currentX = Mathf.Clamp(currentX + keyInput * 4.5f * Time.deltaTime, minX, maxX);
        }
        else if (Camera.main != null)
        {
            float normalizedMouse = Mathf.Clamp01(Input.mousePosition.x / Screen.width);
            currentX = Mathf.Lerp(minX, maxX, normalizedMouse);
        }

        if (dropIndicator != null)
        {
            dropIndicator.position = new Vector3(currentX, dropY + 0.25f, dropZ);
        }

        bool pointerOverUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();

        if ((!pointerOverUI && Input.GetMouseButtonDown(0)) || Input.GetKeyDown(KeyCode.Space))
        {
            TryDropQuarter();
            nextRapidDropTime = Time.time + rapidDropInterval;
        }
        else if (((!pointerOverUI && Input.GetMouseButton(1)) || Input.GetKey(KeyCode.Space)) && Time.time >= nextRapidDropTime)
        {
            TryDropQuarter();
            nextRapidDropTime = Time.time + rapidDropInterval;
        }
    }

    public void TryDropQuarter()
    {
        if (GameManager.Instance == null || !GameManager.Instance.ConsumeQuarter()) return;

        float jitterX = currentDropMode == DropMode.Plinko ? Random.Range(-0.08f, 0.08f) : 0f;
        Vector3 spawnPos = new Vector3(currentX + jitterX, dropY, dropZ);
        Quaternion rot = Quaternion.Euler(90f, 0f, 0f);
        TowerSpawner.Instance.SpawnSingleItem(ItemType.Quarter, spawnPos, rot);
    }
}
