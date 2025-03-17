namespace Abdurrahman.Project_2.Core.Managers
{
    using Abdurrahman.Project_2.Core.Signals;
    using UnityEngine;
    using Zenject;
    using Cinemachine;
    using Cysharp.Threading.Tasks;
    using System.Threading;

    public class CameraManager : MonoBehaviour
    {
        [Inject] private SignalBus _signalBus;

        [Header("Kamera Ayarları")]
        [SerializeField] private CinemachineVirtualCamera _playCamera;
        [SerializeField] private CinemachineVirtualCamera _finalCamera;
        [SerializeField] private float _rotateSpeed = 30f;

        private CinemachineOrbitalTransposer _orbitalTransposer;
        private float _angle;
        private bool _rotate;
        private CancellationTokenSource _cts;

        private void Awake()
        {
            if (_finalCamera != null)
            {
                _orbitalTransposer = _finalCamera.GetCinemachineComponent<CinemachineOrbitalTransposer>();
            }
        }

        [Inject]
        private void Initialize()
        {
            _signalBus.Subscribe<GameSuccessSignal>(OnSuccess);
            _signalBus.Subscribe<LevelReadySignal>(OnLevelReady);
        }

        private void OnDestroy()
        {
            _signalBus.TryUnsubscribe<GameSuccessSignal>(OnSuccess);
            _signalBus.TryUnsubscribe<LevelReadySignal>(OnLevelReady);
            _cts?.Cancel();
            _cts?.Dispose();
        }

        private void OnSuccess()
        {
            SwitchCamera(false);
            StartCameraRotation();
        }

        private void OnLevelReady(LevelReadySignal signal)
        {   
            SwitchCamera(true);
            StopCameraRotation();
        }

        public void SwitchCamera(bool isActive)
        {
            if (_playCamera != null)
            {
                _playCamera.gameObject.SetActive(isActive);
                _finalCamera.gameObject.SetActive(!isActive);
            }
        }

        public void StartCameraRotation()
        {
            if (_orbitalTransposer == null) return;

            _cts?.Cancel();
            _cts = new CancellationTokenSource();
            RotateCameraAsync(_cts.Token).Forget();
        }

        public void StopCameraRotation()
        {
            _rotate = false;
            _cts?.Cancel();
        }

        private async UniTask RotateCameraAsync(CancellationToken cancellationToken)
        {
            _rotate = true;
            _angle = _orbitalTransposer.m_Heading.m_Bias;

            while (_rotate && !cancellationToken.IsCancellationRequested)
            {
                _angle += _rotateSpeed * Time.deltaTime;
                _angle %= 360f;

                _orbitalTransposer.m_Heading.m_Bias = _angle - 180f;

                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
            }
        }
    }
}