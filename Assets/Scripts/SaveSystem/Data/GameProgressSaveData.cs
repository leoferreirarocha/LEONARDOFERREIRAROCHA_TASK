using System;

namespace LeonardoTask.SaveSystem
{
    /// <summary>
    /// Represents persistent world progression stored inside the save file.
    /// </summary>
    [Serializable]
    public sealed class GameProgressSaveData
    {
        public bool frogShopReached;
        public bool shortcutUnlocked;
        public bool frogAwake;
        public bool wandReceived;
        public bool enemyCheckpointReached;
        public bool enemyDefeated;
        public bool castleDoorOpened;
    }
}