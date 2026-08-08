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
    public sealed class GameProgressController :
        MonoBehaviour
    {
        private bool frogShopReached;
        private bool shortcutUnlocked;
        private bool frogAwake;
        private bool wandReceived;
        private bool enemyDefeated;
        private bool castleDoorOpened;

        /// <summary>
        /// Raised whenever persistent world progression changes.
        /// </summary>
        public event Action Changed;

        public bool FrogShopReached =>
            frogShopReached;

        public bool ShortcutUnlocked =>
            shortcutUnlocked;

        public bool FrogAwake =>
            frogAwake;

        public bool WandReceived =>
            wandReceived;

        public bool EnemyDefeated =>
            enemyDefeated;

        public bool CastleDoorOpened =>
            castleDoorOpened;

        /// <summary>
        /// Records the player's first successful arrival at the Frog Shop.
        ///
        /// Reaching the shop does not unlock the shortcut by itself.
        /// The shortcut is unlocked only after activating the shop lever.
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
        ///
        /// Activating the lever also guarantees that the shop has been reached
        /// and permanently unlocks the shortcut.
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
        /// Records that the Frog has been awakened.
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
        /// Records that the ranged enemy has been defeated.
        /// </summary>
        public void MarkEnemyDefeated()
        {
            if (enemyDefeated)
            {
                return;
            }

            enemyDefeated = true;

            Changed?.Invoke();
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
        ///
        /// Listeners are notified only after the complete state has
        /// been restored.
        /// </summary>
        public void RestoreState(
            bool savedFrogShopReached,
            bool savedShortcutUnlocked,
            bool savedFrogAwake,
            bool savedWandReceived,
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

            enemyDefeated =
                savedEnemyDefeated;

            castleDoorOpened =
                savedCastleDoorOpened;

            Changed?.Invoke();
        }
    }
}