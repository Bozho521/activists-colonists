using Tiles;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float rayDistance = 100f;
    private Camera playerCamera;

    private Tile hoveredTile;
    private Vector3 originalScale;

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
        if (playerCamera == null) return;

        Ray ray = playerCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, rayDistance))
        {
            Tile tile = hit.collider.GetComponent<Tile>();

            if (tile != null)
            {
                if (tile != hoveredTile)
                {
                    ResetHoveredTile();

                    hoveredTile = tile;
                    originalScale = hoveredTile.transform.localScale;
                    hoveredTile.transform.localScale = originalScale * 1.1f;
                }
            }
            else
            {
                ResetHoveredTile();
            }
        }
        else
        {
            ResetHoveredTile();
        }

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (hoveredTile != null)
            {
                hoveredTile.Replace();
                hoveredTile = null;
            }
        }
    }

    private void ResetHoveredTile()
    {
        if (hoveredTile != null)
        {
            hoveredTile.transform.localScale = originalScale;
            hoveredTile = null;
        }
    }
}
