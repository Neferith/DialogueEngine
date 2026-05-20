using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MapEditor.Core.Characters;
using System.Collections.ObjectModel;
using System.Xml.Linq;

namespace MapEditor.Avalonia.ViewModels.CharacterRules;

public partial class SkillsViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RemoveSkillCommand))]
    private SkillViewModel? _selectedSkill;

    public ObservableCollection<SkillViewModel> Skills { get; } = new();

    public SkillsViewModel() { }

    public SkillsViewModel(IEnumerable<SkillData> skills)
    {
        foreach (var s in skills)
            Skills.Add(new SkillViewModel(s));
    }

    [RelayCommand]
    private void AddSkill()
    {
        var vm = new SkillViewModel();
        Skills.Add(vm);
        SelectedSkill = vm;
    }

    [RelayCommand(CanExecute = nameof(CanRemoveSkill))]
    private void RemoveSkill()
    {
        if (SelectedSkill == null) return;
        Skills.Remove(SelectedSkill);
        SelectedSkill = Skills.LastOrDefault();
    }

    private bool CanRemoveSkill() => SelectedSkill != null;

    public List<SkillData> ToData() =>
        Skills.Select(s => s.ToData()).ToList();
}

public partial class SkillViewModel : ObservableObject
{
    [ObservableProperty] private string _id = "";
    [ObservableProperty] private string _name = "";
    [ObservableProperty] private string _type = "Technical";
    [ObservableProperty] private string _description = "";
    [ObservableProperty] private decimal _musculature = 0;
    [ObservableProperty] private decimal _flexibility = 0;
    [ObservableProperty] private decimal _brain = 0;
    [ObservableProperty] private decimal _vitality = 0;

    public SkillViewModel() { }

    public SkillViewModel(SkillData data)
    {
        _id = data.Id;
        _name = data.Name;
        _type = data.Type;
        _description = data.Description;
        _musculature = data.Modifier.Musculature;
        _flexibility = data.Modifier.Flexibility;
        _brain = data.Modifier.Brain;
        _vitality = data.Modifier.Vitality;
    }

    public SkillData ToData() => new()
    {
        Id = Id.Trim(),
        Name = Name.Trim(),
        Type = Type.Trim(),
        Description = Description.Trim(),
        Modifier = new AttributesModifierData
        {
            Musculature = (int)Musculature,
            Flexibility = (int)Flexibility,
            Brain = (int)Brain,
            Vitality = (int)Vitality
        }
    };
}