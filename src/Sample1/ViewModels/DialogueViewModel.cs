using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DialogueEngine.Core.Engine;
using DialogueEngine.Core.Models;

namespace Sample1.ViewModels;

public sealed record DialogueResponseItem(string Text, int Index);

public sealed partial class DialogueViewModel : ObservableObject
{
    private readonly DialogueRunner _runner;
    private readonly GameContext    _context;

    private string _fullText     = string.Empty;
    private int    _typewriterPos = 0;
    private DispatcherTimer? _typewriterTimer;
    private DispatcherTimer? _cursorTimer;

    [ObservableProperty] private string  _npcName        = "???";
    [ObservableProperty] private string  _displayedText  = string.Empty;
    [ObservableProperty] private bool    _isTypingDone   = false;
    [ObservableProperty] private bool    _showCursor     = true;
    [ObservableProperty] private bool    _isDialogueOver = false;
    [ObservableProperty] private string  _outcomeText    = string.Empty;
    [ObservableProperty] private int     _selectedIndex  = 0;

    public bool ShowContinueIndicator => IsTypingDone && ShowCursor;

    partial void OnIsTypingDoneChanged(bool value)  => OnPropertyChanged(nameof(ShowContinueIndicator));
    partial void OnShowCursorChanged(bool value)    => OnPropertyChanged(nameof(ShowContinueIndicator));

    public List<DialogueResponseItem> Responses { get; private set; } = [];

    public event Action? DialogueEnded;

    public DialogueViewModel(DialogueFile file, GameContext context, ScriptRegistry scripts)
    {
        _context = context;
        _runner  = new DialogueRunner(scripts);

        _runner.OnNodeEntered      += OnNodeEntered;
        _runner.OnDialogueEnd      += OnEnd;
        _runner.OnDialogueCancelled += id => OnEnd();

        StartCursorBlink();
        _runner.Start(file, context);
    }

    // ── Saisie joueur ─────────────────────────────────────────────────────

    [RelayCommand]
    private void SelectResponse(int index)
    {
        if (!IsTypingDone || IsDialogueOver) return;
        if (index < 0 || index >= Responses.Count) return;
        _selectedIndex = index;
        _runner.Select(index);
    }

    [RelayCommand]
    private void SkipOrConfirm()
    {
        if (!IsTypingDone)
        {
            // Skip typewriter
            _typewriterTimer?.Stop();
            DisplayedText = _fullText;
            IsTypingDone  = true;
            return;
        }

        // Confirme la réponse sélectionnée si une seule réponse dispo
        if (Responses.Count == 1)
            SelectResponse(0);
    }

    public void MoveSelection(int delta)
    {
        if (!IsTypingDone || Responses.Count == 0) return;
        _selectedIndex = (_selectedIndex + delta + Responses.Count) % Responses.Count;
        SelectedIndex  = _selectedIndex;
    }

    public void ConfirmSelection()
    {
        if (!IsTypingDone || IsDialogueOver) return;
        SelectResponse(_selectedIndex);
    }

    // ── Callbacks moteur ──────────────────────────────────────────────────

    private void OnNodeEntered(ResolvedNode node)
    {
        Dispatcher.UIThread.Post(() =>
        {
            NpcName   = node.Source.Id.Contains("officier") ? "VANCE" :
                        node.Source.Id.Contains("deserteur") ? "VANCE" :
                        "GARDE VANCE";
            _fullText = node.Text;
            Responses      = node.Responses
                                .Select((r, i) => new DialogueResponseItem(r.Text, i))
                                .ToList();
            SelectedIndex  = 0;
            _selectedIndex = 0;
            OnPropertyChanged(nameof(Responses));

            StartTypewriter();
        });
    }

    private void OnEnd()
    {
        Dispatcher.UIThread.Post(() =>
        {
            _typewriterTimer?.Stop();
            IsDialogueOver = true;
            OnPropertyChanged(nameof(Responses));
            DialogueEnded?.Invoke();
        });
    }

    // ── Typewriter ────────────────────────────────────────────────────────

    private void StartTypewriter()
    {
        _typewriterTimer?.Stop();
        DisplayedText = string.Empty;
        IsTypingDone  = false;
        _typewriterPos = 0;

        _typewriterTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(28)
        };
        _typewriterTimer.Tick += (_, _) =>
        {
            if (_typewriterPos >= _fullText.Length)
            {
                _typewriterTimer.Stop();
                IsTypingDone = true;
                return;
            }
            DisplayedText  = _fullText[..(++_typewriterPos)];
        };
        _typewriterTimer.Start();
    }

    // ── Curseur clignotant ────────────────────────────────────────────────

    private void StartCursorBlink()
    {
        _cursorTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
        _cursorTimer.Tick += (_, _) => ShowCursor = !ShowCursor;
        _cursorTimer.Start();
    }
}
