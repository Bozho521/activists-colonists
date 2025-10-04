namespace Player
{
    public class PlayerState
    {
        public int Id { get; }
        public int Points { get; private set; }

        public PlayerState(int id) { Id = id; }

        public void AddPoints(int amount) => Points += amount;
        public bool TrySpend(int cost)
        {
            if (Points < cost) return false;
            Points -= cost;
            return true;
        }
    }
}