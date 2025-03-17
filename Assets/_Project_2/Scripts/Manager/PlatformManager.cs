// Platform Yöneticisi
namespace Abdurrahman.Project_2.Core.Managers
{
    using Abdurrahman.Project_2.Core.Models;
    using Abdurrahman.Project_2.Core.Signals;
    using UnityEngine;
    using Zenject;
    using System.Collections.Generic;
    using DG.Tweening;

    public class PlatformManager : MonoBehaviour
    {
        [Inject] private SignalBus _signalBus;
        [Inject] private InputManager _inputManager;
        [Inject] private ObjectPooler _objectPooler;

        [Header("Platform Bileşenleri")]
        [SerializeField] private Transform _stackParent;
        [SerializeField] private Transform _previousStackParent;

        [Header("Hareket Ayarları")]
        [SerializeField] private Ease _pieceMovementEase = Ease.InQuad;
        [SerializeField] private float _cutPieceDestroyDelay = 2f;

        [Header("Renk Ayarları")]
        [SerializeField] private List<Color> _gradientColors;


        private LevelParameters _parameters;
        private LevelParameters _exParameters;
        private Transform _currentPiece;
        private Transform _previousPiece;
        private bool _inputEnabled;
        private Tween _currentTween;
        private float _finishLength;
        private int _pieceCount;
        private float _spawnDistance;
        private float _completedPlatformsOffset = 0f; 
        private int _colorIndex;


        private void Awake()
        {
            // Bitiş çizgisi uzunluğunu hesapla
            var tempFinishLine = _objectPooler.GetPooledObject(1);
            if (tempFinishLine.TryGetComponent<MeshRenderer>(out var renderer))
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
        }

        private void OnDestroy()
        {
            // Sinyallerden çık
            _signalBus.TryUnsubscribe<LevelReadySignal>(OnLevelReady);
            _signalBus.TryUnsubscribe<GameStartSignal>(OnGameStart);
            _signalBus.TryUnsubscribe<RestartLevelSignal>(OnGameReplay);
            _signalBus.TryUnsubscribe<GameFailSignal>(OnGameFail);
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
        private void OnLevelReady(LevelReadySignal signal)//
        {
            _pieceCount = 0;
            _parameters = signal.Parameters;
            _parameters.TargetPosition += _finishLength;

            if (_parameters.IsNextLevel)
            {
                _exParameters = signal.ExParameters;
                KeepPreviousPlatforms();
            }

            _currentPiece = CreateStartingPiece();
            
            var finishLine = _objectPooler.GetPooledObject(1);
            finishLine.SetActive(true);
            finishLine.transform.SetParent(_stackParent);
            finishLine.transform.position = new Vector3(0, 0, _parameters.TargetPosition);
            finishLine.transform.rotation = Quaternion.identity;
        }

        private void KeepPreviousPlatforms()//
        {
            ClearPreviousStack();
            MoveCurrentStackToPrevious();
            UpdatePreviousStackPosition();
        }

        private void ClearPreviousStack()//
        {
            int count = _previousStackParent.childCount;
            for (int i = count - 1; i >= 0; i--)
            {
                Transform child = _previousStackParent.GetChild(i);
                child.SetParent(_objectPooler.spawnTransform);
                child.gameObject.SetActive(false);
            }
            _previousStackParent.localPosition = Vector3.zero;
        }

        private void MoveCurrentStackToPrevious()//
        {
            int childCount = _stackParent.childCount;
            if (childCount == 0) return;

            for (int i = childCount - 1; i >= 0; i--)
            {
                Transform child = _stackParent.GetChild(i);
                child.SetParent(_previousStackParent, true);
            }
        }

        private void UpdatePreviousStackPosition()//
        {
            float gap = _finishLength + _exParameters.Length / 2f;
            _previousStackParent.localPosition = new Vector3(
                _previousStackParent.localPosition.x,
                _previousStackParent.localPosition.y,
                -(_exParameters.TargetPosition + gap)
            );
        }

        private void OnGameReplay()
        {
            _inputEnabled = false;

            if (_currentTween != null && _currentTween.IsActive())
            {
                _currentTween.Kill();
                _currentTween = null;
            }
            ClearCurrentLevelPlatforms();
            _pieceCount = 0;
        }

        private void ClearCurrentLevelPlatforms()
        {
            Debug.Log("ClearCurrentLevelPlatforms");
            DOTween.Kill(_stackParent);

            List<Transform> platformsToRemove = new List<Transform>();

            for (int i = 0; i < _stackParent.childCount; i++)
            {
                var child = _stackParent.GetChild(i);

                // Eğer platform offset'ten sonra oluşturulduysa (mevcut seviyeye aitse)
                if (child.localPosition.z >= _completedPlatformsOffset - 0.1f) // Küçük bir tolerans ekle
                {
                    platformsToRemove.Add(child);
                }
            }

            // Bulunan platformları poola geri koy
            foreach (var platform in platformsToRemove)
            {
                platform.gameObject.SetActive(false);
                platform.position = Vector3.zero;
                platform.SetParent(_objectPooler.spawnTransform);
            }
        }


        private void OnGameFail()
        {
            // Oyun başarısız olduğunda girdiyi devre dışı bırak ve platformu durdur
            _inputEnabled = false;
            _currentPiece.DOKill();
        }

        private void OnGameStart() //
        {
            _inputEnabled = true;
            _spawnDistance = _parameters.Width;
            CreatePlatform();
        }

        private Transform CreateStartingPiece()
        {
            var platform = _objectPooler.GetPooledObject(0);
            platform.SetActive(true);
            var go = platform.transform;
            go.SetParent(_stackParent);
            go.localPosition = new Vector3(0, _parameters.Height / -2f, 0);
            go.localScale = new Vector3(_parameters.Width, _parameters.Height, _parameters.Length);

            if (go.GetChild(0).TryGetComponent<MeshRenderer>(out var renderer))
            {
                renderer.material.color = GetNextPlatformColor();
            }

            return go;
        }
        public void CreatePlatform()
        {
            _pieceCount++;
            Transform newPiece = InstantiateNewPiece();
            SetPiecePosition(newPiece);
            SetPieceColor(newPiece);
            MovePiece(newPiece);
            UpdatePieceReferences(newPiece);
        }

        private Transform InstantiateNewPiece()//
        {
            var platformObj = _objectPooler.GetPooledObject(0);
            platformObj.SetActive(true);
            var piece = platformObj.transform;
            piece.SetParent(_stackParent);
            piece.localScale = _currentPiece.localScale;
            return piece;
        }

        private void SetPiecePosition(Transform piece)//
        {
            var direction = GetRandomDirection();
            var horizontalPosition = CalculateHorizontalPosition(direction);

            var currentPos = _currentPiece.localPosition;
            var currentScale = _currentPiece.localScale;

            piece.localPosition = new Vector3(
                horizontalPosition,
                currentPos.y,
                currentPos.z + currentScale.z
            );
        }

        private int GetRandomDirection()//
        {
            return Random.value > 0.5f ? 1 : -1;
        }

        private float CalculateHorizontalPosition(int direction)//
        {
            return (_spawnDistance + _currentPiece.localScale.x / 2f) * direction;
        }

        private void SetPieceColor(Transform piece)//
        {
            if (piece.GetChild(0).TryGetComponent<MeshRenderer>(out var renderer))
            {
                renderer.material.color = GetNextPlatformColor();
            }
        }

        private void MovePiece(Transform piece)//
        {
            float targetX = -piece.localPosition.x;

            _currentTween = piece.DOLocalMoveX(targetX, _parameters.Speed)
                .SetEase(_pieceMovementEase);
        }

        private void UpdatePieceReferences(Transform newPiece)//
        {
            _previousPiece = _currentPiece;
            _currentPiece = newPiece;
        }

        public Color GetNextPlatformColor() //
        {
            Color color = _gradientColors[_colorIndex % _gradientColors.Count];
            _colorIndex++;
            return color;
        }

        private void HandleInput()
        {
            _inputEnabled = false;
            float remainingTime = (_currentTween != null && _currentTween.IsActive())
                ? _currentTween.Duration() - _currentTween.Elapsed()
                : 0f;

            _currentTween?.Kill();

            Vector3 currentPosition = _currentPiece.localPosition;
            Vector3 currentScale = _currentPiece.localScale;
            Vector3 prevPosition = _previousPiece.localPosition;

            float widthDifference = currentPosition.x - prevPosition.x;
            float absoluteWidthDifference = Mathf.Abs(widthDifference);

            if (absoluteWidthDifference < _parameters.ToleranceWidth)
            {
                PlacePerfectPiece(currentPosition, prevPosition, remainingTime);
            }
            else if (absoluteWidthDifference <= currentScale.x)
            {
                PlaceCutPiece(currentScale, absoluteWidthDifference, currentPosition, widthDifference, remainingTime);
            }
            else
            {
                DOVirtual.DelayedCall(remainingTime, OnPiecePlaced);
            }
        }

        private void PlacePerfectPiece(Vector3 currentPosition, Vector3 prevPosition, float remainingTime)
        {
            currentPosition.x = prevPosition.x;
            _currentPiece.localPosition = currentPosition;

            _signalBus.Fire(new PiecePlacedSignal(PlacementResult.Perfect));
            DOVirtual.DelayedCall(remainingTime, OnPiecePlaced);
        }

        private void PlaceCutPiece(Vector3 currentScale, float absWidthDifference, Vector3 currentPosition,
                                   float widthDifference, float remainingTime)
        {
            PlacementResult placementResult = widthDifference < 0 ? PlacementResult.LeftCut : PlacementResult.RightCut;
            _signalBus.Fire(new PiecePlacedSignal(placementResult));

            float cutWidth = currentScale.x - absWidthDifference;
            Vector3 newScale = new Vector3(cutWidth, currentScale.y, currentScale.z);
            _currentPiece.localScale = newScale;

            float centeredPositionX = currentPosition.x - widthDifference / 2f;
            _currentPiece.localPosition = new Vector3(centeredPositionX, currentPosition.y, currentPosition.z);

            _signalBus.Fire(new PathChangedSignal(centeredPositionX));

            CreateFallingPiece(currentScale, absWidthDifference, widthDifference, currentPosition, centeredPositionX, cutWidth);

            DOVirtual.DelayedCall(remainingTime, OnPiecePlaced);
        }

        private void CreateFallingPiece(Vector3 originalScale, float absWidthDifference, float widthDifference,
                                        Vector3 originalPosition, float centeredPositionX, float cutWidth)
        {
            var cutPieceObj = _objectPooler.GetPooledObject(0);
            cutPieceObj.GetComponentInChildren<MeshRenderer>().material = _currentPiece.GetComponentInChildren<MeshRenderer>().material;
            Transform cutPiece = cutPieceObj.transform;
            cutPiece.SetParent(_stackParent);

            Vector3 cutScale = new Vector3(absWidthDifference, originalScale.y, originalScale.z);
            cutPiece.localScale = cutScale;

            float cutDirection = Mathf.Sign(widthDifference);
            float cutCenteredPositionX = centeredPositionX + cutDirection * (cutWidth + absWidthDifference) / 2f;
            cutPiece.localPosition = new Vector3(cutCenteredPositionX, originalPosition.y, originalPosition.z);
            cutPieceObj.SetActive(true);

            if (cutPiece.TryGetComponent<Rigidbody>(out Rigidbody body))
            {
                body.useGravity = true;
                body.isKinematic = false;
                body.AddForce(Vector3.right * cutDirection * _parameters.Length, ForceMode.Impulse);
            }

            // Delayed method to return the falling piece to the pool
            DOVirtual.DelayedCall(_cutPieceDestroyDelay, () => {
                cutPieceObj.transform.position = Vector3.zero;
                cutPieceObj.transform.SetParent(_objectPooler.spawnTransform);
                cutPieceObj.SetActive(false);
                // Reset rigidbody settings when returning to pool
                if (cutPiece.TryGetComponent<Rigidbody>(out Rigidbody rb))
                {
                    rb.useGravity = false;
                    rb.isKinematic = true;
                    rb.velocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }
            });
        }

        private void OnPiecePlaced()
        {
            if (_pieceCount < _parameters.PieceCount)
            {
                CreatePlatform();
                _inputEnabled = true;
            }
        }
    }
}