// Sinyal sınıfları
namespace Abdurrahman.Project_2.Core.Signals
{
    using Abdurrahman.Project_2.Core.Models;
    using UnityEngine;

    // Oyun durumu sinyalleri
    public class GameStartSignal { }
    public class GameFailSignal { }
    public class GameSuccessSignal { }

    public class NextLevelSignal { }
    public class RestartLevelSignal { }

    // Platform sinyalleri
    public class PiecePlacedSignal
    {
        public PlacementResult Result;

        public PiecePlacedSignal(PlacementResult result)
        {
            Result = result;
        }
    }

    public class PathChangedSignal
    {
        public float XPosition;

        public PathChangedSignal(float xPosition)
        {
            XPosition = xPosition;
        }
    }

    // Seviye sinyalleri
    public class LoadLevelSignal
    {
        public bool KeepPreviousPlatform;
        public LoadLevelSignal(bool keepPreviousPlatform)
        {
            KeepPreviousPlatform = keepPreviousPlatform;
        }
    }
    public class LevelReadySignal
    {
        public LevelParameters Parameters;

        public LevelReadySignal(LevelParameters parameters)
        {
            Parameters = parameters;
        }
    }

    public class LevelNumberChangedSignal
    {
        public int LevelNumber;

        public LevelNumberChangedSignal(int levelNumber)
        {
            LevelNumber = levelNumber;
        }
    }
}