using System.Collections;
using System.Collections.Generic;
using Data;
using UnityEngine;
using Enums;
using GameControllers;

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

        [Header("Hover Outline")]
        [SerializeField] private Material outlineMaterial;
        [SerializeField] private Renderer outlineRenderer;
        [SerializeField] private float outlineYOffset = 0.004f;
        [SerializeField] private Color outlineColor = Color.white;
        [SerializeField] private string outlineColorProperty = "_BaseColor";

        
        [Header("Spawn Animation")]
        [SerializeField] private float spawnDuration = 0.22f;
        [SerializeField] private float spawnOvershoot = 1.12f;
        [SerializeField] private bool  spawnUseUnscaledTime = true;
        private Coroutine _spawnCo;
        
        
        private Renderer _renderer;
        private MaterialPropertyBlock _mpb;

        public TileOwner Owner => owner;
        public bool IsBuildable => buildable;
        public TileConfig Config => config;
        public IReadOnlyList<Tile> Neighbors => neighbors;

        private void Awake()
        {
            _mpb = new MaterialPropertyBlock();

            _renderer = currentGraphics
                ? currentGraphics.GetComponentInChildren<Renderer>()
                : GetComponentInChildren<Renderer>();

            ApplyVisual();
            EnsureOutline();
            SetHover(false);
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
            if (_renderer != null)
            {
                _renderer.GetPropertyBlock(_mpb);
                float dim = buildable ? 1f : 0.8f;
                _mpb.SetFloat("_Surface", 0f);
                _renderer.SetPropertyBlock(_mpb);
                _renderer.sharedMaterial = _renderer.sharedMaterial;
                _renderer.transform.localScale = Vector3.one * (buildable ? 1f : 0.995f);
            }
        }

        public void ApplyVisualTile(GameObject tilePrefab)
        {

            if (currentGraphics == null)
                currentGraphics = this.gameObject;

            if (currentGraphics.transform.childCount > 0)
                Destroy(currentGraphics.transform.GetChild(0).gameObject);

            var go = Instantiate(tilePrefab, currentGraphics.transform);
            go.transform.localPosition = Vector3.zero;
            var rand = Random.Range(0,6);
            go.transform.localRotation = Quaternion.Euler(180f,0f, 60f * rand);

            Vector3 finalScale = Vector3.one * 1.01f;
            go.transform.localScale = Vector3.zero;

            _renderer = currentGraphics.GetComponentInChildren<Renderer>();
            ApplyVisual();

            EnsureOutline(forceRecreate: true);
            SetHover(false);

            if (_spawnCo != null) StopCoroutine(_spawnCo);
            
            _spawnCo = StartCoroutine(BubbleIn(go.transform, finalScale, spawnDuration, spawnOvershoot));
        }
        
        private IEnumerator BubbleIn(Transform tr, Vector3 finalScale, float duration, float overshoot)
        {
            //GameManager.Instance.BlockRayCast =  true;
            
            duration = Mathf.Max(0.01f, duration);
            overshoot = Mathf.Max(1.0f, overshoot);

            float t = 0f;
            Vector3 zero = Vector3.zero;
            Vector3 peak = finalScale * overshoot;

            float Dt() => spawnUseUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

            float upTime = duration * 0.55f;
            while (t < upTime)
            {
                t += Dt();
                float k = Mathf.Clamp01(t / upTime);
                float e = 1f - Mathf.Pow(1f - k, 3f);
                tr.localScale = Vector3.LerpUnclamped(zero, peak, e);
                yield return null;
            }
            SFXManager.PlayRandomSFX("BuildBubble", 1f, null, 0.2f);
            
            t = 0f;
            float downTime = duration - upTime;
            const float back = 1.3f;
            while (t < downTime)
            {
                t += Dt();
                float k = Mathf.Clamp01(t / downTime);
                float e = EaseOutBack(k, back);
                tr.localScale = Vector3.LerpUnclamped(peak, finalScale, e);
                yield return null;
            }

            tr.localScale = finalScale;
            
            //GameManager.Instance.BlockRayCast =  false;
        }

        private static float EaseOutBack(float x, float s = 1.70158f)
        {
            float c1 = s;
            float c3 = c1 + 1f;
            return 1f + c3 * Mathf.Pow(x - 1f, 3f) + c1 * Mathf.Pow(x - 1f, 2f);
        }
        

        private void ApplyVisual()
        {
            if (_renderer == null || config == null) return;

            var mat = owner switch
            {
                TileOwner.P1 => config.p1OwnedMat ? config.p1OwnedMat : config.neutralMat,
                TileOwner.P2 => config.p2OwnedMat ? config.p2OwnedMat : config.neutralMat,
                _            => config.neutralMat
            };

            if (mat != null)
                _renderer.sharedMaterial = mat;
        }

        public void SetHover(bool on)
        {
            if (!outlineRenderer) return;
            outlineRenderer.enabled = on;
            if (on)
            {
                var mpb = new MaterialPropertyBlock();
                outlineRenderer.GetPropertyBlock(mpb);
                mpb.SetColor(outlineColorProperty, outlineColor);
                outlineRenderer.SetPropertyBlock(mpb);
            }
        }

        public void SetHoverColor(Color c)
        {
            outlineColor = c;
            if (!outlineRenderer || !outlineRenderer.enabled) return;
            var mpb = new MaterialPropertyBlock();
            outlineRenderer.GetPropertyBlock(mpb);
            mpb.SetColor(outlineColorProperty, outlineColor);
            outlineRenderer.SetPropertyBlock(mpb);
        }

        private void EnsureOutline(bool forceRecreate = false)
        {
            if (forceRecreate && outlineRenderer)
            {
                DestroyImmediate(outlineRenderer.gameObject);
                outlineRenderer = null;
            }

            if (outlineRenderer) return;
            if (!outlineMaterial) return;

            var sourceRenderer = currentGraphics
                ? currentGraphics.GetComponentInChildren<Renderer>()
                : GetComponentInChildren<Renderer>();
            if (!sourceRenderer) return;

            Mesh sourceMesh = null;
            if (sourceRenderer.TryGetComponent<MeshFilter>(out var srcMf) && srcMf.sharedMesh)
                sourceMesh = srcMf.sharedMesh;
            else if (sourceRenderer is SkinnedMeshRenderer skinned && skinned.sharedMesh)
                sourceMesh = skinned.sharedMesh;

            if (!sourceMesh) return;

            var go = new GameObject("Outline");
            go.transform.SetParent(transform, false);
            go.transform.localPosition = Vector3.up * outlineYOffset;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;

            var mf = go.AddComponent<MeshFilter>();
            mf.sharedMesh = sourceMesh;

            var mr = go.AddComponent<MeshRenderer>();
            mr.sharedMaterial = outlineMaterial;
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            mr.receiveShadows = false;
            mr.enabled = false;

            outlineRenderer = mr;
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
