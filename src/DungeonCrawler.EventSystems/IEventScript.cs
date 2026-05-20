namespace DungeonCrawler.EventSystems;

public record ScriptParameter(
    string Name,
    string Type,           // "string" | "int" | "bool"
    string Description = "",
    object? DefaultValue = null);

public interface IEventScript
{
    string ScriptId { get; }
    string Description { get; }
    IReadOnlyList<ScriptParameter> Parameters { get; }

    object? Execute(EventScriptContext ctx, Dictionary<string, object> parameters);
}