// Ses Yöneticisi
namespace Abdurrahman.Project_2.Core.Managers
{
    using Abdurrahman.Project_2.Core.Interfaces;
    using Abdurrahman.Project_2.Core.Models;
    using Abdurrahman.Project_2.Core.Signals;
    using UnityEngine;
    using Zenject;
    
    public class AudioManager : MonoBehaviour, IAudioManager
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
            // Ses kaynağı bileşeni yoksa oluştur
            if (_audioSource == null)
            {
                _audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        
        [Inject]
        private void Initialize()
        {
            // Sinyal(lere) abone ol
            _signalBus.Subscribe<PiecePlacedSignal>(OnPiecePlaced);
        }
        
        private void OnDestroy()
        {
            // Sinyal(ler)den çık
            _signalBus.TryUnsubscribe<PiecePlacedSignal>(OnPiecePlaced);
        }
        
        private void OnPiecePlaced(PiecePlacedSignal signal)
        {
            // Yerleştirme sonucuna göre uygun sesi çal
            PlayPlacementSound(signal.Result);
        }
        
        public void PlayPlacementSound(PlacementResult result)
        {
            if (result == PlacementResult.Perfect)
            {
                // Mükemmel yerleştirme için perde değerini artır
                _currentStep = Mathf.Clamp(++_currentStep, 0, _pitchStep);
                
                // Perde değerini ve ses seviyesini hesapla
                float ratio = (float)_currentStep / _pitchStep;
                float pitch = Mathf.Lerp(_minimumPitch, _maximumPitch, ratio);
                float volumeScale = 0.5f + ratio / 2;
                
                // Ses ayarlarını uygula
                _audioSource.panStereo = 0;
                _audioSource.pitch = pitch;
                _audioSource.PlayOneShot(_placementSound, volumeScale);
            }
            else
            {
                // Kesme sesi için stereo pan değerini ayarla (sağ veya sol)
                float panStereo = result == PlacementResult.RightCut ? 0.5f : -0.5f;
                
                // Ses ayarlarını uygula
                _audioSource.panStereo = panStereo;
                _audioSource.pitch = 1;
                _audioSource.PlayOneShot(_cutSound);
                
                // Perde adımını sıfırla
                _currentStep = -1;
            }
        }
        
        public void ResetPitch()
        {
            // Perde adımını sıfırla
            _currentStep = 0;
        }
    }
}