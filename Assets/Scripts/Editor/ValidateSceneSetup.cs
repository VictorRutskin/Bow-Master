using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// Editor tool to validate scene setup and report missing references.
/// </summary>
public class ValidateSceneSetup : EditorWindow
{
    [MenuItem("BowMaster/Validate Scene/Check Current Scene")]
    public static void ValidateCurrentScene()
    {
        var scene = EditorSceneManager.GetActiveScene();
        if (scene == null)
        {
            Debug.LogError("[ValidateScene] No active scene!");
            return;
        }

        Debug.Log($"[ValidateScene] ========================================");
        Debug.Log($"[ValidateScene] Validating scene: {scene.name}");
        Debug.Log($"[ValidateScene] ========================================");

        var report = new ValidationReport();
        
        // Validate GameManager
        ValidateGameManager(report);
        
        // Validate LevelController
        ValidateLevelController(report);
        
        // Validate TowerShooterController
        ValidateTowerShooter(report);
        
        // Validate CastleController
        ValidateCastleController(report);
        
        // Validate SpawnControllers
        ValidateSpawnControllers(report);
        
        // Validate MusicManager
        ValidateMusicManager(report);
        
        // Validate Camera
        ValidateCamera(report);
        
        // Validate Services
        ValidateServices(report);
        
        // Print report
        PrintReport(report);
    }

    private static void ValidateGameManager(ValidationReport report)
    {
        report.AddSection("GameManager");
        
        var gameManager = Object.FindFirstObjectByType<GameManager>();
        if (gameManager == null)
        {
            report.AddError("GameManager not found in scene!");
            report.AddSuggestion("Run: BowMaster > Setup Level Scene > Setup Current Scene");
            return;
        }
        
        report.AddSuccess("GameManager found");
        
        // Check if services are initialized (we can't check this in editor, but we can verify GameManager exists)
        report.AddInfo("Services will initialize on Start if autoInitialize is enabled");
    }

    private static void ValidateLevelController(ValidationReport report)
    {
        report.AddSection("LevelController");
        
        var levelController = Object.FindFirstObjectByType<LevelController>();
        if (levelController == null)
        {
            report.AddError("LevelController not found in scene!");
            report.AddSuggestion("Run: BowMaster > Setup Level Scene > Setup Current Scene");
            return;
        }
        
        report.AddSuccess("LevelController found");
        
        if (levelController.level == null)
        {
            report.AddError("LevelController.level (LevelAsset) is not assigned!");
            report.AddSuggestion("Assign a LevelAsset in the Inspector");
        }
        else
        {
            report.AddSuccess($"LevelAsset assigned: {levelController.level.name}");
        }
        
        if (levelController.playerCastle == null)
        {
            report.AddError("LevelController.playerCastle (CastleController) is not assigned!");
            report.AddSuggestion("Assign a CastleController in the Inspector");
        }
        else
        {
            report.AddSuccess("CastleController assigned");
        }
        
        if (levelController.spawners == null || levelController.spawners.Count == 0)
        {
            report.AddError("LevelController.spawners list is empty!");
            report.AddSuggestion("Add SpawnController(s) to the spawners list");
        }
        else
        {
            report.AddSuccess($"Spawners list has {levelController.spawners.Count} entry(ies)");
        }
    }

    private static void ValidateTowerShooter(ValidationReport report)
    {
        report.AddSection("TowerShooterController");
        
        var tower = Object.FindFirstObjectByType<TowerShooterController>();
        if (tower == null)
        {
            report.AddError("TowerShooterController not found in scene!");
            report.AddSuggestion("Add TowerShooterController component to a GameObject");
            return;
        }
        
        report.AddSuccess("TowerShooterController found");
        
        if (tower.arrowPrefab == null)
        {
            report.AddError("TowerShooterController.arrowPrefab is not assigned!");
            report.AddSuggestion("Assign the Arrow prefab in the Inspector");
        }
        else
        {
            report.AddSuccess($"Arrow prefab assigned: {tower.arrowPrefab.name}");
        }
        
        if (tower.arrowSpawnPoint == null)
        {
            report.AddError("TowerShooterController.arrowSpawnPoint is not assigned!");
            report.AddSuggestion("Assign a Transform for the arrow spawn point");
        }
        else
        {
            report.AddSuccess($"Arrow spawn point assigned: {tower.arrowSpawnPoint.name}");
        }
    }

    private static void ValidateCastleController(ValidationReport report)
    {
        report.AddSection("CastleController");
        
        var castle = Object.FindFirstObjectByType<CastleController>();
        if (castle == null)
        {
            report.AddError("CastleController not found in scene!");
            report.AddSuggestion("Add CastleController component to the Castle GameObject");
            return;
        }
        
        report.AddSuccess("CastleController found");
    }

    private static void ValidateSpawnControllers(ValidationReport report)
    {
        report.AddSection("SpawnControllers");
        
        var spawners = Object.FindObjectsByType<SpawnController>(FindObjectsSortMode.None);
        if (spawners == null || spawners.Length == 0)
        {
            report.AddError("No SpawnController found in scene!");
            report.AddSuggestion("Run: BowMaster > Setup Level Scene > Setup Current Scene");
            return;
        }
        
        report.AddSuccess($"Found {spawners.Length} SpawnController(s)");
        
        foreach (var spawner in spawners)
        {
            if (spawner.defaultCastle == null)
            {
                report.AddWarning($"SpawnController '{spawner.name}' has no defaultCastle assigned");
            }
        }
    }

    private static void ValidateMusicManager(ValidationReport report)
    {
        report.AddSection("MusicManager");
        
        var musicManager = Object.FindFirstObjectByType<MusicManager>();
        if (musicManager == null)
        {
            report.AddWarning("MusicManager not found (will auto-create at runtime)");
            return;
        }
        
        report.AddSuccess("MusicManager found");
        
        if (musicManager.MenuMusicClip == null)
        {
            report.AddError("MusicManager.menuMusicClip is not assigned!");
            report.AddSuggestion("Run: BowMaster > Setup Music > Auto-Assign Music Clips");
        }
        else
        {
            report.AddSuccess($"Menu music assigned: {musicManager.MenuMusicClip.name}");
        }
        
        if (musicManager.BattleMusicClip == null)
        {
            report.AddError("MusicManager.battleMusicClip is not assigned!");
            report.AddSuggestion("Run: BowMaster > Setup Music > Auto-Assign Music Clips");
        }
        else
        {
            report.AddSuccess($"Battle music assigned: {musicManager.BattleMusicClip.name}");
        }
    }

    private static void ValidateCamera(ValidationReport report)
    {
        report.AddSection("Camera");
        
        if (Camera.main == null)
        {
            report.AddError("Camera.main is null!");
            report.AddSuggestion("Tag your camera GameObject as 'MainCamera'");
            report.AddSuggestion("InputService requires Camera.main to convert mouse position to world coordinates");
        }
        else
        {
            report.AddSuccess("Camera.main found");
        }
    }

    private static void ValidateServices(ValidationReport report)
    {
        report.AddSection("Services");
        
        var serviceLocator = ServiceLocator.Instance;
        if (serviceLocator == null)
        {
            report.AddWarning("ServiceLocator.Instance is null (will auto-create at runtime)");
        }
        else
        {
            report.AddSuccess("ServiceLocator ready");
            report.AddInfo("Services will be registered by GameManager on Start");
        }
    }

    private static void PrintReport(ValidationReport report)
    {
        Debug.Log($"[ValidateScene] ========================================");
        Debug.Log($"[ValidateScene] VALIDATION REPORT");
        Debug.Log($"[ValidateScene] ========================================");
        
        foreach (var section in report.Sections)
        {
            Debug.Log($"[ValidateScene] --- {section.Name} ---");
            
            foreach (var success in section.Successes)
            {
                Debug.Log($"[ValidateScene] ✓ {success}");
            }
            
            foreach (var warning in section.Warnings)
            {
                Debug.LogWarning($"[ValidateScene] ⚠ {warning}");
            }
            
            foreach (var error in section.Errors)
            {
                Debug.LogError($"[ValidateScene] ✗ {error}");
            }
            
            foreach (var info in section.Infos)
            {
                Debug.Log($"[ValidateScene] ℹ {info}");
            }
            
            foreach (var suggestion in section.Suggestions)
            {
                Debug.Log($"[ValidateScene] 💡 {suggestion}");
            }
        }
        
        Debug.Log($"[ValidateScene] ========================================");
        Debug.Log($"[ValidateScene] Errors: {report.TotalErrors}, Warnings: {report.TotalWarnings}");
        Debug.Log($"[ValidateScene] ========================================");
    }

    private class ValidationReport
    {
        public List<ValidationSection> Sections { get; } = new List<ValidationSection>();
        private ValidationSection _currentSection;
        
        public int TotalErrors { get; private set; }
        public int TotalWarnings { get; private set; }
        
        public void AddSection(string name)
        {
            _currentSection = new ValidationSection { Name = name };
            Sections.Add(_currentSection);
        }
        
        public void AddSuccess(string message)
        {
            _currentSection?.Successes.Add(message);
        }
        
        public void AddWarning(string message)
        {
            _currentSection?.Warnings.Add(message);
            TotalWarnings++;
        }
        
        public void AddError(string message)
        {
            _currentSection?.Errors.Add(message);
            TotalErrors++;
        }
        
        public void AddInfo(string message)
        {
            _currentSection?.Infos.Add(message);
        }
        
        public void AddSuggestion(string message)
        {
            _currentSection?.Suggestions.Add(message);
        }
    }
    
    private class ValidationSection
    {
        public string Name { get; set; }
        public List<string> Successes { get; } = new List<string>();
        public List<string> Warnings { get; } = new List<string>();
        public List<string> Errors { get; } = new List<string>();
        public List<string> Infos { get; } = new List<string>();
        public List<string> Suggestions { get; } = new List<string>();
    }
}

