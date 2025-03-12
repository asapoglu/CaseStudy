// Temel arayüzler
namespace Abdurrahman.Project_2.Core.Interfaces
{
    // Kamera kontrol arayüzü
    public interface ICameraController
    {
        void SwitchToPlayCamera();
        void SwitchToSuccessCamera();
        void StartCameraRotation();
        void StopCameraRotation();
    }
}