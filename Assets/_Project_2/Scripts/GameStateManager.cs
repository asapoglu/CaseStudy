// Oyun Durum Yöneticisi
namespace Abdurrahman.Project_2.Core.Managers
{
    using Abdurrahman.Project_2.Core.Interfaces;
    using Abdurrahman.Project_2.Core.Signals;
    using UnityEngine;
    using Zenject;
    using System.Collections;
    using DG.Tweening;
    
    public class GameStateManager : MonoBehaviour, IGameStateManager
    {
        [Inject] private SignalBus _signalBus;
        [Inject] private ILevelManager _levelManager;
        
        [SerializeField] private float _replayWaitDuration = 2f;
        
        private bool _isGameActive = false;
        
        [Inject]
        private void Initialize()
        {
            // Sinyallere abone ol
            _signalBus.Subscribe<GameFailSignal>(OnGameFail);
            _signalBus.Subscribe<ContinueSignal>(NextLevel);
            _signalBus.Subscribe<ReplaySignal>(ReplayGame);
        }
        
        private void OnDestroy()
        {
            // Sinyallerden çık
            _signalBus.TryUnsubscribe<GameFailSignal>(OnGameFail);
            _signalBus.TryUnsubscribe<ContinueSignal>(NextLevel);
            _signalBus.TryUnsubscribe<ReplaySignal>(ReplayGame);
        }
        
        public void StartGame()
        {
            if (_isGameActive) return;
            
            _isGameActive = true;
            _signalBus.Fire(new GameStartSignal());
        }
        
        public void EndGame(bool isSuccess)
        {
            if (!_isGameActive) return;
            
            _isGameActive = false;
            
            if (isSuccess)
            {
                _signalBus.Fire(new GameSuccessSignal());
            }
            else
            {
                _signalBus.Fire(new GameFailSignal());
            }
        }
        
        public void NextLevel()
        {
            _levelManager.SaveLevelNumber();
            _levelManager.LoadLevel();
            _isGameActive = false;
        }
        
        public void ReplayGame()
        {
            _signalBus.Fire(new ReplaySignal());
            _isGameActive = false;
        }
        
        private void OnGameFail()
        {
            // Oyun başarısız olduğunda kısa bir süre bekleyip yeniden oynat
            DOVirtual.DelayedCall(_replayWaitDuration, TriggerReplay);
        }
        
        private void TriggerReplay()
        {
            ReplayGame();
        }
    }
}