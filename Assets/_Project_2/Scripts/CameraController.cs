// Kamera Kontrolcüsü
namespace Abdurrahman.Project_2.Core.Managers
{
    using Abdurrahman.Project_2.Core.Interfaces;
    using Abdurrahman.Project_2.Core.Signals;
    using UnityEngine;
    using Zenject;
    using System.Collections;
    using Cinemachine;
    
    public class CameraController : MonoBehaviour, ICameraController
    {
        [Inject] private SignalBus _signalBus;
        
        [Header("Kamera Ayarları")]
        [SerializeField] private float _successRotationSpeed = 30f;
        [SerializeField] private CinemachineVirtualCamera _playCamera;
        [SerializeField] private CinemachineVirtualCamera _successCamera;
        
        private CinemachineOrbitalTransposer _orbitalTransposer;
        private readonly WaitForEndOfFrame _waitForEndOfFrame = new WaitForEndOfFrame();
        private float _angle;
        private bool _rotate;
        
        private void Awake()
        {
            // Başarı kamerasının Orbital Transposer bileşenini al
            if (_successCamera != null)
            {
                _orbitalTransposer = _successCamera.GetCinemachineComponent<CinemachineOrbitalTransposer>();
            }
        }
        
        [Inject]
        private void Initialize()
        {
            // Sinyallere abone ol
            _signalBus.Subscribe<GameSuccessSignal>(OnSuccess);
            _signalBus.Subscribe<ReplaySignal>(OnReplay);
            
            // Yeni sinyal abonelikleri ekle
            _signalBus.Subscribe<LevelReadySignal>(OnLevelReady);
        }
        
        private void OnDestroy()
        {
            // Sinyallerden çık
            _signalBus.TryUnsubscribe<GameSuccessSignal>(OnSuccess);
            _signalBus.TryUnsubscribe<ReplaySignal>(OnReplay);
            _signalBus.TryUnsubscribe<LevelReadySignal>(OnLevelReady);
        }
        
        private void OnSuccess()
        {
            Debug.Log("CameraController - Success sinyali alındı, başarı kamerasına geçiliyor");
            // Oyun başarılı olduğunda başarı kamerasına geç ve rotasyonu başlat
            SwitchToSuccessCamera();
            StartCameraRotation();
        }
        
        private void OnReplay()
        {
            Debug.Log("CameraController - Replay sinyali alındı, oyun kamerasına geçiliyor");
            // Yeniden oynatma talebi olduğunda oyun kamerasına geç
            SwitchToPlayCamera();
            StopCameraRotation();
        }
        
        private void OnLevelReady(LevelReadySignal signal)
        {
            Debug.Log("CameraController - Level hazır sinyali alındı, oyun kamerasına geçiliyor");
            // Yeni seviye yüklendiğinde oyun kamerasına geç
            SwitchToPlayCamera();
            StopCameraRotation();
        }
        
        public void SwitchToPlayCamera()
        {
            Debug.Log("CameraController - Oyun kamerasına geçiliyor");
            
            // Oyun kamerasını etkinleştir
            if (_playCamera != null)
            {
                _playCamera.gameObject.SetActive(true);
            }
            
            // Başarı kamerasını devre dışı bırak
            if (_successCamera != null)
            {
                _successCamera.gameObject.SetActive(false);
            }
        }
        
        public void SwitchToSuccessCamera()
        {
            Debug.Log("CameraController - Başarı kamerasına geçiliyor");
            
            // Oyun kamerasını devre dışı bırak
            if (_playCamera != null)
            {
                _playCamera.gameObject.SetActive(false);
            }
            
            // Başarı kamerasını etkinleştir
            if (_successCamera != null)
            {
                _successCamera.gameObject.SetActive(true);
            }
        }
        
        public void StartCameraRotation()
        {
            Debug.Log("CameraController - Kamera rotasyonu başlatılıyor");
            
            // Eğer Orbital Transposer bileşeni yoksa işlem yapma
            if (_orbitalTransposer == null) return;
            
            // Rotasyonu başlat
            StartCoroutine(Rotate());
        }
        
        public void StopCameraRotation()
        {
            Debug.Log("CameraController - Kamera rotasyonu durduruluyor");
            
            // Rotasyonu durdur
            _rotate = false;
        }
        
        IEnumerator Rotate()
        {
            // Rotasyon durumunu etkinleştir
            _rotate = true;
            
            // Mevcut açıyı al
            _angle = _orbitalTransposer.m_Heading.m_Bias;
            
            // Rotasyon döngüsü
            while (_rotate)
            {
                // Açıyı arttır
                _angle += _successRotationSpeed * Time.deltaTime;
                
                // 0-360 dereceler arasında tut
                var val = Mathf.Repeat(_angle, 360);
                
                // 0-1 arasında bir değere normalize et
                var ratio = Mathf.InverseLerp(0, 360, val);
                
                // -180 ile 180 arasında bir açıya çevir
                _orbitalTransposer.m_Heading.m_Bias = Mathf.Lerp(-180, 180, ratio);
                
                // Bir sonraki kareyi bekle
                yield return _waitForEndOfFrame;
            }
        }
    }
}