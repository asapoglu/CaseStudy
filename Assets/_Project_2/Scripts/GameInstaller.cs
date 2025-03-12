// Zenject Yükleme Sınıfları
namespace Abdurrahman.Project_2.Core.Installers
{
    using Abdurrahman.Project_2.Core.Interfaces;
    using Abdurrahman.Project_2.Core.Managers;
    using Abdurrahman.Project_2.Core.Signals;
    using UnityEngine;
    using Zenject;
    
    // Ana oyun yükleyicisi
    public class GameInstaller : MonoInstaller
    {
        // Yönetici referansları
        [SerializeField] private GameStateManager _gameStateManager;
        [SerializeField] private LevelManager _levelManager;
        [SerializeField] private PlatformManager _platformManager;
        [SerializeField] private PlayerController _playerController;
        [SerializeField] private CameraController _cameraController;
        [SerializeField] private AudioManager _audioManager;
        [SerializeField] private InputManager _inputManager;
        [SerializeField] private UIManager _uiManager;
        
        public override void InstallBindings()
        {
            // Sinyal sistemini kur
            SignalBusInstaller.Install(Container);
            
            // Sinyalleri bildir
            DeclareSignals();
            
            // Yöneticileri bağla
            BindManagers();
        }
        
        private void DeclareSignals()
        {
            // Oyun durumu sinyalleri
            Container.DeclareSignal<GameStartSignal>();
            Container.DeclareSignal<GameFailSignal>();
            Container.DeclareSignal<GameSuccessSignal>();
            Container.DeclareSignal<ContinueSignal>();
            Container.DeclareSignal<ReplaySignal>();
            
            // Platform sinyalleri
            Container.DeclareSignal<PiecePlacedSignal>();
            Container.DeclareSignal<PathChangedSignal>();
            
            // Seviye sinyalleri
            Container.DeclareSignal<LevelReadySignal>();
            Container.DeclareSignal<LevelNumberChangedSignal>();
        }
        
        private void BindManagers()
        {
            // Arayüzleri somut tiplerle bağla
            Container.Bind<IGameStateManager>().To<GameStateManager>().FromInstance(_gameStateManager).AsSingle();
            Container.Bind<ILevelManager>().To<LevelManager>().FromInstance(_levelManager).AsSingle();
            Container.Bind<IPlatformManager>().To<PlatformManager>().FromInstance(_platformManager).AsSingle();
            Container.Bind<IPlayerController>().To<PlayerController>().FromInstance(_playerController).AsSingle();
            Container.Bind<ICameraController>().To<CameraController>().FromInstance(_cameraController).AsSingle();
            Container.Bind<IAudioManager>().To<AudioManager>().FromInstance(_audioManager).AsSingle();
            Container.Bind<IInputManager>().To<InputManager>().FromInstance(_inputManager).AsSingle();
            
            // UI Manager doğrudan bağla (arayüz kullanmadan)
            Container.Bind<UIManager>().FromInstance(_uiManager).AsSingle();
        }
    }
    
    // Sinyaller için yükleyici
    public class SignalsInstaller : Installer<SignalsInstaller>
    {
        public override void InstallBindings()
        {
            // Oyun durumu sinyalleri
            Container.DeclareSignal<GameStartSignal>();
            Container.DeclareSignal<GameFailSignal>();
            Container.DeclareSignal<GameSuccessSignal>();
            Container.DeclareSignal<ContinueSignal>();
            Container.DeclareSignal<ReplaySignal>();
            
            // Platform sinyalleri
            Container.DeclareSignal<PiecePlacedSignal>();
            Container.DeclareSignal<PathChangedSignal>();
            
            // Seviye sinyalleri
            Container.DeclareSignal<LevelReadySignal>();
            Container.DeclareSignal<LevelNumberChangedSignal>();
        }
    }
}