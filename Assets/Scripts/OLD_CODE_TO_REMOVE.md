# Old Code Removal Checklist

After verifying the new MVC architecture works correctly, the following old files can be removed:

## Enemy System
- `Assets/Scripts/Enemies/Enemy.cs` - Replaced by `EnemyController`, `EnemyMovementController`, `EnemyCombatController`
- `Assets/Scripts/Enemies/Goblin.cs` - No longer needed (use `EnemyStats` ScriptableObject instead)
- `Assets/Scripts/Enemies/Troll.cs` - No longer needed (use `EnemyStats` ScriptableObject instead)

## Castle System
- `Assets/Scripts/Castle/CastleHealth.cs` - Replaced by `CastleController`
- `Assets/Scripts/Castle/SimpleSproteHealthBar.cs` - Replaced by `CastleHealthBarView`

## Tower System
- `Assets/Scripts/Castle/TowerShooter.cs` - Replaced by `TowerShooterController`, `TowerInputController`, `TowerShooterView`

## Arrow System
- `Assets/Scripts/Arrow/ArrowDamage.cs` - Replaced by `ArrowController`
- `Assets/Scripts/Arrow/ArrowRotation.cs` - Replaced by `ArrowView` (rotation logic moved to view)
- `Assets/Scripts/Arrow/ArrowSelfDestruct.cs` - **KEEP** (still used by new system)

## Level System
- `Assets/Scripts/Levels/LevelDirector.cs` - Replaced by `LevelController`
- `Assets/Scripts/Enemies/Spawner.cs` - Replaced by `SpawnController` and `WaveController`

## UI System
- `Assets/Scripts/Levels/MainMenuUI.cs` - Replaced by `MainMenuView`
- `Assets/Scripts/Levels/CampaignUI.cs` - Replaced by `CampaignView`
- `Assets/Scripts/Levels/LevelCompletion.cs` - Replaced by `LevelCompletionView`
- `Assets/Scripts/Levels/LevelButton.cs` - **KEEP** (still used by new system)
- `Assets/Scripts/Levels/Progress.cs` - **KEEP** (still used by new system)

## Utilities
- `Assets/Scripts/AutoDestroy.cs` - Can be removed if not used elsewhere

## Migration Notes

1. **Before Removing:**
   - Test all game functionality thoroughly
   - Verify all prefabs work with new components
   - Ensure all scenes are updated
   - Check that no other scripts reference old classes

2. **After Removing:**
   - Update any remaining references
   - Clean up unused meta files
   - Update documentation

3. **Backward Compatibility:**
   - The new system includes compatibility layers that check for both old and new components
   - These can be removed once all old code is deleted

## Files to Keep

- `Assets/Scripts/Levels/LevelAsset.cs` - Still used (ScriptableObject)
- `Assets/Scripts/Levels/LevelButton.cs` - Still used by UI
- `Assets/Scripts/Levels/Progress.cs` - Still used for save/load
- `Assets/Scripts/Arrow/ArrowSelfDestruct.cs` - Still used by ArrowController

