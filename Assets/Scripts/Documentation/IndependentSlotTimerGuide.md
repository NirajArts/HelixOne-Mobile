# Independent SlotTimer System Documentation

## Overview

The SlotTimer has been refactored to be completely independent and reusable across different stages with varying completion rules. This modular design allows each stage to define its own completion logic while maintaining a consistent timer interface.

## Architecture

### Core Components

1. **SlotTimer.cs** - The independent timer component
2. **BaseStageTimerController.cs** - Abstract base class for stage controllers
3. **Stage1TimerController.cs** - Transaction collection logic for Stage 1
4. **Stage2TimerController.cs** - Example objective-based logic for Stage 2

## SlotTimer (Independent Component)

### Key Features
- **No hardcoded dependencies** on specific game logic
- **External completion conditions** via delegates/functions
- **Customizable timer completion behavior**
- **Events for external control** and monitoring

### Public Methods
```csharp
// External control methods
void SetCompletionCondition(System.Func<bool> condition)
void SetTimerCompleteBehavior(System.Func<bool> behavior)
void ForceCompleteTimer()

// Timer control
void StartTimer()
void StopTimer()
void ResetTimer()

// State queries
bool IsRunning()
bool IsCompleted()
float GetProgress()
float GetRemainingRealTime()
float GetRemainingDisplayTime()
```

### Events
- `OnTimerComplete` - Triggered when timer completes (early or timeout)
- `OnTimerStart` - Triggered when timer starts
- `OnTimerUpdate` - Triggered every frame with progress (0-1)

## Stage Controller System

### BaseStageTimerController

Abstract base class that provides:
- Common SlotTimer reference management
- Auto-finding of SlotTimer components
- Debug logging capabilities
- Standard interface for all stage controllers

### Required Override Methods
```csharp
protected abstract void SetupStageLogic();
protected abstract bool CheckStageCompletionCondition();
protected abstract bool HandleStageTimerCompletion();
public abstract float GetStageProgress();
public abstract void ResetStageData();
```

## Stage-Specific Implementations

### Stage 1: Transaction Collection
**File**: `Assets/Scripts/Interactables/IngameWeb3/IngameWeb3-St1/Stage1TimerController.cs`

**Completion Rule**: All transactions must be collected
**Timer Behavior**: Always triggers level failed on timeout
**Dependencies**: InGame_TxManager

```csharp
// Usage example
Stage1TimerController stage1 = FindObjectOfType<Stage1TimerController>();
float progress = stage1.GetStageProgress(); // 0.0 to 1.0
int remaining = stage1.GetRemainingTransactions();
```

### Stage 2: Objective-Based (Example)
**File**: `Assets/Scripts/Interactables/IngameWeb3/IngameWeb3-St2/Stage2TimerController.cs`

**Completion Rule**: Complete specified number of objectives
**Timer Behavior**: Triggers level complete if objectives done, level failed if timeout
**Dependencies**: None (self-contained objective system)

```csharp
// Usage example
Stage2TimerController stage2 = FindObjectOfType<Stage2TimerController>();
stage2.CompleteObjective(); // Mark one objective as complete
float progress = stage2.GetObjectiveProgress();
```

## How to Create a New Stage

### Step 1: Create Stage Controller
```csharp
public class Stage3TimerController : BaseStageTimerController
{
    [Header("Stage 3 Specific Settings")]
    public int enemiesDefeated = 0;
    public int totalEnemies = 10;
    
    protected override void SetupStageLogic()
    {
        slotTimer.SetCompletionCondition(CheckStageCompletionCondition);
        slotTimer.SetTimerCompleteBehavior(HandleStageTimerCompletion);
    }
    
    protected override bool CheckStageCompletionCondition()
    {
        return enemiesDefeated >= totalEnemies;
    }
    
    protected override bool HandleStageTimerCompletion()
    {
        // Custom logic for what happens when timer expires
        if (enemiesDefeated >= totalEnemies)
        {
            GameManager.Instance.LevelComplete();
            return false; // Don't trigger level failed
        }
        return true; // Trigger level failed
    }
    
    public override float GetStageProgress()
    {
        return (float)enemiesDefeated / totalEnemies;
    }
    
    public override void ResetStageData()
    {
        enemiesDefeated = 0;
    }
    
    public void DefeatEnemy()
    {
        enemiesDefeated++;
        // Timer will auto-complete when all enemies defeated
    }
}
```

### Step 2: Setup in Scene
1. Add SlotTimer component to scene
2. Add your stage controller component to scene or same GameObject
3. SlotTimer will be auto-found by the controller
4. Configure timer settings in SlotTimer inspector
5. Configure stage-specific settings in your controller inspector

## Migration from Old System

### For Existing Stage 1 Scenes
1. Add `Stage1TimerController` component to scene
2. Remove any direct SlotTimer transaction references
3. The controller will automatically connect SlotTimer with InGame_TxManager

### For New Stages
1. Create a new stage controller inheriting from `BaseStageTimerController`
2. Implement the required abstract methods
3. Add the controller to your scene with SlotTimer

## Benefits of New System

### ✅ Independence
- SlotTimer works without any specific game logic dependencies
- Can be used in any type of level or mini-game

### ✅ Modularity
- Each stage defines its own completion rules
- Easy to add new stages with different mechanics

### ✅ Reusability
- Same SlotTimer component works across all stages
- Stage controllers can be swapped without changing timer setup

### ✅ Maintainability
- Clear separation of concerns
- Stage-specific logic contained in dedicated controllers

### ✅ Extensibility
- Easy to add new completion conditions
- Support for complex multi-condition scenarios

## Examples of Different Stage Types

### Collection-Based (Stage 1)
- Collect all transactions
- Timer stops early when complete
- Fails on timeout

### Objective-Based (Stage 2)
- Complete multiple objectives
- Timer continues or stops based on settings
- Can succeed or fail on timeout

### Survival-Based (Stage 3 Example)
- Survive for the full timer duration
- Timer completing = success
- Early completion not applicable

### Race-Based (Stage 4 Example)
- Reach goal before timer expires
- Early completion = success
- Timeout = failure

### Puzzle-Based (Stage 5 Example)
- Solve puzzle within time limit
- Multiple solution methods possible
- Bonus time for efficient solutions

This new system provides the flexibility to implement any of these stage types while maintaining a consistent timer interface throughout your game.
