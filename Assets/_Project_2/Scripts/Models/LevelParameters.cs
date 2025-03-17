// Temel model sınıfları
namespace Abdurrahman.Project_2.Core.Models
{
    using System;

    [Serializable]
    public class LevelParameters
    {
        public int Level;
        public float Width;
        public float Height;
        public float Length;
        public float ToleranceWidth;
        public float Speed;
        public int PieceCount;
        public float TargetPosition;
        public bool IsNextLevel;
    }
}