using System;
using System.Collections.Generic;
using Data;
using UnityEngine;
using Enums;
using Random = UnityEngine.Random;

namespace Tiles
{
    public partial class HexGridManager : MonoBehaviour
    {
        private readonly List<Tile> _tiles = new();
        public IEnumerable<Tile> AllTiles => _tiles;
        
        [SerializeField] private HexGridConfig hexGridConfig;

        public void IndexExistingTiles()
        {
            _tiles.Clear();
            var tiles = GetComponentsInChildren<Tile>(includeInactive: false);
            foreach (var t in tiles)
            {
                _tiles.Add(t);
                t.Bind(this);
            }
            Debug.Log($"[HexGridManager] Indexed {_tiles.Count} tiles.");
        }
        
        public int CountOwned(TileOwner owner)
        {
            int c = 0;
            foreach (var t in _tiles)
                if (t.Owner == owner) c++;
            return c;
        }

        public IEnumerable<Tile> GetNeighbors(Tile t) => t.Neighbors;

        public bool IsAdjacentToOwner(Tile tile, TileOwner owner)
        {
            foreach (var n in tile.Neighbors)
                if (n != null && n.Owner == owner) return true;
            return false;
        }

        public bool CanBuildOn(Tile tile, TileOwner _currentPlayer)
        {
            if (tile == null) return false;
            if (!tile.IsBuildable) return false;
            if (tile.Owner != TileOwner.None) return false;
            return true;
        }

        public bool CanBuildAdjacent(Tile tile, TileOwner owner)
        {
            if (!CanBuildOn(tile, owner)) return false;
            return IsAdjacentToOwner(tile, owner);
        }

        public bool CanBuildAnywhere(Tile tile, TileOwner owner)
        {
            return CanBuildOn(tile, owner);
        }

        public bool CanTakeOver(Tile tile, TileOwner owner)
        {
            if (tile == null) return false;
            if (tile.Owner == TileOwner.None) return false;
            if (tile.Owner == owner) return false;
            return true;
        }

        public bool HasAvailableBuildableTiles()
        {
            foreach (var t in _tiles)
                if (t.Owner == TileOwner.None && t.IsBuildable) return true;
            return false;
        }

        public void MarkBuilt(Tile tile, TileOwner owner)
        {
            tile.SetOwner(owner);
            tile.SetBuildable(false);
            var tilePref = PickRandomVisual(owner);
            tile.ApplyVisualTile(tilePref);
        }
        
        private GameObject PickRandomVisual(TileOwner owner)
        {
            var tileList = owner switch
            {

                TileOwner.None => hexGridConfig.tiles,
                TileOwner.P1 => hexGridConfig.activistsTiles,
                TileOwner.P2 => hexGridConfig.capitalistsTiles,
                _ => null,
            };

            if (tileList.Count == 0)
            {
                Debug.LogError("Tile contains no tiles.");
                return null;
            }
            
            var max =  tileList.Count ;
            var newTileIndex = Random.Range(0, max);
            return tileList[newTileIndex];
        }

        public void KaboomBoom()
        {
            Debug.Log("[HexGridManager] Kaboom Boom! Clearing all tiles...");

            foreach (var tile in _tiles)
            {
                tile.SetOwner(TileOwner.None);

                tile.SetBuildable(true);

                tile.ClearVisual();
            }

            Debug.Log("[HexGridManager] All tiles cleared.");
        }

    }
}
