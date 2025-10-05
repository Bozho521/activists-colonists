using System.Collections;
using TMPro;
using UnityEngine;

namespace Player
{
    public class CardInfoPanel : MonoBehaviour
    {
        [Header("UI Refs")]
        [SerializeField] RectTransform panel;         
        [SerializeField] CanvasGroup group;           

        [Header("Optional Content")]
        [SerializeField] TextMeshProUGUI title;
        [SerializeField] TextMeshProUGUI description;

        [Header("Animation")]
        [SerializeField] float slideDistance = 220f; 
        [SerializeField] float showTime = 0.18f;
        [SerializeField] float hideTime = 0.14f;
        [SerializeField] bool useUnscaledTime = true;

        Canvas _canvas;
        bool _worldSpace;            
        Vector2 _showPosAnchored;    
        Vector2 _hidePosAnchored;
        Vector3 _showPosLocal;       
        Vector3 _hidePosLocal;

        Coroutine _co;

        void Awake()
        {
            if (!panel) panel = GetComponent<RectTransform>();
            if (!group) group = GetComponent<CanvasGroup>();
            _canvas = GetComponentInParent<Canvas>();
            _worldSpace = _canvas && _canvas.renderMode == RenderMode.WorldSpace;

            if (!panel.gameObject.activeSelf) panel.gameObject.SetActive(true);
            if (!group) group = panel.gameObject.AddComponent<CanvasGroup>();

            if (_worldSpace)
            {
                _showPosLocal = panel.localPosition;
                _hidePosLocal = _showPosLocal + new Vector3(0f, -Mathf.Abs(slideDistance), 0f);
                panel.localPosition = _hidePosLocal;
            }
            else
            {
                _showPosAnchored = panel.anchoredPosition;
                _hidePosAnchored = _showPosAnchored + new Vector2(0f, -Mathf.Abs(slideDistance));
                panel.anchoredPosition = _hidePosAnchored;
            }

            group.alpha = 0f;
            group.interactable = false;
            group.blocksRaycasts = false;
        }

        float DT() => useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

        public void Show(CardInteractable card)
        {
            if (card && card.data)
            {
                if (title)       title.text = card.data.title;
                if (description) description.text = card.data.description;
            }
            if (_co != null) StopCoroutine(_co);
            _co = StartCoroutine(Slide(true));
        }

        public void Hide()
        {
            if (_co != null) StopCoroutine(_co);
            _co = StartCoroutine(Slide(false));
        }

        IEnumerator Slide(bool toShow)
        {
            group.blocksRaycasts = toShow;
            group.interactable   = false;

            float t = 0f, dur = toShow ? showTime : hideTime;
            float a0 = group.alpha, a1 = toShow ? 1f : 0f;

            if (_worldSpace)
            {
                Vector3 from = panel.localPosition;
                Vector3 to   = toShow ? _showPosLocal : _hidePosLocal;

                while (t < dur)
                {
                    t += DT();
                    float k = Mathf.Clamp01(t / dur);
                    float e = toShow ? (1f - Mathf.Pow(1f - k, 3f)) : (k * k * k);
                    panel.localPosition = Vector3.LerpUnclamped(from, to, e);
                    group.alpha = Mathf.LerpUnclamped(a0, a1, e);
                    yield return null;
                }
                panel.localPosition = to;
            }
            else
            {
                Vector2 from = panel.anchoredPosition;
                Vector2 to   = toShow ? _showPosAnchored : _hidePosAnchored;

                while (t < dur)
                {
                    t += DT();
                    float k = Mathf.Clamp01(t / dur);
                    float e = toShow ? (1f - Mathf.Pow(1f - k, 3f)) : (k * k * k);
                    panel.anchoredPosition = Vector2.LerpUnclamped(from, to, e);
                    group.alpha = Mathf.LerpUnclamped(a0, a1, e);
                    yield return null;
                }
                panel.anchoredPosition = to;
            }

            group.alpha = a1;
            group.interactable = toShow;
            _co = null;
        }

        [ContextMenu("Debug/Show")]
        void DebugShow() => Show(null);

        [ContextMenu("Debug/Hide")]
        void DebugHide() => Hide();
    }
}
