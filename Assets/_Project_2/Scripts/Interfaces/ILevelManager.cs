// Temel arayüzler
namespace Abdurrahman.Project_2.Core.Interfaces
{
    using Abdurrahman.Project_2.Core.Models;

    // Seviye yönetimi arayüzü
    public interface ILevelManager
    {
        void LoadLevel();
        void LoadLevelNumber();
        void SaveLevelNumber();
        LevelParameters GetCurrentParameters();
        int CurrentLevel { get; }
    }
}