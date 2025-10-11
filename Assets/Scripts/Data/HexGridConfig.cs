using System.Collections.Generic;
using NUnit.Framework;
using UI;
using UnityEngine;
using UnityEngine.Serialization;

namespace Data
{
    [CreateAssetMenu(menuName = "Config/Hex Grid", fileName = "HexGridConfig")]
    public class HexGridConfig : ScriptableObject
    {
        public int radius = 6; 
        public TileConfig defaultTileType;
        public float tileSize = 1f;
        
        
        public List<GameObject> tiles;
        public List<GameObject> capitalistsTiles;
        public List<GameObject> activistsTiles;


        public FloatingText floatingText;
    }
}