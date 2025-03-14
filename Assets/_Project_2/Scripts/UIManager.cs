// UI Yöneticisi
namespace Abdurrahman.Project_2.Core.Managers
{
    using Abdurrahman.Project_2.Core.Interfaces;
    using Abdurrahman.Project_2.Core.Signals;
    using UnityEngine;
    using Zenject;
    using TMPro;
    
    public class UIManager : MonoBehaviour
    {
        [Inject] private SignalBus _signalBus;
        [Inject] private IGameStateManager _gameStateManager;
        
        [Header("UI Panelleri")]
        [SerializeField] private GameObject _startPanel;    // Oyun başlangıç paneli
        [SerializeField] private GameObject _inGamePanel;   // Oyun sırasında gösterilen panel
        [SerializeField] private GameObject _winPanel;      // Başarılı olunca gösterilen panel
        [SerializeField] private GameObject _failPanel;     // Başarısız olunca gösterilen panel
        
        [Header("UI Elemanları")]
        [SerializeField] private TextMeshProUGUI _levelNumberText;
        
        private void Awake()
        {
            // Başlangıçta sadece start panel göster
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
            _signalBus.Subscribe<ReplaySignal>(OnReplay);
            
            Debug.Log("UIManager başlatıldı - Panel referansları kontrol ediliyor");
            CheckPanelReferences();
        }
        
        // Panel referanslarını kontrol et
        private void CheckPanelReferences()
        {
            if (_startPanel == null)
                Debug.LogWarning("Start panel referansı atanmamış!");
            
            if (_inGamePanel == null)
                Debug.LogWarning("InGame panel referansı atanmamış!");
            
            if (_winPanel == null)
                Debug.LogWarning("Win panel referansı atanmamış!");
            
            if (_failPanel == null)
                Debug.LogWarning("Fail panel referansı atanmamış!");
        }
        
        private void OnDestroy()
        {
            // Sinyallerden çık
            _signalBus.TryUnsubscribe<LevelReadySignal>(OnLevelGenerated);
            _signalBus.TryUnsubscribe<GameFailSignal>(OnFail);
            _signalBus.TryUnsubscribe<GameSuccessSignal>(OnSuccess);
            _signalBus.TryUnsubscribe<GameStartSignal>(OnGameStart);
            _signalBus.TryUnsubscribe<LevelNumberChangedSignal>(ShowLevel);
            _signalBus.TryUnsubscribe<ReplaySignal>(OnReplay);
        }
        
        // Seviye hazır olduğunda
        private void OnLevelGenerated(LevelReadySignal signal)
        {
            Debug.Log("UIManager - Seviye hazır, UI güncelleniyor");
            
            // Seviye yüklendiğinde Start panelini göster
            ShowOnlyPanel(_startPanel);
        }
        
        // Oyun başladığında
        private void OnGameStart()
        {
            Debug.Log("UIManager - Oyun başladı, InGame panel gösteriliyor");
            ShowOnlyPanel(_inGamePanel);
        }
        
        // Oyun başarısız olduğunda
        private void OnFail()
        {
            Debug.Log("UIManager - Oyun başarısız oldu, Fail panel gösteriliyor");
            ShowOnlyPanel(_failPanel);
        }
        
        // Oyun başarılı olduğunda
        private void OnSuccess()
        {
            Debug.Log("UIManager - Oyun başarılı oldu, Win panel gösteriliyor");
            ShowOnlyPanel(_winPanel);
        }
        
        // Replay sinyali alındığında
        private void OnReplay()
        {
            Debug.Log("UIManager - Replay sinyali alındı, Start panel gösteriliyor");
            ShowOnlyPanel(_startPanel);
        }
        
        // Seviye numarasını güncelle
        private void ShowLevel(LevelNumberChangedSignal signal)
        {
            if (_levelNumberText != null)
                _levelNumberText.text = "Level " + signal.LevelNumber;
            else
                Debug.LogWarning("Level numarası text referansı atanmamış!");
        }
        
        // Sadece belirtilen paneli göster, diğerlerini gizle
        private void ShowOnlyPanel(GameObject panelToShow)
        {
            Debug.Log("UIManager - Panel değiştiriliyor: " + (panelToShow != null ? panelToShow.name : "null"));
            
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
            Debug.Log("UIManager - Start butonu tıklandı");
            _gameStateManager.StartGame();
        }
        
        // Win Panel içindeki Next Level butonuna tıklandığında çağrılacak metod
        public void OnNextLevelButtonClicked()
        {
            Debug.Log("UIManager - Next Level butonu tıklandı");
            _gameStateManager.NextLevel();
        }
        
        // Fail Panel içindeki Replay butonuna tıklandığında çağrılacak metod
        public void OnReplayButtonClicked()
        {
            Debug.Log("UIManager - Replay butonu tıklandı");
            _gameStateManager.ReplayGame();
        }
    }
}