using System.Collections;
using UnityEngine;

namespace UI
{
    public class FloatingText : MonoBehaviour
    {
        [Header("Timing")]
        [SerializeField] private float duration = 1.0f;
        [SerializeField] private bool useUnscaledTime = false;

        [Header("Motion")]
        [SerializeField] private float distance = 1.0f;
        [SerializeField] private float speed = 0f; 
        
        [SerializeField] private AnimationCurve moveYCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Header("Fade")]
        [SerializeField] private AnimationCurve alphaCurve = new AnimationCurve(
            new Keyframe(0f, 1f), 
            new Keyframe(0.7f, 1f), 
            new Keyframe(1f, 0f)
        );

        [Header("Refs")]
        [SerializeField] private CanvasGroup canvasGroup;

        private Coroutine _co;

        private void Reset()
        {
            canvasGroup = GetComponentInChildren<CanvasGroup>();
        }

        public void BeginFloatingText()
        {
            if (canvasGroup == null) canvasGroup = GetComponentInChildren<CanvasGroup>();
            Init();
            _co = StartCoroutine(FloatingTextAnimation());
        }

        private void OnDisable()
        {
            if (_co != null) StopCoroutine(_co);
        }

        private void Init()
        {
            canvasGroup.alpha = 1f;
        }

        private IEnumerator FloatingTextAnimation()
        {
            if (duration <= 0f) duration = 0.01f;

            var totalDistance = distance != 0f ? distance : speed * duration;

            var startPos = transform.position;
            var elapsed = 0f;

            while (elapsed < duration)
            {
                var dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                elapsed += dt;
                var t = Mathf.Clamp01(elapsed / duration);
                
                var y01 = (moveYCurve != null && moveYCurve.length > 0) ? moveYCurve.Evaluate(t) : t;
                var targetPos = startPos + Vector3.up * (totalDistance * y01);
                transform.position = targetPos;

                var a = (alphaCurve != null && alphaCurve.length > 0) ? alphaCurve.Evaluate(t) : (1f - t);
                canvasGroup.alpha = Mathf.Clamp01(a);

                yield return null;
            }

            canvasGroup.alpha = 0f;
            Destroy(gameObject);
        }
    }
}
