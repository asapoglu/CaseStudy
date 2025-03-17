// Sinyal sınıfları
namespace Abdurrahman.Project_2.Core.Signals
{
    using Abdurrahman.Project_2.Core.Models;

    // Oyun durumu sinyalleri
    public class GameStartSignal { }
    public class GameFailSignal { }
    public class GameSuccessSignal { }

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
        public bool IsNextLevel;
        public LoadLevelSignal(bool isNextLevel)
        {
            IsNextLevel = isNextLevel;
        }
    }
    public class LevelReadySignal
    {
        public LevelParameters Parameters;
        public LevelParameters ExParameters;

        public LevelReadySignal(LevelParameters parameters, LevelParameters exParameters = null)
        {
            Parameters = parameters;
            ExParameters = exParameters;
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