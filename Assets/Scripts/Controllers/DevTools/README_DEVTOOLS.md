# Dev Tools Setup Guide

## Quick Setup (Recommended)

1. **Open your scene** (e.g., Level_1)
2. **In Unity Editor** (NOT in play mode), go to menu: **BowMaster > Setup Dev Tools**
3. This will automatically:
   - Create a Canvas (if one doesn't exist)
   - Create the DevToolsPanel UI
   - Create a DevToolsController GameObject
   - Wire everything together

4. **Save your scene**

5. **Enter Play Mode** and press **F1** to toggle the dev tools panel!

## Manual Setup (Alternative)

If you prefer to set it up manually:

1. **Create an empty GameObject** in your scene (right-click in Hierarchy > Create Empty)
2. **Name it** "DevToolsController"
3. **Add Component** > Search for "DevToolsController" and add it
4. **Add Component** > Search for "DevToolsView" and add it
5. **Run the SetupDevTools menu** to create the UI, OR manually create the UI elements

## Troubleshooting

### F1 doesn't work?
- Make sure you're in **Play Mode**
- Check the Console for any errors
- Verify that a GameObject with `DevToolsController` component exists in the scene
- Check that the GameObject is **active** (not disabled)

### UI doesn't appear?
- Make sure you ran **BowMaster > Setup Dev Tools** in the editor (not play mode)
- Check that a Canvas exists in your scene
- Look for "DevToolsPanel" in the Hierarchy

### Functions work but no UI?
- The functions will still work via code/inspector even without the UI
- Press F1 to see available commands in the console
- You can call functions directly from code: `FindObjectOfType<DevToolsController>().KillAllEnemies()`

## Available Functions

- `KillAllEnemies()` - Kill all enemies on screen
- `WinLevel()` - Complete the current level
- `HealCastle()` - Restore castle to full health
- `DamageCastle(int damage)` - Damage castle (default: 10)
- `ToggleGodMode()` - Make castle invincible
- `SetTimeScale(float scale)` - Change game speed (0-10x)
- `ResetTimeScale()` - Reset to 1x
- `RestartLevel()` - Reload current level
- `LoadNextLevel()` - Go to next level
- `LoadMainMenu()` - Return to main menu

