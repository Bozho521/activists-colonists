using UnityEngine;

namespace Data
{
    [CreateAssetMenu(menuName = "Cards/Card Data", fileName = "CardData")]
    public class CardData : ScriptableObject
    {
        public string title;
        [TextArea(2, 6)] public string description;
        public int cost;
        public int key;
    }
}