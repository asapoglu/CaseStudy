namespace Abdurrahman.Project_2.Core.Managers
{
    using Abdurrahman.Project_2.Core.Interfaces;
    using Abdurrahman.Project_2.Core.Models;
    using Abdurrahman.Project_2.Core.Signals;
    using UnityEngine;
    using Zenject;
    using Cysharp.Threading.Tasks;
    using DG.Tweening;

    public class PlayerController : MonoBehaviour, IPlayerController
    {
        [Inject] private SignalBus _signalBus;
        [Inject] private IGameStateManager _gameStateManager;

        [Header("Oyuncu Ayarları")]
        [SerializeField] private float _movementDuration = 0.5f;
        [SerializeField] private float _fallLag = 0.5f;

        private Transform _playerTransform;
        private Animator _animator;
        private LevelParameters _parameters;
        private float _playerSpeed;
        private bool _isRunning;
        private Rigidbody _rigidbody;

        private readonly int _runTrigger = Animator.StringToHash("Run");
        private readonly int _danceTrigger = Animator.StringToHash("Dance");
        private readonly int _failTrigger = Animator.StringToHash("Fail");
        private readonly int _idleTrigger = Animator.StringToHash("Idle");

        public Transform PlayerTransform => _playerTransform;

        private void Awake()
        {
            _playerTransform = transform;
            _animator = GetComponentInChildren<Animator>();
            _rigidbody = _playerTransform.GetComponent<Rigidbody>();
        }

        [Inject]
        private void Initialize()
        {
            _signalBus.Subscribe<GameStartSignal>(OnGameStart);
            _signalBus.Subscribe<LevelReadySignal>(OnLevelReady);
            _signalBus.Subscribe<PathChangedSignal>(OnPathChange);
            _signalBus.Subscribe<RestartLevelSignal>(OnGameReplay);
        }

        private void OnDestroy()
        {
            _signalBus.TryUnsubscribe<GameStartSignal>(OnGameStart);
            _signalBus.TryUnsubscribe<LevelReadySignal>(OnLevelReady);
            _signalBus.TryUnsubscribe<PathChangedSignal>(OnPathChange);
            _signalBus.TryUnsubscribe<RestartLevelSignal>(OnGameReplay);
        }

        public void ResetPlayerPosition()
        {
            _playerTransform.DOKill();
            _playerTransform.localPosition = Vector3.zero;

            if (_rigidbody != null)
            {
                _rigidbody.drag = 0;
                _rigidbody.velocity = Vector3.zero;
                _rigidbody.angularVelocity = Vector3.zero;
                _rigidbody.isKinematic = true;
                _rigidbody.useGravity = false;
            }

            SetPlayerAnimation("Idle");
            _isRunning = false;
        }

        public void OnLevelReady(LevelReadySignal signal)
        {
            _parameters = signal.Parameters;
            ResetPlayerPosition();
        }

        public void OnPathChange(PathChangedSignal signal)
        {
            _playerTransform.DOLocalMoveX(signal.XPosition, _movementDuration)
                .SetEase(Ease.OutQuad);
        }

        private void OnGameReplay() => ResetPlayerPosition();

        private void OnGameStart()
        {
            DOVirtual.DelayedCall(_parameters.Speed / 2f, StartMoving);
        }

        public void StartMoving()
        {
            var finalPosition = _parameters.TargetPosition;
            var pieceSpeed = _parameters.Length / _parameters.Speed;
            _playerSpeed = finalPosition / pieceSpeed;

            _playerTransform.DOLocalMoveZ(finalPosition, _playerSpeed)
                .SetEase(Ease.Linear).AwaitForComplete().ContinueWith(MoveComplete).Forget();

            SetPlayerAnimation("Run");
            _isRunning = true;
            GroundCheckAsync().Forget();
        }

        private async UniTaskVoid MoveComplete()
        {
            _isRunning = false;
            SetPlayerAnimation("Dance");
            await UniTask.Delay(1500);
            _gameStateManager.EndGame(true);
        }

        private async UniTaskVoid FailJump()
        {
            if (!_isRunning) return;

            _isRunning = false;
            _playerTransform.DOKill();

            if (_rigidbody != null)
            {
                _rigidbody.isKinematic = false;
                _rigidbody.useGravity = true;
            }

            SetPlayerAnimation("Fail");
            await UniTask.Delay(1500);
            _gameStateManager.EndGame(false);
            _rigidbody.drag = 10;
        }

        public void SetPlayerAnimation(string animationName)
        {
            switch (animationName)
            {
                case "Run":
                    _animator.ResetTrigger(_idleTrigger);
                    _animator.SetTrigger(_runTrigger);
                    break;
                case "Dance":
                    _animator.SetTrigger(_danceTrigger);
                    _animator.ResetTrigger(_runTrigger);
                    break;
                case "Fail":
                    _animator.SetTrigger(_failTrigger);
                    break;
                case "Idle":
                default:
                    _animator.SetTrigger(_idleTrigger);
                    _animator.ResetTrigger(_danceTrigger);
                    _animator.ResetTrigger(_failTrigger);
                    break;
            }
        }

        private async UniTaskVoid GroundCheckAsync()
        {
            while (_isRunning)
            {
                Vector3 raycastOrigin = _playerTransform.position + Vector3.up * _parameters.Height / 2f;
                if (!Physics.Raycast(raycastOrigin, Vector3.down, _parameters.Height))
                {
                    await UniTask.Delay((int)(_fallLag * 1000));
                    _playerTransform.DOKill();
                    FailJump().Forget();
                    return;
                }

                await UniTask.Delay(100);
            }
        }

    }
}