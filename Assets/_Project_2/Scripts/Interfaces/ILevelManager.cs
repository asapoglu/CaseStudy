// Temel arayüzler
namespace Abdurrahman.Project_2.Core.Interfaces
{
    using Abdurrahman.Project_2.Core.Models;
    using Abdurrahman.Project_2.Core.Signals;

    // Seviye yönetimi arayüzü
    public interface ILevelManager
    {
        void LoadLevel(LoadLevelSignal signal);
        void SaveLevelNumber();
        LevelParameters GetCurrentParameters();
        int CurrentLevel { get; }
    }
}