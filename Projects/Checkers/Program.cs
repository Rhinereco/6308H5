//RequirementA-4: Pieces can move in all 8 directions
// •	The Board class defines movement logic in GetPossibleMoves(Piece piece), allowing movement 
// 		in eight directions ((-1,-1), (-1,1), (1,-1), (1,1), (0,1), (0,-1), (1,0), (-1,0)).
// •	The function ValidateDiagonalMove(int dx, int dy) determines whether a move is valid for the 
// 		given piece, ensuring movement in all possible directions (Board.cs).
// RequirementA-5: Player can undo a move (Maybe have only 3 undos per game)
// •	The Game class maintains a stack-based history (moveHistory) of moves to allow undo (Game.cs).
// •	PerformMove(Move move) records each move in moveHistory before execution.
// •	UndoMove() retrieves and reverts the last move except for capture moves, which cannot be undone.
// •	Player class defines undo_count = 3, limiting undos per player (Player.cs).
// •	Players press [D] to undo; if all undo chances are used, undo is no longer available.
// RequirementA-6: Player can have an extra turn to use at any point in the game
// •	The Player class tracks whether a player has used the extra turn (switchTurnUsed = false).
// •	During the opponent's turn, the player can press [F] to instantly switch the turn to themselves.
// •	CanSwitchTurn() ensures that this action can only be used once per game (Player.cs).
// •	RunGameLoop() in Program.cs checks for [F] key input during the opponent's move, and if pressed, 
// 		it switches turns (game.Turn = opponent.Color).
// RequirementB-4: Stamina System
// •	Each player starts their turn with step_count = 1 (Player (1).cs).
// •	After a move, step_count is reduced to 0, forcing a turn switch (PerformMove() in Game.cs).
// •	In RunGameLoop() (Program.cs), if step_count reaches 0, the turn automatically switches.
// •	This ensures that each player can only make one move per turn unless special rules (e.g., captures) 
// 		allow otherwise.


using System.Threading;

Exception? exception = null;
//Declares an exception object (can be null) to store error information when an exception is caught
Encoding encoding = Console.OutputEncoding;
//Saves the current console output encoding to restore it in the finally block

try
{
	Console.OutputEncoding = Encoding.UTF8;
	Game game = ShowIntroScreenAndGetOption();
	//Calls a method to display the game introduction screen and get the user-defined game mode
	Console.Clear();
	RunGameLoop(game);//Starts the main game loop
	RenderGameState(game, promptPressKey: true);//Renders the current game state to the console
	Console.ReadKey(true);//revent the program from exiting
}
//Catches the exception, stores it in the exception object
catch (Exception e)
{
	exception = e;
	throw;
}
finally
{
	Console.OutputEncoding = encoding;
	Console.CursorVisible = true;
	Console.Clear();
	Console.WriteLine(exception?.ToString() ?? "Checkers was closed.");
}

//display the game introduction and get the user's game mode selection
Game ShowIntroScreenAndGetOption()
{
	Console.Clear();
	Console.WriteLine();
	Console.WriteLine("  Checkers");
	Console.WriteLine();
	Console.WriteLine("  Checkers is played on an 8x8 board between two sides commonly known as black");
	Console.WriteLine("  and white. The objective is simple - capture all your opponent's pieces. An");
	Console.WriteLine("  alternative way to win is to trap your opponent so that they have no valid");
	Console.WriteLine("  moves left.");
	Console.WriteLine();
	Console.WriteLine("  Black starts first and players take it in turns to move their pieces forward");
	Console.WriteLine("  across the board diagonally. Should a piece reach the other side of the board");
	Console.WriteLine("  the piece becomes a king and can then move diagonally backwards as well as");
	Console.WriteLine("  forwards.");
	Console.WriteLine();
	Console.WriteLine("  Pieces are captured by jumping over them diagonally. More than one enemy piece");
	Console.WriteLine("  can be captured in the same turn by the same piece. If you can capture a piece");
	Console.WriteLine("  you must capture a piece.");
	Console.WriteLine();
	Console.WriteLine("  Moves are selected with the arrow keys. Use the [enter] button to select the");
	Console.WriteLine("  from and to squares. Invalid moves are ignored.");
	Console.WriteLine();
	Console.WriteLine("  Press a number key to choose number of human players:");
	Console.WriteLine("    [0] Black (computer) vs White (computer)");
	Console.WriteLine("    [1] Black (human) vs White (computer)");
	Console.Write("    [2] Black (human) vs White (human)");

    //store the number of human players selected by the user
	int? humanPlayerCount = null;
	//keep looping until user entering a valid game mode
	while (humanPlayerCount is null)
	{
		Console.CursorVisible = false;
		switch (Console.ReadKey(true).Key)
		{
			case ConsoleKey.D0 or ConsoleKey.NumPad0: humanPlayerCount = 0; break;
			case ConsoleKey.D1 or ConsoleKey.NumPad1: humanPlayerCount = 1; break;
			case ConsoleKey.D2 or ConsoleKey.NumPad2: humanPlayerCount = 2; break;
		}
	}
	return new Game(humanPlayerCount.Value, RenderGameStateWrapper);
}

// signature of the RenderGameState method is incompatible with Action<Game> 
// RenderGameState expects multiple parameters
// whereas Action<Game> requires a single parameter of type Game
void RenderGameStateWrapper(Game game)
{
    RenderGameState(game, playerMoved: null, promptPressKey: false);
}

//=================================================A5 + A6=======================================================
// PerformMove() processes the player's move (which may trigger UndoMove()).
// RenderGameState() updates the board and displays the undo prompt.
// Console.ReadKey(true); waits for player confirmation to prevent the UI from refreshing too quickly.

//runs until a player wins and not null
void RunGameLoop(Game game)
{
	while (game.Winner is null)//keeps the game running until a player wins
	{
		//get current player's turn
		//switch to the current player
		Player currentPlayer = game.Players.First(player => player.Color == game.Turn);
		Player opponent = game.Players.First(player => player.Color != game.Turn); // get the opponent

		//Ensure that Stamina is restored to 1 at the beginning of each player's turn
		currentPlayer.step_count = 1;

		if (currentPlayer.IsHuman)//if human, execute the logic for a human turn
		{
			while (game.Turn == currentPlayer.Color)
			{
				//========================================B-4:StaminaSystem====================================
				//when stamina is depleted, switch to the next player
				if(currentPlayer.step_count <= 0)
				{
					//If the player is black, switch to white; otherwise, switch to black.
					game.Turn = currentPlayer.Color == PieceColor.Black ? PieceColor.White : PieceColor.Black;
					//currentPlayer.step_count++;
					continue;
				}
				//=============================================================================================
				(int X, int Y)? selectionStart = null;
				//if aggressor, sets from to its position
				(int X, int Y)? from = game.Board.Aggressor is not null ? (game.Board.Aggressor.X, game.Board.Aggressor.Y) : null;
				//Retrieves all possible moves for the current player to validate user input
				List<Move> moves = game.Board.GetPossibleMoves(game.Turn);
				//check if only one piece can move, auto-select it to simplify user input
				if (moves.Select(move => move.PieceToMove).Distinct().Count() is 1)
				{
					Move must = moves.First();
					from = (must.PieceToMove.X, must.PieceToMove.Y);
					selectionStart = must.To;
				}
				//Calls HumanMoveSelection to let the user select a start position
				while (from is null)
				{
					from = HumanMoveSelection(game);
					selectionStart = from;
				}
				//Allows the user to select a target position
				(int X, int Y)? to = HumanMoveSelection(game, selectionStart: selectionStart, from: from);
				Piece? piece = null;
				piece = game.Board[from.Value.X, from.Value.Y];
				//Checks if there is a piece at the start position and its color matches the current turn
				if (piece is null || piece.Color != game.Turn)
				{
					from = null;
					to = null;
				}
				if (from is not null && to is not null)
				{
					//Validates the move and performs it if it's valid
					Move? move = game.Board.ValidateMove(game.Turn, from.Value, to.Value);
					if (move is not null &&
						(game.Board.Aggressor is null || move.PieceToMove == game.Board.Aggressor))
					{
						game.PerformMove(move);// when PerformMove triggered，add Undo notice

						// make sure player can still move after undo the last move
						if (currentPlayer.step_count > 0)
						{
							continue; // back to the loop again, let the player keep moving
						}
					}
				}
			}
		}
		//handle AI turn
		else
		{
			List<Move> moves = game.Board.GetPossibleMoves(game.Turn);

			// only when there is remaining switch chance, will offer player 3 sec to press F
            if (opponent.CanSwitchTurn()) 
            {
                Console.WriteLine("Press [F] switch to your turn...");
                DateTime startTime = DateTime.Now;
                while ((DateTime.Now - startTime).TotalSeconds < 3) // 3 sec
                {
                    if (Console.KeyAvailable)
                    {
                        ConsoleKey key = Console.ReadKey(true).Key;
                        if (key == ConsoleKey.F && opponent.CanSwitchTurn()) // check if the player still can switch
                        {
                            opponent.UseSwitchTurn();
                            game.Turn = opponent.Color; // switch at once
                            Console.WriteLine($"Player {opponent.Color} Switch the Turn！");
                            break;
                        }
                    }
                }
            }

            // if no F press, AI move
			if (game.Turn == currentPlayer.Color) 
			{
				List<Move> captures = moves.Where(move => move.PieceToCapture is not null).ToList();
				if (captures.Count > 0) // capture first
				{
					game.PerformMove(captures[Random.Shared.Next(captures.Count)]);
				}
				else if (!game.Board.Pieces.Any(piece => piece.Color == game.Turn && !piece.Promoted)) // move promoted
				{
					var (a, b) = game.Board.GetClosestRivalPieces(game.Turn);
					Move? priorityMove = moves.FirstOrDefault(move => move.PieceToMove == a && Board.IsTowards(move, b));
					game.PerformMove(priorityMove ?? moves[Random.Shared.Next(moves.Count)]);
				}
				else // move randomly
				{
					game.PerformMove(moves[Random.Shared.Next(moves.Count)]);
				}
			}
		}

		RenderGameState(game, playerMoved: currentPlayer, promptPressKey: true);
		// Since PerformMove may trigger UndoMove and revert the last move,
		// still need Console.ReadKey(true) to wait for player input.
		Console.ReadKey(true);
		
	}
}
//=================================================A5 + A6=======================================================

void RenderGameState(Game game, Player? playerMoved = null, (int X, int Y)? selection = null, (int X, int Y)? from = null, bool promptPressKey = false)
{
	const char BlackPiece = '○';
	const char BlackKing  = '☺';
	const char WhitePiece = '◙';
	const char WhiteKing  = '☻';
	const char Vacant     = '·';

	//Makes the console cursor invisible to avoid disrupting the display
	Console.CursorVisible = false;
	//Resets the cursor to the top-left corner to redraw the board from the beginning
	Console.SetCursorPosition(0, 0);
	StringBuilder sb = new();//build multi-line strings, minimizing console operations

	//Outputs the Checkers title and top border of the board
	sb.AppendLine();
	sb.AppendLine("  Checkers");
	sb.AppendLine();
	sb.AppendLine($"    ╔═══════════════════╗");
	//Renders each row of the board from row 8 to row 1. Uses B(x, y) to get the piece character for each cell
	sb.AppendLine($"  8 ║  {B(0, 7)} {B(1, 7)} {B(2, 7)} {B(3, 7)} {B(4, 7)} {B(5, 7)} {B(6, 7)} {B(7, 7)}  ║ {BlackPiece} = Black");
	sb.AppendLine($"  7 ║  {B(0, 6)} {B(1, 6)} {B(2, 6)} {B(3, 6)} {B(4, 6)} {B(5, 6)} {B(6, 6)} {B(7, 6)}  ║ {BlackKing} = Black King");
	sb.AppendLine($"  6 ║  {B(0, 5)} {B(1, 5)} {B(2, 5)} {B(3, 5)} {B(4, 5)} {B(5, 5)} {B(6, 5)} {B(7, 5)}  ║ {WhitePiece} = White");
	sb.AppendLine($"  5 ║  {B(0, 4)} {B(1, 4)} {B(2, 4)} {B(3, 4)} {B(4, 4)} {B(5, 4)} {B(6, 4)} {B(7, 4)}  ║ {WhiteKing} = White King");
	sb.AppendLine($"  4 ║  {B(0, 3)} {B(1, 3)} {B(2, 3)} {B(3, 3)} {B(4, 3)} {B(5, 3)} {B(6, 3)} {B(7, 3)}  ║");
	sb.AppendLine($"  3 ║  {B(0, 2)} {B(1, 2)} {B(2, 2)} {B(3, 2)} {B(4, 2)} {B(5, 2)} {B(6, 2)} {B(7, 2)}  ║ Taken:");
	sb.AppendLine($"  2 ║  {B(0, 1)} {B(1, 1)} {B(2, 1)} {B(3, 1)} {B(4, 1)} {B(5, 1)} {B(6, 1)} {B(7, 1)}  ║ {game.TakenCount(White),2} x {WhitePiece}");
	sb.AppendLine($"  1 ║  {B(0, 0)} {B(1, 0)} {B(2, 0)} {B(3, 0)} {B(4, 0)} {B(5, 0)} {B(6, 0)} {B(7, 0)}  ║ {game.TakenCount(Black),2} x {BlackPiece}");
	sb.AppendLine($"    ╚═══════════════════╝");
	sb.AppendLine($"       A B C D E F G H");
	sb.AppendLine();

	//check if a selection exists, highlights the selected position with a special character.
	if (selection is not null)
	{
		sb.Replace(" $ ", $"[{ToChar(game.Board[selection.Value.X, selection.Value.Y])}]");
	}
	if (from is not null)
	{
		char fromChar = ToChar(game.Board[from.Value.X, from.Value.Y]);
		sb.Replace(" @ ", $"<{fromChar}>");
		sb.Replace("@ ",  $"{fromChar}>");
		sb.Replace(" @",  $"<{fromChar}");
	}
	PieceColor? wc = game.Winner;
	PieceColor? mc = playerMoved?.Color;
	PieceColor? tc = game.Turn;
	// Note: these strings need to match in length
	string w = $"  *** {wc} wins ***";//Builds the winner information string to display when the game ends
	string m = $"  {mc} moved       ";//Builds a message indicating which player moved last
	string t = $"  {tc}'s turn      ";//Builds a message indicating whose turn it is
	sb.AppendLine(
		game.Winner is not null ? w ://Outputs the winner message if the game is over.
		playerMoved is not null ? m ://Outputs the last player who moved
		t);//Otherwise, it shows whose turn it is

	Player currentPlayer = game.Players.First(player => player.Color == game.Turn); // get the current player
	string stamina = $"  Stamina: {currentPlayer.step_count}       "; // show current player's stamina
	sb.AppendLine(stamina); // add stamina information to the UI

	string p = "  Press any key to continue...";//Prompt message asking the user to press any key
	string s = "                              ";//Empty string to maintain consistent output format
	sb.AppendLine(promptPressKey ? p : s);
	Console.Write(sb);//Writes the constructed string sb to the console to render the board and messages

	char B(int x, int y) =>
		(x, y) == selection ? '$' :
		(x, y) == from ? '@' :
		ToChar(game.Board[x, y]);

	static char ToChar(Piece? piece) =>
		piece is null ? Vacant ://Returns Vacant for an empty square
		(piece.Color, piece.Promoted) switch
		{
			(Black, false) => BlackPiece,
			(Black, true)  => BlackKing,
			(White, false) => WhitePiece,
			(White, true)  => WhiteKing,
			_ => throw new NotImplementedException(),
		};
}

(int X, int Y)? HumanMoveSelection(Game game, (int X, int y)? selectionStart = null, (int X, int Y)? from = null)
{
	(int X, int Y) selection = selectionStart ?? (3, 3);//initial position
	while (true)
	{
		RenderGameState(game, selection: selection, from: from);// from means beginning position
		switch (Console.ReadKey(true).Key)
		{
			case ConsoleKey.DownArrow:  selection.Y = Math.Max(0, selection.Y - 1); break;
			case ConsoleKey.UpArrow:    selection.Y = Math.Min(7, selection.Y + 1); break;
			case ConsoleKey.LeftArrow:  selection.X = Math.Max(0, selection.X - 1); break;
			case ConsoleKey.RightArrow: selection.X = Math.Min(7, selection.X + 1); break;
			case ConsoleKey.Enter:      return selection;
			case ConsoleKey.Escape:     return null;
		}
	}
}
