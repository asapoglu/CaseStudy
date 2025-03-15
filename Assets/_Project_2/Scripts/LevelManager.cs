// Seviye Yöneticisi için güncellemeler
// Next Level işlemi için gerekli ayarlamalar

namespace Abdurrahman.Project_2.Core.Managers
{
    using Abdurrahman.Project_2.Core.Interfaces;
    using Abdurrahman.Project_2.Core.Models;
    using Abdurrahman.Project_2.Core.Signals;
    using UnityEngine;
    using Zenject;

    public class LevelManager : MonoBehaviour, ILevelManager
    {
        [Inject] private SignalBus _signalBus;

        [Header("Platform Boyutları")]
        [SerializeField] private float _pieceWidth = 3f;
        [SerializeField] private float _pieceHeight = 1f;
        [SerializeField] private float _pieceLength = 3f;
        [SerializeField] private float _toleranceWidth = 0.25f;

        [Header("Hız Parametreleri")]
        [SerializeField] private float _speedMin = 2f;
        [SerializeField] private float _speedMax = 5f;
        [SerializeField] private AnimationCurve _speedCurve;

        [Header("Parça Sayısı Parametreleri")]
        [SerializeField] private int _pieceCountMin = 5;
        [SerializeField] private int _pieceCountMax = 15;
        [SerializeField] private AnimationCurve _pieceCountCurve;

        [Header("Seviye Parametreleri")]
        [SerializeField] private int _maxLevel = 30;
        [SerializeField] private bool _resetLevelOnGameRestart = false; // Oyun tamamen yeniden başlatıldığında seviyeyi de sıfırla

        private int _currentLevel;
        private LevelParameters _currentParameters;

        public int CurrentLevel => _currentLevel;


        [Inject]
        private void Initialize()
        {
            _signalBus.Subscribe<LoadLevelSignal>(LoadLevel);
            _signalBus.Subscribe<NextLevelSignal>(OnContinue);
            _signalBus.Subscribe<GameFailSignal>(OnGameFail);
            _signalBus.Subscribe<RestartLevelSignal>(OnReplay);
        }

        private void OnDestroy()
        {
            _signalBus.TryUnsubscribe<LoadLevelSignal>(LoadLevel);
            _signalBus.TryUnsubscribe<NextLevelSignal>(OnContinue);
            _signalBus.TryUnsubscribe<GameFailSignal>(OnGameFail);
            _signalBus.TryUnsubscribe<RestartLevelSignal>(OnReplay);
        }

        public void LoadLevel(LoadLevelSignal signal)
        {
            bool _keepPreviousPlatform = signal.KeepPreviousPlatform;

            _currentLevel = PlayerPrefs.GetInt("Level", 1);
            // Seviye zorluğunu hesapla ve seviye parametrelerini oluştur
            _currentParameters = CalculateDifficulty();

            // Hedef pozisyonu hesapla
            _currentParameters.TargetPosition = (_currentParameters.Length * _currentParameters.PieceCount) +
                                               (_currentParameters.Length / 2f);

            _currentParameters.KeepCompletedPlatforms = _keepPreviousPlatform;
            // Seviye değişikliği ve hazır sinyallerini gönder
            _signalBus.Fire(new LevelNumberChangedSignal(_currentLevel));
            _signalBus.Fire(new LevelReadySignal(_currentParameters));
        }

        public void SaveLevelNumber()
        {
            // Seviye numarasını bir artır ve kaydet
            _currentLevel++;
            PlayerPrefs.SetInt("Level", _currentLevel);
            PlayerPrefs.Save();
        }

        private void OnContinue()
        {
            // Devam sinyali alındığında bir sonraki seviyeye geç
            SaveLevelNumber();
            // LoadLevel();
        }

        // Replay sinyali için yeni metod
        private void OnReplay()
        {

            // Eğer ayarlandıysa, replay durumunda seviyeyi sıfırla
            if (_resetLevelOnGameRestart)
            {
                _currentLevel = 1;
                PlayerPrefs.SetInt("Level", _currentLevel);
                PlayerPrefs.Save();
            }

            // Mevcut seviyeyi yeniden yükle (seviye numarası değişmedi)
            // LoadLevel();
        }



        public LevelParameters GetCurrentParameters()
        {
            return _currentParameters;
        }

        private LevelParameters CalculateDifficulty()
        {
            // Seviye zorluğunu hesapla
            float t = (float)_currentLevel / _maxLevel;

            // Parça sayısı ve hızı eğrilere göre belirle
            int pieceCount = Mathf.CeilToInt(Mathf.Lerp(_pieceCountMin, _pieceCountMax, _pieceCountCurve.Evaluate(t)));
            float speed = Mathf.Lerp(_speedMin, _speedMax, _speedCurve.Evaluate(t));

            // Seviye parametrelerini oluştur
            return new LevelParameters
            {
                Width = _pieceWidth,
                Height = _pieceHeight,
                Length = _pieceLength,
                ToleranceWidth = _toleranceWidth,
                Speed = speed,
                PieceCount = pieceCount,
                TargetPosition = 0 // Daha sonra hesaplanacak
            };
        }

        private void OnGameFail()
        {
            // Oyun başarısız olduğunda herhangi bir işlem yok
        }

        // Oyunu tamamen sıfırlamak için (dışarıdan çağrılabilir)
        public void ResetGameCompletely()
        {
            _currentLevel = 1;
            PlayerPrefs.SetInt("Level", _currentLevel);
            PlayerPrefs.Save();


            // LoadLevel();
        }
    }
}