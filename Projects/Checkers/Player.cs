namespace Checkers;

public class Player
{
	public int step_count = 1;
	public int undo_count = 3;//3 undo chances per game
	public bool switchTurnUsed = false;
	public bool IsHuman { get; }
	public PieceColor Color { get; }

	public Player(bool isHuman, PieceColor color)
	{
		IsHuman = isHuman;
		Color = color;
	}

	public bool CanUndo() => undo_count > 0; // check if player still can undo this game
    public void UseUndo() => undo_count--; // use an undo chance

	public bool CanSwitchTurn() => !switchTurnUsed; // only can force to switch turn once per game 
    public void UseSwitchTurn() => switchTurnUsed = true; //sign for chance used
}
