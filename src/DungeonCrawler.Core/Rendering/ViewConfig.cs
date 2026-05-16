namespace DungeonCrawler.Core.Rendering;

/// <summary>
/// Controls the shape of the view frustum passed to ViewBuilder.
/// Defaults reproduce a MM3-style narrowing cone.
/// </summary>
public class ViewConfig
{
    /// <summary>Maximum depth in tiles. Tiles beyond this are not included in DungeonView.</summary>
    public int MaxDepth { get; init; } = 5;

    /// <summary>
    /// Maximum lateral spread (in tiles) at each depth level.
    /// Index 0 = depth 1 (adjacent tile), index 1 = depth 2, etc.
    ///
    /// Default:  depth 1 → ±2,  depth 2 → ±2,  depth 3 → ±1,  depth 4 → ±1,  depth 5 → ±0
    /// </summary>
    public IReadOnlyList<int> LateralSpread { get; init; } = [3, 3, 3, 3, 3];

    public int GetSpread(int depth)
    {
        int index = depth - 1;
        return (index >= 0 && index < LateralSpread.Count) ? LateralSpread[index] : 0;
    }
}
