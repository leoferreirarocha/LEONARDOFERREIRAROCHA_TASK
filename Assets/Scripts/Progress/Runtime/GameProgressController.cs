using System;
using UnityEngine;

namespace LeonardoTask.Progress
{
    /// <summary>
    /// Owns the persistent progression state of the current game.
    ///
    /// Inventory ownership is intentionally handled separately.
    /// This component stores only progression that affects the world.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class GameProgressController : MonoBehaviour
    {
        private bool frogShopReached;
        private bool shortcutUnlocked;
        private bool frogAwake;
        private bool wandReceived;
        private bool enemyCheckpointReached;
        private bool enemyDefeated;
        private bool castleDoorOpened;

        /// <summary>
        /// Raised whenever persistent world progression changes.
        /// </summary>
        public event Action Changed;

        /// <summary>
        /// Raised specifically after progression has been restored
        /// from persistent save data.
        ///
        /// Systems that need to reposition or rebuild themselves
        /// immediately after loading can listen to this event.
        /// </summary>
        public event Action StateRestored;

        public bool FrogShopReached =>
            frogShopReached;

        public bool ShortcutUnlocked =>
            shortcutUnlocked;

        public bool FrogAwake =>
            frogAwake;

        public bool WandReceived =>
            wandReceived;

        public bool EnemyCheckpointReached =>
            enemyCheckpointReached;

        public bool EnemyDefeated =>
            enemyDefeated;

        public bool CastleDoorOpened =>
            castleDoorOpened;

        /// <summary>
        /// Records the player's first successful arrival at the Frog Shop.
        /// </summary>
        public void ReachFrogShop()
        {
            if (frogShopReached)
            {
                return;
            }

            frogShopReached = true;

            Changed?.Invoke();
        }

        /// <summary>
        /// Records the permanent activation of the Frog Shop lever.
        /// </summary>
        public void ActivateFrogShopLever()
        {
            bool changed = false;

            if (!frogShopReached)
            {
                frogShopReached = true;
                changed = true;
            }

            if (!shortcutUnlocked)
            {
                shortcutUnlocked = true;
                changed = true;
            }

            if (changed)
            {
                Changed?.Invoke();
            }
        }

        /// <summary>
        /// Records that the Frog has been permanently awakened.
        /// </summary>
        public void MarkFrogAwake()
        {
            if (frogAwake)
            {
                return;
            }

            frogAwake = true;

            Changed?.Invoke();
        }

        /// <summary>
        /// Records that the Wand reward has already been granted.
        /// </summary>
        public void MarkWandReceived()
        {
            if (wandReceived)
            {
                return;
            }

            wandReceived = true;

            Changed?.Invoke();
        }

        /// <summary>
        /// Records that the player has reached the checkpoint
        /// immediately before the enemy encounter.
        /// </summary>
        public void MarkEnemyCheckpointReached()
        {
            if (enemyCheckpointReached)
            {
                return;
            }

            enemyCheckpointReached = true;

            Changed?.Invoke();
        }

        /// <summary>
        /// Records that the ranged enemy has been permanently defeated.
        ///
        /// Defeating the enemy also guarantees that the encounter
        /// checkpoint has logically been reached.
        /// </summary>
        public void MarkEnemyDefeated()
        {
            bool changed = false;

            if (!enemyCheckpointReached)
            {
                enemyCheckpointReached = true;
                changed = true;
            }

            if (!enemyDefeated)
            {
                enemyDefeated = true;
                changed = true;
            }

            if (changed)
            {
                Changed?.Invoke();
            }
        }

        /// <summary>
        /// Records that the castle entrance has been permanently opened.
        /// </summary>
        public void MarkCastleDoorOpened()
        {
            if (castleDoorOpened)
            {
                return;
            }

            castleDoorOpened = true;

            Changed?.Invoke();
        }

        /// <summary>
        /// Restores the complete persistent world progression state.
        /// </summary>
        public void RestoreState(
            bool savedFrogShopReached,
            bool savedShortcutUnlocked,
            bool savedFrogAwake,
            bool savedWandReceived,
            bool savedEnemyCheckpointReached,
            bool savedEnemyDefeated,
            bool savedCastleDoorOpened
        )
        {
            frogShopReached =
                savedFrogShopReached;

            shortcutUnlocked =
                savedShortcutUnlocked;

            frogAwake =
                savedFrogAwake;

            wandReceived =
                savedWandReceived;

            // Older saves do not contain the checkpoint field.
            // If the enemy was already defeated, the player must
            // necessarily have progressed beyond this checkpoint.
            enemyCheckpointReached =
                savedEnemyCheckpointReached ||
                savedEnemyDefeated;

            enemyDefeated =
                savedEnemyDefeated;

            castleDoorOpened =
                savedCastleDoorOpened;

            Changed?.Invoke();
            StateRestored?.Invoke();
        }
    }
}