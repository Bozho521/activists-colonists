using Enums;
using Tiles;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private HexGridManager hexGridManager;
        public float rayDistance = 100f;
        private Camera playerCamera;

        [SerializeField] private TileOwner inputTileOwner;

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

                if (tile == null)
                {
                    ResetHoveredTile();
                }
                
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

            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                if (hoveredTile == null) return;

                hexGridManager.MarkBuilt(hoveredTile, inputTileOwner);
                hoveredTile = null;
                
                if (TryGetComponent<Tile>(out var tile))
                {
                    if (!tile.IsBuildable) return;
                    
                    
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
}
