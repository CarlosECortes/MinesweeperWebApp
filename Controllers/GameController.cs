using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // For EF queries
using MinesweeperWebApp.Data;       // ApplicationDbContext
using MinesweeperWebApp.Models;
using MinesweeperWebApp.Models.GameLogic;
using MinesweeperWebApp.Models.GameLogic.Services;
using MinesweeperWebApp.Models.ViewModels;
using Newtonsoft.Json;              // For serialization

namespace MinesweeperWebApp.Controllers
{
    public class GameController : Controller

    {
        private readonly ApplicationDbContext _context;

        public GameController(ApplicationDbContext context)
        {
            _context = context;
        }

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
        // SAVE GAME
        [HttpPost]
        public async Task<IActionResult> SaveGame()
        {
            // Ensure user is logged in
            string username = HttpContext.Session.GetString("Username");
            if (username == null)
                return RedirectToAction("Login", "Account");

            // Ensure a game is in progress
            if (GlobalBoardStore.CurrentBoard == null)
                return RedirectToAction("StartGame");

            // Look up the logged-in user
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null)
                return RedirectToAction("Login", "Account");

            // Serialize the current board
            string jsonBoard = JsonConvert.SerializeObject(GlobalBoardStore.CurrentBoard);

            // Create a new saved game entry
            GameModel game = new GameModel
            {
                UserId = user.Id,
                DateSaved = DateTime.Now,
                GameData = jsonBoard
            };

            // Save to DB
            _context.Games.Add(game);
            await _context.SaveChangesAsync();

            TempData["SaveMessage"] = "Game saved successfully!";

            return RedirectToAction("MineSweeperBoard");
        }
        public async Task<IActionResult> SavedGames()
        {
            string username = HttpContext.Session.GetString("Username");
            if (username == null)
                return RedirectToAction("Login", "Account");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null)
                return RedirectToAction("Login", "Account");

            // Get all saved games for this user
            var games = await _context.Games
                .Where(g => g.UserId == user.Id)
                .OrderByDescending(g => g.DateSaved)
                .ToListAsync();

            return View(games);
        }

        // --------------------------------------------------------------
        // POST: Load a saved game from the database
        // --------------------------------------------------------------
        [HttpPost]
        public async Task<IActionResult> LoadGame(int id)
        {
            // Ensure user is logged in
            string username = HttpContext.Session.GetString("Username");
            if (username == null)
                return RedirectToAction("Login", "Account");

            // Get the logged-in user record
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null)
                return RedirectToAction("Login", "Account");

            // Retrieve the saved game using its ID
            var game = await _context.Games.FirstOrDefaultAsync(g => g.Id == id && g.UserId == user.Id);

            // If the saved game doesn't exist or belongs to another user
            if (game == null)
                return RedirectToAction("SavedGames");

            // Deserialize the JSON back into a Board object
            GlobalBoardStore.CurrentBoard = JsonConvert.DeserializeObject<Board>(game.GameData);

            // Mark the session so the board is considered active
            HttpContext.Session.SetString("BoardExists", "true");

            // Redirect to the game board so the user resumes playing
            return RedirectToAction("MineSweeperBoard");
        }

        // --------------------------------------------------------------
        // POST: Delete a saved game from the database
        // --------------------------------------------------------------
        [HttpPost]
        public async Task<IActionResult> DeleteGame(int id)
        {
            // Ensure user is logged in
            string username = HttpContext.Session.GetString("Username");
            if (username == null)
                return RedirectToAction("Login", "Account");

            // Get the logged-in user record
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null)
                return RedirectToAction("Login", "Account");

            // Find the game only if it belongs to this user
            var game = await _context.Games.FirstOrDefaultAsync(g => g.Id == id && g.UserId == user.Id);

            // If not found, return to the list
            if (game == null)
                return RedirectToAction("SavedGames");

            // Remove game from database
            _context.Games.Remove(game);
            await _context.SaveChangesAsync();

            // Show a confirmation message
            TempData["SaveMessage"] = "Saved game deleted.";

            // Return to the saved games list
            return RedirectToAction("SavedGames");
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
