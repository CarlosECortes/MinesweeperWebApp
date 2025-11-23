using MinesweeperWebApp.Models.GameLogic;

namespace MinesweeperWebApp.Models.GameLogic
{
    public static class GlobalBoardStore
    {
        // Stores the LIVE board object for the current user's game
        // No serialization. No corruption. No reference loss.
        public static Board? CurrentBoard { get; set; }
    }
}
