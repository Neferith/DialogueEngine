namespace DungeonCrawler.EventSystems.Scripts;

public class SetFlagScript : IEventScript
{
    public string ScriptId => "SetFlag";
    public string Description => "Positionne un flag dans le WorldState.";
    public IReadOnlyList<ScriptParameter> Parameters =>
    [
        new("flagId", "string", "Identifiant du flag à positionner")
    ];

    public object? Execute(EventScriptContext ctx, Dictionary<string, object> p)
    {
        ctx.SetFlag(p["flagId"].ToString()!);
        return null;
    }
}

public class ClearFlagScript : IEventScript
{
    public string ScriptId => "ClearFlag";
    public string Description => "Supprime un flag du WorldState.";
    public IReadOnlyList<ScriptParameter> Parameters =>
    [
        new("flagId", "string", "Identifiant du flag à supprimer")
    ];

    public object? Execute(EventScriptContext ctx, Dictionary<string, object> p)
    {
        ctx.ClearFlag(p["flagId"].ToString()!);
        return null;
    }
}

public class SetVariableScript : IEventScript
{
    public string ScriptId => "SetVariable";
    public string Description => "Définit une variable entière dans le WorldState.";
    public IReadOnlyList<ScriptParameter> Parameters =>
    [
        new("key",   "string", "Nom de la variable"),
        new("value", "int",    "Valeur à affecter", 0)
    ];

    public object? Execute(EventScriptContext ctx, Dictionary<string, object> p)
    {
        ctx.SetVar(p["key"].ToString()!, Convert.ToInt32(p["value"]));
        return null;
    }
}

public class IncrVariableScript : IEventScript
{
    public string ScriptId => "IncrVariable";
    public string Description => "Incrémente une variable entière.";
    public IReadOnlyList<ScriptParameter> Parameters =>
    [
        new("key",    "string", "Nom de la variable"),
        new("amount", "int",    "Valeur à ajouter", 1)
    ];

    public object? Execute(EventScriptContext ctx, Dictionary<string, object> p)
    {
        ctx.IncrVar(p["key"].ToString()!, Convert.ToInt32(p["amount"]));
        return null;
    }
}

public class StartDialogueScript : IEventScript
{
    public string ScriptId => "StartDialogue";
    public string Description => "Lance un dialogue.";
    public IReadOnlyList<ScriptParameter> Parameters =>
    [
        new("dialogueId", "string", "Identifiant du fichier de dialogue")
    ];

    public object? Execute(EventScriptContext ctx, Dictionary<string, object> p)
    {
        ctx.StartDialogue(p["dialogueId"].ToString()!);
        return null;
    }
}

public class ShowMessageScript : IEventScript
{
    public string ScriptId => "ShowMessage";
    public string Description => "Affiche un message à l'écran.";
    public IReadOnlyList<ScriptParameter> Parameters =>
    [
        new("message", "string", "Texte à afficher")
    ];

    public object? Execute(EventScriptContext ctx, Dictionary<string, object> p)
    {
        ctx.ShowMessage(p["message"].ToString()!);
        return null;
    }
}

public class GiveItemScript : IEventScript
{
    public string ScriptId => "GiveItem";
    public string Description => "Donne un item au joueur.";
    public IReadOnlyList<ScriptParameter> Parameters =>
    [
        new("itemId",   "string", "Identifiant de l'item"),
        new("quantity", "int",    "Quantité", 1)
    ];

    public object? Execute(EventScriptContext ctx, Dictionary<string, object> p)
    {
        ctx.GiveItem(p["itemId"].ToString()!, Convert.ToInt32(p["quantity"]));
        return null;
    }
}

public class NpcSetHostilityScript : IEventScript
{
    public string ScriptId => "NpcSetHostility";
    public string Description => "Modifie l'hostilité d'un NPC.";
    public IReadOnlyList<ScriptParameter> Parameters =>
    [
        new("npcId",  "string", "Identifiant du NPC"),
        new("value",  "int",    "Valeur d'hostilité (0-100)"),
        new("mode",   "string", "set | add | sub", "set")
    ];

    public object? Execute(EventScriptContext ctx, Dictionary<string, object> p)
    {
        var npc = ctx.Npc(p["npcId"].ToString()!);
        var value = Convert.ToInt32(p["value"]);
        var mode = p.GetValueOrDefault("mode", "set").ToString();

        npc.Hostility = mode switch
        {
            "add" => Math.Clamp(npc.Hostility + value, 0, 100),
            "sub" => Math.Clamp(npc.Hostility - value, 0, 100),
            _ => Math.Clamp(value, 0, 100)
        };
        return null;
    }
}