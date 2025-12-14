using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MinesweeperWebApp.Data;
using MinesweeperWebApp.Models;

namespace MinesweeperWebApp.Controllers
{
    // This controller serves REST endpoints — no login or views required.
    // All methods return JSON instead of HTML.
    [Route("api")]
    [ApiController]
    public class GameApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public GameApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // --------------------------------------------------------------
        // GET: api/showSavedGames
        // Returns a JSON list of ALL saved games (no login required)
        // --------------------------------------------------------------
        [HttpGet("showSavedGames")]
        public async Task<IActionResult> GetAllGames()
        {
            // Return all game rows from the database
            var games = await _context.Games.ToListAsync();
            return Ok(games); // Automatically serializes to JSON
        }

        // --------------------------------------------------------------
        // GET: api/showSavedGames/{id}
        // Returns a single game object as JSON
        // --------------------------------------------------------------
        [HttpGet("showSavedGames/{id}")]
        public async Task<IActionResult> GetGameById(int id)
        {
            // Attempt to find the game by its primary key
            var game = await _context.Games.FirstOrDefaultAsync(g => g.Id == id);

            // If not found, return a 404 response
            if (game == null)
                return NotFound(new { Message = "Game not found." });

            return Ok(game); // Automatically serializes to JSON
        }

        // --------------------------------------------------------------
        // DELETE: api/deleteOneGame/{id}
        // Deletes a saved game without requiring login
        // --------------------------------------------------------------
        [HttpDelete("deleteOneGame/{id}")]
        public async Task<IActionResult> DeleteGameApi(int id)
        {
            // Attempt to locate the game in the database
            var game = await _context.Games.FirstOrDefaultAsync(g => g.Id == id);

            if (game == null)
                return NotFound(new { Message = "Game not found." });

            // Remove it from SQL database
            _context.Games.Remove(game);
            await _context.SaveChangesAsync();

            // Return a confirmation response
            return Ok(new { Message = "Game deleted successfully." });
        }
    }
}
