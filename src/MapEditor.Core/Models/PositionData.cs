namespace MapEditor.Core.Models;

public record PositionData(int X, int Y)
{
    public PositionData() : this(0, 0) { }
}

public record SizeData(int Width, int Height)
{
    public SizeData() : this(16, 16) { }
}
