# Leonardo Rocha - Unity Programmer Task

A small 2D platformer prototype developed in **Unity 6.3 LTS (6000.3.10f1)** for the Unity Programmer interview task.

## How to Play

Your goal is to reach the castle. Explore the level, reach the Frog Shop, activate the lever to unlock the shortcut, return home to collect the Trumpet, wake the Frog, receive the Wand, defeat the enemy, collect the Castle Key, and reach the castle.

## Controls

| Input | Action |
|---|---|
| **A / D** | Move left / right |
| **Space** | Jump |
| **Shift** | Run |
| **E** | Interact / use equipped item |
| **1-6** | Select inventory pocket |
| **7** | Select Hand slot |
| **Mouse Click** | Select inventory slot |
| **Drag & Drop** | Move or swap inventory items |

When a valid world interaction is nearby, **E prioritizes that interaction**. Otherwise, E uses the equipped Hand item. The Trumpet plays while E is held. The Wand fires projectiles with E.

## Main Systems

The prototype includes responsive Rigidbody2D movement, contextual world interaction, NPC dialogue, item pickups, a six-slot pocket inventory plus Hand equipment slot, drag-and-drop item swapping, dynamic item details, exact slot-based JSON persistence, checkpoints, death/respawn feedback, progression state, audio, projectile combat, enemy burst attacks, and a lightweight beginning-to-end gameplay loop.

Inventory contents and exact slot positions are restored when the game starts. Progression such as the Frog Shop shortcut, Frog awakening, enemy defeat, and castle completion is also persisted.

## Technical Notes

The project uses small, reusable components with separate responsibilities for input, movement, inventory data/runtime/UI, dialogue, interaction, save/load, progression, combat, and respawning. ScriptableObjects provide stable item definitions while runtime save data stores item IDs rather than scene references.

## Development

The repository was developed with incremental Git commits throughout the task rather than a single final commit.

The final build was playtested from a fresh start through the complete gameplay loop, including save/load restoration, respawning, item progression, combat, and level completion.

## Unity Version

**Unity 6.3 LTS - 6000.3.10f1**
