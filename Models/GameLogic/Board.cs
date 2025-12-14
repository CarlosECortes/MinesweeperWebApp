using System;
using System.Collections.Generic;

namespace MinesweeperWebApp.Models.GameLogic
{
    /// <summary>
    /// Represents the entire Minesweeper board and all game logic.
    /// Now fully JSON-serializable for saving and loading game state.
    /// </summary>
    public class Board
    {
        // --------------------------- PUBLIC PROPERTIES ---------------------------

        /// <summary>
        /// Size of the board (NxN). Setter must be public for JSON restore.
        /// </summary>
        public int Size { get; set; }

        /// <summary>
        /// Jagged array storing all cells. Setter must be public for JSON restore.
        /// </summary>
        public Cell[][] Grid { get; set; }

        /// <summary>
        /// Percentage of mines (0.10 = easy, 0.15 = medium, etc.)
        /// This is also saved and restored.
        /// </summary>
        public double Difficulty { get; set; }

        // --------------------------- REQUIRED FOR JSON ---------------------------

        /// <summary>
        /// Empty constructor REQUIRED for JSON deserialization.
        /// DO NOT initialize the grid here — JSON will populate it automatically.
        /// </summary>
        public Board()
        {
        }

        // --------------------------- NORMAL CONSTRUCTOR ---------------------------

        /// <summary>
        /// Creates a fresh board when starting a NEW game.
        /// </summary>
        public Board(int size)
        {
            Size = size;

            // Create the jagged cell array
            Grid = new Cell[size][];

            for (int r = 0; r < size; r++)
            {
                Grid[r] = new Cell[size];

                for (int c = 0; c < size; c++)
                {
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
                mineCount = 1;

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
                    if (Grid[r][c].Live)
                        continue;

                    int count = 0;

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

        // --------------------------- FLOOD FILL ---------------------------

        /// <summary>
        /// External method used by controller/service to reveal tiles.
        /// Returns list of cells to reveal. Does not mark visited.
        /// </summary>
        public List<(int row, int col)> FloodFillMVC(int row, int col)
        {
            var toReveal = new List<(int row, int col)>();
            FloodFillInternal(row, col, toReveal);
            return toReveal;
        }

        private void FloodFillInternal(int row, int col, List<(int, int)> toReveal)
        {
            if (row < 0 || row >= Size || col < 0 || col >= Size)
                return;

            var cell = Grid[row][col];

            if (cell.Live)
                return;

            if (toReveal.Contains((row, col)))
                return;

            toReveal.Add((row, col));

            if (cell.LiveNeighbors > 0)
                return;

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
        /// A win occurs when all non-mine cells have been visited.
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

        // --------------------------- REVEAL ALL MINES ON LOSS ---------------------------

        /// <summary>
        /// On game loss, show all mines by marking them visited.
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
