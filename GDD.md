# Shadow Ruins (2D Action Platformer)

## 1. Project Overview

- **Purpose:** Demonstrate fundamental 2D physics, responsive character movement, melee combat systems, enemy AI behaviors, and state persistence.
- **Portfolio Goal:** Prove capability in building custom platformer controllers, gameplay state machines, combat interactions, and robust checkpoint/save systems.
- **Target Role:** Gameplay Programmer / Generalist Programmer.
- **Project Scope:** 2 weeks. Fantasy castle/cave environment, female knight character, enemy creatures, and a guardian boss encounter.

---

## 2. High-Level Game Concept

- **Concept Summary:** A 2D action platformer set in ancient cursed ruins. The player controls a lost female knight exploring forgotten underground castles, overcoming platforming challenges, defeating monsters, activating checkpoints, and defeating an ancient guardian protecting the ruins.

- **Player Experience:** Precise platforming movement combined with responsive melee combat, rewarding exploration, and clear combat feedback.

- **Core Gameplay Loop:**  
  Main Menu -> Explore Ruins -> Traverse Platforming Challenges -> Defeat Enemies -> Activate Checkpoints -> Fight Guardian Boss -> Complete Level -> Save Progress.

- **Example Session:** The player wall-jumps across a broken castle wall, dashes through a collapsing corridor, defeats a group of skeleton warriors using sword combos, activates a checkpoint banner, and enters an ancient guardian boss arena.

---

## 3. Design Goals

- **Primary Goals:**
  - Custom or fine-tuned 2D character controller with:
    - Variable jump height
    - Coyote time
    - Roll movement
    - Dash movement

  - Responsive melee combat system with:
    - Four-hit attack combo
    - Hit detection
    - Damage reactions
    - Knockback
    - Invincibility frames
    - Perfect timing Shield Bash mechanic

- **Secondary Goals:**
  - Checkpoint save system writing player position, health, and progress state to local storage.
  - Enemy AI state machines with different behaviors and attack patterns.

- **Technical Goals:**
  - Smooth physics interaction using proper FixedUpdate synchronization.
  - Decoupled gameplay systems using events and reusable components.

---

## 4. Gameplay Systems

### Player Systems:

- 2D character controller
- Running and jumping
- Dash ability
- Roll ability
- Sword combat
- Four-hit combo system
- Shield Bash (perfect timing)
- Damage response
- Death and respawn
- Checkpoint respawn

### Enemy Systems:

#### Bush Monster

- Idle behavior
- Chase behavior
- Spike attack
- Damage reactions
- Death handling

#### White Skeleton

- Patrol behavior
- Player detection
- Chase behavior
- Two melee attacks
- Damage reactions
- Death handling

#### Golden Skeleton

- Patrol behavior
- Player detection
- Chase behavior
- Heavy melee attacks
- Damage reactions
- Death handling

### Boss System:

- Flying movement
- Ground Smash attack
- Bat Summon attack
- Attack cooldown management
- Damage reactions
- Death sequence

### Platform Systems:

- Moving platforms
- Falling platforms
- Environmental hazards
- Trigger-based interactions

---

## 5. Combat Design

### Player Character: Female Knight

The player fights using a one-handed sword and shield. Combat focuses on timing, positioning, and reading enemy attacks rather than button mashing.

### Basic Combo

#### Attack 1: Overhead Slash

- Fast downward sword attack.
- Starts the combo chain.
- Fast recovery.

#### Attack 2: Horizontal Slash

- Wide horizontal sword swing.
- Continues the combo.
- Good for multiple nearby enemies.

#### Attack 3: Sword Thrust

- Forward thrust attack.
- Longer reach than the previous attacks.
- Useful for keeping distance from enemies.

#### Attack 4: Spinning Slash

- Single 360-degree sword swing.
- Highest damage attack in the combo.
- Longer recovery time.

### Defensive Abilities

#### Dash

- Fast horizontal movement.
- Grants a short invincibility window.
- Can be used to avoid attacks.

#### Roll

- Quick dodge movement.
- Short invincibility window.
- Useful for repositioning during combat.

#### Shield Bash (Perfect Timing)

- Defensive ability performed with the shield.
- If timed correctly:
  - Blocks the incoming attack.
  - Pushes the enemy backwards.
  - Creates a short opening for a counter attack.
- If mistimed:
  - The player takes normal damage.
  - No pushback is applied to the enemy.

---

## 6. Enemy Design

### Enemy 1: Bush Monster

#### Purpose:

Demonstrate a simple melee enemy that teaches players to recognize attack timing.

#### Behavior States:

- Idle
- Chase
- Attack
- Take Hit
- Dead

#### Attack:

##### Spike Burst

- The monster grows sharp spikes around its body.
- Damages the player at close range.
- Short attack range with a clear animation before dealing damage.

#### Systems Demonstrated:

- Basic melee AI
- Attack cooldowns
- Animation events
- Damage handling

---

### Enemy 2: White Skeleton

#### Purpose:

Demonstrate standard melee enemy AI with multiple attack choices.

#### Behavior States:

- Idle
- Walk
- Detect Player
- Chase
- Attack
- Take Hit
- Dead

#### Attacks:

##### Attack 1: Sword Slash

- Quick horizontal sword swing.
- Short range.
- Fast recovery.

##### Attack 2: Sword Thrust

- Fast forward thrust.
- Longer range than the slash attack.
- Narrow hit area.

#### Systems Demonstrated:

- AI state machine
- Multiple attack selection
- Detection range
- Combat timing
- Damage reactions

---

### Enemy 3: Golden Skeleton

#### Purpose:

Demonstrate a stronger enemy with slower but more powerful attacks.

#### Behavior States:

- Idle
- Walk
- Detect Player
- Chase
- Attack
- Take Hit
- Dead

#### Attacks:

##### Attack 1: Rising Axe

- Upward axe swing.
- Medium attack speed.
- High damage.

##### Attack 2: Side Sweep

- Wide horizontal axe swing.
- Covers a larger area.
- Longer recovery after attacking.

#### Systems Demonstrated:

- Heavy enemy behavior
- Attack telegraphs
- Combat timing
- Multiple attack patterns

---

## 7. Boss Design

### Ruins Guardian

An ancient flying creature protecting the forgotten ruins. The boss constantly moves around the arena and attacks from different positions, forcing the player to react and find safe opportunities to attack.

#### Behavior States:

- Idle
- Fly
- Ground Smash
- Recover
- Summon Bat
- Take Hit
- Dead

#### Attack 1: Ground Smash

- The boss flies above the player's current position.
- It quickly dives into the ground.
- The impact creates an area attack around the landing position.
- After landing, the boss enters a recovery animation, giving the player a short opportunity to attack.

#### Attack 2: Bat Summon

- The boss summons a small flying bat.
- The bat immediately flies toward the player's current position.
- On contact with the player, it explodes and deals damage.
- The bat disappears after hitting the player or another obstacle.

#### Systems Demonstrated:

- Flying AI
- Boss attack scheduling
- Area damage attacks
- Projectile spawning
- Attack cooldowns
- Recovery windows

---

## 8. Technical Architecture

- **Main Components:**
  - `PlayerController2D`
  - `PlayerAnimationController`
  - `PlayerStateMachine`
  - `PlayerCombatController`
  - `EnemyAIBase`
  - `BushMonster`
  - `WhiteSkeleton`
  - `GoldenSkeleton`
  - `BossController`
  - `CheckpointManager`
  - `SaveSystem2D`

- **Communication:**
  - Combat systems communicate through damage events.
  - Enemy AI reacts to player detection events.
  - Boss controller manages attack selection and cooldowns.
  - Save system serializes player progress and checkpoint data.

---

## 9. Detailed Feature Breakdown

### Feature: Responsive 2D Character Controller & Combat System

- **Purpose:**
  - Provide precise platforming movement combined with responsive melee gameplay.

- **Implementation:**
  - Custom movement controller using raycast/boxcast collision checks.
  - Combat system using attack states, hitboxes, animation events, and damage interfaces.
  - Player actions controlled through a state machine.

- **Classes:**
  - `PlayerController2D`
  - `PlayerCombatController`
  - `DamageReceiver`
  - `Hitbox`

---

## 10. Development Roadmap

- **Phase 1:**
  - [x] Build 2D character controller.
  - [x] Implement movement mechanics:
    - [x] Variable jump height
    - [x] Coyote time
    - [x] Roll
    - [x] Dash

- **Phase 2:**
  - [x] Create sword combat system.
  - [ ] Implement combat mechanics:
    - [x] Four-hit combo
    - [x] Shield Bash
    - [ ] Hit detection
    - [ ] Damage system

- [ ] **Phase 3:**
  - [ ] Build enemy AI systems.
  - [ ] Implement:
    - [ ] Bush Monster
    - [ ] White Skeleton
    - [ ] Golden Skeleton
    - [ ] AI state machines

- **Phase 4:**
  - [ ] Create boss arena.
  - [ ] Implement:
    - [ ] Guardian boss
    - [ ] Checkpoints
    - [ ] Save system
    - [ ] Final polish

---

## 11. MVP Version

- 1 complete castle/cave level.
- Player movement system.
- Sword combat system.
- Shield Bash mechanic.
- 3 enemy types:
  - Bush Monster
  - White Skeleton
  - Golden Skeleton
- 1 guardian boss.
- Working checkpoint and save system.

---

## 12. Learning Objectives / Technical Concepts Demonstrated

- **Custom Character Controller Development**
  - Learn how to build responsive player movement without relying entirely on Unity's default physics controller.
  - Understand acceleration, deceleration, gravity handling, velocity control, collision detection, and movement constraints.

- **Advanced Platformer Movement Techniques**
  - Implement professional platforming mechanics:
    - Coyote time
    - Variable jump height
    - Dash movement
    - Roll movement

- **Combat System Architecture**
  - Build reusable melee combat systems using:
    - Combo attacks
    - Hitboxes
    - Hurtboxes
    - Damage interfaces
    - Knockback handling
    - Invincibility frames
    - Perfect timing Shield Bash

- **State Machine Architecture**
  - Create reusable state machines for:
    - Player actions
    - Enemy behaviors
    - Boss behavior
    - Combat transitions

- **Physics Synchronization**
  - Understand Update vs FixedUpdate.
  - Handle movement, collisions, and physics interactions correctly.

- **Event-Driven Gameplay Systems**
  - Build communication between gameplay systems using events and delegates.
  - Practice decoupling:
    - Damage events
    - Enemy death events
    - Checkpoint activation
    - Boss attack selection

- **AI Fundamentals**
  - Learn:
    - Enemy detection
    - Patrol systems
    - Chase behaviors
    - Attack decision logic
    - Recovery states

- **Save System Fundamentals**
  - Learn serialization and persistence using JSON.
  - Store:
    - Player position
    - Health state
    - Checkpoint progress
    - World state
