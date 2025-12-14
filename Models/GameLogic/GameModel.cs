using System;

namespace MinesweeperWebApp.Models
{
    public class GameModel
    {
        public int Id { get; set; }          // PK in the Games table
        public int UserId { get; set; }      // FK to Users table
        public DateTime DateSaved { get; set; }  // Timestamp
        public string GameData { get; set; } // JSON string of the board + metadata
    }
}
