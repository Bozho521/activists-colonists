namespace Enums
{
    public enum ActionMode
    {
        BasicBuild,       // build 1 adjacent tile (cost 0)
        BuildTwo,         // build 2 adjacent tiles (cost: GameConfig.cost_BuildTwo)
        BuildAnywhere,    // build 1 tile anywhere (cost: GameConfig.cost_BuildAnywhere)
        TakeOver          // take over 1 enemy tile (cost: GameConfig.cost_TakeOver)
    }
}