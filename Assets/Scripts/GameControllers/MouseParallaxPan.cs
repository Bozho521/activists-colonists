using UnityEngine;

namespace GameControllers
{
    [DefaultExecutionOrder(50)]
    [RequireComponent(typeof(Camera))]
    public class MouseParallaxPan : MonoBehaviour
    {
        [Header("Pan amount (world units at screen edges)")]
        public Vector2 maxOffset = new Vector2(3f, 2f);

        [Header("Feel")]
        [Range(0f, 0.5f)] public float deadZone = 0.06f;
        [Range(0.01f, 0.6f)] public float smoothTime = 0.18f;
        public bool useUnscaledTime = true;

        [Header("Bounds (optional)")]
        public bool clampToBounds = false;
        public Vector2 worldXLimits = new Vector2(-999f, 999f);
        public Vector2 worldZLimits = new Vector2(-999f, 999f);

        [Header("Origin")]
        public Transform originOverride;
        public bool lockY = true;

        [Header("Shake Defaults")]
        public float defaultFrequency = 22f;

        Camera _cam;
        Vector3 _origin;
        Vector3 _velocity;

        // Shake state
        float _shakeTimeLeft;
        float _shakeAmplitude;
        float _shakeFrequency;
        float _shakeDecay;
        Vector2 _noiseSeed;        
        Vector3 _shakeOffset;   
        Vector3 _lastSmoothed;   

        void Awake()
        {
            _cam = GetComponent<Camera>();
            _origin = transform.position;

            _noiseSeed = new Vector2(Random.value * 1000f, Random.value * 1000f);
        }

        void LateUpdate()
        {
            float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

            if (originOverride)
            {
                _origin = originOverride.position;
                if (lockY) _origin.y = transform.position.y;
            }

            Vector2 n = GetMouseCenterNorm();
            n = ApplyDeadZone(n);

            Vector3 right = Vector3.ProjectOnPlane(transform.right, Vector3.up).normalized;
            Vector3 fwd   = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;

            Vector3 worldOffset = right * (n.x * maxOffset.x) + fwd * (n.y * maxOffset.y);
            Vector3 target = _origin + worldOffset;
            if (lockY) target.y = transform.position.y;

            if (clampToBounds)
            {
                target.x = Mathf.Clamp(target.x, worldXLimits.x, worldXLimits.y);
                target.z = Mathf.Clamp(target.z, worldZLimits.x, worldZLimits.y);
            }

            _lastSmoothed = Vector3.SmoothDamp(transform.position - _shakeOffset, target, ref _velocity, smoothTime, Mathf.Infinity, dt);

            UpdateShake(dt, right, fwd);

            transform.position = _lastSmoothed + _shakeOffset;
        }

        Vector2 GetMouseCenterNorm()
        {
            if (Screen.width <= 1 || Screen.height <= 1) return Vector2.zero;
            Vector2 m = Input.mousePosition;
            return new Vector2(
                (m.x / Screen.width) * 2f - 1f,
                (m.y / Screen.height) * 2f - 1f
            );
        }

        static Vector2 ApplyDeadZone(Vector2 v, float dz = 0f)
        {
            if (dz <= 0f) return v;
            float mag = v.magnitude;
            if (mag < dz) return Vector2.zero;
            return v * ((mag - dz) / (1f - dz));
        }

        void UpdateShake(float dt, Vector3 rightOnPlane, Vector3 fwdOnPlane)
        {
            if (_shakeTimeLeft > 0f)
            {
                _shakeTimeLeft -= dt;

                float t = Mathf.Max(0f, _shakeTimeLeft);
                float ampNow = _shakeAmplitude;
                if (_shakeDecay > 0f)
                {
                    ampNow *= Mathf.Exp(-_shakeDecay * (1f + (_shakeFrequency * 0.02f)) * (1f - Mathf.Clamp01(t / Mathf.Max(t, 0.00001f))));
                }

                float time = (useUnscaledTime ? Time.unscaledTime : Time.time);

                float nx = Mathf.PerlinNoise(_noiseSeed.x, time * _shakeFrequency) * 2f - 1f;
                float ny = Mathf.PerlinNoise(_noiseSeed.y, time * _shakeFrequency) * 2f - 1f;

                Vector2 n = new Vector2(nx, ny);
                if (n.sqrMagnitude > 1e-4f) n.Normalize();

                _shakeOffset = rightOnPlane * (n.x * ampNow) + fwdOnPlane * (n.y * ampNow);

                if (lockY) _shakeOffset.y = 0f;

                if (_shakeTimeLeft <= 0f)
                {
                    _shakeOffset = Vector3.zero;
                    _shakeAmplitude = 0f;
                }
            }
            else
            {
                _shakeOffset = Vector3.zero;
            }
        }

   
    
        public void Shake(float amplitude, float frequency, float duration, float decay = 1f)
        {
            _shakeAmplitude = Mathf.Max(0f, amplitude);
            _shakeFrequency = frequency > 0f ? frequency : defaultFrequency;
            _shakeTimeLeft  = Mathf.Max(0f, duration);
            _shakeDecay     = Mathf.Max(0f, decay);
        }


        public void ExplosionShake(Vector3 epicenter, float radius, float maxAmplitude, float duration, float decay = 1f, AnimationCurve falloff = null)
        {
            float d = Vector3.Distance(GetCameraGroundPos(), new Vector3(epicenter.x, transform.position.y, epicenter.z));
            float k = Mathf.Clamp01(1f - (d / Mathf.Max(0.0001f, radius)));
            if (falloff != null) k = Mathf.Clamp01(falloff.Evaluate(k));
            float amp = maxAmplitude * k;
            if (amp <= 0f) return;
            Shake(amp, defaultFrequency, duration, decay);
        }

        public void SetOrigin(Vector3 p, bool keepCurrentY = true)
        {
            _origin = p;
            if (keepCurrentY || lockY) _origin.y = transform.position.y;
        }

        public void SnapToOrigin()
        {
            Vector3 p = _origin;
            if (lockY) p.y = transform.position.y;
            _velocity = Vector3.zero;
            _shakeOffset = Vector3.zero;
            transform.position = p;
        }

        Vector3 GetCameraGroundPos()
        {
            return new Vector3(transform.position.x, 0f, transform.position.z);
        }
    }
}
