# Project Overview
- **Game Title**: Thorn
- **High-Level Concept**: A challenging 2D top-down arena combat game where a player fights waves of crawling/slithering enemies and bosses, unlocking permanent stat upgrades and powerful elemental abilities.
- **Players**: Single-player
- **Inspiration / Reference Games**: Risk of Rain, Hades, Vampire Survivors, 2D Arena Battlers
- **Tone / Art Direction**: Retro pixel-art style with neon visual effects
- **Target Platform**: PC (StandaloneWindows64)
- **Screen Orientation / Resolution**: Landscape (1920x1080)
- **Render Pipeline**: Universal Render Pipeline (URP)

# Game Mechanics
## Core Gameplay Loop
The player spawns in an arena, faces waves of unique enemies (gnomes, snakes, spiders), and handles boss fights at regular milestones (Wave 6, 12, 18). Clearing waves gives rewards, heals the player, and unlocks permanent stats (Max HP, Damage) or elemental upgrades.

## Controls and Input Methods
- **WASD**: Movement
- **Left Click**: Attack
- **Right Click**: Block / Guard

## New Mechanic: Elementals
- **Starting Choice**: At the start of the game, the player is presented with a 3-choice screen to pick their core element: **Poison**, **Dark**, or **Lightning**.
- **Activation Chance**: Every attack has a base **15% chance** to activate the selected element.
- **Boss Scaling**: After every boss fight (Wave 6, 12, 18), the activation chance **doubles** (15% -> 30% -> 60% -> 100%).
- **AOE Propagation**: When an element activates on a hit target, it spreads the effect to all other enemies within a **3-unit radius**.
- **Damage over Time (DoT)**: Affected enemies are poisoned, cursed, or electrocuted, losing **10 HP per second** continuously.
- **Visual Feedback**: Enemies are tinted matching their active element (Green for Poison, Purple for Dark, Yellow for Lightning) and play a looped sprite animation above their head.

# UI
A new **Elemental Selection Panel** will overlay the center of the screen at the very beginning of the run (before the tutorial panel):
- **Layout**: Dark-themed panel with translucent backdrop.
- **Elements**:
  - Title: "SELECT YOUR ELEMENT"
  - Three distinct selection cards (Buttons):
    - **Poison**: "15% chance to infect target and nearby enemies in a radius. Deals 10 DPS." (Green Theme)
    - **Dark**: "15% chance to curse target and nearby enemies in a radius. Deals 10 DPS." (Purple Theme)
    - **Lightning**: "15% chance to shock target and nearby enemies in a radius. Deals 10 DPS." (Yellow Theme)
  - Layout is controlled using a horizontal layout group for responsiveness.
- **Sequence**: Once an element is selected, the choice is saved to the player, the selection panel hides, and the standard tutorial panel is shown.

# Key Asset & Context

We will create/modify the following key assets:

### 1. `ElementalManager.cs` (New Script)
Stores the chosen element type, manages the current activation chance, handles doubling logic, and holds references to the sliced spritesheets for the looping visual effects.
```csharp
public enum ElementalType { None, Poison, Dark, Lightning }

public class ElementalManager : MonoBehaviour
{
    public static ElementalManager Instance { get; private set; }
    
    public ElementalType SelectedElement { get; private set; } = ElementalType.None;
    public float ActivationChance { get; private set; } = 0.15f; // 15%

    [Header("Elemental Sprite Animations")]
    public Sprite[] poisonSprites;
    public Sprite[] darkSprites;
    public Sprite[] lightningSprites;
    
    // Handles doubling chance
    public void DoubleChance();
    // Sets the element
    public void SetElement(ElementalType type);
}
```

### 2. `ElementalStatusEffect.cs` (New Script)
Added dynamically to enemies when affected. Handles ticking the 10 damage/sec, applying color tints, and playing the sprite animation loop.
```csharp
public class ElementalStatusEffect : MonoBehaviour
{
    private float damagePerSecond = 10f;
    private ElementalType elementType;
    
    public void Initialize(ElementalType type);
}
```

### 3. `GameUIManager.cs` (Modified Script)
Will be updated to show the selection panel at the start, handle selection callbacks, and proceed to the tutorial sequence.

### 4. `PlayerCombat.cs` (Modified Script)
When hitting an enemy, rolls a random chance. If it succeeds, triggers the elemental AOE spread.

### 5. `WaveManager.cs` (Modified Script)
When a boss wave (Wave 6, 12, 18) is successfully cleared, calls the `ElementalManager` to double the activation chance.

# Implementation Steps

- **Step 1: Implement `ElementalManager.cs` and `ElementalStatusEffect.cs`**
  - **Description**: Code the core data structures, singleton manager, and dynamic mono-behavior DoT system that damages `EnemyHealth` and `Boss_HP` components. Include a looping frame animator for sprites.
  - **Assigned role**: developer
  - **Dependencies**: None
  - **Parallelizable**: Yes

- **Step 2: Modify `PlayerCombat.cs` to trigger elementals on successful hits**
  - **Description**: Add chance rolls to `ApplyPendingAttack()`. If the roll succeeds, apply status to the target and run `Physics2D.OverlapCircleAll` within a 3-unit radius to propagate the effect to nearby enemies.
  - **Assigned role**: developer
  - **Dependencies**: Step 1
  - **Parallelizable**: No

- **Step 3: Update `WaveManager.cs` to scale the chance**
  - **Description**: Hook into `OnEnemyKilled()` when `isBossWave` clears to double the activation chance through `ElementalManager.Instance.DoubleChance()`.
  - **Assigned role**: developer
  - **Dependencies**: Step 1
  - **Parallelizable**: Yes

- **Step 4: Create the Elemental Selection UI**
  - **Description**: Build the UGUI selection card panel inside the existing `UICanvas` hierarchy. Add three beautifully colored choice buttons. Link them to a newly declared field in `GameUIManager.cs`.
  - **Assigned role**: developer
  - **Dependencies**: Step 1
  - **Parallelizable**: No

- **Step 5: Integrate UI sequencing in `GameUIManager.cs`**
  - **Description**: Modify `Start()` to show the selection panel first. When a button is clicked, set the element on `ElementalManager`, disable the selection panel, and enable the tutorial panel to start the game flow.
  - **Assigned role**: developer
  - **Dependencies**: Step 4
  - **Parallelizable**: No

# Verification & Testing
- **UI Test**: Verify that launching the game displays the Elemental Selection UI first with 3 buttons. Clicking any button should successfully open the tutorial panel.
- **DoT Test**: Pick Poison at the start, modify/force the activation chance to 100% in a test inspect tool, hit an enemy, and check if they turn green and tick down 10 HP per second until death.
- **Propagation Test**: Group multiple gnomes close together, trigger an element on one, and check if all adjacent gnomes within 3 units receive the same visual tint and DoT.
- **Double Chance Test**: Complete Wave 6 (first boss wave). Verify that the activation chance in `ElementalManager` prints a debug log showing it doubled to 30%.
