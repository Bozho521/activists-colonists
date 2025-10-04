using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameControllers
{
    public class GameManager : MonoBehaviour
    {

        
        public enum GameState
        {
            Playing,
            Paused
        }

        [Header("Players")] [SerializeField] private List<PlayerController> players; 
        

        [Header("HexGrid")]
        [SerializeField] private HexGridManager hexGridManager;
        

        [Header("Vote Stats")]
        [SerializeField] private int increasePerTile;
        private VoteManager _voteManager;
        

        private void Awake()
        {
            _voteManager = new VoteManager();
            
        }


        private void Start()
        {
            if (players.Count < 1)
            {
                Debug.LogError("No players found");
            }
        }

        
        
        void Update()
        {
        
        }
    }
}
