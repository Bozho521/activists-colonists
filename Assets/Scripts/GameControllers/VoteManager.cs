using System;
using UnityEngine;

namespace GameControllers
{
    public class VoteManager
    {
        public int P1 { get; private set; } 
        public int P2 => 100 - P1;

        public int MinPercent { get; }
        public int MaxPercent => 100 - MinPercent;

        private readonly System.Random _rng;

        public event Action<int,int> OnVoteChanged;

        public VoteManager(int startP1, int? seed = null, int minPercent = 10)
        {
            if (minPercent < 0 || minPercent > 50)
                throw new ArgumentOutOfRangeException(nameof(minPercent), "minPercent must be in [0,50].");

            MinPercent = minPercent;

            P1 = ClampToBand(startP1);
            _rng = seed.HasValue ? new System.Random(seed.Value) : new System.Random();
        }

        public int RollWinner()
        {
            int roll = _rng.Next(0, 100);
            Debug.Log($"P1 :: {P1}% --- P2 :: {P2}% --- Roll :: {roll} / 100");
            return roll < P1 ? 1 : 2;
        }

        public int AdjustVotes(int deltaP1)
        {
            int old = P1;
            int next = ClampToBand(P1 + deltaP1);
            int applied = next - old;
            if (applied != 0)
            {
                P1 = next;
                OnVoteChanged?.Invoke(P1, P2);
            }
            return applied;
        }

        public bool SetVotes(int p1)
        {
            int clamped = ClampToBand(p1);
            if (clamped == P1) return false;
            P1 = clamped;
            OnVoteChanged?.Invoke(P1, P2);
            return true;
        }

        public bool IsAtFloorForP1() => P1 <= MinPercent;
        public bool IsAtFloorForP2() => P1 >= MaxPercent;

        private int ClampToBand(int p1) => Math.Clamp(p1, MinPercent, MaxPercent);
    }
}