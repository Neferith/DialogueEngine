using DialogueEngine.Core.Engine;
using DialogueEngine.Core.Interfaces;
using DialogueEngine.Core.Models;
using FluentAssertions;
using Xunit;

namespace DialogueEngine.Core.Tests;

// ── Helpers ───────────────────────────────────────────────────────────────────

file sealed class TestContext : IDialogueContext
{
    private readonly Dictionary<string, string> _vars;
    public IVariableResolver Variables { get; }

    public TestContext(Dictionary<string, string>? vars = null)
    {
        _vars     = vars ?? [];
        Variables = new LambdaResolver(k => _vars.TryGetValue(k, out var v) ? v : $"{{{k}}}");
    }

    private sealed class LambdaResolver(Func<string, string> fn) : IVariableResolver
    {
        public string Resolve(string key) => fn(key);
    }
}

file static class Build
{
    public static DialogueFile File(params Node[] nodes) => new()
    {
        Id    = "test",
        Nodes = nodes
    };

    // Sans condition — responses directement en params
    public static Node Node(string id, string text, params Response[] responses)
        => new()
        {
            Id        = id,
            Text      = LocalizedText.Simple(text),
            Responses = responses
        };

    // Avec condition explicite
    public static Node Node(string id, string text, string? condition, params Response[] responses)
        => new()
        {
            Id           = id,
            Text         = LocalizedText.Simple(text),
            ConditionKey = condition,
            Responses    = responses
        };

    // Réponse simple sans destination (fin de dialogue)
    public static Response Response(string text, string? condition = null)
        => new()
        {
            Text         = LocalizedText.Simple(text),
            ConditionKey = condition,
            NextNodeIds  = []
        };

    // Réponse avec nœuds suivants — params doit rester seul pour éviter
    // l'ambiguïté avec l'overload à condition : appeler ResponseTo(text, nextId1, nextId2…)
    public static Response ResponseTo(string text, params string[] nextIds)
        => new()
        {
            Text        = LocalizedText.Simple(text),
            NextNodeIds = nextIds
        };

    // Réponse conditionnelle avec nœuds suivants — condition passée explicitement
    public static Response ConditionalResponseTo(string text, string? condition, params string[] nextIds)
        => new()
        {
            Text         = LocalizedText.Simple(text),
            ConditionKey = condition,
            NextNodeIds  = nextIds
        };

    public static ScriptRegistry Registry(
        Dictionary<string, bool>? conditions = null,
        HashSet<string>?          executed   = null)
    {
        var reg = new ScriptRegistry();
        foreach (var (key, value) in conditions ?? [])
            reg.Condition(key, _ => value);
        if (executed is not null)
            reg.Consequence("track", _ => executed.Add("track"));
        return reg;
    }
}

// ── Tests ─────────────────────────────────────────────────────────────────────

public sealed class DialogueRunnerTests
{
    [Fact]
    public void Starts_on_first_node_when_no_condition()
    {
        var engine   = new DialogueRunner(Build.Registry());
        ResolvedNode? received = null;
        engine.OnNodeEntered += n => received = n;

        engine.Start(Build.File(Build.Node("n1", "Bonjour.")), new TestContext());

        received.Should().NotBeNull();
        received!.Source.Id.Should().Be("n1");
        received.Text.Should().Be("Bonjour.");
    }

    [Fact]
    public void Skips_node_when_condition_false()
    {
        var reg    = Build.Registry(new() { ["cond"] = false });
        var engine = new DialogueRunner(reg);
        ResolvedNode? received = null;
        engine.OnNodeEntered += n => received = n;

        engine.Start(Build.File(
            Build.Node("n1", "Invisible.", condition: "cond"),
            Build.Node("n2", "Visible.")), new TestContext());

        received!.Source.Id.Should().Be("n2");
    }

    [Fact]
    public void Ends_when_no_node_passes()
    {
        var reg    = Build.Registry(new() { ["cond"] = false });
        var engine = new DialogueRunner(reg);
        var ended  = false;
        engine.OnDialogueEnd += () => ended = true;

        engine.Start(Build.File(Build.Node("n1", "Invisible.", condition: "cond")), new TestContext());

        ended.Should().BeTrue();
    }

    [Fact]
    public void Selects_response_and_navigates()
    {
        var engine   = new DialogueRunner(Build.Registry());
        var nodes    = new List<string>();
        engine.OnNodeEntered += n => nodes.Add(n.Source.Id);

        engine.Start(Build.File(
            Build.Node("n1", "Question ?",
                responses: Build.ResponseTo("Oui", "n2")),
            Build.Node("n2", "Suite.")),
            new TestContext());

        engine.Select(0);

        nodes.Should().Equal("n1", "n2");
    }

    [Fact]
    public void Response_with_multiple_nextNodeIds_picks_first_passing()
    {
        var reg    = Build.Registry(new() { ["cond"] = false });
        var engine = new DialogueRunner(reg);
        ResolvedNode? last = null;
        engine.OnNodeEntered += n => last = n;

        engine.Start(Build.File(
            Build.Node("n1", "Q ?",
                responses: Build.ResponseTo("R", "n2", "n3")),
            Build.Node("n2", "Conditionnel.", condition: "cond"),
            Build.Node("n3", "Fallback.")),
            new TestContext());

        engine.Select(0);

        last!.Source.Id.Should().Be("n3");
    }

    [Fact]
    public void Empty_nextNodeIds_ends_dialogue()
    {
        var engine = new DialogueRunner(Build.Registry());
        var ended  = false;
        engine.OnDialogueEnd += () => ended = true;

        engine.Start(Build.File(
            Build.Node("n1", "Fin.", responses: Build.Response("OK"))),
            new TestContext());

        engine.Select(0);
    }

    [Fact]
    public void Filters_responses_by_condition()
    {
        var reg    = Build.Registry(new() { ["show"] = false });
        var engine = new DialogueRunner(reg);
        ResolvedNode? node = null;
        engine.OnNodeEntered += n => node = n;

        engine.Start(Build.File(
            Build.Node("n1", "Q ?",
                Build.Response("Cachée", condition: "show"),
                Build.Response("Visible"))),
            new TestContext());

        node!.Responses.Should().HaveCount(1);
        node.Responses[0].Text.Should().Be("Visible");
    }

    [Fact]
    public void Substitutes_variables_in_text()
    {
        var engine  = new DialogueRunner(Build.Registry());
        ResolvedNode? node = null;
        engine.OnNodeEntered += n => node = n;

        engine.Start(Build.File(
            Build.Node("n1", "Bonjour {player.name} !")),
            new TestContext(new() { ["player.name"] = "Eleanor" }));

        node!.Text.Should().Be("Bonjour Eleanor !");
    }

    [Fact]
    public void Cancel_fires_event_with_nodeId_and_executes_consequence()
    {
        var executed = new HashSet<string>();
        var reg      = new ScriptRegistry().Consequence("cancel_fx", _ => executed.Add("cancel_fx"));
        var engine   = new DialogueRunner(reg);
        string? cancelledId = null;
        engine.OnDialogueCancelled += id => cancelledId = id;

        engine.Start(Build.File(new Node
        {
            Id                   = "n1",
            Text                 = "Texte.",
            CancelConsequenceKey = "cancel_fx",
            Responses            = []
        }), new TestContext());

        engine.Cancel();

        cancelledId.Should().Be("n1");
        executed.Should().Contain("cancel_fx");
    }

    [Fact]
    public void Cannot_start_while_active()
    {
        var engine = new DialogueRunner(Build.Registry());
        engine.Start(Build.File(Build.Node("n1", ".")), new TestContext());

        var act = () => engine.Start(Build.File(Build.Node("n1", ".")), new TestContext());
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Serialization_roundtrip()
    {
        var file = Build.File(
            Build.Node("n1", "Bonjour.",
                Build.ResponseTo("Ok.", "n2")),
            Build.Node("n2", "Au revoir."));

        var json         = Serialization.DialogueFileSerializer.Serialize(file);
        var deserialized = Serialization.DialogueFileSerializer.Deserialize(json);

        deserialized.Id.Should().Be(file.Id);
        deserialized.Nodes.Should().HaveCount(2);
        deserialized.Nodes[0].Responses[0].NextNodeIds.Should().Equal("n2");
    }
}
