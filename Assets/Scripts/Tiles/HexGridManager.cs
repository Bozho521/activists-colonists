using System.Collections.Generic;
using UnityEngine;
using Enums;

namespace Tiles
{
    public partial class HexGridManager : MonoBehaviour
    {
        private readonly List<Tile> _tiles = new();
        public IEnumerable<Tile> AllTiles => _tiles;

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
        }
    }
}
