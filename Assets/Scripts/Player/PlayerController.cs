using UnityEngine;
using UnityEngine.InputSystem; // new input system

public class PlayerController : MonoBehaviour
{
    public float rayDistance = 100f;
    private Camera playerCamera;

    void Start()
    {
        playerCamera = Camera.main;
        if (playerCamera == null)
        {
            Debug.LogError("No Main Camera found in the scene! Please tag your camera as 'MainCamera'.");
        }
    }

    void Update()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (playerCamera == null) return;

            Ray ray = playerCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, rayDistance))
            {
                Tile tile = hit.collider.GetComponent<Tile>();
                if (tile != null)
                {
                    Destroy(tile.gameObject);
                }
            }
        }
    }
}
