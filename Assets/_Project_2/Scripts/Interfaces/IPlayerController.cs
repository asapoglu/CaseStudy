// Temel arayüzler
namespace Abdurrahman.Project_2.Core.Interfaces
{
    using UnityEngine;

    // Oyuncu kontrol arayüzü
    public interface IPlayerController
    {
        void ResetPlayerPosition();
        void MovePlayer(float targetPosition);
        void StopPlayer();
        void SetPlayerAnimation(string animationName);
        Transform PlayerTransform { get; }
    }
}