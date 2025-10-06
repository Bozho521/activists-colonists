using System;
using System.Collections.Generic;
using Data;
using UnityEngine;
using Enums;
using GameControllers;
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
            
            Debug.Log("OWNER" + owner);
            Debug.Log("OWNER" + tileList);

            if (tileList == null || tileList.Count == 0)
            {
                Debug.LogError("Tile contains no tiles.");
                return null;
            }
            
            var max =  tileList.Count;
            var newTileIndex = Random.Range(0, max);
            return tileList[newTileIndex];
        }
        
        
        public void TriggerKaboom(Tile centerTile)
        {
            if (centerTile == null)
            {
                Debug.LogWarning("[Kaboom] No center tile provided!");
                return;
            }
            
            List<Tile> tilesToReset = new List<Tile> { centerTile };
            var neighbors = new List<Tile>(GetNeighbors(centerTile));
            

            for (int i = 0; i < neighbors.Count; i++)
            {
                int rand = Random.Range(i, neighbors.Count);
                (neighbors[i], neighbors[rand]) = (neighbors[rand], neighbors[i]);
            }

            int guaranteed = Mathf.Min(3, neighbors.Count);
            for (int i = 0; i < guaranteed; i++)
                tilesToReset.Add(neighbors[i]);

            int extra = Mathf.Min(3, neighbors.Count - guaranteed);
            for (int i = guaranteed; i < guaranteed + extra; i++)
            {
                if (Random.value < 0.5f)
                    tilesToReset.Add(neighbors[i]);
            }
            
            Debug.Log($"[Kaboom] Destroying {tilesToReset.Count} tiles around {centerTile.name}, -=---= {tilesToReset}");
            Debug.Log("");
            
            SFXManager.PlaySFX("bomb", 1, null, 0f);
            foreach (var tile in tilesToReset)
            {
                tile.SetOwner(TileOwner.None);
                tile.SetBuildable(true);
                Debug.Log($"item with name {tile.gameObject.name} has no child transform");
                
                var hasChild = tile.currentGraphics.transform.childCount > 0;
                if(!hasChild) continue;
                var tileGraphicsGO = tile.currentGraphics.transform.GetChild(0)?.gameObject;
                if (tileGraphicsGO == null) continue;
                
                Destroy(tileGraphicsGO);
                

                tile.ApplyVisual();
            }
        }
        
    }
}
