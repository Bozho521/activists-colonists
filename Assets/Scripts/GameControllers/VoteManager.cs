using System;

namespace GameControllers
{
    public class VoteManager
    {
        public int P1 { get; private set; }
        public int P2 => 100 - P1;

        private readonly Random _rng;

        public event Action<int,int> OnVoteChanged;

        public VoteManager(int startP1, int? seed = null)
        {
            P1 = Math.Clamp(startP1, 0, 100);
            _rng = seed.HasValue ? new Random(seed.Value) : new Random();
        }

        
        public int RollWinner()
        {
            int roll = _rng.Next(0, 100);
            return roll < P1 ? 1 : 2;
        }
        
        public void AdjustVotes(int deltaP1)
        {
            int old = P1;
            P1 = Math.Clamp(P1 + deltaP1, 0, 100);
            if (P1 != old) OnVoteChanged?.Invoke(P1, P2);
        }

        public void SetVotes(int p1)
        {
            p1 = Math.Clamp(p1, 0, 100);
            if (p1 != P1)
            {
                P1 = p1;
                OnVoteChanged?.Invoke(P1, P2);
            }
        }
        
        
    }
}