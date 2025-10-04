using System.Collections.Generic;
using Data;
using UnityEngine;
using Enums;

namespace Tiles
{
    [RequireComponent(typeof(Collider))]
    public partial class Tile : MonoBehaviour
    {
        [Header("Authoring")]
        [SerializeField] private TileConfig config;
        [SerializeField] private TileOwner owner = TileOwner.None;
        [SerializeField] private bool buildable = true;

        [Tooltip("Assign neighbor tiles here (6 max for hex).")]
        [SerializeField] private List<Tile> neighbors = new List<Tile>();

        public GameObject replacementPrefab;

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

        private void ApplyVisual()
        {
            if (_renderer == null || config == null) return;
            var mat = owner switch
            {
                TileOwner.P1 => config.p1OwnedMat,
                TileOwner.P2 => config.p2OwnedMat,
                _ => config.neutralMat
            };
            if (mat) _renderer.sharedMaterial = mat;
        }

        public void Replace()
        {
            if (replacementPrefab != null)
            {
                Instantiate(replacementPrefab, transform.position, transform.rotation);
                Destroy(gameObject);
            }
            else
            {
                Debug.Log(gameObject.name + " has no replacement prefab set!");
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