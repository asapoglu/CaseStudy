// Girdi Yöneticisi
namespace Abdurrahman.Project_2.Core.Managers
{
    using Abdurrahman.Project_2.Core.Interfaces;
    using UnityEngine;
    using Zenject;
    
    public class InputManager : MonoBehaviour, IInputManager
    {
        private bool _inputEnabled = false;
        
        private void Update()
        {
            // Oyunun mevcut durumuna göre girdiyi kontrol et
            // Bu örnekte sadece dokunma girişi algılanıyor
            if (!_inputEnabled) return;
            
            // Mobil platformlar için dokunma girişi
            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                _inputEnabled = false;
                return;
            }
            
            // PC platformu için fare tıklaması
            if (Input.GetMouseButtonDown(0))
            {
                _inputEnabled = false;
                return;
            }
        }
        
        public bool IsInputReceived()
        {
            // Mobil platformlar için dokunma girişi
            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                return true;
            }
            
            // PC platformu için fare tıklaması
            if (Input.GetMouseButtonDown(0))
            {
                return true;
            }
            
            return false;
        }
        
        public void UpdateInputState(bool state)
        {
            // Girdi durumunu güncelle
            _inputEnabled = state;
        }
    }
}