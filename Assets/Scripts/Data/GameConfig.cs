using UnityEngine;
using UnityEngine.Serialization;

namespace Data
{
    [CreateAssetMenu(menuName = "Config/Game Config")]
    public class GameConfig : ScriptableObject
    {
        [Range(0, 100)] public int startVoteP1 = 50;
        public int rngSeed = 12345;
        public float uiTweenSeconds = 0.25f;
        public bool deterministic = true;

        [Header("Tiles Materials")]
        public GameObject selectedTilePrefab;


        [Header("Actions Vars")]
        public int requiredTargetsToBuild = 2;
        
        [Header("Action Costs (points)")]
        public int pointsPerTile = 1 ;
        public int cost_BuildTwo = 2;
        public int cost_BuildAnywhere = 3;
        public int cost_TakeOver = 4;
        
        [Header("Votes")]
        [Range(0,50)] public int minVotePercent = 10;
        public int voteDeltaPerBuild = 5;

        
        
        
        [Header("Turn Camera Color")]
        public Color p1Color;
        public Color p2Color;
        
        [Header("End Animation")]
        public float endAnimDuration = 1.5f;
        public bool focusFromWinnerRegion = true; 
    }
}