// Temel arayüzler
namespace Abdurrahman.Project_2.Core.Interfaces
{
    using Abdurrahman.Project_2.Core.Models;
    
    // Ses yönetimi arayüzü
    public interface IAudioManager
    {
        void PlayPlacementSound(PlacementResult result);
        void ResetPitch();
    }
}