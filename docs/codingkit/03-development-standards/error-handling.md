### Error Handling

#### Try-Catch Pattern
```csharp
// Always wrap potentially failing operations in try-catch
public void LoadLanguage(string lang)
{
    try
    {
        Locale = "en";
        if (LanguageData.LanguageReference.ContainsKey(lang))
        {
            Locale = lang;
        }
        MainLangDict = LoadLanguageFile(Locale);
    }
    catch (Exception ex)
    {
        Logger.Error($"Could not load language file \"{lang}\"");
        Logger.Error(ex);
    }
}
```

#### Fail-Safe Methods
```csharp
// Methods should return safe defaults on error
// Example from IPackageManager interface:
/// <summary>
/// This method is fail-safe and will return an empty array if an error occurs.
/// </summary>
public IReadOnlyList<IPackage> GetInstalledPackages();
```

#### Logging Errors
```csharp
// Use Logger class for error tracking
try
{
    await riskyOperation();
}
catch (Exception ex)
{
    Logger.Error("Operation failed");
    Logger.Error(ex);
    // Optionally rethrow or return default value
}
```

