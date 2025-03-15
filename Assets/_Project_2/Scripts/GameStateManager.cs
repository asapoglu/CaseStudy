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
        [Inject] private ILevelManager _levelManager;

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

        [Inject]
        private void Initialize()
        {
        }

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


            // Durumu güncelle
            _currentState = GameState.Playing;

            // Oyunun başladığını bildiren sinyali gönder
            _signalBus.Fire(new GameStartSignal());
        }

        // Oyunu bitir (başarılı veya başarısız)
        public void EndGame(bool isSuccess)
        {
            // Oyun zaten bitmiş durumdaysa tekrar işlem yapma
            if (_currentState == GameState.Success || _currentState == GameState.Failed)
            {
                return;
            }

            // Oyun oynama durumunda değilse uyarı ver
            if (_currentState != GameState.Playing)
            {
                return;
            }


            // Durumu güncelle
            _currentState = isSuccess ? GameState.Success : GameState.Failed;

            // Uygun sinyali gönder
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
            // _signalBus.Fire(new NextLevelSignal());
            // // Mevcut seviyeyi kaydet ve yeni seviyeyi yükle
            // _levelManager.SaveLevelNumber();
            // _levelManager.LoadLevel();

            // Durumu başlangıç durumuna getir
            _currentState = GameState.NotStarted;
        }

        // Oyunu yeniden başlat
        public void ReplayGame()
        {

            // Yeniden başlatma sinyalini gönder
            _signalBus.Fire(new RestartLevelSignal());

            // Durumu başlangıç durumuna getir
            _currentState = GameState.NotStarted;
        }
    }
}