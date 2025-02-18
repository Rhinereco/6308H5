namespace Checkers;

public class Player
{
	public int step_count = 1;
	public bool IsHuman { get; }
	public PieceColor Color { get; }

	public Player(bool isHuman, PieceColor color)
	{
		IsHuman = isHuman;
		Color = color;
	}
}
