using System.Text.Json;
using AquaPP.Models;

namespace AquaPP.Services;

public class UserPreferencesService
{
    private readonly string _preferencesFilePath;
    private UserPreferences _preferences = new();

    public UserPreferencesService(string preferencesFilePath)
    {
        _preferencesFilePath = preferencesFilePath;
        LoadPreferences();
    }

    public UserPreferences GetPreferences()
    {
        return _preferences;
    }

    public void SavePreferences(UserPreferences preferences)
    {
        _preferences = preferences;
        var jsonString = JsonSerializer.Serialize(_preferences, new JsonSerializerOptions { WriteIndented = true });
        System.IO.File.WriteAllText(_preferencesFilePath, jsonString);
    }

    private void LoadPreferences()
    {
        if (System.IO.File.Exists(_preferencesFilePath))
        {
            var jsonString = System.IO.File.ReadAllText(_preferencesFilePath);
            _preferences = JsonSerializer.Deserialize<UserPreferences>(jsonString) ?? new UserPreferences();
        }
        else
        {
            _preferences = new UserPreferences();
            SavePreferences(_preferences); // Create the file with default preferences
        }
    }
}