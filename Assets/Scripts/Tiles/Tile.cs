using System.Collections.Generic;
using Data;
using UnityEngine;
using Enums;
using Mono.Cecil;

namespace Tiles
{
    [RequireComponent(typeof(Collider))]
    public partial class Tile : MonoBehaviour
    {
        [Header("Authoring")]
        [SerializeField] private TileConfig config;
        [SerializeField] private TileOwner owner = TileOwner.None;
        [SerializeField] private bool buildable = true;
        [SerializeField] private GameObject currentGraphics;

        [Tooltip("Assign neighbor tiles here (6 max for hex).")]
        [SerializeField] private List<Tile> neighbors = new List<Tile>();
        
        private Renderer _renderer;
        public TileOwner Owner => owner;
        public bool IsBuildable => buildable;
        public TileConfig Config => config;
        public IReadOnlyList<Tile> Neighbors => neighbors;

        private void Awake()
        {
            _renderer = GetComponentInChildren<Renderer>();
            ApplyVisual();
        }

        internal void Bind(HexGridManager grid)
        {
            
        }
        
        public void SetOwner(TileOwner newOwner)
        {
            if (owner == newOwner) return;
            owner = newOwner;
            ApplyVisual();
        }

        public void SetBuildable(bool value)
        {
            buildable = value;
            // TODO: dim/outline, etc.
        }
        
        public void ApplyVisualTile(GameObject tilePrefab)
        {
            if (currentGraphics.transform.childCount > 0)
            {
                Destroy(currentGraphics.transform.GetChild(0).gameObject);
            }
            
            var go = Instantiate(tilePrefab, currentGraphics.transform);
            go.transform.localPosition = Vector3.zero;
            go.transform.localScale = Vector3.one * 1.01f;
            go.transform.localRotation = Quaternion.Euler(180f, 0f, 0f);
        }


        private void ApplyVisual()
        {
            if (_renderer == null || config == null) return;
            var mat = config.neutralMat;
            if (mat)
                _renderer.sharedMaterial = mat;
        }

        public void ClearVisual()
        {
            if (currentGraphics != null && currentGraphics.transform.childCount > 0)
            {
                for (int i = currentGraphics.transform.childCount - 1; i >= 0; i--)
                {
                    Destroy(currentGraphics.transform.GetChild(i).gameObject);
                }
            }
        }

#if UNITY_EDITOR
        public void Editor_SetNeighbors(List<Tile> list)
        {
            neighbors = list;
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }
}