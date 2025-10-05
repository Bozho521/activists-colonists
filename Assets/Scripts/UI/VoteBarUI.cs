using System.Collections.Generic;
using UnityEngine;

namespace UI
{
    public class VoteBarUI : MonoBehaviour
    {
        [Header("Human References (8 total)")]
        [SerializeField] private List<MeshRenderer> humans;

        [Header("Materials")]
        [SerializeField] private Material p1Material;
        [SerializeField] private Material p2Material;

        private const int TotalHumans = 8;

        public void SetVotes(int p1Percent, int p2Percent)
        {
            p1Percent = Mathf.Clamp(p1Percent, 10, 90);
            p2Percent = Mathf.Clamp(p2Percent, 10, 90);

            float normalized = Mathf.InverseLerp(10f, 90f, p1Percent);
            int p1Count = Mathf.RoundToInt(normalized * TotalHumans);

            int p2Count = TotalHumans - p1Count;

            Debug.Log($"[VoteBar] P1={p1Percent}% ({p1Count} humans), P2={p2Percent}% ({p2Count} humans)");

            for (int i = 0; i < TotalHumans; i++)
            {
                if (i < p1Count)
                    humans[i].material = p1Material;
                else
                    humans[i].material = p2Material;
            }
        }
    }
}
