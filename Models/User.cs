using System.ComponentModel.DataAnnotations;

namespace MinesweeperWebApp.Models
{
    public class User
    {
        // Primary key for each user.
        [Key]  
        public int Id { get; set; }

        // The user's first name [Required] means the field acannot be empty
        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        // The user's last name
        [Required]
        public string LastName { get; set; }

        // The user's gender. Will be using a dropdown or radio button later
        [Required]
        public string Sex { get; set; }

        // The user's age,must be between 1 and 120 to pass validation
        [Required]
        public int Age { get; set; }

        // The U.S. state the user lives in. Will be using a dropdown
        [Required]
        public string State { get; set; }

        // The user's email address. The [EmailAddress] attribute ensures proper email format
        [Required]
        [EmailAddress]
        public string EmailAddress { get; set; }

        // The username used for login
        [Required]
        public string Username { get; set; }

        // The user's password
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
