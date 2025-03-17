// Oyun Durum Yöneticisi
namespace Abdurrahman.Project_2.Core.Managers
{
    using Abdurrahman.Project_2.Core.Interfaces;
    using Abdurrahman.Project_2.Core.Signals;
    using UnityEngine;
    using Zenject;
    using DG.Tweening;

    [DefaultExecutionOrder(-100)]
    public class GameStateManager : MonoBehaviour, IGameStateManager
    {
        [Inject] private SignalBus _signalBus;
        // Oyun durumları için enum
        public enum GameState
        {
            NotStarted,  // Oyun henüz başlamadı
            Playing,     // Oyun aktif olarak oynanıyor
            Success,     // Oyun başarıyla tamamlandı
            Failed       // Oyun başarısız oldu
        }

        // Mevcut oyun durumu
        private GameState _currentState = GameState.NotStarted;

        // Oyun durumunu dış dünyaya açmak için property
        public GameState CurrentState => _currentState;

        void Start()
        {
            _signalBus.Fire(new LoadLevelSignal(false));
        }

        private void OnDestroy()
        {
            // Tüm DOTween işlemlerini temizle
            DOTween.Kill(this);
        }

        // Oyunu başlat
        public void StartGame()
        {
            // Eğer oyun zaten başladıysa işlem yapma
            if (_currentState == GameState.Playing)
            {
                return;
            }
            _currentState = GameState.Playing;
            _signalBus.Fire(new GameStartSignal());
        }

        // Oyunu bitir (başarılı veya başarısız)
        public void EndGame(bool isSuccess)
        {
            if (_currentState == GameState.Success || _currentState == GameState.Failed)
            {
                return;
            }

            if (_currentState != GameState.Playing)
            {
                return;
            }


            _currentState = isSuccess ? GameState.Success : GameState.Failed;

            if (isSuccess)
            {
                _signalBus.Fire(new GameSuccessSignal());
            }
            else
            {
                _signalBus.Fire(new GameFailSignal());
            }
        }

        // Bir sonraki seviyeye geç
        public void NextLevel()
        {
            _signalBus.Fire(new LoadLevelSignal(true));
            _currentState = GameState.NotStarted;
        }

        // Oyunu yeniden başlat
        public void ReplayGame()
        {
            _signalBus.Fire(new RestartLevelSignal());
            _signalBus.Fire(new LoadLevelSignal(false));
            _currentState = GameState.NotStarted;
        }
    }
}