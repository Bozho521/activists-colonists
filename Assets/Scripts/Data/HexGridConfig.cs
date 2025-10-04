using UnityEngine;

namespace Data
{
    [CreateAssetMenu(menuName = "Config/Hex Grid", fileName = "HexGridConfig")]
    public class HexGridConfig : ScriptableObject
    {
        public int radius = 6; 
        public GameObject tilePrefab;
        public TileConfig defaultTileType;
        public float tileSize = 1f;
    }
}