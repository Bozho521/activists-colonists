// GameConfig.cs
using UnityEngine;

[CreateAssetMenu(menuName = "Config/Game Config")]
public class GameConfig : ScriptableObject
{
    [Range(0, 100)] public int startVoteP1 = 50;
    public int rngSeed = 12345;
    public float uiTweenSeconds = 0.25f;
    public bool deterministic = true;

    [Header("Action Costs (points)")]
    public int cost_BuildTwo = 2;
    public int cost_BuildAnywhere = 3;
    public int cost_TakeOver = 4;

    [Header("Effects")]
    public int voteDeltaPerBuild = 5; // +5 for actor means -5 for opponent
}