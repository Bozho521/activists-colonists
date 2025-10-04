#if UNITY_EDITOR
using UnityEngine;

namespace Tiles
{
    public partial class Tile
    {
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            foreach (var n in neighbors)
                if (n) Gizmos.DrawLine(
                    transform.position + Vector3.up * 0.02f,
                    n.transform.position + Vector3.up * 0.02f);
        }
    }
}
#endif