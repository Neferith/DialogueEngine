namespace DungeonCrawler.EventSystems;

public interface IGameAction { }

public record StartDialogueAction(string DialogueId) : IGameAction;
public record GiveItemAction(string ItemId, int Qty) : IGameAction;
public record StartCombatAction(string EncounterId) : IGameAction;
public record ShowMessageAction(string Message) : IGameAction;
public record RecruitAction(string CharacterId) : IGameAction;
public record RemoveEntityAction(string EntityId) : IGameAction;