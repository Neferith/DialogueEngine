using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DialogueEngine.Core.Engine;
using DialogueEngine.Core.Models;

namespace Sample2.ViewModels;

public sealed record DialogueResponseItem(string Text, int Index);

public sealed partial class DialogueViewModel : ObservableObject
{
    private readonly DialogueRunner _runner;

    private string           _fullText      = string.Empty;
    private int              _typewriterPos = 0;
    private DispatcherTimer? _typewriterTimer;
    private DispatcherTimer? _cursorTimer;
    private int              _selectedIndex = 0;

    [ObservableProperty] private string _speakerName   = string.Empty;
    [ObservableProperty] private string _displayedText = string.Empty;
    [ObservableProperty] private bool   _isTypingDone  = false;
    [ObservableProperty] private bool   _showCursor    = true;

    public bool ShowContinueIndicator => IsTypingDone && ShowCursor;

    partial void OnIsTypingDoneChanged(bool _) => OnPropertyChanged(nameof(ShowContinueIndicator));
    partial void OnShowCursorChanged(bool _)   => OnPropertyChanged(nameof(ShowContinueIndicator));

    public List<DialogueResponseItem> Responses { get; private set; } = [];

    public event Action? DialogueEnded;

    public DialogueViewModel(DialogueFile file, GameContext ctx, ScriptRegistry scripts, string speakerName)
    {
        _speakerName = speakerName;
        _runner = new DialogueRunner(scripts);
        _runner.OnNodeEntered       += OnNodeEntered;
        _runner.OnDialogueEnd       += () => Dispatcher.UIThread.Post(() => DialogueEnded?.Invoke());
        _runner.OnDialogueCancelled += _ => Dispatcher.UIThread.Post(() => DialogueEnded?.Invoke());

        StartCursorBlink();
        _runner.Start(file, ctx);
    }

    public void MoveSelection(int delta)
    {
        if (!IsTypingDone || Responses.Count == 0) return;
        _selectedIndex = (_selectedIndex + delta + Responses.Count) % Responses.Count;
        OnPropertyChanged(nameof(SelectedIndex));
    }

    public int SelectedIndex => _selectedIndex;

    public void ConfirmSelection()
    {
        if (!IsTypingDone) { SkipTypewriter(); return; }
        if (_selectedIndex < 0 || _selectedIndex >= Responses.Count) return;
        _runner.Select(_selectedIndex);
    }

    [RelayCommand]
    private void SelectResponse(int index)
    {
        _selectedIndex = index;
        ConfirmSelection();
    }

    private void SkipTypewriter()
    {
        _typewriterTimer?.Stop();
        DisplayedText = _fullText;
        IsTypingDone  = true;
    }

    private void OnNodeEntered(ResolvedNode node)
    {
        Dispatcher.UIThread.Post(() =>
        {
            _fullText      = node.Text;
            _selectedIndex = 0;
            Responses      = node.Responses.Select((r, i) => new DialogueResponseItem(r.Text, i)).ToList();
            OnPropertyChanged(nameof(Responses));
            OnPropertyChanged(nameof(SelectedIndex));
            StartTypewriter();
        });
    }

    private void StartTypewriter()
    {
        _typewriterTimer?.Stop();
        DisplayedText  = string.Empty;
        IsTypingDone   = false;
        _typewriterPos = 0;

        _typewriterTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(28) };
        _typewriterTimer.Tick += (_, _) =>
        {
            if (_typewriterPos >= _fullText.Length) { _typewriterTimer.Stop(); IsTypingDone = true; return; }
            DisplayedText = _fullText[..(++_typewriterPos)];
        };
        _typewriterTimer.Start();
    }

    private void StartCursorBlink()
    {
        _cursorTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
        _cursorTimer.Tick += (_, _) => ShowCursor = !ShowCursor;
        _cursorTimer.Start();
    }
}
