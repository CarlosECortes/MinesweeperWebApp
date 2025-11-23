namespace MinesweeperWebApp.Models.GameLogic
{
    /// <summary>
    /// Represents a single Minesweeper tile.
    /// Must stay simple so serialization is never needed.
    /// </summary>
    public class Cell
    {
        /// <summary>The row index of this cell.</summary>
        public int Row { get; set; }

        /// <summary>The column index of this cell.</summary>
        public int Column { get; set; }

        /// <summary>Whether this tile has been clicked / revealed.</summary>
        public bool Visited { get; set; } = false;

        /// <summary>True if this tile contains a bomb.</summary>
        public bool Live { get; set; } = false;

        /// <summary>Number of adjacent mines.</summary>
        public int LiveNeighbors { get; set; } = 0;

        /// <summary>True if the player placed a flag.</summary>
        public bool IsFlagged { get; set; } = false;
    }
}
