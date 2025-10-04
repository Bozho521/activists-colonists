#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;

namespace Tiles
{
    public partial class HexGridManager
    {
        [Header("Editor Neighbor Wiring")]
        [SerializeField] private float neighborCenterDist = 1.0f; 
        [SerializeField] private float tolerance = 0.15f;

        [ContextMenu("Tiles/Auto-Wire Neighbors (distance)")]
        private void AutoWireNeighbors()
        {
            var tiles = GetComponentsInChildren<Tile>(true);
            var list = new List<Tile>(tiles);
            int links = 0;

            float minD = (1f - tolerance) * neighborCenterDist;
            float maxD = (1f + tolerance) * neighborCenterDist;
            float minD2 = minD * minD;
            float maxD2 = maxD * maxD;

            for (int i = 0; i < list.Count; i++)
            {
                var ti = list[i];
                var pi = ti.transform.position;
                var neighbors = new List<Tile>(6);

                for (int j = 0; j < list.Count; j++)
                {
                    if (i == j) continue;
                    var tj = list[j];
                    var pj = tj.transform.position;

                    // distance on XZ plane (ignore Y)
                    Vector3 d = pj - pi; d.y = 0f;
                    float d2 = d.sqrMagnitude;

                    if (d2 >= minD2 && d2 <= maxD2)
                        neighbors.Add(tj);
                }

                ti.Editor_SetNeighbors(neighbors);
                links += neighbors.Count;
            }

            Debug.Log($"[HexGridManager] Auto-wired neighbors. Tiles={list.Count}, total links={links}.");
        }
    }
}
#endif