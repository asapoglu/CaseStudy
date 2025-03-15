// Platform Yöneticisi
namespace Abdurrahman.Project_2.Core.Managers
{
    using Abdurrahman.Project_2.Core.Interfaces;
    using Abdurrahman.Project_2.Core.Models;
    using Abdurrahman.Project_2.Core.Signals;
    using UnityEngine;
    using Zenject;
    using System.Collections.Generic;
    using DG.Tweening;

    public class PlatformManager : MonoBehaviour, IPlatformManager
    {
        [Inject] private SignalBus _signalBus;
        [Inject] private IInputManager _inputManager;

        [Header("Platform Bileşenleri")]
        [SerializeField] private Transform _stackParent;
        [SerializeField] private Transform _previousStackParent;
        [SerializeField] private GameObject _platformPrefab;
        [SerializeField] private GameObject _finishLinePrefab;

        [Header("Hareket Ayarları")]
        [SerializeField] private AnimationCurve _pieceMovementCurve;
        [SerializeField] private float _cutPieceDestroyDelay = 2f;

        private LevelParameters _parameters;
        private Transform _currentPiece;
        private Transform _previousPiece;
        private bool _inputEnabled;
        private Tween _currentTween;
        private GameObject _finishLine;
        private float _finishLength;
        private int _pieceCount;
        private float _spawnDistance;
        private float _completedPlatformsOffset = 0f; // Tamamlanan platformların Z pozisyon değeri


        private void Awake()
        {
            // Bitiş çizgisi uzunluğunu hesapla
            if (_finishLinePrefab != null && _finishLinePrefab.TryGetComponent<MeshRenderer>(out var renderer))
            {
                _finishLength = renderer.bounds.extents.z;
            }
        }

        [Inject]
        private void Initialize()
        {
            // Sinyallere abone ol
            _signalBus.Subscribe<LevelReadySignal>(OnLevelReady);

            _signalBus.Subscribe<GameStartSignal>(OnGameStart);
            _signalBus.Subscribe<RestartLevelSignal>(OnGameReplay);
            _signalBus.Subscribe<GameFailSignal>(OnGameFail);
            _signalBus.Subscribe<NextLevelSignal>(OnContinue); // Next Level işlemi için
        }

        private void OnDestroy()
        {
            // Sinyallerden çık
            _signalBus.TryUnsubscribe<LevelReadySignal>(OnLevelReady);
            _signalBus.TryUnsubscribe<GameStartSignal>(OnGameStart);
            _signalBus.TryUnsubscribe<RestartLevelSignal>(OnGameReplay);
            _signalBus.TryUnsubscribe<GameFailSignal>(OnGameFail);
            _signalBus.TryUnsubscribe<NextLevelSignal>(OnContinue);
        }

        private void Update()
        {
            // Girdi kontrolü
            if (!_inputEnabled) return;

            if (_inputManager.IsInputReceived())
            {
                HandleInput();
            }
        }
        private void OnContinue()
        {

            // Mevcut tamamlanmış platformları tut
            KeepCompletedPlatforms();

            // Yeni seviye için oyun alanını hazırla
            PrepareNewLevelPlatforms();
        }
        private void KeepCompletedPlatforms()
        {

            // Mevcut oyun alanını tut ve Z pozisyonunu kaydet
            if (_stackParent.childCount > 0)
            {
                // Eğer bitiş çizgisi varsa kaldır (yeni seviyede tekrar oluşturulacak)
                if (_finishLine != null)
                {
                    Destroy(_finishLine);
                }

                // Son platformun pozisyonunu al
                Transform lastPlatform = null;
                float maxZ = float.MinValue;

                for (int i = 0; i < _stackParent.childCount; i++)
                {
                    Transform platform = _stackParent.GetChild(i);

                    // Bitiş çizgisi dışındaki platformlarda son platformu bul
                    if (platform.name != "FinishLine(Clone)" && platform.localPosition.z > maxZ)
                    {
                        maxZ = platform.localPosition.z;
                        lastPlatform = platform;
                    }
                }

                if (lastPlatform != null)
                {
                    // Son platformun uzunluğunu hesaba katarak bir sonraki platformun başlangıç pozisyonunu belirle
                    float platformLength = lastPlatform.localScale.z;
                    _completedPlatformsOffset = maxZ + platformLength;

                }
            }
        }
        private void PrepareNewLevelPlatforms()
        {

            // Hiçbir mevcut platformu temizlemeyin - onları koruyun
            // Sadece potansiyel olarak aktif Tween'leri temizleyin
            if (_currentTween != null && _currentTween.IsActive())
            {
                _currentTween.Kill();
                _currentTween = null;
            }

            // Parça sayacını sıfırla
            _pieceCount = 0;
        }
        private void OnLevelReady(LevelReadySignal signal)
        {
            _pieceCount = 0;
            _parameters = signal.Parameters;
            _parameters.TargetPosition = _parameters.TargetPosition + _finishLength;
            if (_parameters.KeepCompletedPlatforms)
            {
                KeepPreviousPlatforms();
                _currentPiece = CreateStartingPiece();
                CreateFinishLineWithOffset();
                Debug.Log("KeepCompletedPlatforms");
            }
            else
            {
                _currentPiece = CreateStartingPiece();
                CreateFinishLine();
                Debug.Log("DontKeepCompletedPlatforms");
            }
        }

        private void KeepPreviousPlatforms()
        {
            int childCount = _previousStackParent.childCount;
            if (childCount > 0)
            {   
                for (int i = childCount - 1; i >= 0; i--)
                {
                    Destroy(_previousStackParent.GetChild(i).gameObject);
                }
                _previousStackParent.localPosition = Vector3.zero;
            }

            childCount = _stackParent.childCount;

            if (childCount == 0) return;

            // Döngü için geçici değişkeni dışarda tanımla (GC optimize)
            Transform child;

            // Doğrudan taşı - liste kullanmadan
            // Not: Geriye doğru iterasyon yapıyoruz, böylece çocuklar taşındıkça indeks problemi olmaz
            for (int i = childCount - 1; i >= 0; i--)
            {
                child = _stackParent.GetChild(i);
                child.SetParent(_previousStackParent, true);
            }

            // Arkaplan parçalarını uygun pozisyona taşı
            var previousPos = _previousStackParent.localPosition;
            var gap = _finishLength + _parameters.Length / 2f;
            previousPos.z -= _parameters.TargetPosition + gap;
            _previousStackParent.localPosition = previousPos;
        }

        private void CreateFinishLineWithOffset()
        {

            // Bitiş çizgisini tamamlanan platformların sonundan itibaren oluştur
            float finishPosition = _completedPlatformsOffset + _parameters.TargetPosition;

            _finishLine = Instantiate(_finishLinePrefab, new Vector3(0, 0, finishPosition),
                Quaternion.identity, _stackParent);
        }
        private void MoveCompletedPlatformsToBackground()
        {
            // Tamamlanan parçaları arkaplanda göster
            for (int i = _stackParent.childCount - 1; i >= 0; i--)
            {
                _stackParent.GetChild(i).SetParent(_previousStackParent, true);
            }

            // Arkaplan parçalarını uygun pozisyona taşı
            var previousPos = _previousStackParent.localPosition;
            var gap = _finishLength + _parameters.Length / 2f;
            previousPos.z -= _parameters.TargetPosition + gap;
            _previousStackParent.localPosition = previousPos;
        }

        private void CreateFinishLine()
        {
            // Bitiş çizgisini oluştur
            _finishLine = Instantiate(_finishLinePrefab, new Vector3(0, 0, _parameters.TargetPosition),
                Quaternion.identity, _stackParent);
        }


        private void OnGameReplay()
        {

            // Girdiyi devre dışı bırak
            _inputEnabled = false;

            // Tüm aktif Tween işlemlerini durdur
            if (_currentTween != null && _currentTween.IsActive())
            {
                _currentTween.Kill();
                _currentTween = null;
            }

            // SADECE MEVCUT SEVİYE için eklenen platformları temizle
            // Önceki seviyelerin platformlarını KORUMALISINIZ
            ClearCurrentLevelPlatforms();

            // Son tamamlanan seviyeden başlayarak yeniden başlat
            _currentPiece = CreateStartingPiece();
            CreateFinishLineWithOffset();

            // Parça sayacını sıfırla
            _pieceCount = 0;
        }

        private void ClearCurrentLevelPlatforms()
        {

            // Tüm aktif DOTween işlemlerini durdur
            DOTween.Kill(_stackParent);

            List<Transform> platformsToRemove = new List<Transform>();

            // Mevcut seviyeye ait platformları bul (offset'ten sonra olanlar)
            for (int i = 0; i < _stackParent.childCount; i++)
            {
                var child = _stackParent.GetChild(i);

                // Eğer platform offset'ten sonra oluşturulduysa (mevcut seviyeye aitse)
                if (child.localPosition.z >= _completedPlatformsOffset - 0.1f) // Küçük bir tolerans ekle
                {
                    platformsToRemove.Add(child);
                }
            }

            // Bulunan platformları sil
            foreach (var platform in platformsToRemove)
            {
                Destroy(platform.gameObject);
            }
        }
        private void ClearAddedPlatforms()
        {

            // Tüm aktif DOTween işlemlerini durdur
            DOTween.Kill(_stackParent);

            // İlk platform (başlangıç platformu) ve bitiş çizgisini koru, diğerlerini sil
            for (int i = _stackParent.childCount - 1; i > 1; i--)
            {
                var child = _stackParent.GetChild(i);

                // Eğer bu bitiş çizgisi değilse sil
                if (child.name != "FinishLine(Clone)")
                {
                    Destroy(child.gameObject);
                }
            }

            // Ayrıca düşen parçaları da temizle
            for (int i = 0; i < _stackParent.childCount; i++)
            {
                var child = _stackParent.GetChild(i);
                Rigidbody childRb = child.GetComponent<Rigidbody>();

                if (childRb != null && !childRb.isKinematic)
                {
                    Destroy(child.gameObject);
                }
            }
        }


        private void OnGameFail()
        {
            // Oyun başarısız olduğunda girdiyi devre dışı bırak ve platformu durdur
            _inputEnabled = false;
            _currentPiece.DOKill();
        }

        private void OnGameStart()
        {
            Debug.Log(this.GetType().Name + " - OnGameStart");
            // Oyun başladığında girdiyi etkinleştir ve mesafeyi belirle
            _inputEnabled = true;
            _spawnDistance = _parameters.Width;
            CreateNewPlatform();
        }

        private Transform CreateStartingPiece()
        {

            var go = Instantiate(_platformPrefab, _stackParent).transform;
            go.localPosition = new Vector3(0, _parameters.Height / -2f, 0);
            go.localScale = new Vector3(_parameters.Width, _parameters.Height, _parameters.Length);

            // Platform rengini ayarla
            if (go.GetChild(0).TryGetComponent<MeshRenderer>(out var renderer))
            {
                renderer.material.color = GetRandomColor();
            }

            return go;
        }

        public void CreatePlatforms()
        {
            // İmplementasyon gereksinimlere bağlı olarak doldurulacak
        }

        public void CreateNewPlatform()
        {
            // Parça sayısını artır ve yeni platform oluştur
            _pieceCount++;

            var piece = Instantiate(_currentPiece, _stackParent).transform;
            piece.name = "p" + _pieceCount;

            // Rastgele yön belirle (sağ veya sol)
            var direction = Random.value > 0.5f ? 1 : -1;

            // Pozisyon ve ölçek hesaplamaları
            var currentPos = _currentPiece.localPosition;
            var currentScale = _currentPiece.localScale;
            var horizontalPosition = (_spawnDistance + currentScale.x / 2f) * direction;
            var startingPosition = new Vector3(horizontalPosition, currentPos.y, currentPos.z + currentScale.z);

            // Yeni platformu başlangıç pozisyonuna yerleştir
            piece.localPosition = startingPosition;

            // Platform rengini ayarla
            if (piece.GetChild(0).TryGetComponent<MeshRenderer>(out var renderer))
            {
                renderer.material.color = GetRandomColor();
            }

            // Platformu hareket ettir
            _currentTween = piece.DOLocalMoveX(-startingPosition.x, _parameters.Speed)
                .SetEase(_pieceMovementCurve);

            // Referansları güncelle
            _previousPiece = _currentPiece;
            _currentPiece = piece;
        }

        public void PlacePlatform()
        {
            // Bu metod dış çağrılardan ziyade HandleInput tarafından kullanılacak
        }

        public void ClearAllPlatforms()
        {

            // Tüm aktif DOTween işlemlerini durdur
            DOTween.Kill(_stackParent);
            DOTween.Kill(_previousStackParent);

            // Tüm platformları temizle
            for (int i = _stackParent.childCount - 1; i >= 0; i--)
            {
                Destroy(_stackParent.GetChild(i).gameObject);
            }

            for (int i = _previousStackParent.childCount - 1; i >= 0; i--)
            {
                Destroy(_previousStackParent.GetChild(i).gameObject);
            }
        }

        private void HandleInput()
        {
            // Girdiyi işle ve platformu yerleştir
            _inputEnabled = false;
            float remainingTime = 0f;

            // Eğer platform hareket halindeyse durdur
            if (_currentTween != null && _currentTween.IsActive())
            {
                remainingTime = _currentTween.Duration() - _currentTween.Elapsed();
                _currentTween.Kill();
            }

            // Pozisyon ve ölçek hesaplamaları
            var currentPosition = _currentPiece.localPosition;
            var currentScale = _currentPiece.localScale;
            var prevPosition = _previousPiece.localPosition;
            var widthDifference = currentPosition.x - prevPosition.x;
            var absoluteWidthDifference = Mathf.Abs(widthDifference);

            if (absoluteWidthDifference < _parameters.ToleranceWidth) // Mükemmel yerleştirme
            {
                PlacePerfectPiece(currentPosition, prevPosition, remainingTime);
            }
            else if (absoluteWidthDifference > currentScale.x) // Sınırların dışına çıkan yerleştirme
            {
                // Şu an için işlem yapılmıyor
            }
            else // Kısmen örtüşen yerleştirme
            {
                PlaceCutPiece(currentScale, absoluteWidthDifference, currentPosition, widthDifference, remainingTime);
            }
        }

        private void PlacePerfectPiece(Vector3 currentPosition, Vector3 prevPosition, float remainingTime)
        {
            // Mükemmel yerleştirme için platformun pozisyonunu düzelt
            currentPosition.x = prevPosition.x;
            _currentPiece.localPosition = currentPosition;

            // Belirlenen süre sonra yeni platform oluştur
            DOVirtual.DelayedCall(remainingTime, OnPiecePlaced);

            // Mükemmel yerleştirme sinyali gönder
            _signalBus.Fire(new PiecePlacedSignal(PlacementResult.Perfect));
        }

        private void PlaceCutPiece(Vector3 currentScale, float absWidthDifference, Vector3 currentPosition,
                                  float widthDifference, float remainingTime)
        {
            // Kısmen örtüşen yerleştirme için platform kesme işlemi

            // Kesme yönünü belirle (sağ veya sol)
            var placementResult = widthDifference < 0 ? PlacementResult.LeftCut : PlacementResult.RightCut;
            _signalBus.Fire(new PiecePlacedSignal(placementResult));

            // Kesilen platformun boyutunu hesapla
            var cutWidth = currentScale.x - absWidthDifference;
            _currentPiece.localScale = new Vector3(cutWidth, currentScale.y, currentScale.z);

            // Kesilen platformun pozisyonunu düzelt
            var centeredPositionX = currentPosition.x - widthDifference / 2f;
            _currentPiece.localPosition = new Vector3(centeredPositionX, currentPosition.y, currentPosition.z);

            // Yol değişikliği sinyali gönder
            _signalBus.Fire(new PathChangedSignal(centeredPositionX));

            // Düşen parçayı oluştur
            CreateFallingPiece(currentScale, absWidthDifference, widthDifference, currentPosition, centeredPositionX, cutWidth);

            // Belirlenen süre sonra yeni platform oluştur
            DOVirtual.DelayedCall(remainingTime, OnPiecePlaced);
        }

        private void CreateFallingPiece(Vector3 currentScale, float absWidthDifference, float widthDifference,
                                       Vector3 currentPosition, float centeredPositionX, float cutWidth)
        {
            // Düşen parçayı oluştur
            var cutPiece = Instantiate(_currentPiece, _stackParent);

            // Düşen parçanın boyutunu ayarla
            var cutScale = cutPiece.localScale;
            cutScale.x = absWidthDifference;
            var cutDirection = Mathf.Sign(widthDifference);
            cutPiece.localScale = cutScale;

            // Düşen parçanın pozisyonunu hesapla
            var cutCenteredPositionX = centeredPositionX + cutDirection * ((cutWidth + cutScale.x) / 2f);
            cutPiece.localPosition = new Vector3(cutCenteredPositionX, currentPosition.y, currentPosition.z);

            // Düşen parçaya fizik ekle
            if (cutPiece.TryGetComponent<Rigidbody>(out var body))
            {
                body.useGravity = true;
                body.isKinematic = false;
                body.AddForce(new Vector3(cutDirection * _parameters.Length, 0, 0), ForceMode.Impulse);
            }

            // Düşen parçayı belirli süre sonra yok et
            Destroy(cutPiece.gameObject, _cutPieceDestroyDelay);
        }
        public void ResetGameCompletely()
        {

            // Tüm platformları temizle
            ClearAllPlatforms();

            // Offset'i sıfırla
            _completedPlatformsOffset = 0f;

            // Yeni başlangıç platformu oluştur
            _currentPiece = CreateStartingPiece();
            CreateFinishLine();

            // Parça sayacını sıfırla
            _pieceCount = 0;
        }
        private void OnPiecePlaced()
        {
            // Eğer hedef parça sayısına ulaşıldıysa oyunu bitir
            if (_pieceCount >= _parameters.PieceCount)
            {
                return;
            }

            // Hedef parça sayısına ulaşılmadıysa yeni platform oluştur
            CreateNewPlatform();
            _inputEnabled = true;
        }

        private Color GetRandomColor()
        {
            // Basit renk oluşturma (gerçek implementasyonda gradient kullanılabilir)
            return new Color(
                Random.Range(0.2f, 0.8f),
                Random.Range(0.2f, 0.8f),
                Random.Range(0.5f, 1.0f)
            );
        }
    }
}