namespace Checkers;

public class Game
{
	private const int PiecesPerColor = 12;
	

	public PieceColor Turn { get; set; }
	public Board Board { get; }
	public PieceColor? Winner { get; set; }//remove the private here, or cannot directly change the color
	public List<Player> Players { get; }

	private Action<Game> renderCallback;

	public Game(int humanPlayerCount, Action<Game> renderCallback)
	{
		if (humanPlayerCount < 0 || 2 < humanPlayerCount) throw new ArgumentOutOfRangeException(nameof(humanPlayerCount));
		Board = new Board();
		Players = new()
		{
			new Player(humanPlayerCount >= 1, Black),
			new Player(humanPlayerCount >= 2, White),
		};
		Turn = Black;
		Winner = null;

		// store the passed rendering method
		this.renderCallback = renderCallback;
	}

	//========================================A-5:undo a move====================================
	private Stack<Move> moveHistory = new(); // take player's history of action
	//========================================A-5:undo a move====================================

	public void PerformMove(Move move)
	{
		(move.PieceToMove.X, move.PieceToMove.Y) = move.To;
		if ((move.PieceToMove.Color is Black && move.To.Y is 7) ||
			(move.PieceToMove.Color is White && move.To.Y is 0))
		{
			move.PieceToMove.Promoted = true;
		}
		if (move.PieceToCapture is not null)
		{
			Board.Pieces.Remove(move.PieceToCapture);
			 moveHistory.Pop(); //cannot undo the capture move
		}
		if (move.PieceToCapture is not null &&
			Board.GetPossibleMoves(move.PieceToMove).Any(m => m.PieceToCapture is not null))
		{
			Board.Aggressor = move.PieceToMove;
		}
		else
		{
			Board.Aggressor = null;
			Turn = Turn is Black ? White : Black;
		}

		// get the current player and reduced the Stamina
		Player currentPlayer = Players.First(p => p.Color == move.PieceToMove.Color);
		currentPlayer.step_count = 0; // after move turn Stamina to 0 immediately

		// call the render method
		renderCallback(this);

    	Console.WriteLine("  Want to undo the last step? ");
		Console.WriteLine("  Press [D] to undo. Press [Enter] to continue...");

		if (currentPlayer.CanUndo())
		{
			ConsoleKey key = Console.ReadKey(true).Key;
			if (key == ConsoleKey.D)
			{
				UndoMove();
				return;
			}
		}
		//Console.ReadKey(true); 
		// wait for player pressing Enter

		CheckForWinner();
	}

	// ======================================A5:Undo===================================================
    // After undoing a move, the piece will correctly return to its original position.  
	// If the move was a capture, undo is not allowed.  
	// The undo count decreases, with a maximum of 3 undos per game.  
	public void UndoMove()
	{
		if (moveHistory.Count == 0) return;

		Move lastMove = moveHistory.Pop();

		// lastMove.PieceToMove.X = lastMove.To.X;
		// lastMove.PieceToMove.Y = lastMove.To.Y;
		//make the piece back to the previous place
		lastMove.PieceToMove.X = lastMove.PieceToMove.X - (lastMove.To.X - lastMove.PieceToMove.X);  
    	lastMove.PieceToMove.Y = lastMove.PieceToMove.Y - (lastMove.To.Y - lastMove.PieceToMove.Y);  

		if (lastMove.PieceToCapture != null)
		{
			Board.Pieces.Add(lastMove.PieceToCapture);
		}

		//switch turn, reduce chance by 1
		Player currentPlayer = Players.First(p => p.Color == Turn);
		currentPlayer.UseUndo(); // decrease player's remaining undo count

		Turn = Turn is Black ? White : Black; // back to gameplay next turn after undo the action
		renderCallback(this);
	}
	// ======================================A5:Undo===================================================
	
	public void CheckForWinner()
	{
		if (!Board.Pieces.Any(piece => piece.Color is Black))
		{
			Winner = White;
		}
		if (!Board.Pieces.Any(piece => piece.Color is White))
		{
			Winner = Black;
		}
		if (Winner is null && Board.GetPossibleMoves(Turn).Count is 0)
		{
			Winner = Turn is Black ? White : Black;
		}
	}

	public int TakenCount(PieceColor colour) =>
		PiecesPerColor - Board.Pieces.Count(piece => piece.Color == colour);
}
