using System;
using System.IO;
using LeonardoTask.Inventory;
using LeonardoTask.Progress;
using UnityEngine;

namespace LeonardoTask.SaveSystem
{
    /// <summary>
    /// Coordinates persistent save and load operations for the game.
    ///
    /// Runtime state is converted into plain serializable data,
    /// written as JSON to Application.persistentDataPath, and restored
    /// through the inventory's stable item identifiers.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class GameSaveController : MonoBehaviour
    {
        [Header("References")]

        [Tooltip(
            "Inventory system whose pocket and equipment state will be persisted."
        )]
        [SerializeField]
        private InventoryController inventory;

        [Tooltip(
            "Database used to resolve saved item identifiers back into item definitions."
        )]
        [SerializeField]
        private ItemDatabase itemDatabase;

        [Tooltip(
            "Persistent world progression that will be saved alongside the inventory."
        )]
        [SerializeField]
        private GameProgressController progress;

        [Header("Save Configuration")]

        [Tooltip(
            "Name of the JSON file stored inside Application.persistentDataPath."
        )]
        [SerializeField]
        private string saveFileName = "save.json";

        [Tooltip(
            "Formats the JSON with indentation to improve readability while developing."
        )]
        [SerializeField]
        private bool prettyPrintJson = true;

        private bool isReady;
        private bool saveQueued;

        /// <summary>
        /// Gets the complete platform-specific save file path.
        /// </summary>
        public string SavePath =>
            Path.Combine(
                Application.persistentDataPath,
                saveFileName
            );

        /// <summary>
        /// Gets whether a save file currently exists on disk.
        /// </summary>
        public bool HasSaveFile =>
            File.Exists(SavePath);

        private void Start()
        {
            if (!ValidateReferences())
            {
                enabled = false;
                return;
            }

            // InventoryController initializes its runtime models in Awake.
            // Loading in Start guarantees that this initialization has already
            // happened before persistent state is applied.
            LoadFromDisk();

            inventory.InventoryChanged +=
                HandleRuntimeStateChanged;

            inventory.EquipmentChanged +=
                HandleRuntimeStateChanged;

            progress.Changed +=
                HandleRuntimeStateChanged;

            isReady = true;
        }

        private void LateUpdate()
        {
            if (!isReady ||
                !saveQueued)
            {
                return;
            }

            // Multiple inventory events during the same frame are collapsed
            // into a single disk write.
            saveQueued = false;

            SaveToDisk();
        }

        private void OnDestroy()
        {
            if (inventory != null)
            {
                inventory.InventoryChanged -=
                    HandleRuntimeStateChanged;

                inventory.EquipmentChanged -=
                    HandleRuntimeStateChanged;
            }

            if (progress != null)
            {
                progress.Changed -=
                    HandleRuntimeStateChanged;
            }
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (!pauseStatus ||
                !isReady)
            {
                return;
            }

            SaveToDisk();
        }

        private void OnApplicationQuit()
        {
            if (!isReady)
            {
                return;
            }

            SaveToDisk();
        }

        /// <summary>
        /// Serializes the current inventory and equipment state
        /// and writes it to the persistent save file.
        /// </summary>
        public bool SaveToDisk()
        {
            if (!ValidateReferences())
            {
                return false;
            }

            try
            {
                GameSaveData saveData =
                    CaptureSaveData();

                string json =
                    JsonUtility.ToJson(
                        saveData,
                        prettyPrintJson
                    );

                Directory.CreateDirectory(
                    Application.persistentDataPath
                );

                File.WriteAllText(
                    SavePath,
                    json
                );

                saveQueued = false;

                return true;
            }
            catch (Exception exception)
            {
                Debug.LogError(
                    $"Failed to save game data to '{SavePath}'.\n{exception}",
                    this
                );

                return false;
            }
        }

        /// <summary>
        /// Loads persistent game data when a save file exists.
        ///
        /// Missing save files are treated as a normal first-time launch
        /// rather than an error.
        /// </summary>
        public bool LoadFromDisk()
        {
            if (!ValidateReferences())
            {
                return false;
            }

            if (!File.Exists(SavePath))
            {
                return false;
            }

            try
            {
                string json =
                    File.ReadAllText(
                        SavePath
                    );

                if (string.IsNullOrWhiteSpace(json))
                {
                    Debug.LogWarning(
                        $"Save file '{SavePath}' is empty.",
                        this
                    );

                    return false;
                }

                GameSaveData saveData =
                    JsonUtility.FromJson<GameSaveData>(
                        json
                    );

                if (saveData == null)
                {
                    Debug.LogWarning(
                        $"Save file '{SavePath}' could not be deserialized.",
                        this
                    );

                    return false;
                }

                if (saveData.version !=
                    GameSaveData.CurrentVersion)
                {
                    Debug.LogWarning(
                        $"Save version {saveData.version} differs from the current version {GameSaveData.CurrentVersion}. " +
                        "A best-effort load will be attempted.",
                        this
                    );
                }

                ApplySaveData(
                    saveData
                );

                return true;
            }
            catch (Exception exception)
            {
                Debug.LogError(
                    $"Failed to load game data from '{SavePath}'.\n{exception}",
                    this
                );

                return false;
            }
        }

        /// <summary>
        /// Deletes the persistent save file when one exists.
        ///
        /// This does not modify the current runtime state.
        /// </summary>
        public bool DeleteSaveFile()
        {
            if (!File.Exists(SavePath))
            {
                return false;
            }

            try
            {
                File.Delete(
                    SavePath
                );

                return true;
            }
            catch (Exception exception)
            {
                Debug.LogError(
                    $"Failed to delete save file '{SavePath}'.\n{exception}",
                    this
                );

                return false;
            }
        }

        /// <summary>
        /// Captures the current runtime state using stable item identifiers.
        /// </summary>
        private GameSaveData CaptureSaveData()
        {
            GameSaveData saveData =
                new GameSaveData();

            for (int i = 0;
                 i < inventory.PocketSlotCount;
                 i++)
            {
                InventorySlot slot =
                    inventory.GetPocketSlot(i);

                if (slot.IsEmpty)
                {
                    saveData.pocketSlots.Add(
                        new InventorySlotSaveData(
                            string.Empty,
                            0
                        )
                    );

                    continue;
                }

                saveData.pocketSlots.Add(
                    new InventorySlotSaveData(
                        slot.Item.Id,
                        slot.Quantity
                    )
                );
            }

            saveData.handItemId =
                inventory.HandItem == null
                    ? string.Empty
                    : inventory.HandItem.Id;
            saveData.progress =
                new GameProgressSaveData
                {
                    frogShopReached =
                        progress.FrogShopReached,

                    shortcutUnlocked =
                        progress.ShortcutUnlocked,

                    frogAwake =
                        progress.FrogAwake,

                    wandReceived =
                        progress.WandReceived,

                    enemyCheckpointReached =
                        progress.EnemyCheckpointReached,

                    enemyDefeated =
                        progress.EnemyDefeated,

                    castleDoorOpened =
                        progress.CastleDoorOpened
                };
            return saveData;
        }

        /// <summary>
        /// Resolves saved item identifiers and restores their exact
        /// pocket slot positions and active equipment state.
        /// </summary>
        private void ApplySaveData(
            GameSaveData saveData
        )
        {
            int slotCount =
                inventory.PocketSlotCount;

            ItemDefinition[] pocketItems =
                new ItemDefinition[slotCount];

            int[] pocketQuantities =
                new int[slotCount];

            if (saveData.pocketSlots != null)
            {
                int savedSlotCount =
                    Mathf.Min(
                        slotCount,
                        saveData.pocketSlots.Count
                    );

                for (int i = 0;
                     i < savedSlotCount;
                     i++)
                {
                    InventorySlotSaveData slotData =
                        saveData.pocketSlots[i];

                    if (slotData == null ||
                        string.IsNullOrWhiteSpace(slotData.itemId) ||
                        slotData.quantity <= 0)
                    {
                        continue;
                    }

                    if (!itemDatabase.TryGetItem(
                            slotData.itemId,
                            out ItemDefinition item
                        ))
                    {
                        Debug.LogWarning(
                            $"Saved item ID '{slotData.itemId}' could not be found in the Item Database. " +
                            $"Pocket slot {i} will remain empty.",
                            this
                        );

                        continue;
                    }

                    pocketItems[i] = item;

                    pocketQuantities[i] =
                        Mathf.Clamp(
                            slotData.quantity,
                            1,
                            item.MaximumStackSize
                        );
                }

                if (saveData.pocketSlots.Count >
                    slotCount)
                {
                    Debug.LogWarning(
                        "The save file contains more pocket slots than the current inventory configuration. " +
                        "Additional saved slots will be ignored.",
                        this
                    );
                }
            }

            ItemDefinition handItem =
                ResolveSavedHandItem(
                    saveData.handItemId
                );

            inventory.RestoreState(
                pocketItems,
                pocketQuantities,
                handItem
            );
            GameProgressSaveData progressData =
                saveData.progress ??
                new GameProgressSaveData();

            progress.RestoreState(
                progressData.frogShopReached,
                progressData.shortcutUnlocked,
                progressData.frogAwake,
                progressData.wandReceived,
                progressData.enemyCheckpointReached,
                progressData.enemyDefeated,
                progressData.castleDoorOpened
            );
        }

        /// <summary>
        /// Resolves and validates the item stored in the saved hand slot.
        /// </summary>
        private ItemDefinition ResolveSavedHandItem(
            string itemId
        )
        {
            if (string.IsNullOrWhiteSpace(itemId))
            {
                return null;
            }

            if (!itemDatabase.TryGetItem(
                    itemId,
                    out ItemDefinition item
                ))
            {
                Debug.LogWarning(
                    $"Saved hand item ID '{itemId}' could not be found in the Item Database.",
                    this
                );

                return null;
            }

            if (!item.IsEquippable)
            {
                Debug.LogWarning(
                    $"Saved hand item '{item.DisplayName}' is no longer equippable and will not be restored.",
                    this
                );

                return null;
            }

            return item;
        }

        /// <summary>
        /// Queues one save operation for the end of the current frame.
        /// </summary>
        private void HandleRuntimeStateChanged()
        {
            if (!isReady)
            {
                return;
            }

            saveQueued = true;
        }

        private bool ValidateReferences()
        {
            bool valid = true;

            if (inventory == null)
            {
                Debug.LogError(
                    $"{nameof(GameSaveController)} on '{name}' requires an InventoryController reference.",
                    this
                );

                valid = false;
            }

            if (itemDatabase == null)
            {
                Debug.LogError(
                    $"{nameof(GameSaveController)} on '{name}' requires an ItemDatabase reference.",
                    this
                );

                valid = false;
            }

            if (string.IsNullOrWhiteSpace(saveFileName))
            {
                Debug.LogError(
                    $"{nameof(GameSaveController)} on '{name}' requires a valid save file name.",
                    this
                );

                valid = false;
            }
            if (progress == null)
            {
                Debug.LogError(
                    $"{nameof(GameSaveController)} on '{name}' requires a GameProgressController reference.",
                    this
                );

                valid = false;
            }

            return valid;
        }

        [ContextMenu("Log Save File Path")]
        private void LogSaveFilePath()
        {
            Debug.Log(
                $"Save file path: {SavePath}",
                this
            );
        }

        [ContextMenu("Delete Save File")]
        private void DeleteSaveFileFromContextMenu()
        {
            bool deleted =
                DeleteSaveFile();

            if (deleted)
            {
                Debug.Log(
                    $"Deleted save file: {SavePath}",
                    this
                );
            }
            else
            {
                Debug.Log(
                    $"No save file was deleted at: {SavePath}",
                    this
                );
            }
        }
    }
}