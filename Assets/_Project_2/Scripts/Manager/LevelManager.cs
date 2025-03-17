

namespace Abdurrahman.Project_2.Core.Managers
{
    using Abdurrahman.Project_2.Core.Models;
    using Abdurrahman.Project_2.Core.Signals;
    using DG.Tweening;
    using UnityEngine;
    using Zenject;

    public class LevelManager : MonoBehaviour
    {
        [Inject] private SignalBus _signalBus;
        [SerializeField] private float _toleranceWidth = 0.25f;

        [Header("Hız Parametreleri")]
        [SerializeField] private float _speedMin = 2f;
        [SerializeField] private float _speedMax = 5f;
        [SerializeField] private Ease _speedEase = Ease.InQuad;

        [Header("Parça Sayısı Parametreleri")]
        [SerializeField] private int _pieceCountMin = 5;
        [SerializeField] private int _pieceCountMax = 15;
        [SerializeField] private Ease _pieceCountEase = Ease.InQuad;

        [Header("Seviye Parametreleri")]
        [SerializeField] private int _maxLevel = 30;
        private int _currentLevel;
        private LevelParameters _currentParameters;
        private LevelParameters _exParameters;
        private float _pieceWidth = 3f;
        private float _pieceHeight = .5f;
        private float _pieceLength = 3f;

        public int CurrentLevel => _currentLevel;

        [Inject]
        private void Initialize()
        {
            _signalBus.Subscribe<LoadLevelSignal>(LoadLevel);
        }

        private void OnDestroy()
        {
            _signalBus.TryUnsubscribe<LoadLevelSignal>(LoadLevel);
        }

        public void LoadLevel(LoadLevelSignal signal)
        {
            bool _isNextLevel = signal.IsNextLevel;
            if (_isNextLevel)
            {
                _exParameters = GetExLevelParameters();
                SetLevel();
            }
            _currentLevel = PlayerPrefs.GetInt("Level", 1);
            _currentParameters = CalculateDifficulty();
            _currentParameters.Level = _currentLevel;
            _currentParameters.TargetPosition = (_currentParameters.Length * _currentParameters.PieceCount) +
                                               (_currentParameters.Length / 2f);
            _currentParameters.IsNextLevel = _isNextLevel;
            _signalBus.Fire(new LevelNumberChangedSignal(_currentLevel));
            _signalBus.Fire(new LevelReadySignal(_currentParameters, _exParameters));
        }

        public void SetLevel()
        {
            _currentLevel++;
            PlayerPrefs.SetInt("Level", _currentLevel);
            PlayerPrefs.Save();
        }


        private LevelParameters CalculateDifficulty()
        {
            // Seviye zorluğunu hesapla
            float t = (float)_currentLevel / _maxLevel;

            float pieceCountT = DOVirtual.EasedValue(0, 1, t, _pieceCountEase);
            Debug.Log("Piece Count T: " + pieceCountT);
            int pieceCount = Mathf.CeilToInt(Mathf.Lerp(_pieceCountMin, _pieceCountMax, pieceCountT));
            float speedT = DOVirtual.EasedValue(0, 1, t, _speedEase);
            Debug.Log("Speed T: " + speedT);
            float speed = Mathf.Lerp(_speedMin, _speedMax, speedT);

            // Seviye parametrelerini oluştur
            return new LevelParameters
            {
                Width = _pieceWidth,
                Height = _pieceHeight,
                Length = _pieceLength,
                ToleranceWidth = _toleranceWidth,
                Speed = speed,
                PieceCount = pieceCount,
                TargetPosition = 0
            };
        }

        public LevelParameters GetExLevelParameters()
        {
            return new LevelParameters
            {
                Width = _currentParameters.Width,
                Height = _currentParameters.Height,
                Length = _currentParameters.Length,
                ToleranceWidth = _currentParameters.ToleranceWidth,
                Speed = _currentParameters.Speed,
                PieceCount = _currentParameters.PieceCount,
                TargetPosition = _currentParameters.TargetPosition
            };
        }
    }
}