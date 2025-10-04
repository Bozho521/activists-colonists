using TMPro;
using UnityEngine;

namespace UI
{
    public class TurnBannerUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI label;

        public void ShowTurn(int player)
        {
            if (label) label.text = $"Player {player}'s turn";
        }
    }
}