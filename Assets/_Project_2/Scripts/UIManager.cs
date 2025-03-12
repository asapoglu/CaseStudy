// UI Yöneticisi
namespace Abdurrahman.Project_2.Core.Managers
{
    using Abdurrahman.Project_2.Core.Interfaces;
    using Abdurrahman.Project_2.Core.Models;
    using Abdurrahman.Project_2.Core.Signals;
    using UnityEngine;
    using Zenject;
    using TMPro;
    
    public class UIManager : MonoBehaviour
    {
        [Inject] private SignalBus _signalBus;
        [Inject] private IGameStateManager _gameStateManager;
        
        [Header("UI Panelleri")]
        [SerializeField] private GameObject _loadingPanel;
        [SerializeField] private GameObject _startButton;
        [SerializeField] private GameObject _continueButton;
        [SerializeField] private GameObject _failPanel;
        
        [Header("UI Elemanları")]
        [SerializeField] private TextMeshProUGUI _levelNumberText;
        
        private void Awake()
        {
            // Başlangıçta yükleme panelini göster
            if (_loadingPanel != null)
            {
                _loadingPanel.SetActive(true);
            }
        }
        
        [Inject]
        private void Initialize()
        {
            // Sinyallere abone ol
            _signalBus.Subscribe<LevelReadySignal>(OnLevelGenerated);
            _signalBus.Subscribe<GameFailSignal>(OnFail);
            _signalBus.Subscribe<LevelNumberChangedSignal>(ShowLevel);
            _signalBus.Subscribe<GameStartSignal>(OnGameStart);
            _signalBus.Subscribe<GameSuccessSignal>(OnSuccess);
            _signalBus.Subscribe<ContinueSignal>(OnContinueNewLevel);
            _signalBus.Subscribe<ReplaySignal>(OnGameReplay);
        }
        
        private void OnDestroy()
        {
            // Sinyallerden çık
            _signalBus.TryUnsubscribe<LevelReadySignal>(OnLevelGenerated);
            _signalBus.TryUnsubscribe<GameFailSignal>(OnFail);
            _signalBus.TryUnsubscribe<LevelNumberChangedSignal>(ShowLevel);
            _signalBus.TryUnsubscribe<GameStartSignal>(OnGameStart);
            _signalBus.TryUnsubscribe<GameSuccessSignal>(OnSuccess);
            _signalBus.TryUnsubscribe<ContinueSignal>(OnContinueNewLevel);
            _signalBus.TryUnsubscribe<ReplaySignal>(OnGameReplay);
        }
        
        private void OnGameStart()
        {
            // Oyun başladığında başlat butonunu gizle
            if (_startButton != null)
            {
                _startButton.SetActive(false);
            }
        }
        
        private void OnGameReplay()
        {
            // Oyun yeniden başladığında başarısız panelini gizle ve başlat butonunu göster
            if (_failPanel != null)
            {
                _failPanel.SetActive(false);
            }
            
            if (_startButton != null)
            {
                _startButton.SetActive(true);
            }
        }
        
        private void OnLevelGenerated(LevelReadySignal signal)
        {
            // Seviye oluşturulduğunda yükleme panelini gizle ve başlat butonunu göster
            if (_loadingPanel != null)
            {
                _loadingPanel.SetActive(false);
            }
            
            if (_startButton != null)
            {
                _startButton.SetActive(true);
            }
        }
        
        private void OnFail()
        {
            // Oyun başarısız olduğunda başarısız panelini göster
            if (_failPanel != null)
            {
                _failPanel.SetActive(true);
            }
        }
        
        private void ShowLevel(LevelNumberChangedSignal signal)
        {
            // Seviye numarasını göster
            if (_levelNumberText != null)
            {
                _levelNumberText.text = "Level " + signal.LevelNumber;
            }
        }
        
        private void OnSuccess()
        {
            // Oyun başarılı olduğunda devam butonunu göster
            if (_continueButton != null)
            {
                _continueButton.SetActive(true);
            }
        }
        
        private void OnContinueNewLevel()
        {
            // Yeni seviyeye geçildiğinde yükleme panelini göster ve devam butonunu gizle
            if (_loadingPanel != null)
            {
                _loadingPanel.SetActive(true);
            }
            
            if (_continueButton != null)
            {
                _continueButton.SetActive(false);
            }
        }
        
        // Başlat butonu için çağrılacak metod
        public void OnStartButtonClick()
        {
            _gameStateManager.StartGame();
        }
        
        // Devam butonu için çağrılacak metod
        public void OnContinueButtonClick()
        {
            _signalBus.Fire(new ContinueSignal());
        }
    }
}