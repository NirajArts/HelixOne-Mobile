# GameLevelManager Event System Documentation

## Overview
The GameLevelManager has been refactored to be completely independent from UI systems. Instead of directly calling UI methods, it now uses a clean event-driven architecture that allows UI components to subscribe to level management events.

## Architecture Benefits
- **🔄 Loose Coupling**: No direct dependencies between GameLevelManager and UI systems
- **📡 Event-Driven**: Clean communication through static events
- **🎯 Modular**: UI systems can subscribe/unsubscribe as needed
- **🔧 Extensible**: New systems can easily listen to level events
- **♻️ Memory Safe**: Proper event cleanup prevents memory leaks

## Events Available

### 1. OnLevelCompleted
**Type**: `Action`  
**Triggered**: When a level is successfully completed  
**Usage**: UI shows win screen, updates score, etc.

### 2. OnLevelFailed
**Type**: `Action`  
**Triggered**: When player dies or level fails  
**Usage**: UI shows death/failure screen, restart options, etc.

### 3. OnLevelTransitionStarted
**Type**: `Action<string>`  
**Parameter**: `levelName` - Name of the level being loaded  
**Triggered**: When scene transition begins  
**Usage**: UI shows loading screen, disables input, etc.

### 4. OnTeleportationProgress
**Type**: `Action<float>`  
**Parameter**: `progress` - Float value from 0 to 1  
**Triggered**: During teleportation countdown (real-time updates)  
**Usage**: UI shows countdown timer, progress bar, etc.

## Implementation Examples

### For Game_UI_Manager (Already Implemented)
```csharp
void Start()
{
    // Subscribe to GameLevelManager events
    GameLevelManager.OnLevelCompleted += OnLevelCompleted;
    GameLevelManager.OnLevelFailed += OnLevelFailed;
    GameLevelManager.OnLevelTransitionStarted += OnLevelTransitionStarted;
    GameLevelManager.OnTeleportationProgress += OnTeleportationProgress;
}

private void OnLevelCompleted()
{
    UpdateLevelCompleteUI(); // Your existing method
}

private void OnLevelFailed()
{
    UpdateLevelFailedUI(); // Your existing method
}

void OnDestroy()
{
    // Always unsubscribe to prevent memory leaks
    GameLevelManager.OnLevelCompleted -= OnLevelCompleted;
    GameLevelManager.OnLevelFailed -= OnLevelFailed;
    GameLevelManager.OnLevelTransitionStarted -= OnLevelTransitionStarted;
    GameLevelManager.OnTeleportationProgress -= OnTeleportationProgress;
}
```

### For Audio Manager
```csharp
void Start()
{
    GameLevelManager.OnLevelCompleted += PlayVictorySound;
    GameLevelManager.OnLevelFailed += PlayDefeatSound;
}

private void PlayVictorySound()
{
    // Play victory music/sound
}

private void PlayDefeatSound()
{
    // Play defeat music/sound
}
```

### For Analytics System
```csharp
void Start()
{
    GameLevelManager.OnLevelCompleted += LogLevelCompletion;
    GameLevelManager.OnLevelFailed += LogLevelFailure;
}

private void LogLevelCompletion()
{
    // Send analytics data
    Analytics.LogEvent("level_completed", currentLevelName);
}

private void LogLevelFailure()
{
    // Send analytics data
    Analytics.LogEvent("level_failed", currentLevelName);
}
```

## GameLevelManager Public API

### Properties
- `bool IsTransitioning()` - Check if currently transitioning
- `string GetCurrentLevelName()` - Get current level name
- `bool isTeleported` - Current teleportation state
- `bool isPlayerDead` - Current player death state

### Methods
- `void TriggerTeleportation()` - Start teleportation countdown
- `void LoadNextLevel()` - Load the next level (uses nextLevelName field)
- `void LoadLevel(string levelName)` - Load specific level by name
- `void RestartLevel()` - Restart current level
- `void LoadLevelByIndex(int buildIndex)` - Load level by build index
- `void OnPlayerDeath()` - Trigger player death (broadcasts OnLevelFailed)
- `void ResetPlayerState()` - Reset player states
- `void SetNextLevel(string levelName)` - Set next level name

### Setup Fields (Inspector)
- `float transitionDelay` - Time before auto-transition
- `Animator fadeScreen` - Fade animation controller
- `bool allowDirectLoading` - Skip UI delays
- `float fadeDuration` - Fade animation duration
- `string nextLevelName` - Target level for auto-transitions
- `string currentLevelName` - Current level (auto-detected if empty)

## Migration Guide

### From Old System
```csharp
// OLD - Direct UI dependency
if (gameUIManager != null)
    gameUIManager.UpdateLevelCompleteUI();
```

```csharp
// NEW - Event-driven
OnLevelCompleted?.Invoke();
```

### UI Component Setup
1. Remove `Game_UI_Manager` references from GameLevelManager
2. Add event subscription in UI component's `Start()` method
3. Add event unsubscription in UI component's `OnDestroy()` method
4. Handle events in private methods that call your existing UI update methods

## Best Practices
1. **Always Unsubscribe**: Prevent memory leaks by unsubscribing in OnDestroy()
2. **Null Checks**: Use `?.Invoke()` when broadcasting events
3. **Scene-Specific**: Events are static, so they work across all GameLevelManager instances
4. **Error Handling**: Wrap event handlers in try-catch for production code
5. **Performance**: Events are lightweight but avoid subscribing/unsubscribing every frame

## Compatibility
- ✅ Fully backward compatible with existing GameLevelManager public methods
- ✅ Game_UI_Manager now subscribes to events automatically
- ✅ Legacy methods marked with `[Obsolete]` for gradual migration
- ✅ Scene-specific behavior maintained
- ✅ All existing inspector settings preserved

The system is now completely independent and ready for use in any scene! 🚀
