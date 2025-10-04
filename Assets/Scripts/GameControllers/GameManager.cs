using System;
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
        // TODO: hook an action selection UI later that calls SelectAction(...)

        [SerializeField]private ActionMode currentAction = ActionMode.BasicBuild;
        
        
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
                gameConfig.deterministic ? gameConfig.rngSeed : (int?)null
            );
            _votes.OnVoteChanged += (p1, p2) => voteBar?.SetVotes(p1, p2, gameConfig.uiTweenSeconds);

            _players = new[] { null, new PlayerState(1), new PlayerState(2) }; // index by `1/2`
        }

        private void Start()
        {
            grid.IndexExistingTiles();
            voteBar?.SetVotes(_votes.P1, 100 - _votes.P1, 0f);

            ActivePlayer = _votes.RollWinner();
            BeginTurn();
        }

        private void Update()
        {
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

            Transition(GameState.PlayerTurn);
            OnTurnChanged?.Invoke(ActivePlayer);
        }

        private void EndTurn()
        {
            ActivePlayer = _votes.RollWinner();

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
                    t.SetOwner(ToOwner(ActivePlayer));
                    // keep it buildable=false once owned (rules can vary)
                    t.SetBuildable(false);
                }
                    break;
            }

            if (builtCount > 0 && gameConfig.voteDeltaPerBuild != 0)
            {
                int delta = (ActivePlayer == 1 ? +1 : -1) * (gameConfig.voteDeltaPerBuild * builtCount);
                _votes.AdjustVotes(delta);
            }

            ClearTargets();
            EndTurn();
        }
        
        
        private void EndGame()
        {
            Transition(GameState.Ended);
            int p1Owned = grid.CountOwned(TileOwner.P1);
            int p2Owned = grid.CountOwned(TileOwner.P2);
            int winner = p1Owned == p2Owned ? (_votes.P1 >= 50 ? 1 : 2) : (p1Owned > p2Owned ? 1 : 2);
            Debug.Log($"Game Over. Tiles P1={p1Owned} P2={p2Owned}. Winner: P{winner}");
            // TODO: end screen
        }

        private void Transition(GameState s)
        {
            State = s;
            OnGameStateChanged?.Invoke(s);
        }

        private static TileOwner ToOwner(int p) => p == 1 ? TileOwner.P1 : TileOwner.P2;
    }
}
