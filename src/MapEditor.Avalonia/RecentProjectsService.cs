using System.Text.Json;

namespace MapEditor.Avalonia;

public class RecentProjectsService
{
    private const int MaxRecent = 8;

    private readonly string _filePath;
    private List<RecentProject> _projects;

    private static readonly JsonSerializerOptions _options = new() { WriteIndented = true };

    public RecentProjectsService()
    {
        var dir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "MapEditor");
        Directory.CreateDirectory(dir);
        _filePath = Path.Combine(dir, "recent_projects.json");
        _projects = LoadFromDisk();
    }

    public IReadOnlyList<RecentProject> Projects => _projects;

    public void Add(string name, string path)
    {
        _projects.RemoveAll(p => p.Path == path);
        _projects.Insert(0, new RecentProject { Name = name, Path = path });
        if (_projects.Count > MaxRecent)
            _projects = _projects.Take(MaxRecent).ToList();
        Save();
    }

    public void Remove(string path)
    {
        _projects.RemoveAll(p => p.Path == path);
        Save();
    }

    private List<RecentProject> LoadFromDisk()
    {
        try
        {
            if (!File.Exists(_filePath)) return new();
            var json = File.ReadAllText(_filePath);
            return JsonSerializer.Deserialize<List<RecentProject>>(json) ?? new();
        }
        catch { return new(); }
    }

    private void Save()
    {
        try { File.WriteAllText(_filePath, JsonSerializer.Serialize(_projects, _options)); }
        catch { /* non bloquant */ }
    }
}