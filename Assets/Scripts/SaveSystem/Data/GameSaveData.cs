using System;
using System.Collections.Generic;

namespace LeonardoTask.SaveSystem
{
    /// <summary>
    /// Represents the persistent game state stored on disk.
    ///
    /// The save format starts with inventory and equipment state and can
    /// later be extended with world progression without replacing the
    /// persistence architecture.
    /// </summary>
    [Serializable]
    public sealed class GameSaveData
    {
        /// <summary>
        /// Current version of the save data format.
        /// </summary>
        public const int CurrentVersion = 1;

        /// <summary>
        /// Version used when this save file was created.
        /// </summary>
        public int version = CurrentVersion;

        /// <summary>
        /// Persistent pocket slots stored in their exact inventory order.
        /// </summary>
        public List<InventorySlotSaveData> pocketSlots = new();

        /// <summary>
        /// Stable identifier of the item currently equipped in the hand.
        /// An empty string represents an empty hand.
        /// </summary>
        public string handItemId = string.Empty;
    }
}