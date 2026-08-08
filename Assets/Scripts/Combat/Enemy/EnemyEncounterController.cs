using LeonardoTask.Inventory;
using LeonardoTask.Progress;
using UnityEngine;

namespace LeonardoTask.Combat
{
    /// <summary>
    /// Coordinates the persistent state of the ranged enemy encounter.
    ///
    /// Defeating the enemy permanently records world progression and
    /// reveals the Castle Key until the player collects it.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class EnemyEncounterController :
        MonoBehaviour
    {
        [Header("Progress")]

        [SerializeField]
        private GameProgressController progress;

        [Header("Inventory")]

        [SerializeField]
        private InventoryController inventory;

        [SerializeField]
        private ItemDefinition castleKeyItem;

        [Header("Enemy")]

        [SerializeField]
        private GameObject enemyObject;

        [SerializeField]
        private DamageableHealth2D enemyHealth;

        [Header("Reward")]

        [SerializeField]
        private GameObject castleKeyPickup;

        private void OnEnable()
        {
            if (enemyHealth != null)
            {
                enemyHealth.Died +=
                    HandleEnemyDied;
            }

            if (progress != null)
            {
                progress.Changed +=
                    ApplyPersistentState;
            }

            if (inventory != null)
            {
                inventory.InventoryChanged +=
                    ApplyPersistentState;

                inventory.EquipmentChanged +=
                    ApplyPersistentState;
            }
        }

        private void Start()
        {
            ApplyPersistentState();
        }

        private void OnDisable()
        {
            if (enemyHealth != null)
            {
                enemyHealth.Died -=
                    HandleEnemyDied;
            }

            if (progress != null)
            {
                progress.Changed -=
                    ApplyPersistentState;
            }

            if (inventory != null)
            {
                inventory.InventoryChanged -=
                    ApplyPersistentState;

                inventory.EquipmentChanged -=
                    ApplyPersistentState;
            }
        }

        /// <summary>
        /// Permanently records the enemy defeat.
        /// </summary>
        private void HandleEnemyDied()
        {
            if (progress == null)
            {
                return;
            }

            progress.MarkEnemyDefeated();
        }

        /// <summary>
        /// Reconstructs the enemy encounter from persistent progression
        /// and current Castle Key ownership.
        /// </summary>
        private void ApplyPersistentState()
        {
            if (progress == null ||
                inventory == null ||
                castleKeyItem == null)
            {
                return;
            }

            bool enemyDefeated =
                progress.EnemyDefeated;

            bool ownsCastleKey =
                inventory.Contains(
                    castleKeyItem
                ) ||
                inventory.IsEquipped(
                    castleKeyItem
                );

            if (enemyObject != null)
            {
                enemyObject.SetActive(
                    !enemyDefeated
                );
            }

            if (castleKeyPickup != null)
            {
                bool keyShouldBeAvailable =
                    enemyDefeated &&
                    !ownsCastleKey &&
                    !progress.CastleDoorOpened;

                castleKeyPickup.SetActive(
                    keyShouldBeAvailable
                );
            }
        }
    }
}