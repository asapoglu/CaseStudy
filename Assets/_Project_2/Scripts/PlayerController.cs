// Oyuncu Kontrolcüsü
namespace Abdurrahman.Project_2.Core.Managers
{
    using Abdurrahman.Project_2.Core.Interfaces;
    using Abdurrahman.Project_2.Core.Models;
    using Abdurrahman.Project_2.Core.Signals;
    using UnityEngine;
    using Zenject;
    using System.Collections;
    using DG.Tweening;
    
    public class PlayerController : MonoBehaviour, IPlayerController
    {
        [Inject] private SignalBus _signalBus;
        [Inject] private IGameStateManager _gameStateManager;
        
        [Header("Oyuncu Bileşenleri")]
        [SerializeField] private Transform _mover;
        [SerializeField] private Transform _container;
        [SerializeField] private Animator _animator;
        
        [Header("Oyuncu Ayarları")]
        [SerializeField] private bool _adaptChanges = true;
        [SerializeField] private float _groundCheckInterval = 0.1f;
        
        private LevelParameters _parameters;
        private float _playerSpeed;
        private WaitForSeconds _waitForGroundCheckInterval;
        private bool _isRunning;
        private Rigidbody _rigidbody;
        
        // Animation parameter hash değerleri
        private readonly int _runTrigger = Animator.StringToHash("Run");
        private readonly int _danceTrigger = Animator.StringToHash("Dance");
        private readonly int _failTrigger = Animator.StringToHash("Fail");
        private readonly int _idleTrigger = Animator.StringToHash("Idle");
        
        public Transform PlayerTransform => _container;
        
        private void Awake()
        {
            // Rigidbody bileşenini al
            _rigidbody = _container.GetComponent<Rigidbody>();
            if (_rigidbody == null)
            {
                _rigidbody = _container.gameObject.AddComponent<Rigidbody>();
                _rigidbody.isKinematic = true;
                _rigidbody.useGravity = false;
            }
            
            // Yer kontrolü için bekleme süresini oluştur
            _waitForGroundCheckInterval = new WaitForSeconds(_groundCheckInterval);
        }
        
        [Inject]
        private void Initialize()
        {
            // Sinyallere abone ol
            _signalBus.Subscribe<GameStartSignal>(OnGameStart);
            _signalBus.Subscribe<LevelReadySignal>(OnLevelReady);
            _signalBus.Subscribe<ContinueSignal>(OnNewLevelRequest);
            _signalBus.Subscribe<ReplaySignal>(OnGameReplay);
            _signalBus.Subscribe<PathChangedSignal>(OnPathChange);
        }
        
        private void OnDestroy()
        {
            // Sinyallerden çık
            _signalBus.TryUnsubscribe<GameStartSignal>(OnGameStart);
            _signalBus.TryUnsubscribe<LevelReadySignal>(OnLevelReady);
            _signalBus.TryUnsubscribe<ContinueSignal>(OnNewLevelRequest);
            _signalBus.TryUnsubscribe<ReplaySignal>(OnGameReplay);
            _signalBus.TryUnsubscribe<PathChangedSignal>(OnPathChange);
        }
        
        public void ResetPlayerPosition()
        {
            // Oyuncuyu başlangıç pozisyonuna sıfırla
            _mover.localPosition = Vector3.zero;
            _rigidbody.isKinematic = true;
            _rigidbody.useGravity = false;
            _rigidbody.transform.localPosition = Vector3.zero;
            
            // Animasyonları sıfırla
            SetPlayerAnimation("Idle");
            
            // Tüm hareketleri durdur
            _mover.DOKill();
        }
        
        private void OnLevelReady(LevelReadySignal signal)
        {
            // Seviye parametrelerini kaydet
            _parameters = signal.Parameters;
            
            // Yer kontrolü aralığını ayarla
            if (_parameters.Speed < _groundCheckInterval)
            {
                _groundCheckInterval = _parameters.Speed / 2f;
                _waitForGroundCheckInterval = new WaitForSeconds(_groundCheckInterval);
            }
            
            // Oyuncuyu sıfırla
            ResetPlayerPosition();
        }
        
        private void OnPathChange(PathChangedSignal signal)
        {
            // Eğer değişikliklere uyum sağlanıyorsa oyuncuyu yeni yola ayarla
            if (!_adaptChanges) return;
            
            _mover.DOLocalMoveX(signal.XPosition, _parameters.Speed / 2f);
        }
        
        private void OnGameReplay()
        {
            // Oyun yeniden başladığında oyuncuyu sıfırla
            ResetPlayerPosition();
        }
        
        private void OnNewLevelRequest()
        {
            // Yeni seviye talebi geldiğinde oyuncuyu sıfırla
            ResetPlayerPosition();
        }
        
        private void OnGameStart()
        {
            // Oyun başladığında kısa bir gecikmeyle hareketi başlat
            DOVirtual.DelayedCall(_parameters.Speed / 2f, StartMoving);
        }
        
        private void StartMoving()
        {
            // Hedef pozisyonu ve hızı hesapla
            var finalPosition = _parameters.TargetPosition;
            var pieceSpeed = _parameters.Length / _parameters.Speed;
            _playerSpeed = finalPosition / pieceSpeed;
            
            // Oyuncuyu hedefe doğru hareket ettir
            _mover.DOLocalMoveZ(finalPosition, _playerSpeed)
                .SetEase(Ease.Linear)
                .OnComplete(MoveComplete);
            
            // Koşma animasyonunu başlat
            SetPlayerAnimation("Run");
            
            // Koşma durumunu etkinleştir ve yer kontrolü coroutine'ini başlat
            _isRunning = true;
            StartCoroutine(GroundCheck());
        }
        
        private void MoveComplete()
        {
            // Hareket tamamlandığında koşmayı durdur ve dans animasyonunu başlat
            _isRunning = false;
            SetPlayerAnimation("Dance");
            
            // Oyunu başarılı olarak bitir
            _gameStateManager.EndGame(true);
        }
        
        private void FailJump()
        {
            // Eğer zaten koşmuyorsa işlem yapma
            if (!_isRunning) return;
            
            // Tüm coroutine'leri durdur ve koşmayı devre dışı bırak
            StopAllCoroutines();
            _isRunning = false;
            
            // Fiziği etkinleştir ve başarısız animasyonunu başlat
            _rigidbody.isKinematic = false;
            _rigidbody.useGravity = true;
            SetPlayerAnimation("Fail");
            
            // Düşüş hızını yavaşlat ve başarısız sinyalini gönder
            DOVirtual.Float(1, 7.5f, 1.5f, SlowDown)
                .SetDelay(1)
                .OnComplete(TriggerFailEvent);
        }
        
        private void SlowDown(float value)
        {
            // Düşüş hızını yavaşlat
            _rigidbody.drag = value;
        }
        
        private void TriggerFailEvent()
        {
            // Oyunu başarısız olarak bitir
            _gameStateManager.EndGame(false);
        }
        
        public void MovePlayer(float targetPosition)
        {
            // Oyuncuyu belirli bir pozisyona hareket ettir
            _mover.DOLocalMoveZ(targetPosition, _playerSpeed)
                .SetEase(Ease.Linear)
                .OnComplete(MoveComplete);
        }
        
        public void StopPlayer()
        {
            // Oyuncuyu durdur
            _mover.DOKill();
            _isRunning = false;
        }
        
        public void SetPlayerAnimation(string animationName)
        {
            // İstenilen animasyonu başlat
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
        
        IEnumerator GroundCheck()
        {
            // Oyuncu koşarken sürekli olarak yer kontrolü yap
            while (_isRunning)
            {
                // Zeminin altında bir ray ile kontrol et
                if (!Physics.Raycast(_container.position + Vector3.up * _parameters.Height / 2f, 
                                    Vector3.down, _parameters.Height))
                {
                    // Zemin bulunamazsa düşüşü başlat
                    _mover.DOKill();
                    FailJump();
                }
                
                // Sonraki kontrole kadar bekle
                yield return _waitForGroundCheckInterval;
            }
        }
    }
}