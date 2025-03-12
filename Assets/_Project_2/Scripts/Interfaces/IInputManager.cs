// Temel arayüzler
namespace Abdurrahman.Project_2.Core.Interfaces
{
    // Girdi yönetimi arayüzü
    public interface IInputManager
    {
        bool IsInputReceived();
        void UpdateInputState(bool state);
    }
}