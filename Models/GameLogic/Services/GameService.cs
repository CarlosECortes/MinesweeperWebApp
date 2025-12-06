using MinesweeperWebApp.Models.GameLogic;

namespace MinesweeperWebApp.Models.GameLogic.Services
{
    public class GameService
    {
        public Board Board { get; private set; }

        public GameService(Board board)
        {
            Board = board;
        }

        // ------------------------------------------------------
        // LEFT CLICK LOGIC
        // ------------------------------------------------------
        public string HandleLeftClick(int row, int col)
        {
            var cell = Board.Grid[row][col];

            // Already visited? Ignore
            if (cell.Visited)
                return "continue";

            // Flagged tiles can't be opened
            if (cell.IsFlagged)
                return "continue";

            // Bomb clicked
            if (cell.Live)
            {
                cell.Visited = true;
                Board.RevealAllMines();
                return "loss";
            }

            // Safe click
            cell.Visited = true;

            var list = Board.FloodFillMVC(row, col);
            foreach (var (r, c) in list)
            {
                Board.Grid[r][c].Visited = true;
            }

            // Check win
            if (Board.CheckForWin())
                return "win";

            return "continue";
        }

        // ------------------------------------------------------
        // RIGHT CLICK FLAG TOGGLE
        // ------------------------------------------------------
        public void ToggleFlag(int row, int col)
        {
            var cell = Board.Grid[row][col];

            // Can't flag visited (revealed) cells
            if (cell.Visited)
                return;

            cell.IsFlagged = !cell.IsFlagged;
        }
    }
}
