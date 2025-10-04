using System;
using System.Collections;
using System.Collections.Generic;
using Data;
using Enums;
using Player;
using Tiles;
using UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace GameControllers
{
    public class GameManager : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private Camera mainCamera;
        [SerializeField] private HexGridManager grid;
        [SerializeField] private GameConfig gameConfig;

        [Header("UI")]
        [SerializeField] private VoteBarUI voteBar;
        [SerializeField] private TurnBannerUI turnUI;
        

        [Header("Action Elements")]
        [SerializeField]private ActionMode currentAction = ActionMode.BasicBuild;
        
        public bool BlockRayCast { get; set; }
        
        public GameState State { get; private set; } = GameState.Boot;
        public int ActivePlayer { get; private set; } = 1; // 1 or 2

        private VoteManager _votes;
        private PlayerState[] _players;

        private readonly List<Tile> _pendingTargets = new();
        private int _requiredTargets = 1;
        private int _actionCost = 0;

        public event Action<int> OnTurnChanged;
        public event Action<GameState> OnGameStateChanged;

        private void Awake()
        {
            if (!mainCamera) mainCamera = Camera.main;

            _votes = new VoteManager(
                gameConfig.startVoteP1,
                gameConfig.deterministic ? gameConfig.rngSeed : (int?)null,
                gameConfig.minVotePercent
            );
            _votes.OnVoteChanged += (p1, p2) => voteBar?.SetVotes(p1, p2);

            _players = new[] { null, new PlayerState(1), new PlayerState(2) }; // index by `1/2`
        }

        private void Start()
        {
            grid.IndexExistingTiles();
            voteBar?.SetVotes(_votes.P1, 100 - _votes.P1);

            ActivePlayer = _votes.RollWinner();
            turnUI.UpdatePlayerPoints(ActivePlayer, _players[ActivePlayer].Points);
            
            BeginTurn();
        }

        private void Update()
        {
            if (BlockRayCast) return;
            if (State != GameState.PlayerTurn) return;

            if (Keyboard.current.digit1Key.wasPressedThisFrame) SelectAction(ActionMode.BasicBuild);
            if (Keyboard.current.digit2Key.wasPressedThisFrame) SelectAction(ActionMode.BuildTwo);
            if (Keyboard.current.digit3Key.wasPressedThisFrame) SelectAction(ActionMode.BuildAnywhere);
            if (Keyboard.current.digit4Key.wasPressedThisFrame) SelectAction(ActionMode.TakeOver);

            if (Mouse.current.leftButton.wasPressedThisFrame)
                HandleClick();
            if (Keyboard.current.escapeKey.wasPressedThisFrame)
                ClearTargets();
        }

        private void BeginTurn()
        {
            turnUI?.ShowTurn(ActivePlayer);
            _players[ActivePlayer].AddPoints(1);
            SelectAction(ActionMode.BasicBuild);
            turnUI?.UpdatePlayerPoints(ActivePlayer, _players[ActivePlayer].Points);
            Transition(GameState.PlayerTurn);
            OnTurnChanged?.Invoke(ActivePlayer);
        }

        private void EndTurn()
        {
            ActivePlayer = _votes.RollWinner();
            turnUI.UpdatePlayerPoints(ActivePlayer, _players[ActivePlayer].Points);

            if (!grid.HasAvailableBuildableTiles())
            {
                EndGame();
                return;
            }

            BeginTurn();
        }

        private void HandleClick()
        {
            var ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (!Physics.Raycast(ray, out RaycastHit hit, 1000f)) return;
            var tile = hit.collider.GetComponentInParent<Tile>();
            if (tile == null) return;

            if (_pendingTargets.Contains(tile)) return;
            if (!IsTileValidForCurrentAction(tile)) return;

            Debug.Log("_pendingTargets"+ _pendingTargets);
            _pendingTargets.Add(tile);

            if (_pendingTargets.Count >= _requiredTargets)
                TryExecuteCurrentAction();
        }

        private void SelectAction(ActionMode mode)
        {
            currentAction = mode;
            ClearTargets();

            switch (mode)
            {
                case ActionMode.BasicBuild:
                    _requiredTargets = 1;
                    _actionCost = 0;
                    break;
                case ActionMode.BuildTwo:
                    _requiredTargets = 2;
                    _actionCost = gameConfig.cost_BuildTwo;
                    break;
                case ActionMode.BuildAnywhere:
                    _requiredTargets = 1;
                    _actionCost = gameConfig.cost_BuildAnywhere;
                    break;
                case ActionMode.TakeOver:
                    _requiredTargets = 1;
                    _actionCost = gameConfig.cost_TakeOver;
                    break;
            }
            // TODO: update UI to show selected action & cost & available points
        }

        private void ClearTargets() => _pendingTargets.Clear();

        private bool IsTileValidForCurrentAction(Tile tile)
        {
            var owner = ToOwner(ActivePlayer);
            return currentAction switch
            {
                ActionMode.BasicBuild      => grid.CanBuildAdjacent(tile, owner),
                ActionMode.BuildTwo        => grid.CanBuildAdjacent(tile, owner),
                ActionMode.BuildAnywhere   => grid.CanBuildAnywhere(tile, owner),
                ActionMode.TakeOver        => grid.CanTakeOver(tile, owner),
                _ => false
            };
        }

        private void TryExecuteCurrentAction()
        {
            var me = _players[ActivePlayer];
            if (me.Points < _actionCost)
            {
                Debug.Log("Not enough points for selected action.");
                ClearTargets();
                return;
            }

            foreach (var t in _pendingTargets)
                if (!IsTileValidForCurrentAction(t))
                {
                    Debug.Log("Action failed validation.");
                    ClearTargets();
                    return;
                }

            if (!me.TrySpend(_actionCost))
            {
                Debug.Log("Point spend failed race.");
                ClearTargets();
                return;
            }

            Transition(GameState.Resolving);

            int builtCount = 0;

            switch (currentAction)
            {
                case ActionMode.BasicBuild:
                case ActionMode.BuildTwo:
                case ActionMode.BuildAnywhere:
                    foreach (var t in _pendingTargets)
                    {
                        grid.MarkBuilt(t, ToOwner(ActivePlayer));
                        builtCount++;
                    }
                    break;

                case ActionMode.TakeOver:
                {
                    var t = _pendingTargets[0];
                    //t.SetOwner(ToOwner(ActivePlayer));
                    //t.SetBuildable(false); 
                    grid.MarkBuilt(t, ToOwner(ActivePlayer));
                    
                }
                    break;
            }
            
            if (builtCount > 0 && gameConfig.voteDeltaPerBuild != 0)
            {
                int desired = (ActivePlayer == 1 ? +1 : -1) * (gameConfig.voteDeltaPerBuild * builtCount);
                int applied = _votes.AdjustVotes(desired);

                if (applied == 0)
                {
                    //todo: hint that it capped
                }
                
            }

            ClearTargets();
            EndTurn();
        }
        
        private void Transition(GameState s)
        {
            State = s;
            OnGameStateChanged?.Invoke(s);
        }

        private static TileOwner ToOwner(int p) => p == 1 ? TileOwner.P1 : TileOwner.P2;

        
        
        private void EndGame()
        {
            int p1Owned = grid.CountOwned(TileOwner.P1);
            int p2Owned = grid.CountOwned(TileOwner.P2);
            int winnerIndex = p1Owned == p2Owned ? (_votes.P1 >= 50 ? 1 : 2) : (p1Owned > p2Owned ? 1 : 2);
            var winnerOwner = ToOwner(winnerIndex);

            // stop accepting inputs
            ClearTargets();
            Transition(GameState.Ending);

            StartCoroutine(AnimateVictorySweep(winnerOwner, winnerIndex, p1Owned, p2Owned));
        }


        private IEnumerator AnimateVictorySweep(TileOwner winner, int winnerIndex, int p1Owned, int p2Owned) 
        {
            var tiles = new List<Tile>(grid.AllTiles);
            
            Vector3 focus = Vector3.zero;
            int count = 0;
            if (gameConfig.focusFromWinnerRegion)
            {
                foreach (var t in tiles)
                {
                    if (t.Owner == winner)
                    {
                        focus += t.transform.position;
                        count++;
                    }
                }
            }
            if (count == 0) { focus = ComputeCenter(tiles); }
            else            { focus /= count; }

            tiles.Sort((a, b) =>
            {
                float da = (a.transform.position - focus).sqrMagnitude;
                float db = (b.transform.position - focus).sqrMagnitude;
                return da.CompareTo(db);
            });

            int n = tiles.Count;
            float total = Mathf.Max(0.1f, gameConfig.endAnimDuration);
            float step = Mathf.Max(0.01f, total / Mathf.Max(1, n));

            for (int i = 0; i < n; i++)
            {
                var t = tiles[i];

                if (t.Owner != winner)
                {
                    grid.MarkBuilt(t, winner);
                }

                yield return new WaitForSeconds(step);
            }

            Transition(GameState.Ended);

            Debug.Log($"Game Over. Tiles P1={p1Owned} P2={p2Owned}. Winner: P{winnerIndex}");
            // TODO: Show an end-screen
        }

        private static Vector3 ComputeCenter(List<Tile> tiles)
        {
            if (tiles == null || tiles.Count == 0) return Vector3.zero;
            Vector3 acc = Vector3.zero;
            foreach (var t in tiles) acc += t.transform.position;
            return acc / tiles.Count;
        }
                
        
    }
}
