using GameControllers;
using UnityEngine;
using UnityEngine.InputSystem;
using Tiles;

[DefaultExecutionOrder(100)]
public class TileHoverHighlighter : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private LayerMask tileMask = ~0;
    [SerializeField] private float rayDistance = 1000f;

    Tile _current;

    void Awake()
    {
        if (!cam) cam = Camera.main;
    }

    void Update()
    {
        var mouse = Mouse.current;
        if (mouse == null || cam == null) return;

        var ray = cam.ScreenPointToRay(mouse.position.ReadValue());
        if (Physics.Raycast(ray, out var hit, rayDistance, tileMask, QueryTriggerInteraction.Ignore))
        {
            var tile = hit.collider.GetComponentInParent<Tile>();
            if (tile != _current)
            {
                SFXManager.PlayRandomSFX("Hover", 0.7f, null, 0.2f);
                if (_current) _current.SetHover(false);
                _current = tile;
                if (_current) _current.SetHover(true);
            }
        }
        else
        {
            if (_current) _current.SetHover(false);
            _current = null;
        }
    }

    public void ForceClear()
    {
        if (_current) _current.SetHover(false);
        _current = null;
    }
}