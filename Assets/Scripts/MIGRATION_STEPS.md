# Step-by-Step Migration Guide

Follow these steps in order to migrate your project to the new MVC architecture.

## Step 1: Create ScriptableObject Assets

1. Open Unity Editor
2. Go to menu: **BowMaster > Create Stats Assets > All Stats**
   - This creates:
     - `Assets/Data/ScriptableObjects/GoblinStats.asset`
     - `Assets/Data/ScriptableObjects/TrollStats.asset`
     - `Assets/Data/ScriptableObjects/CastleStats.asset`
     - `Assets/Data/ScriptableObjects/ArrowStats.asset`

## Step 2: Migrate Prefabs

1. Go to menu: **BowMaster > Migrate Prefabs > Migrate All Prefabs**
   - This updates:
     - Goblin.prefab
     - Troll.prefab
     - Arrow.prefab
   - Note: Castle prefab migration is done in scenes (see Step 3)

2. **Manual Steps for Prefabs:**
   - Open each prefab and verify components are correct
   - For Goblin/Troll: Set `deathVfxPrefab` on `EnemyController` if you had one
   - For Arrow: VFX reference should be copied automatically

## Step 3: Migrate Scenes

For each scene (Level_1, Level_2, Level_3, etc.):

1. Open the scene
2. Go to menu: **BowMaster > Migrate Scene > Migrate Current Scene**
   - This automatically:
     - Migrates Castle GameObject
     - Migrates Tower GameObject
     - Migrates LevelDirector to LevelController
     - Migrates Spawners to SpawnControllers

3. **Manual Verification:**
   - Check that `LevelController` has `playerCastle` reference set
   - Check that `LevelController.spawners` list contains all spawners
   - Verify `TowerShooterController` has arrow prefab and spawn point set
   - Verify `CastleController` has `CastleStats` assigned

## Step 4: Add GameManager to Scenes

1. Create empty GameObject named "GameManager"
2. Add `GameManager` component
3. (Optional) Add `GameStateController` component
4. Save scene

## Step 5: Test Everything

Test checklist:
- [ ] Enemies spawn correctly
- [ ] Enemies move toward castle
- [ ] Enemies attack castle
- [ ] Castle health bar updates
- [ ] Tower shoots arrows
- [ ] Arrows hit enemies
- [ ] Enemies take damage and die
- [ ] Death animations play
- [ ] Level progression works
- [ ] Waves spawn correctly
- [ ] Game over on castle destruction

## Step 6: Remove Old Code (After Verification)

**ONLY AFTER THOROUGH TESTING:**

1. Go to menu: **BowMaster > Remove Old Code > Remove All Old Code (CAREFUL!)**
   - This removes all old monolithic classes
   - Make sure you have backups!

2. Or remove individually:
   - **BowMaster > Remove Old Code > Remove Enemy System**
   - **BowMaster > Remove Old Code > Remove Castle System**
   - etc.

## Troubleshooting

### "ScriptableObject not found" errors
- Make sure you ran Step 1 to create the assets
- Check that assets are in `Assets/Data/ScriptableObjects/`

### Prefab references broken
- Re-assign references in prefab inspector
- Check that new components are added

### Scene migration didn't work
- Run migration again
- Check console for errors
- Manually add missing components

### Old code still referenced
- Search project for old class names (Enemy, CastleHealth, etc.)
- Update any custom scripts that reference old classes
- Remove old code only after all references are updated

## Files to Keep

These files are still used by the new system:
- `Assets/Scripts/Levels/LevelAsset.cs` - ScriptableObject
- `Assets/Scripts/Levels/LevelButton.cs` - UI component
- `Assets/Scripts/Levels/Progress.cs` - Save/load system
- `Assets/Scripts/Arrow/ArrowSelfDestruct.cs` - Still used by ArrowController

## Need Help?

Check:
- `MIGRATION_GUIDE.md` - Detailed architecture explanation
- `OLD_CODE_TO_REMOVE.md` - List of files to remove

