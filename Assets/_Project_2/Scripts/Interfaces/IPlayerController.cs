// Temel arayüzler
namespace Abdurrahman.Project_2.Core.Interfaces
{
    using Abdurrahman.Project_2.Core.Signals;
    using UnityEngine;

    public interface IPlayerController
    {
        void ResetPlayerPosition();
        void StartMoving();
        void SetPlayerAnimation(string animationName);
        void OnPathChange(PathChangedSignal signal);
        void OnLevelReady(LevelReadySignal signal);
        Transform PlayerTransform { get; }
    }
}