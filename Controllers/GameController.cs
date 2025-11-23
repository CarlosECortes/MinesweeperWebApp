using Microsoft.AspNetCore.Mvc;
using MinesweeperWebApp.Models.GameLogic;
using MinesweeperWebApp.Models.ViewModels;

namespace MinesweeperWebApp.Controllers
{
    public class GameController : Controller
    {
        private const string SessionBoardExists = "BoardExists";

        // --------------------------------------------------------------------
        // GET: Start Game
        // --------------------------------------------------------------------
        public IActionResult StartGame()
        {
            if (HttpContext.Session.GetString("Username") == null)
                return RedirectToAction("Login", "Account");

            // Load username into ViewBag
            ViewBag.Username = HttpContext.Session.GetString("Username");

            var model = new StartGameViewModel();
            return View(model);
        }

        // --------------------------------------------------------------------
        // POST: Start Game
        // --------------------------------------------------------------------
        [HttpPost]
        public IActionResult StartGame(StartGameViewModel model)
        {
            if (!ModelState.IsValid)
                return View(new StartGameViewModel());

            double difficultyValue = model.Difficulty.ToLower() switch
            {
                "easy" => 0.10,
                "medium" => 0.15,
                "hard" => 0.20,
                _ => 0.10
            };

            // Create a new board object
            Board board = new Board(model.BoardSize)
            {
                Difficulty = difficultyValue
            };

            board.SetupLiveNeighbors();
            board.CalculateLiveNeighbors();

            // Store board in global memory
            GlobalBoardStore.CurrentBoard = board;

            // Just mark that a board exists
            HttpContext.Session.SetString(SessionBoardExists, "true");

            return RedirectToAction("MineSweeperBoard");
        }

        // --------------------------------------------------------------------
        // GET: Minesweeper Game Board
        // --------------------------------------------------------------------
        public IActionResult MineSweeperBoard()
        {
            // Make sure a board actually exists
            if (GlobalBoardStore.CurrentBoard == null ||
                HttpContext.Session.GetString(SessionBoardExists) == null)
            {
                return RedirectToAction("StartGame");
            }

            return View(GlobalBoardStore.CurrentBoard);
        }

        // --------------------------------------------------------------------
        // POST: Cell Click
        // --------------------------------------------------------------------
        [HttpPost]
        public IActionResult CellClick(int row, int col)
        {
            Board? board = GlobalBoardStore.CurrentBoard;

            if (board == null)
                return RedirectToAction("StartGame");

            Cell cell = board.Grid[row][col];

            // ------------------- CLICKED ON A BOMB -------------------
            if (cell.Live)
            {
                cell.Visited = true;

                // Reveal all bombs
                foreach (var r in Enumerable.Range(0, board.Size))
                {
                    foreach (var c in Enumerable.Range(0, board.Size))
                    {
                        if (board.Grid[r][c].Live)
                            board.Grid[r][c].Visited = true;
                    }
                }

                return RedirectToAction("GameLost");
            }

            // ------------------- SAFE CLICK -------------------
            cell.Visited = true;

            // Flood fill for empty tiles
            var list = board.FloodFillMVC(row, col);

            foreach (var (r, c) in list)
            {
                board.Grid[r][c].Visited = true;
            }

            // ------------------- CHECK WIN -------------------
            if (board.CheckForWin())
                return RedirectToAction("GameWon");

            return RedirectToAction("MineSweeperBoard");
        }

        // --------------------------------------------------------------------
        public IActionResult GameWon() => View();
        public IActionResult GameLost() => View();
    }
}
