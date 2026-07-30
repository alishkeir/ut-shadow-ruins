# Shadow Ruins - Combat & Gameplay Systems

## Project Overview

Shadow Ruins is a 2D action platformer built in **Unity 6** using **C#**. The project focuses on creating responsive gameplay systems with a modular architecture that can easily be expanded with new enemies, attacks, abilities, and gameplay mechanics.

Instead of building a large game, the goal is to build a polished gameplay experience while demonstrating clean programming practices commonly used in professional game development.

---

# What I Built

### Player Systems
- Responsive 2D Character Controller
- Variable Jump Height
- Coyote Time
- Jump Buffering
- Dash
- Roll
- Four-Hit Combo System
- Shield Bash (Perfect Timing)
- Damage & Knockback System
- Invincibility Frames

### Combat Systems
- Animation Event Driven Combat
- Modular Hitbox & Hurtbox System
- Reusable Damage Pipeline
- Combo Chaining
- Attack Cooldowns
- Hit Reactions

### AI Systems
- Reusable Enemy State Machine
- Bush Monster AI
- White Skeleton AI
- Golden Skeleton AI
- Flying Boss AI

### World Systems
- Checkpoint System
- Save & Load System
- Moving Platforms
- Environmental Hazards

---

# How It Works

## 1. Character Movement

```text
Player Input
      │
      ▼
Player Controller
      │
      ▼
Movement State
      │
      ▼
Physics Controller
      │
      ▼
Animator
```

The player controller handles every movement action before passing the final state to the animation system.

### Mechanics

- Running
- Variable Jump Height
- Coyote Time
- Jump Buffering
- Dash
- Roll

---

## 2. Combo Combat

```text
Attack Input
      │
      ▼
Combo Controller
      │
      ▼
Play Attack Animation
      │
      ▼
Animation Event
      │
      ▼
Enable Hitbox
      │
      ▼
Enemy Hurtbox
      │
      ▼
Damage System
      │
      ▼
Hit Reaction
```

Each attack is animation-driven. Damage is only dealt while the weapon hitbox is active, ensuring accurate hit timing and responsive combat.

### Mechanics

- Four-hit combo
- Combo chaining
- Animation events
- Hitboxes
- Hurtboxes
- Knockback
- Invincibility frames

---

## 3. Shield Bash

```text
Shield Bash Input
        │
        ▼
Perfect Timing Window
        │
        ▼
Enemy Attack
        │
        ▼
Timing Check
   ┌───────────────┐
   │               │
Success        Failed
   │               │
Enemy Stunned   Player Takes Damage
```

Shield Bash is a high-risk defensive ability. Blocking at the correct moment interrupts the enemy attack and creates an opening for a counter attack.

### Mechanics

- Perfect timing detection
- Enemy interruption
- Pushback effect
- Counter attack opportunity

---

## 4. Enemy AI

```text
Idle
 │
 ▼
Patrol
 │
 ▼
Detect Player
 │
 ▼
Chase
 │
 ▼
Attack
 │
 ▼
Recover
 │
 └──────────────┐
                ▼
             Repeat
```

All enemies share the same reusable state machine while each enemy has its own movement style and attack behaviour.

### Enemy Types

#### Bush Monster

- Close-range spike attack
- Short attack range
- Teaches attack timing

---

#### White Skeleton

- Sword Slash
- Sword Thrust
- Fast melee enemy

---

#### Golden Skeleton

- Rising Axe Attack
- Wide Side Swing
- Heavy damage
- Slower attack speed

---

## 5. Boss AI

```text
Idle
 │
 ▼
Fly Around Arena
 │
 ▼
Choose Attack
 ├──────────────┐
 │              │
 ▼              ▼
Ground Smash  Summon Bat
 │              │
 └──────┬───────┘
        ▼
Recovery
        │
        ▼
Repeat
```

The boss constantly moves around the arena before choosing its next attack. Recovery windows give the player safe opportunities to attack.

### Attacks

#### Ground Smash

- Tracks the player's position
- Dives into the ground
- Deals area damage
- Recovery animation after landing

#### Bat Summon

- Spawns a flying bat
- Bat targets the player
- Explodes on impact
- Acts as a moving projectile

---

## 6. Damage System

```text
Attack Hitbox
       │
       ▼
Hurtbox
       │
       ▼
Damage Receiver
       │
       ▼
Health Component
       │
       ▼
Hit Reaction
       │
       ▼
Death
```

Every character uses the same damage pipeline, making the combat system reusable across players, enemies, and bosses.

### Features

- Shared health system
- Shared damage interface
- Knockback
- Death handling
- Invincibility frames

---

## 7. Checkpoint System

```text
Player
   │
   ▼
Checkpoint Trigger
   │
   ▼
Save Progress
   │
   ▼
Update Respawn Position
```

Whenever a checkpoint is activated, the player's progress is saved and becomes the new respawn location.

### Saved Data

- Player position
- Player health
- Activated checkpoints
- Current game progress

---

## 8. Animation System

```text
Gameplay State
       │
       ▼
Animator
       │
       ▼
Animation
       │
       ▼
Animation Events
       │
       ▼
Gameplay Logic
```

Animations are responsible for triggering gameplay events instead of relying only on timers.

### Animation Events

- Enable attack hitboxes
- Disable attack hitboxes
- Play sound effects
- Spawn particles
- End attack states
- Trigger recovery states

---

# Key Takeaways

- Responsive combat depends heavily on accurate animation timing.
- Separating gameplay systems into independent components improves scalability and maintainability.
- Animation Events provide reliable synchronization between visuals and gameplay.
- Reusable state machines reduce duplicated code across different enemy types.
- A shared damage pipeline makes it easy to introduce new characters, enemies, weapons, and bosses.
- Designing systems with modularity in mind allows the project to grow without requiring major refactoring.
