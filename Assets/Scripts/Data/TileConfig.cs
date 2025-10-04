using UnityEngine;

namespace Data
{
    [CreateAssetMenu(menuName = "Config/Tile Config",  fileName = "TileConfig")]
    public class TileConfig : ScriptableObject
    {
        public string id;
        public Material neutralMat;
        public Material p1OwnedMat;
        public Material p2OwnedMat;
        public bool buildable = true; 
    }
}