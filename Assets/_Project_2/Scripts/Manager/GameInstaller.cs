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
        [SerializeField] private CameraManager _cameraManager;
        [SerializeField] private AudioManager _audioManager;
        [SerializeField] private InputManager _inputManager;
        [SerializeField] private UIManager _uiManager;
        [SerializeField] private ObjectPooler _objectPooler;


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
            Container.DeclareSignal<RestartLevelSignal>();

            // Platform sinyalleri
            Container.DeclareSignal<PiecePlacedSignal>();
            Container.DeclareSignal<PathChangedSignal>();

            // Seviye sinyalleri
            Container.DeclareSignal<LoadLevelSignal>();
            Container.DeclareSignal<LevelReadySignal>();
            Container.DeclareSignal<LevelNumberChangedSignal>();
        }

        private void BindManagers()
        {
            // Arayüzleri somut tiplerle bağla
            Container.Bind<IGameStateManager>().To<GameStateManager>().FromInstance(_gameStateManager).AsSingle();
            Container.Bind<LevelManager>().To<LevelManager>().FromInstance(_levelManager).AsSingle();
            Container.Bind<PlatformManager>().To<PlatformManager>().FromInstance(_platformManager).AsSingle();
            Container.Bind<IPlayerController>().To<PlayerController>().FromInstance(_playerController).AsSingle();
            Container.Bind<CameraManager>().To<CameraManager>().FromInstance(_cameraManager).AsSingle();
            Container.Bind<AudioManager>().To<AudioManager>().FromInstance(_audioManager).AsSingle();
            Container.Bind<InputManager>().To<InputManager>().FromInstance(_inputManager).AsSingle();
            Container.Bind<UIManager>().FromInstance(_uiManager).AsSingle();
            Container.Bind<ObjectPooler>().FromInstance(_objectPooler).AsSingle();
        }
    }

}