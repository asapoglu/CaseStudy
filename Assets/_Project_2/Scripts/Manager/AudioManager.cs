namespace Abdurrahman.Project_2.Core.Managers
{
    using Abdurrahman.Project_2.Core.Models;
    using Abdurrahman.Project_2.Core.Signals;
    using UnityEngine;
    using Zenject;

    public class AudioManager : MonoBehaviour
    {
        [Inject] private SignalBus _signalBus;

        [Header("Ses Bileşenleri")]
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private AudioClip _placementSound;
        [SerializeField] private AudioClip _cutSound;

        [Header("Ses Ayarları")]
        [Range(0, 3)]
        [SerializeField] private float _minimumPitch = 0.5f;
        [Range(0, 3)]
        [SerializeField] private float _maximumPitch = 2.0f;
        [Min(1)]
        [SerializeField] private int _pitchStep = 5;

        private int _currentStep;

        private void Awake()
        {
            if (_audioSource == null)
            {
                _audioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        [Inject]
        private void Initialize()
        {
            _signalBus.Subscribe<PiecePlacedSignal>(OnPiecePlaced);
            _signalBus.Subscribe<LevelReadySignal>(ResetPitch);
        }

        private void OnDestroy()
        {
            _signalBus.TryUnsubscribe<PiecePlacedSignal>(OnPiecePlaced);
            _signalBus.TryUnsubscribe<LevelReadySignal>(ResetPitch);
        }

        private void OnPiecePlaced(PiecePlacedSignal signal)
        {
            PlayPlacementSound(signal.Result);
        }

        public void PlayPlacementSound(PlacementResult result)
        {
            if (result == PlacementResult.Perfect)
            {
                _currentStep = Mathf.Clamp(++_currentStep, 0, _pitchStep);

                float ratio = (float)_currentStep / _pitchStep;
                float pitch = Mathf.Lerp(_minimumPitch, _maximumPitch, ratio);
                float volumeScale = 0.5f + ratio / 2;

                _audioSource.panStereo = 0;
                _audioSource.pitch = pitch;
                _audioSource.PlayOneShot(_placementSound, volumeScale);
            }
            else
            {
                float panStereo = result == PlacementResult.RightCut ? 0.5f : -0.5f;

                _audioSource.panStereo = panStereo;
                _audioSource.PlayOneShot(_cutSound);
                ResetPitch();
            }
        }

        public void ResetPitch()
        {
            _audioSource.pitch = 1;
            _currentStep = -1;
        }
    }
}