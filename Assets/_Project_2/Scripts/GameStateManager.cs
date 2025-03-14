// Oyun Durum Yöneticisi
namespace Abdurrahman.Project_2.Core.Managers
{
    using Abdurrahman.Project_2.Core.Interfaces;
    using Abdurrahman.Project_2.Core.Signals;
    using UnityEngine;
    using Zenject;
    using DG.Tweening;
    
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
            Debug.Log("GameStateManager başlatıldı, başlangıç durumu: " + _currentState);
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
                Debug.LogWarning("Oyun zaten başlamış durumda!");
                return;
            }
            
            Debug.Log("Oyun başlatılıyor. Önceki durum: " + _currentState);
            
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
                Debug.LogWarning("Oyun zaten sonlanmış durumda: " + _currentState);
                return;
            }
            
            // Oyun oynama durumunda değilse uyarı ver
            if (_currentState != GameState.Playing)
            {
                Debug.LogWarning("Oyun aktif değilken EndGame çağrıldı. Mevcut durum: " + _currentState);
                return;
            }
            
            Debug.Log("Oyun sonlandırılıyor. Başarı durumu: " + isSuccess);
            
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
            Debug.Log("Bir sonraki seviyeye geçiliyor. Mevcut durum: " + _currentState);
            
            // Mevcut seviyeyi kaydet ve yeni seviyeyi yükle
            _levelManager.SaveLevelNumber();
            _levelManager.LoadLevel();
            
            // Durumu başlangıç durumuna getir
            _currentState = GameState.NotStarted;
        }
        
        // Oyunu yeniden başlat
        public void ReplayGame()
        {
            Debug.Log("Oyun yeniden başlatılıyor. Mevcut durum: " + _currentState);
            
            // Yeniden başlatma sinyalini gönder
            _signalBus.Fire(new ReplaySignal());
            
            // Durumu başlangıç durumuna getir
            _currentState = GameState.NotStarted;
        }
    }
}