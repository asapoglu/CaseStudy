// UI Yöneticisi
namespace Abdurrahman.Project_2.Core.Managers
{
    using Abdurrahman.Project_2.Core.Interfaces;
    using Abdurrahman.Project_2.Core.Signals;
    using UnityEngine;
    using Zenject;
    using TMPro;
    using System;

    public class UIManager : MonoBehaviour
    {
        [Inject] private SignalBus _signalBus;
        [Inject] private IGameStateManager _gameStateManager;

        [Header("UI Panelleri")]
        [SerializeField] private GameObject _startPanel;
        [SerializeField] private GameObject _inGamePanel;
        [SerializeField] private GameObject _winPanel;
        [SerializeField] private GameObject _failPanel;

        [Header("UI Elemanları")]
        [SerializeField] private TextMeshProUGUI _levelNumberText;

        private void Awake()
        {
            ShowOnlyPanel(_startPanel);
        }

        [Inject]
        private void Initialize()
        {
            // Sinyallere abone ol
            _signalBus.Subscribe<LevelReadySignal>(OnLevelGenerated);
            _signalBus.Subscribe<GameFailSignal>(OnFail);
            _signalBus.Subscribe<GameSuccessSignal>(OnSuccess);
            _signalBus.Subscribe<GameStartSignal>(OnGameStart);
            _signalBus.Subscribe<LevelNumberChangedSignal>(ShowLevel);
            _signalBus.Subscribe<RestartLevelSignal>(OnReplay);

        }

        private void OnDestroy()
        {
            // Sinyallerden çık
            _signalBus.TryUnsubscribe<LevelReadySignal>(OnLevelGenerated);
            _signalBus.TryUnsubscribe<GameFailSignal>(OnFail);
            _signalBus.TryUnsubscribe<GameSuccessSignal>(OnSuccess);
            _signalBus.TryUnsubscribe<GameStartSignal>(OnGameStart);
            _signalBus.TryUnsubscribe<LevelNumberChangedSignal>(ShowLevel);
            _signalBus.TryUnsubscribe<RestartLevelSignal>(OnReplay);
        }

        // Seviye hazır olduğunda
        private void OnLevelGenerated(LevelReadySignal signal)
        {
            ShowOnlyPanel(_startPanel);
        }

        // Oyun başladığında
        private void OnGameStart()
        {
            ShowOnlyPanel(_inGamePanel);
        }

        // Oyun başarısız olduğunda
        private void OnFail()
        {
            ShowOnlyPanel(_failPanel);
        }

        // Oyun başarılı olduğunda
        private void OnSuccess()
        {
            ShowOnlyPanel(_winPanel);
        }

        // Replay sinyali alındığında
        private void OnReplay()
        {
            ShowOnlyPanel(_startPanel);
        }

        private void OnNextLevel()
        {
            ShowOnlyPanel(_startPanel);
        }

        // Seviye numarasını güncelle
        private void ShowLevel(LevelNumberChangedSignal signal)
        {
            if (_levelNumberText != null)
                _levelNumberText.text = "Level " + signal.LevelNumber;
        }

        // Sadece belirtilen paneli göster, diğerlerini gizle
        private void ShowOnlyPanel(GameObject panelToShow)
        {

            if (_startPanel != null)
                _startPanel.SetActive(panelToShow == _startPanel);

            if (_inGamePanel != null)
                _inGamePanel.SetActive(panelToShow == _inGamePanel);

            if (_winPanel != null)
                _winPanel.SetActive(panelToShow == _winPanel);

            if (_failPanel != null)
                _failPanel.SetActive(panelToShow == _failPanel);
        }

        // Start Panel butonuna tıklandığında çağrılacak metod
        public void OnStartButtonClicked()
        {
            _gameStateManager.StartGame();
        }

        // Win Panel içindeki Next Level butonuna tıklandığında çağrılacak metod
        public void OnNextLevelButtonClicked()
        {
            _gameStateManager.NextLevel();
        }

        // Fail Panel içindeki Replay butonuna tıklandığında çağrılacak metod
        public void OnReplayButtonClicked()
        {
            _gameStateManager.ReplayGame();
        }
    }
}