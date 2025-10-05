using System.Collections;
using UnityEngine;
using Data;
using GameControllers;

namespace Player
{
    [RequireComponent(typeof(Collider))]
    public class CardInteractable : MonoBehaviour
    {
        [Header("Data")]
        public CardData data;

        [Header("Tilt")]
        public float maxTilt = 6f;
        [Range(0.01f, 0.6f)] public float tiltSmooth = 0.12f;
        public float tiltScreenRadius = 180f;

        [Header("Hover")]
        public float hoverScale = 1.06f;
        public float hoverInTime = 0.12f;
        public float hoverOutTime = 0.10f;

        [Header("Select")]
        public float liftDistance = 0.15f;
        public float selectScale = 1.18f;
        public float selectRiseTime = 0.12f;
        public float selectSettleTime = 0.15f;
        public float selectTwistDeg = 10f;

        [Header("Misc")]
        public bool useUnscaledTime = true;
        

        public bool IsHovered { get; private set; }
        public bool IsSelected { get; private set; }

        Transform _t;
        Vector3 _basePosLocal;
        Quaternion _baseRotLocal;
        Vector3 _baseScale;

        Coroutine _hoverCo;
        Coroutine _selectCo;
        float _currentPitch, _currentRoll;

        void Awake()
        {
            _t = transform;
            _basePosLocal = _t.localPosition;
            _baseRotLocal = _t.localRotation;
            _baseScale = _t.localScale;
        }

        void Update()
        {
            UpdateTilt(Camera.main, Input.mousePosition);
        }

        public void Hover(bool on)
        {
            if (IsHovered == on) return;
            IsHovered = on;

            if (on)
            {
               //if (!string.IsNullOrEmpty(sfxHoverKey)) 
               SFXManager.PlaySFX("Hover",0.9f, null, 0.2f);
               
               if (IsSelected) return;
               
               if (_hoverCo != null) StopCoroutine(_hoverCo);
                _hoverCo = StartCoroutine(ScaleTo(_baseScale * hoverScale, hoverInTime));
            }
            else
            {
                if (IsSelected) return;
                if (_hoverCo != null) StopCoroutine(_hoverCo);
                _hoverCo = StartCoroutine(ScaleTo(_baseScale, hoverOutTime));
            }
        }

        [ContextMenu("Select Card")]
        public void Select()
        {
            if (IsSelected) return;
            IsSelected = true;
            
            //todo : change to select
            SFXManager.PlaySFX("Hover", 0.9f, null, 0.2f);
            

            if (_hoverCo != null) { StopCoroutine(_hoverCo); _hoverCo = null; }
            if (_selectCo != null) StopCoroutine(_selectCo);
            _selectCo = StartCoroutine(SelectAnim());
        }

        [ContextMenu("Deselect Card")]
        public void DeselectImmediate()
        {
            if (!IsSelected && !IsHovered) return;

            if (_selectCo != null) { StopCoroutine(_selectCo); _selectCo = null; }
            if (_hoverCo   != null) { StopCoroutine(_hoverCo);   _hoverCo   = null; }

            IsSelected = false;
            IsHovered  = false;

            _t.localPosition = _basePosLocal;
            _t.localRotation = _baseRotLocal;
            _t.localScale    = _baseScale;

            _currentPitch = 0f;
            _currentRoll  = 0f;
        }

        
        void UpdateTilt(Camera cam, Vector3 mousePos)
        {
            if (!cam) return;

            var screenPos = cam.WorldToScreenPoint(_t.position);
            var delta = (Vector2)(mousePos - screenPos);

            float r = Mathf.Max(4f, tiltScreenRadius);
            Vector2 n = Vector2.ClampMagnitude(delta / r, 1f);

            Vector2 target = (IsHovered || IsSelected) ? n : Vector2.zero;

            float targetPitch = -target.y * maxTilt;
            float targetRoll  =  target.x * maxTilt;

            float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            float k  = 1f - Mathf.Exp(-dt / Mathf.Max(0.0001f, tiltSmooth));

            _currentPitch = Mathf.Lerp(_currentPitch, targetPitch, k);
            _currentRoll  = Mathf.Lerp(_currentRoll , targetRoll , k);

            var tiltDelta = Quaternion.AngleAxis(_currentPitch, Vector3.right)
                          * Quaternion.AngleAxis(_currentRoll , Vector3.forward);

            _t.localRotation = _baseRotLocal * tiltDelta;
        }

        IEnumerator ScaleTo(Vector3 target, float time)
        {
            time = Mathf.Max(0.01f, time);
            Vector3 start = _t.localScale;
            float t = 0f;
            float dt() => useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            while (t < time)
            {
                t += dt();
                float k = Mathf.Clamp01(t / time);
                float e = 1f - Mathf.Pow(1f - k, 3f);
                _t.localScale = Vector3.LerpUnclamped(start, target, e);
                yield return null;
            }
            _t.localScale = target;
        }

        IEnumerator SelectAnim()
        {
            Vector3 startPos = _basePosLocal;
            Vector3 upPos    = _basePosLocal + Vector3.up * liftDistance;

            Quaternion startRot  = _t.localRotation;
            Quaternion twistRot  = _baseRotLocal * Quaternion.Euler(0f, selectTwistDeg, 0f);

            Vector3 startScale = _t.localScale;
            Vector3 big        = _baseScale * selectScale;

            float dt() => useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

            float t = 0f;
            float upTime = Mathf.Max(0.01f, selectRiseTime);
            while (t < upTime)
            {
                t += dt();
                float k = Mathf.Clamp01(t / upTime);
                float e = 1f - Mathf.Pow(1f - k, 3f);
                _t.localPosition = Vector3.LerpUnclamped(startPos, upPos, e);
                _t.localRotation = Quaternion.Slerp(startRot, twistRot, e);
                _t.localScale    = Vector3.LerpUnclamped(startScale, big, e);
                yield return null;
            }

            t = 0f;
            float settle = Mathf.Max(0.01f, selectSettleTime);
            Quaternion settleRot = Quaternion.Slerp(twistRot, _baseRotLocal, 0.3f);
            while (t < settle)
            {
                t += dt();
                float k = Mathf.Clamp01(t / settle);
                float e = 1f - Mathf.Pow(1f - k, 3f);
                _t.localPosition = upPos;
                _t.localRotation = Quaternion.Slerp(twistRot, settleRot, e);
                _t.localScale    = big;
                yield return null;
            }
        }
    }
}
