// Temel arayüzler
namespace Abdurrahman.Project_2.Core.Interfaces
{
    // Platform yönetimi arayüzü
    public interface IPlatformManager
    {
        void CreatePlatforms();
        void CreateNewPlatform();
        void PlacePlatform();
        void ClearAllPlatforms();
    }
}