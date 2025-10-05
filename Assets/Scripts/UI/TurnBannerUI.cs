using System;
using System.Collections;
using GameControllers;
using TMPro;
using UnityEngine;

namespace UI
{
    public class TurnBannerUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI label;

        [Header("Player 1 HUD")]
        [SerializeField] private GameObject player1Ui;
        [SerializeField] private TextMeshProUGUI player1Points;

        [Header("Player 2 HUD")]
        [SerializeField] private GameObject player2Ui;
        [SerializeField] private TextMeshProUGUI player2Points;

        [Header("HUD Animation")]
        [SerializeField] private float switchDuration = 0.25f;
        [SerializeField] private float shrunkenScale = 0.9f;
        [SerializeField] private float fadedAlpha = 0.5f;
        [SerializeField] private bool useUnscaledTime = true;

        [Header("Turn Flash (slide then deactivate)")]
        [SerializeField] private RectTransform p1TurnScreen;
        [SerializeField] private RectTransform p2TurnScreen;
        [SerializeField] private float flashInDuration  = 0.18f;
        [SerializeField] private float flashHoldTime    = 0.05f;
        [SerializeField] private float flashOutDuration = 0.18f;
        [SerializeField] private float offscreenMargin  = 60f;

        [Header("Per-Player Flash Y Offset")]
        [SerializeField] private float p1FlashYOffset = 0f;
        [SerializeField] private float p2FlashYOffset = 24f;

        CanvasGroup _p1Group, _p2Group;
        RectTransform _p1Rect, _p2Rect;

        Vector2 _p1Center, _p2Center;
        float _offLeftX, _offRightX;
        Coroutine _swapCo, _flashCo;
        int _lastPlayer = -1;

        private GameManager _gameManager;

        //[Obsolete("Obsolete")]
        void Awake()
        {
            _p1Group = RequireCanvasGroup(player1Ui);
            _p2Group = RequireCanvasGroup(player2Ui);
            _p1Rect  = player1Ui.GetComponent<RectTransform>();
            _p2Rect  = player2Ui.GetComponent<RectTransform>();
            
            _gameManager = FindAnyObjectByType<GameManager>();

            if (p1TurnScreen) _p1Center = p1TurnScreen.anchoredPosition;
            if (p2TurnScreen) _p2Center = p2TurnScreen.anchoredPosition;

            var refRect = GetRefRect();
            float refWidth = refRect ? refRect.rect.width : Screen.width;

            float half = refWidth * 0.5f;
            _offLeftX  = -half - offscreenMargin;
            _offRightX =  half + offscreenMargin;

            if (p1TurnScreen) p1TurnScreen.gameObject.SetActive(false);
            if (p2TurnScreen) p2TurnScreen.gameObject.SetActive(false);
        }

        RectTransform GetRefRect()
        {
            if (p1TurnScreen && p1TurnScreen.parent is RectTransform pr1) return pr1;
            if (p2TurnScreen && p2TurnScreen.parent is RectTransform pr2) return pr2;
            var anyCanvas = GetComponentInParent<Canvas>();
            if (anyCanvas && anyCanvas.rootCanvas && anyCanvas.rootCanvas.transform is RectTransform rootRt) return rootRt;
            if (transform is RectTransform selfRt) return selfRt;
            return null;
        }

        public void ShowTurn(int player)
        {
            if (label) 
            { 
                label.text = player == 2? $"Capitalist Turn": $"Activist Turn";
                label.color = player == 2? Color.red: Color.green;
            }

            if (_lastPlayer == player)
            {
                SetHudInstant(player);
                return;
            }

            _lastPlayer = player;
            ChangePlayerUiAnimated(player);
            PlayTurnFlash(player);
        }

        private void Start()
        {
            UpdatePlayerPoints(1, 0);
            UpdatePlayerPoints(2, 0);
        }

        public void UpdatePlayerPoints(int player, int amount)
        {
            amount = amount * 500;
            if (player == 1) player1Points.text = amount.ToString() + "$";
            else             player2Points.text = amount.ToString() + "$";
            StartCoroutine(PunchTextScale(player == 1 ? player1Points.rectTransform : player2Points.rectTransform));
        }

        static CanvasGroup RequireCanvasGroup(GameObject go)
        {
            var cg = go.GetComponent<CanvasGroup>();
            if (!cg) cg = go.AddComponent<CanvasGroup>();
            return cg;
        }

        void SetHudInstant(int player)
        {
            player1Ui.SetActive(true);
            player2Ui.SetActive(true);
            bool p1Active = player == 1;
            SetScaleAlpha(_p1Rect, _p1Group, p1Active ? 1f : shrunkenScale, p1Active ? 1f : fadedAlpha);
            SetScaleAlpha(_p2Rect, _p2Group, p1Active ? shrunkenScale : 1f, p1Active ? fadedAlpha : 1f);
            if (_flashCo != null) { StopCoroutine(_flashCo); _flashCo = null; }
            if (p1TurnScreen) p1TurnScreen.gameObject.SetActive(false);
            if (p2TurnScreen) p2TurnScreen.gameObject.SetActive(false);
        }

        static void SetScaleAlpha(RectTransform rt, CanvasGroup cg, float scale, float alpha)
        {
            if (rt) rt.localScale = new Vector3(scale, scale, 1f);
            if (cg) cg.alpha = alpha;
        }

        void ChangePlayerUiAnimated(int player)
        {
            _gameManager.BlockRayCast = true;
            player1Ui.SetActive(true);
            player2Ui.SetActive(true);
            bool p1Active = player == 1;
            if (_swapCo != null) StopCoroutine(_swapCo);
            _swapCo = StartCoroutine(SwapTween(
                _p1Rect, _p1Group,
                _p2Rect, _p2Group,
                p1Active ? 1f : shrunkenScale, p1Active ? 1f : fadedAlpha,
                p1Active ? shrunkenScale : 1f, p1Active ? fadedAlpha : 1f,
                switchDuration
            ));
        }

        IEnumerator SwapTween(RectTransform aRect, CanvasGroup aGrp,
                              RectTransform bRect, CanvasGroup bGrp,
                              float aScale, float aAlpha,
                              float bScale, float bAlpha,
                              float dur)
        {
            float t = 0f;
            float aScale0 = aRect.localScale.x;
            float aAlpha0 = aGrp.alpha;
            float bScale0 = bRect.localScale.x;
            float bAlpha0 = bGrp.alpha;

            Vector3 A(float s) => new Vector3(s, s, 1f);
            float Dt() => useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

            while (t < dur)
            {
                t += Dt();
                float k = Mathf.Clamp01(t / dur);
                float e = 1f - Mathf.Pow(1f - k, 3f);
                aRect.localScale = A(Mathf.Lerp(aScale0, aScale, e));
                aGrp.alpha       = Mathf.Lerp(aAlpha0, aAlpha, e);
                bRect.localScale = A(Mathf.Lerp(bScale0, bScale, e));
                bGrp.alpha       = Mathf.Lerp(bAlpha0, bAlpha, e);
                yield return null;
            }

            aRect.localScale = A(aScale);
            aGrp.alpha       = aAlpha;
            bRect.localScale = A(bScale);
            bGrp.alpha       = bAlpha;
        }

        IEnumerator PunchTextScale(RectTransform rt)
        {
            const float up = 1.15f, dur = 0.12f;
            float t = 0f;
            var baseS = Vector3.one;
            float Dt() => useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            while (t < dur)
            {
                t += Dt();
                float k = Mathf.Clamp01(t / dur);
                float s = Mathf.Lerp(1f, up, 1f - (1f - k) * (1f - k));
                rt.localScale = new Vector3(s, s, 1f);
                yield return null;
            }
            rt.localScale = baseS;
        }

        void PlayTurnFlash(int player)
        {
            if (_flashCo != null) StopCoroutine(_flashCo);
            _flashCo = StartCoroutine(TurnFlashRoutine(player));
        }

        IEnumerator TurnFlashRoutine(int player)
        {
            RectTransform rt = (player == 1) ? p1TurnScreen : p2TurnScreen;
            if (!rt) yield break;

            Vector2 baseCenter = (player == 1) ? _p1Center : _p2Center;
            float yOffset      = (player == 1) ? p1FlashYOffset : p2FlashYOffset;
            Vector2 center     = new Vector2(baseCenter.x, baseCenter.y + yOffset);

            float fromX = (player == 1) ? _offLeftX : _offRightX;

            yield return new WaitForSeconds(0.25f);
            rt.gameObject.SetActive(true);
            rt.anchoredPosition = new Vector2(fromX, center.y);
            
            yield return SlideX(rt, fromX, center.x, flashInDuration);

            yield return Hold(flashHoldTime);

            yield return SlideX(rt, center.x, fromX, flashOutDuration);

            rt.gameObject.SetActive(false);
            _gameManager.BlockRayCast = false;
        }

        IEnumerator SlideX(RectTransform rt, float fromX, float toX, float dur)
        {
            dur = Mathf.Max(0.01f, dur);
            float t = 0f;
            float Dt() => useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            float y = rt.anchoredPosition.y;
            
            SFXManager.PlaySFX("Slide", 0.7f, null, 0.2f);

            while (t < dur)
            {
                t += Dt();
                float k = Mathf.Clamp01(t / dur);
                float e = 1f - Mathf.Pow(1f - k, 3f);
                float x = Mathf.LerpUnclamped(fromX, toX, e);
                rt.anchoredPosition = new Vector2(x, y);
                yield return null;
            }
            rt.anchoredPosition = new Vector2(toX, y);
        }

        IEnumerator Hold(float seconds)
        {
            if (seconds <= 0f) yield break;
            float t = 0f;
            float Dt() => useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            while (t < seconds) { t += Dt(); yield return null; }
        }
    }
}
