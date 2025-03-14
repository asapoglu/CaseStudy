// Oyuncu Kontrolcüsü - Düzeltilmiş
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


        [Header("Oyuncu Ayarları")]
        [SerializeField] private float _movementDuration = 0.5f; // Yanal hareket süresi
        [SerializeField] private float _fallLag = 0.5f; // Havada kalma süresi

        private Transform _playerTransform; // Ana oyuncu transform'u
        private Animator _animator;
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

        public Transform PlayerTransform => _playerTransform;

        private void Awake()
        {
            _playerTransform = transform;
            _animator = GetComponentInChildren<Animator>();
            // Rigidbody bileşenini al (ana karakter nesnesinde olmalı)
            _rigidbody = _playerTransform.GetComponent<Rigidbody>();
            if (_rigidbody == null)
            {
                _rigidbody = _playerTransform.gameObject.AddComponent<Rigidbody>();
                _rigidbody.isKinematic = true;
                _rigidbody.useGravity = false;
            }

            // Yer kontrolü için bekleme süresini oluştur
            _waitForGroundCheckInterval = new WaitForSeconds(.1f);
        }

        [Inject]
        private void Initialize()
        {
            // Mevcut sinyaller
            _signalBus.Subscribe<GameStartSignal>(OnGameStart);
            _signalBus.Subscribe<LevelReadySignal>(OnLevelReady);
            _signalBus.Subscribe<PathChangedSignal>(OnPathChange);
            _signalBus.Subscribe<ReplaySignal>(OnGameReplay);
        }

        private void OnDestroy()
        {
            // Mevcut sinyal aboneliklerinin çıkışı
            _signalBus.TryUnsubscribe<GameStartSignal>(OnGameStart);
            _signalBus.TryUnsubscribe<LevelReadySignal>(OnLevelReady);
            _signalBus.TryUnsubscribe<PathChangedSignal>(OnPathChange);
            _signalBus.TryUnsubscribe<ReplaySignal>(OnGameReplay);
        }

        public void ResetPlayerPosition()
        {
            Debug.Log("PlayerController - Oyuncu pozisyonu sıfırlanıyor");

            // Tüm Tween işlemlerini durdur
            _playerTransform.DOKill();

            // Oyuncuyu başlangıç pozisyonuna sıfırla
            _playerTransform.localPosition = Vector3.zero;

            // Fiziği sıfırla
            if (_rigidbody != null)
            {
                _rigidbody.velocity = Vector3.zero;
                _rigidbody.angularVelocity = Vector3.zero;
                _rigidbody.isKinematic = true;
                _rigidbody.useGravity = false;
            }

            // Animasyonları sıfırla
            SetPlayerAnimation("Idle");

            // Koşma durumunu devre dışı bırak
            _isRunning = false;

            // Tüm coroutine'leri durdur
            StopAllCoroutines();
        }

        private void OnLevelReady(LevelReadySignal signal)
        {
            Debug.Log("PlayerController - Seviye hazır sinyali alındı");

            // Seviye parametrelerini kaydet
            _parameters = signal.Parameters;

            // Oyuncuyu sıfırla
            ResetPlayerPosition();
        }


        private void OnPathChange(PathChangedSignal signal)
        {
            // Karakteri platformun merkezine doğru hareket ettir
            _playerTransform.DOLocalMoveX(signal.XPosition, _movementDuration)
                .SetEase(Ease.OutQuad);
        }

        private void OnGameReplay()
        {
            Debug.Log("PlayerController - Oyun yeniden başlatılıyor");

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
            _playerTransform.DOLocalMoveZ(finalPosition, _playerSpeed)
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
            if (!_isRunning)
            {
                Debug.Log("FailJump çağrıldı fakat oyuncu zaten koşmuyor");
                return;
            }

            Debug.Log("FailJump başlatıldı");

            // Tüm coroutine'leri durdur ve koşmayı devre dışı bırak
            StopAllCoroutines();
            _isRunning = false;

            // DOTween animasyonlarını durdur
            _playerTransform.DOKill();

            // Fiziği etkinleştir ve başarısız animasyonunu başlat
            if (_rigidbody != null)
            {
                _rigidbody.isKinematic = false;
                _rigidbody.useGravity = true;
            }

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
            _playerTransform.DOLocalMoveZ(targetPosition, _playerSpeed)
                .SetEase(Ease.Linear)
                .OnComplete(MoveComplete);
        }

        public void StopPlayer()
        {
            // Oyuncuyu durdur
            _playerTransform.DOKill();
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
                // Karakterin doğru pozisyonundan ışın gönder
                Vector3 raycastOrigin = _playerTransform.position + Vector3.up * _parameters.Height / 2f;
                Debug.DrawRay(raycastOrigin, Vector3.down * _parameters.Height, Color.red); // Görsel hata ayıklama

                // Zemin kontrolü
                if (!Physics.Raycast(raycastOrigin, Vector3.down, _parameters.Height))
                {
                    // Zemin bulunamazsa düşüşü başlat
                    yield return new WaitForSeconds(_fallLag);
                    _playerTransform.DOKill();
                    FailJump();
                    yield break; // Coroutine'i sonlandır
                }

                // Sonraki kontrole kadar bekle
                yield return _waitForGroundCheckInterval;
            }
        }
    }
}