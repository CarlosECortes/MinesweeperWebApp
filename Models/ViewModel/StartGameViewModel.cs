using System.ComponentModel.DataAnnotations;

namespace MinesweeperWebApp.Models.ViewModels
{
    public class StartGameViewModel
    {
        // The selected board size (ex: 5, 8, 10, 12)
        // [Required] means the form cannot be submitted without choosing a size
        [Required]
        [Display(Name = "Board Size")]
        public int BoardSize { get; set; }

        // The selected difficulty (Easy, Medium, Hard)
        // Also required — cannot submit form without choosing a difficulty
        [Required]
        [Display(Name = "Difficulty Level")]
        public string Difficulty { get; set; }

        // A list of available sizes that will populate the dropdown menu
        public List<int> AvailableSizes { get; set; }

        // A list of difficulty names for the dropdown
        public List<string> AvailableDifficulties { get; set; }

        // Constructor initializes default dropdown values
        public StartGameViewModel()
        {
            // Standard sizes most Minesweeper games use
            AvailableSizes = new List<int> { 5, 8, 10, 12 };

            // Difficulty names shown to the user
            AvailableDifficulties = new List<string> { "Easy", "Medium", "Hard" };
        }
    }
}
