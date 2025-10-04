using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class VoteBarUI : MonoBehaviour
    {
        [SerializeField] private Image p1Fill;
        [SerializeField] private Image p2Fill;

        public void SetVotes(int p1, int p2, float tweenSeconds)
        {
            StopAllCoroutines();
            StartCoroutine(TweenVotes(p1, p2, tweenSeconds));
        }

        private IEnumerator TweenVotes(int p1, int p2, float t)
        {
            float start1 = p1Fill.fillAmount;
            float start2 = p2Fill.fillAmount;
            float end1 = Mathf.Clamp01(p1 / 100f);
            float end2 = Mathf.Clamp01(p2 / 100f);

            float e = Mathf.Max(0f, t);
            float elapsed = 0f;
            while (elapsed < e)
            {
                float k = elapsed / e;
                p1Fill.fillAmount = Mathf.Lerp(start1, end1, k);
                p2Fill.fillAmount = Mathf.Lerp(start2, end2, k);
                elapsed += Time.deltaTime;
                yield return null;
            }
            p1Fill.fillAmount = end1;
            p2Fill.fillAmount = end2;
        }
    }
}