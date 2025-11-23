using System;
using System.Collections.Generic;

namespace MinesweeperWebApp.Models.GameLogic
{
    /// <summary>
    /// Represents the entire Minesweeper board and all game logic.
    /// Compatible with GlobalBoardStore (no JSON serialization required).
    /// </summary>
    public class Board
    {
        // --------------------------- PUBLIC PROPERTIES ---------------------------

        /// <summary>The size of the board (NxN).</summary>
        public int Size { get; private set; }

        /// <summary>
        /// The game grid stored as a jagged array (serializable-friendly structure).
        /// </summary>
        public Cell[][] Grid { get; private set; }

        /// <summary>
        /// Percentage of mines (0.10 = easy, 0.15 = medium, etc.)
        /// </summary>
        public double Difficulty { get; set; }

        // --------------------------- CONSTRUCTOR ---------------------------

        /// <summary>
        /// Creates an empty NxN Minesweeper grid. Mines are placed later.
        /// </summary>
        public Board(int size)
        {
            Size = size;

            // Create the jagged array
            Grid = new Cell[size][];

            for (int r = 0; r < size; r++)
            {
                Grid[r] = new Cell[size];

                for (int c = 0; c < size; c++)
                {
                    // Create each individual cell
                    Grid[r][c] = new Cell
                    {
                        Row = r,
                        Column = c,
                        Visited = false,
                        IsFlagged = false,
                        Live = false,
                        LiveNeighbors = 0
                    };
                }
            }
        }

        // --------------------------- MINE PLACEMENT ---------------------------

        /// <summary>
        /// Randomly places mines on the board based on selected difficulty.
        /// </summary>
        public void SetupLiveNeighbors()
        {
            Random rand = new Random();

            int totalCells = Size * Size;
            int mineCount = (int)(totalCells * Difficulty);

            if (mineCount < 1)
                mineCount = 1; // Always at least one mine.

            int placed = 0;

            while (placed < mineCount)
            {
                int r = rand.Next(Size);
                int c = rand.Next(Size);

                if (!Grid[r][c].Live)
                {
                    Grid[r][c].Live = true;
                    placed++;
                }
            }
        }

        // --------------------------- COUNT NEIGHBORING MINES ---------------------------

        /// <summary>
        /// Calculates the number of nearby mines for every non-mine cell.
        /// </summary>
        public void CalculateLiveNeighbors()
        {
            for (int r = 0; r < Size; r++)
            {
                for (int c = 0; c < Size; c++)
                {
                    // Skip bombs
                    if (Grid[r][c].Live)
                        continue;

                    int count = 0;

                    // Check all 8 neighbors around current cell
                    for (int nr = Math.Max(0, r - 1); nr <= Math.Min(Size - 1, r + 1); nr++)
                    {
                        for (int nc = Math.Max(0, c - 1); nc <= Math.Min(Size - 1, c + 1); nc++)
                        {
                            if (nr == r && nc == c)
                                continue;

                            if (Grid[nr][nc].Live)
                                count++;
                        }
                    }

                    Grid[r][c].LiveNeighbors = count;
                }
            }
        }

        // --------------------------- FLOOD FILL (SAFE EXPANSION) ---------------------------

        /// <summary>
        /// Returns a list of coordinates to reveal.
        /// Does NOT mark cells visited. Controller must do that.
        /// </summary>
        public List<(int row, int col)> FloodFillMVC(int row, int col)
        {
            var toReveal = new List<(int, int)>();
            FloodFillInternal(row, col, toReveal);
            return toReveal;
        }

        /// <summary>
        /// Internal recursive flood fill. Reveals empty areas.
        /// </summary>
        private void FloodFillInternal(int row, int col, List<(int, int)> toReveal)
        {
            // Out of bounds check
            if (row < 0 || row >= Size || col < 0 || col >= Size)
                return;

            var cell = Grid[row][col];

            // Never reveal a mine
            if (cell.Live)
                return;

            // Prevent infinite recursion
            if (toReveal.Contains((row, col)))
                return;

            // Add this cell to reveal list
            toReveal.Add((row, col));

            // If numbered tile, stop expansion here
            if (cell.LiveNeighbors > 0)
                return;

            // Recursively reveal neighbors ONLY for zero-value cells
            FloodFillInternal(row - 1, col, toReveal);
            FloodFillInternal(row + 1, col, toReveal);
            FloodFillInternal(row, col - 1, toReveal);
            FloodFillInternal(row, col + 1, toReveal);
            FloodFillInternal(row - 1, col - 1, toReveal);
            FloodFillInternal(row - 1, col + 1, toReveal);
            FloodFillInternal(row + 1, col - 1, toReveal);
            FloodFillInternal(row + 1, col + 1, toReveal);
        }

        // --------------------------- CHECK FOR WIN ---------------------------

        /// <summary>
        /// If every non-mine cell has been revealed → player wins.
        /// </summary>
        public bool CheckForWin()
        {
            for (int r = 0; r < Size; r++)
            {
                for (int c = 0; c < Size; c++)
                {
                    if (!Grid[r][c].Live && !Grid[r][c].Visited)
                        return false;
                }
            }
            return true;
        }

        // --------------------------- REVEAL MINES ON LOSS ---------------------------

        /// <summary>
        /// Marks all bombs as visited so the UI can show them.
        /// </summary>
        public void RevealAllMines()
        {
            for (int r = 0; r < Size; r++)
            {
                for (int c = 0; c < Size; c++)
                {
                    if (Grid[r][c].Live)
                        Grid[r][c].Visited = true;
                }
            }
        }
    }
}
