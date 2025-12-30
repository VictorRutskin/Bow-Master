# Diagnostic Guide: Why Enemies Don't Spawn & Arrows Don't Shoot

This guide explains the dependency chains and how to diagnose issues.

## 🔴 Problem 1: No Enemies Spawn

### The Spawning Chain

```
GameManager (Start)
  └─> InitializeServices()
      └─> Registers IEnemyService, ISpawnService
          │
LevelController (Start)
  └─> Gets IEnemyService from ServiceLocator
      └─> If autoStart = true:
          └─> StartLevel()
              ├─> Checks: level (LevelAsset) assigned? ❌ FAILS HERE IF MISSING
              ├─> Checks: playerCastle assigned? ❌ FAILS HERE IF MISSING
              ├─> Checks: spawners list has entries? ❌ FAILS HERE IF EMPTY
              └─> RunLevel() coroutine
                  └─> For each wave:
                      └─> WaveController.RunWave()
                          └─> SpawnController.SpawnEnemy()
                              ├─> Needs: enemyPrefab (from WaveEntry) ❌ FAILS IF NULL
                              ├─> Needs: ISpawnService from ServiceLocator ❌ FAILS IF NULL
                              └─> Needs: defaultCastle Transform ❌ ENEMIES WON'T MOVE IF MISSING
```

### Checklist: Why Enemies Don't Spawn

**Step 1: Check GameManager**
- [ ] Is `GameManager` GameObject in the scene?
- [ ] Does `GameManager` component have `autoInitialize = true`?
- [ ] Check Console for: `[GameManager] Registered IEnemyService` and `[GameManager] Registered ISpawnService`
- **If missing:** Run `BowMaster > Setup Level Scene > Setup Current Scene`

**Step 2: Check LevelController**
- [ ] Is `LevelController` GameObject in the scene?
- [ ] In Inspector, check `LevelController` component:
  - [ ] `level` field: Is a `LevelAsset` assigned? (NOT "None")
  - [ ] `playerCastle` field: Is a `CastleController` assigned? (NOT "None")
  - [ ] `spawners` list: Does it have at least 1 entry?
  - [ ] `autoStart` checkbox: Is it checked?
- **If missing:** Run `BowMaster > Setup Level Scene > Setup Current Scene`

**Step 3: Check LevelAsset**
- [ ] Open the `LevelAsset` assigned to `LevelController`
- [ ] Check `waves` list: Does it have at least 1 wave?
- [ ] For each wave, check `entries` list: Does it have at least 1 entry?
- [ ] For each entry, check `enemyPrefab`: Is a prefab assigned? (Goblin or Troll)
- **If missing:** Create waves and assign enemy prefabs in the LevelAsset

**Step 4: Check SpawnController**
- [ ] Is at least one `SpawnController` GameObject in the scene?
- [ ] Is it added to `LevelController.spawners` list?
- [ ] Check `SpawnController.defaultCastle`: Is a Transform assigned? (should point to Castle)
- **If missing:** Run `BowMaster > Setup Level Scene > Setup Current Scene`

**Step 5: Check Console Messages**
Look for these error messages:
- `[LevelController] No LevelAsset assigned!` → Assign LevelAsset
- `[LevelController] playerCastle is not assigned!` → Assign CastleController
- `[LevelController] No spawners assigned!` → Add SpawnController to list
- `[LevelController] IEnemyService not found` → GameManager didn't initialize services
- `[SpawnController] No target set. Enemies won't move.` → Assign defaultCastle

### Quick Fix

Run the validation tool:
```
BowMaster > Validate Scene > Check Current Scene
```

This will tell you exactly what's missing!

---

## 🏹 Problem 2: Can't Shoot Arrows

### The Shooting Chain

```
GameManager (Start)
  └─> InitializeServices()
      └─> Registers IInputService
          │
TowerShooterController (Start)
  └─> Gets TowerInputController component
      └─> Initializes it with arrowSpawnPoint.position
          │
TowerInputController (Awake)
  └─> Gets IInputService from ServiceLocator
      └─> If null: ❌ SHOOTING WON'T WORK
          │
TowerInputController (Update)
  └─> Checks: _inputService != null? ❌ RETURNS EARLY IF NULL
      └─> Gets mouse world position via IInputService
          └─> IInputService.GetMouseWorldPosition()
              └─> Uses Camera.main ❌ RETURNS (0,0,0) IF NULL
                  │
      └─> On mouse click near arrowSpawnPoint:
          └─> Sets _isDragging = true
              │
      └─> On mouse drag:
          └─> Updates CurrentDragPosition
              │
      └─> On mouse release:
          └─> Fires OnShootRequested event
              │
TowerShooterController.HandleShootRequest()
  └─> Checks: arrowPrefab != null? ❌ RETURNS IF NULL
      └─> Checks: arrowSpawnPoint != null? ❌ RETURNS IF NULL
          └─> Instantiates arrow
              └─> Applies force to Rigidbody2D
```

### Checklist: Why Arrows Don't Shoot

**Step 1: Check GameManager & Services**
- [ ] Is `GameManager` GameObject in the scene?
- [ ] Check Console for: `[GameManager] Registered IInputService`
- [ ] Check Console for: `[GameManager] Camera.main is null!` warning
- **If missing:** Run `BowMaster > Setup Level Scene > Setup Current Scene`

**Step 2: Check Camera**
- [ ] Is there a Camera in the scene?
- [ ] Is the Camera tagged as "MainCamera"?
- [ ] Check: `Camera.main` should not be null (check in Console or Inspector)
- **If missing:** Tag your camera as "MainCamera"

**Step 3: Check TowerShooterController**
- [ ] Is `TowerShooterController` GameObject in the scene?
- [ ] In Inspector, check `TowerShooterController` component:
  - [ ] `arrowPrefab`: Is the Arrow prefab assigned? (NOT "None")
  - [ ] `arrowSpawnPoint`: Is a Transform assigned? (NOT "None")
- **If missing:** Run `BowMaster > Setup Level Scene > Setup Current Scene`

**Step 4: Check TowerInputController**
- [ ] Does the same GameObject have `TowerInputController` component? (auto-added by TowerShooterController)
- [ ] Check Console for: `[TowerInputController] IInputService not found` warning
- **If warning appears:** GameManager didn't initialize services - check GameManager exists

**Step 5: Check Input**
- [ ] Are you clicking near the arrow spawn point? (within `grabRadius`, default 3 units)
- [ ] Are you dragging away from the spawn point?
- [ ] Are you releasing the mouse button?
- **Note:** The shooting mechanic requires:
  1. Click near spawn point (within grab radius)
  2. Drag away (this sets the direction and force)
  3. Release (this fires the arrow)

**Step 6: Check Console Messages**
Look for these error messages:
- `[TowerInputController] IInputService not found` → GameManager didn't initialize
- `[TowerInputController] ServiceLocator.Instance is null` → GameManager missing
- `[GameManager] Camera.main is null!` → Camera not tagged as MainCamera
- `[TowerShooterController] Shooting arrow` → This should appear when shooting works

### Quick Fix

Run the validation tool:
```
BowMaster > Validate Scene > Check Current Scene
```

This will check:
- GameManager exists
- TowerShooterController has arrowPrefab and arrowSpawnPoint
- Camera.main exists

---

## 🔧 How to Use the Diagnostic Tools

### 1. Validation Tool
```
BowMaster > Validate Scene > Check Current Scene
```
This checks everything and reports what's missing.

### 2. Setup Tool
```
BowMaster > Setup Level Scene > Setup Current Scene
```
This auto-fixes missing references (if assets exist).

### 3. Check Console
Always check the Unity Console for error messages. The new system logs detailed errors explaining what's missing.

---

## 📋 Complete Setup Checklist

For a level scene to work, you need:

**Required GameObjects:**
- [ ] GameManager (with GameManager component)
- [ ] LevelController (with LevelController component)
- [ ] CastleController (with CastleController component)
- [ ] TowerShooterController (with TowerShooterController component)
- [ ] At least one SpawnController (with SpawnController component)
- [ ] Camera (tagged as "MainCamera")

**Required References:**
- [ ] LevelController.level → LevelAsset
- [ ] LevelController.playerCastle → CastleController
- [ ] LevelController.spawners → List with at least 1 SpawnController
- [ ] TowerShooterController.arrowPrefab → Arrow prefab
- [ ] TowerShooterController.arrowSpawnPoint → Transform
- [ ] SpawnController.defaultCastle → Castle Transform
- [ ] CastleController.stats → CastleStats ScriptableObject

**Required Assets:**
- [ ] LevelAsset (with waves configured)
- [ ] CastleStats ScriptableObject
- [ ] Arrow prefab
- [ ] Enemy prefabs (Goblin, Troll) referenced in LevelAsset waves

---

## 🎯 Most Common Issues

1. **"No enemies spawn"**
   - **90% of the time:** LevelController.level is not assigned
   - **80% of the time:** LevelController.spawners list is empty
   - **70% of the time:** LevelAsset has no waves or empty waves

2. **"Can't shoot arrows"**
   - **90% of the time:** TowerShooterController.arrowPrefab is not assigned
   - **80% of the time:** Camera.main is null (camera not tagged)
   - **70% of the time:** IInputService not initialized (GameManager missing)

3. **"Everything seems set but still doesn't work"**
   - Check Console for error messages
   - Run validation tool
   - Verify you're in a level scene, not MainMenu scene

---

## 🚀 Quick Start Fix

If nothing works, run these in order:

1. **Create assets:**
   ```
   BowMaster > Create Stats Assets > All Stats
   ```

2. **Setup scene:**
   ```
   BowMaster > Setup Level Scene > Setup Current Scene
   ```

3. **Validate:**
   ```
   BowMaster > Validate Scene > Check Current Scene
   ```

4. **Check Console** for any remaining errors

5. **Manually fix** anything the validation tool reports

6. **Test in Play mode**

