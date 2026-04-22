using System.Text.RegularExpressions;
using DialogueEngine.Core.Interfaces;
using DialogueEngine.Core.Models;

namespace DialogueEngine.Core.Engine;

/// <summary>
/// Nœud résolu prêt à l'affichage — texte final + réponses filtrées.
/// </summary>
public sealed record ResolvedNode
{
    public required Node                     Source    { get; init; }
    public required string                   Text      { get; init; }
    public required IReadOnlyList<ResolvedResponse> Responses { get; init; }
}

public sealed record ResolvedResponse
{
    public required string  Text           { get; init; }
    public required string? ConsequenceKey { get; init; }
    public required string[] NextNodeIds   { get; init; }
}

// ─────────────────────────────────────────────────────────────────────────────

public sealed partial class DialogueRunner
{
    private readonly ScriptRegistry   _scripts;
    private readonly EngineConfig     _config;

    // ── État de session ───────────────────────────────────────────────────

    private DialogueFile?    _file;
    private IDialogueContext? _context;
    private Node?             _currentNode;

    // ── Événements ────────────────────────────────────────────────────────

    public event Action<ResolvedNode>? OnNodeEntered;
    public event Action?               OnDialogueEnd;
    public event Action<string>?       OnDialogueCancelled; // nodeId

    public bool IsActive => _file is not null;

    public DialogueRunner(ScriptRegistry scripts, EngineConfig? config = null)
    {
        _scripts = scripts;
        _config  = config ?? new EngineConfig();
    }

    // ── API publique ──────────────────────────────────────────────────────

    public void Start(DialogueFile file, IDialogueContext context)
    {
        if (IsActive) throw new InvalidOperationException("Un dialogue est déjà actif.");

        _file    = file;
        _context = context;

        var node = ResolveNextNode(file.Nodes, 0);
        if (node is null) { End(); return; }

        EnterNode(node);
    }

    public void Select(int responseIndex)
    {
        EnsureActive();
        var session = CurrentResponses();

        if (responseIndex < 0 || responseIndex >= session.Count)
            throw new ArgumentOutOfRangeException(nameof(responseIndex));

        var response = session[responseIndex];

        // Conséquence du choix
        if (response.ConsequenceKey is not null)
            _scripts.Execute(response.ConsequenceKey, _context!);

        // Résolution du nœud suivant
        var next = ResolveNextFromIds(response.NextNodeIds);
        if (next is null) { End(); return; }

        EnterNode(next);
    }

    public void Cancel()
    {
        EnsureActive();
        var nodeId = _currentNode!.Id;

        if (_currentNode.CancelConsequenceKey is not null)
            _scripts.Execute(_currentNode.CancelConsequenceKey, _context!);

        _file = null; _context = null; _currentNode = null;
        OnDialogueCancelled?.Invoke(nodeId);
    }

    // ── Interne ───────────────────────────────────────────────────────────

    private void EnterNode(Node node)
    {
        _currentNode = node;

        // Conséquence d'entrée dans le nœud
        if (node.ConsequenceKey is not null)
            _scripts.Execute(node.ConsequenceKey, _context!);

        var text      = ResolveText(node.Text);
        var responses = BuildResponses(node);

        // Si aucune réponse : auto-génère Continuer/Terminé
        if (responses.Count == 0)
        {
            responses = [new ResolvedResponse
            {
                Text           = _config.EndLabel,
                ConsequenceKey = null,
                NextNodeIds    = []
            }];
        }

        OnNodeEntered?.Invoke(new ResolvedNode
        {
            Source    = node,
            Text      = text,
            Responses = responses
        });
    }

    private List<ResolvedResponse> BuildResponses(Node node)
        => node.Responses
               .Where(r => r.ConditionKey is null || _scripts.Evaluate(r.ConditionKey, _context!))
               .Select(r => new ResolvedResponse
               {
                   Text           = ResolveText(r.Text),
                   ConsequenceKey = r.ConsequenceKey,
                   NextNodeIds    = r.NextNodeIds
               })
               .ToList();

    /// <summary>Parcourt nodes[startIndex..] et retourne le premier dont la condition passe.</summary>
    private Node? ResolveNextNode(Node[] nodes, int startIndex)
    {
        for (var i = startIndex; i < nodes.Length; i++)
        {
            var node = nodes[i];
            if (node.ConditionKey is null || _scripts.Evaluate(node.ConditionKey, _context!))
                return node;
        }
        return null;
    }

    /// <summary>Parcourt nextNodeIds et retourne le premier nœud trouvé dont la condition passe.</summary>
    private Node? ResolveNextFromIds(string[] ids)
    {
        foreach (var id in ids)
        {
            var node = _file!.Nodes.FirstOrDefault(n => n.Id == id);
            if (node is null) continue;
            if (node.ConditionKey is null || _scripts.Evaluate(node.ConditionKey, _context!))
                return node;
        }
        return null;
    }

    private string ResolveText(LocalizedText text)
    {
        var raw = SelectVariant(text);
        return SubstituteVariables(raw);
    }

    private string SelectVariant(LocalizedText text)
    {
        if (!text.IsLocalized) return text.SimpleText!;

        foreach (var v in text.Variants!)
        {
            if (v.ConditionKey is null || _scripts.Evaluate(v.ConditionKey, _context!))
                return v.Value;
        }

        throw new InvalidOperationException("Aucune variante de texte ne correspond et pas de fallback.");
    }

    private string SubstituteVariables(string raw)
        => VariablePattern().Replace(raw, m => _context!.Variables.Resolve(m.Groups[1].Value));

    [System.Text.RegularExpressions.GeneratedRegex(@"\{([^{}]+)\}")]
    private static partial Regex VariablePattern();

    private void End()
    {
        _file = null; _context = null; _currentNode = null;
        OnDialogueEnd?.Invoke();
    }

    private void EnsureActive()
    {
        if (!IsActive) throw new InvalidOperationException("Aucun dialogue actif.");
    }

    private IReadOnlyList<ResolvedResponse> CurrentResponses()
        => BuildResponses(_currentNode!);
}

// ─────────────────────────────────────────────────────────────────────────────

public sealed class EngineConfig
{
    public string EndLabel      { get; init; } = "Terminé";
    public string ContinueLabel { get; init; } = "Continuer";
}
