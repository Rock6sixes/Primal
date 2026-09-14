namespace Menu.Remix;

public static class UniClipboard
{
	public static IBoard _board;

	public static IBoard board
	{
		get
		{
			if (_board == null)
			{
				_board = new StandardBoard();
			}
			return _board;
		}
	}

	public static void SetText(string str)
	{
		board.SetText(str);
	}

	public static string GetText()
	{
		return board.GetText();
	}
}
