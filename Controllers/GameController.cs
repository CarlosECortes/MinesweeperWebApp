using Microsoft.AspNetCore.Mvc;
using MinesweeperWebApp.Models.GameLogic;
using MinesweeperWebApp.Models.GameLogic.Services;
using MinesweeperWebApp.Models.ViewModels;

namespace MinesweeperWebApp.Controllers
{
    public class GameController : Controller
    {
        private const string SessionBoardExists = "BoardExists";

        // --------------------------------------------------------------
        // GET: Start Game
        // --------------------------------------------------------------
        public IActionResult StartGame()
        {
            if (HttpContext.Session.GetString("Username") == null)
                return RedirectToAction("Login", "Account");

            ViewBag.Username = HttpContext.Session.GetString("Username");
            return View(new StartGameViewModel());
        }

        // --------------------------------------------------------------
        // POST: Start Game
        // --------------------------------------------------------------
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

            // Create and configure the board
            Board board = new Board(model.BoardSize)
            {
                Difficulty = difficultyValue
            };

            board.SetupLiveNeighbors();
            board.CalculateLiveNeighbors();

            GlobalBoardStore.CurrentBoard = board;
            HttpContext.Session.SetString(SessionBoardExists, "true");

            return RedirectToAction("MineSweeperBoard");
        }

        // --------------------------------------------------------------
        // GET: Game Board
        // --------------------------------------------------------------
        public IActionResult MineSweeperBoard()
        {
            if (GlobalBoardStore.CurrentBoard == null ||
                HttpContext.Session.GetString(SessionBoardExists) == null)
            {
                return RedirectToAction("StartGame");
            }

            return View(GlobalBoardStore.CurrentBoard);
        }

        // --------------------------------------------------------------
        // AJAX: LEFT CLICK
        // --------------------------------------------------------------
        [HttpPost]
        public IActionResult CellClickAjax(int row, int col)
        {
            if (GlobalBoardStore.CurrentBoard == null)
                return BadRequest("Board not found.");

            var service = new GameService(GlobalBoardStore.CurrentBoard);
            string result = service.HandleLeftClick(row, col);

            if (result == "win")
                return Json(new { redirect = Url.Action("GameWon") });

            if (result == "loss")
                return Json(new { redirect = Url.Action("GameLost", new { showBoard = true }) });

            return PartialView("_BoardPartial", GlobalBoardStore.CurrentBoard);
        }

        // --------------------------------------------------------------
        // AJAX: RIGHT CLICK TOGGLE FLAG
        // --------------------------------------------------------------
        [HttpPost]
        public IActionResult ToggleFlagAjax(int row, int col)
        {
            if (GlobalBoardStore.CurrentBoard == null)
                return BadRequest("Board not found.");

            var service = new GameService(GlobalBoardStore.CurrentBoard);
            service.ToggleFlag(row, col);

            return PartialView("_BoardPartial", GlobalBoardStore.CurrentBoard);
        }

        // --------------------------------------------------------------
        // AJAX: TIMESTAMP (simple partial)
        // --------------------------------------------------------------
        public IActionResult Timestamp()
        {
            return PartialView("_TimestampPartial", DateTime.Now);
        }

        // --------------------------------------------------------------
        // WIN VIEW
        // --------------------------------------------------------------
        public IActionResult GameWon() => View();

        // --------------------------------------------------------------
        // LOSS VIEW
        // --------------------------------------------------------------
        public IActionResult GameLost(bool showBoard = false)
        {
            if (showBoard && GlobalBoardStore.CurrentBoard != null)
                return View(GlobalBoardStore.CurrentBoard);

            return View();
        }
    }
}
