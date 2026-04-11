using UniRx;
using UnityEngine;
using VContainer;

namespace MornLib
{
    public sealed class MornBeatScaler : MonoBehaviour
    {
        [Inject] private MornBeatController _beatController;
        [SerializeField] private MornBeatScaleSettings _settings;
        [SerializeField] private bool _isAutoStart = true;
        private Vector3 _originScale;
        private Vector3 _adjustedAimScale;
        private bool _isActive;

        public bool IsActive
        {
            get => _isActive;
            set
            {
                _isActive = value;
                if (!_isActive)
                {
                    transform.localScale = _originScale;
                }
            }
        }

        private void Start()
        {
            if (_settings == null) _settings = MornBeatGlobal.I.DefaultScaleSettings;
            _originScale = transform.localScale;
            _adjustedAimScale = CalcAdjustedAimScale();
            _isActive = _isAutoStart;
            _beatController.PlayModule.OnBeat
                .Where(x => _isActive && x.IsJustForAnyBeat(_settings.PerBeat))
                .Subscribe(_ => transform.localScale = _adjustedAimScale)
                .AddTo(this);
        }

        private void Update()
        {
            if (!_isActive) return;
            var scale = Vector3.Lerp(transform.localScale, _originScale, Time.deltaTime * _settings.LerpSpeed);
            transform.localScale = scale;
        }

        private Vector3 CalcAdjustedAimScale()
        {
            var aim = _settings.AimScale;
            var rt = transform as RectTransform;
            if (rt == null) return aim;

            var size = rt.sizeDelta;
            if (size.x <= 0 || size.y <= 0) return aim;

            var minSide = Mathf.Min(size.x, size.y);
            var ratioX = minSide / size.x;
            var ratioY = minSide / size.y;
            return new Vector3(
                1f + (aim.x - 1f) * ratioX,
                1f + (aim.y - 1f) * ratioY,
                aim.z);
        }
    }
}
