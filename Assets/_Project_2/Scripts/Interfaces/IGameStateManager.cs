// Temel arayüzler
namespace Abdurrahman.Project_2.Core.Interfaces
{
    // Oyun durum yönetimi arayüzü
    public interface IGameStateManager
    {
        void StartGame();
        void EndGame(bool isSuccess);
        void NextLevel();
        void ReplayGame();
    }
}