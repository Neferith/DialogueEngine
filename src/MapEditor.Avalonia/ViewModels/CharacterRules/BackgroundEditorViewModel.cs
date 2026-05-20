using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using MapEditor.Core.Characters;
using System.Xml.Linq;

namespace MapEditor.Avalonia.ViewModels.CharacterRules;

public partial class BackgroundEditorViewModel : ObservableObject
{
    [ObservableProperty] private string _id = "";
    [ObservableProperty] private string _name = "";
    [ObservableProperty] private string _description = "";
    [ObservableProperty] private string _typeId = "";

    // Modificateurs
    [ObservableProperty] private decimal _musculature = 0;
    [ObservableProperty] private decimal _flexibility = 0;
    [ObservableProperty] private decimal _brain = 0;
    [ObservableProperty] private decimal _vitality = 0;

    // Skills de départ (IDs séparés par des virgules)
    [ObservableProperty] private string _startingSkillIds = "";

    // Requirement (tailles autorisées, séparées par des virgules)
    [ObservableProperty] private string _allowedSizes = "";

    public BackgroundEditorViewModel() { }

    public BackgroundEditorViewModel(BackgroundData data)
    {
        _id = data.Id;
        _name = data.Name;
        _description = data.Description;
        _typeId = data.TypeId;
        _musculature = data.AttributesModifier.Musculature;
        _flexibility = data.AttributesModifier.Flexibility;
        _brain = data.AttributesModifier.Brain;
        _vitality = data.AttributesModifier.Vitality;
        _startingSkillIds = string.Join(", ", data.StartingSkillIds);
        _allowedSizes = data.Requirement?.AllowedSizes != null
            ? string.Join(", ", data.Requirement.AllowedSizes)
            : "";
    }

    public BackgroundData ToData() => new()
    {
        Id = Id.Trim(),
        Name = Name.Trim(),
        Description = Description.Trim(),
        TypeId = TypeId.Trim(),
        AttributesModifier = new AttributesModifierData
        {
            Musculature = (int)Musculature,
            Flexibility = (int)Flexibility,
            Brain = (int)Brain,
            Vitality = (int)Vitality
        },
        StartingSkillIds = StartingSkillIds
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList(),
        Requirement = AllowedSizes.Trim().Length > 0
            ? new BackgroundRequirementData
            {
                AllowedSizes = AllowedSizes
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .ToList()
            }
            : null
    };
}