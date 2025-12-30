# 🚨 Emergency Fix - Broken Scene References

## Problem
After deleting old code, the scene still references deleted scripts (CampaignUI, MainMenuUI, etc.), causing everything to disappear.

## Quick Fix (2 minutes)

### Step 1: Fix the Scene Reference
1. **In Unity Editor**, go to menu: **BowMaster > Fix Broken References > Fix MainMenu Scene**
2. This will add the new `CampaignView` component to replace the broken `CampaignUI`

### Step 2: Reassign References (IMPORTANT!)
1. **Open the MainMenu scene** (if not already open)
2. **In Hierarchy**, find and select the **"CampaignUI"** GameObject
3. **In Inspector**, you'll see the `CampaignView` component
4. **Reassign these fields:**
   - **Levels**: Click the "+" button 3 times, then set:
     - Element 0: `displayName = "Level 1"`, `sceneName = "Level_1"`
     - Element 1: `displayName = "Level 2"`, `sceneName = "Level_2"`
     - Element 2: `displayName = "Level 3"`, `sceneName = "Level_3"`
   - **Content Parent**: Drag the "Content" child GameObject (under CampaignUI > Scroll View > Viewport > Content)
   - **Level Button Prefab**: Drag the LevelButton prefab from `Assets/Prefabs/LevelButton.prefab`
5. **Save the scene** (Ctrl+S)

### Step 3: Check for MainMenuView
1. **In Hierarchy**, look for a GameObject that should have `MainMenuView` component
2. If you see a broken/missing script icon, select that GameObject
3. **In Inspector**, if there's a "Missing (MainMenuUI)" component:
   - Click the 3-dots menu on that component
   - Select "Remove Component"
   - Add `MainMenuView` component instead
   - Reassign `mainPanel` and `campaignPanel` references

### Step 4: Verify
1. **Play the scene**
2. You should see the menu UI
3. Click "Campaign" button - you should see level buttons

## Alternative: Manual Fix

If the menu doesn't work:

1. **Find broken references:**
   - Menu: **BowMaster > Fix Broken References > Find All Broken References**
   - Check console for warnings

2. **For each broken reference:**
   - Select the GameObject
   - Remove the broken component (3-dots menu > Remove Component)
   - Add the new component:
     - `CampaignUI` → `CampaignView`
     - `MainMenuUI` → `MainMenuView`

3. **Reassign all references** in Inspector

## Still Not Working?

1. **Check Console** for errors
2. **Verify these files exist:**
   - `Assets/Scripts/Views/UI/CampaignView.cs`
   - `Assets/Scripts/Views/UI/MainMenuView.cs`
   - `Assets/Scripts/Levels/LevelButton.cs`
   - `Assets/Scripts/Levels/Progress.cs`

3. **If files are missing**, they may have been accidentally deleted. Check your backup or re-run the migration.

## Prevention

**Before deleting old code:**
1. ✅ Test everything works
2. ✅ Verify all scenes are migrated
3. ✅ Check for broken references
4. ✅ Make a backup!

