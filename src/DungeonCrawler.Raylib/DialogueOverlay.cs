using DialogueEngine.Core;
using DialogueEngine.Core.Engine;
using DialogueEngine.Core.Interfaces;
using DialogueEngine.Serialization;
using Raylib_cs;
using System.Numerics;

namespace DungeonCrawler.RaylibGame;

public enum DialogueOverlayState { Idle, Typing, WaitingInput, Done }

public class DialogueOverlay
{
    private readonly CampaignConfig _config;

    private DialogueRunner? _runner;
    private ResolvedNode? _currentNode;
    private string _displayText = "";
    private float _typeProgress;
    private const float CharsPerSec = 40f;

    private DialogueOverlayState _state = DialogueOverlayState.Idle;

    public bool IsActive => _state != DialogueOverlayState.Idle
                         && _state != DialogueOverlayState.Done;

    public DialogueOverlay(CampaignConfig config)
    {
        _config = config;
    }

    // ── Démarrage ─────────────────────────────────────────────────────────────

    public void Start(string dialogueId)
    {
        var path = Path.Combine(_config.DialoguesPath, $"{dialogueId}.json");
        if (!File.Exists(path))
        {
            Console.Error.WriteLine($"[DialogueOverlay] Fichier introuvable : {path}");
            return;
        }

        var json = File.ReadAllText(path);
        var file = DialogueFileSerializer.Deserialize(json);
        if (file == null) return;

        _runner = new DialogueRunner(new ScriptRegistry());
        _runner.OnNodeEntered += OnNodeEntered;
        _runner.OnDialogueEnd += OnDialogueEnd;
        _runner.OnDialogueCancelled += _ => OnDialogueEnd();

        _state = DialogueOverlayState.Typing;
        _runner.Start(file, new EmptyDialogueContext());
    }

    // ── Update ────────────────────────────────────────────────────────────────
    private bool _justSkipped;
    public void Update(float dt)
    {
        if (!IsActive) return;

        switch (_state)
        {
            case DialogueOverlayState.Typing:
                _typeProgress += CharsPerSec * dt;
                int visible = Math.Min((int)_typeProgress, _currentNode?.Text.Length ?? 0);
                _displayText = _currentNode?.Text[..visible] ?? "";

                // Texte entièrement affiché → attente input
                if (visible >= (_currentNode?.Text.Length ?? 0))
                    _state = DialogueOverlayState.WaitingInput;

                // Espace pour skip le typewriter
                if (Raylib.IsKeyPressed(KeyboardKey.Space) ||
                    Raylib.IsKeyPressed(KeyboardKey.Enter))
                {
                    _displayText = _currentNode?.Text ?? "";
                    _state = DialogueOverlayState.WaitingInput;
                    _justSkipped = true;
                }
                break;

            case DialogueOverlayState.WaitingInput:
                if (_justSkipped) { _justSkipped = false; break; }
                if (_currentNode?.Responses.Count == 0)
                {
                    if (Raylib.IsKeyPressed(KeyboardKey.Space) ||
                        Raylib.IsKeyPressed(KeyboardKey.Enter))
                        OnDialogueEnd();
                }
                else
                {
                    if (Raylib.IsKeyPressed(KeyboardKey.Space) ||
                        Raylib.IsKeyPressed(KeyboardKey.Enter))
                        TryAdvance(0);

                    for (int i = 1; i < (_currentNode?.Responses.Count ?? 0); i++)
                    {
                        if (Raylib.IsKeyPressed(KeyboardKey.One + i))
                        {
                            TryAdvance(i);
                            break;
                        }
                    }
                }
                break;
        }
    }

    private void TryAdvance(int responseIndex)
    {
        try
        {
            _runner?.Select(responseIndex);
        }
        catch (ArgumentOutOfRangeException)
        {
            OnDialogueEnd();
        }
    }

    // ── Draw ──────────────────────────────────────────────────────────────────

    public void Draw(int w, int h)
    {
        if (!IsActive) return;

        var colors = _config.Colors;
        float panH = h * 0.28f;
        float panY = h - panH - 16f;
        float panX = w * 0.06f;
        float panW = w * 0.88f;

        var panRect = new Rectangle(panX, panY, panW, panH);

        // Fond semi-transparent
        Raylib.DrawRectangleRounded(panRect, 0.06f, 8,
            new Color(10, 8, 6, 220));
        Raylib.DrawRectangleRoundedLines(panRect, 0.06f, 8, 2f, colors.Primary);

        // Texte principal
        float textX = panX + 24f;
        float textY = panY + 20f;
        float maxTextW = panW - 48f;
        float fontSize = 18f;

        DrawWrappedText(_displayText, textX, textY, maxTextW, fontSize, colors.Text);

        // Réponses
        if (_state == DialogueOverlayState.WaitingInput &&
            _currentNode?.Responses.Count > 0)
        {
            float respY = panY + panH * 0.55f;
            for (int i = 0; i < _currentNode.Responses.Count; i++)
            {
                var resp = _currentNode.Responses[i];
                var label = $"{i + 1}. {resp.Text}";
                FantasyUI.Label(label, textX, respY + i * 26f, 16f, colors,
                    colorOverride: colors.Accent);
            }
        }
        else if (_state == DialogueOverlayState.WaitingInput)
        {
            // Hint "continuer"
            var hint = "[ Espace ] Continuer";
            var hintW = FantasyUI.MeasureText(hint, 13f).X;
            FantasyUI.Label(hint,
                panX + panW - hintW - 16f,
                panY + panH - 26f,
                13f, colors, colorOverride: colors.TextMuted);
        }
    }

    // ── Internals ─────────────────────────────────────────────────────────────

    private void OnNodeEntered(ResolvedNode node)
    {
        _currentNode = node;
        _typeProgress = 0f;
        _displayText = "";
        _state = DialogueOverlayState.Typing;
        _justSkipped = false;
    }

    private void OnDialogueEnd()
    {
        _state = DialogueOverlayState.Idle;
        _currentNode = null;
        _displayText = "";
        _runner = null;
    }

    private void DrawWrappedText(string text, float x, float y,
                                  float maxW, float fontSize, Color color)
    {
        var words = text.Split(' ');
        var line = "";
        float lineY = y;
        float lineH = fontSize + 4f;

        foreach (var word in words)
        {
            var test = line.Length == 0 ? word : line + " " + word;
            if (FantasyUI.MeasureText(test, fontSize).X > maxW && line.Length > 0)
            {
                FantasyUI.Label(line, x, lineY, fontSize, _config.Colors,
                    colorOverride: color);
                line = word;
                lineY += lineH;
            }
            else
            {
                line = test;
            }
        }

        if (line.Length > 0)
            FantasyUI.Label(line, x, lineY, fontSize, _config.Colors,
                colorOverride: color);
    }
}

/// <summary>Contexte vide pour les dialogues narratifs sans variables.</summary>
public class EmptyDialogueContext : IDialogueContext
{
    public IVariableResolver Variables { get; } = new NoOpResolver();

    private class NoOpResolver : IVariableResolver
    {
        public string Resolve(string key) => $"{{{key}}}";
    }
}