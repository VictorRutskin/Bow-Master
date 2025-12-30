# 🔧 Manual Fix - Do This Right Now

## Quick Manual Fix (No Menu Needed)

### Step 1: Open MainMenu Scene
1. In Unity, open `Assets/Scenes/MainMenu.unity`

### Step 2: Fix CampaignUI GameObject
1. In **Hierarchy**, find and select **"CampaignUI"** (it's under Canvas)
2. In **Inspector**, you'll see a broken/missing script component (red icon or "Missing" text)
3. **Remove the broken component:**
   - Click the 3-dots menu (⋮) on the broken component
   - Select **"Remove Component"**

4. **Add the new component:**
   - Click **"Add Component"** button
   - Search for **"CampaignView"**
   - Click to add it

5. **Reassign the data:**
   - **Levels** (array): Click the "+" button 3 times, then fill in:
     - Element 0: `displayName = "Level 1"`, `sceneName = "Level_1"`
     - Element 1: `displayName = "Level 2"`, `sceneName = "Level_2"`
     - Element 2: `displayName = "Level 3"`, `sceneName = "Level_3"`
   - **Content Parent**: In Hierarchy, find `CampaignUI > Scroll View > Viewport > Content` and drag it here
   - **Level Button Prefab**: Drag `Assets/Prefabs/LevelButton.prefab` from Project window

### Step 3: Check for MainMenuView
1. In **Hierarchy**, look for any GameObject that might need `MainMenuView`
2. If you see another broken script, remove it and add `MainMenuView` component
3. Reassign `mainPanel` and `campaignPanel` if needed

### Step 4: Save and Test
1. **Save the scene** (Ctrl+S or File > Save)
2. **Press Play** - the menu should appear!

## If Menu Still Doesn't Show

1. **Force Unity to recompile:**
   - Go to **Assets > Reimport All**
   - Or close and reopen Unity

2. **Or use the menu after recompile:**
   - **BowMaster > Fix Broken References > Fix MainMenu Scene**

## Still Broken?

Check the **Console** window for errors. Common issues:
- Missing prefab references
- Missing script files
- Scene not saved

