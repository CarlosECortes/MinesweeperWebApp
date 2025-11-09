using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.ValueGeneration.Internal;
using MinesweeperWebApp.Data;
using MinesweeperWebApp.Models;
using Microsoft.EntityFrameworkCore;

namespace MinesweeperWebApp.Controllers
{
    public class AccountController : Controller
    {

        // Create a private variable to store the database connection
        private readonly ApplicationDbContext _context;

        // Use dependency injection to get access to the ApplicationDbContext
        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Display the registration page when the user navigates to /Account/Register
        [HttpGet] // Respongs to GET requests
        public IActionResult Register()
        {
            // returns the Register.cshtml (registration form)
            return View();
        }

        // Handle the form submission when the user presses "Register"
        [HttpPost] // Responds to POST request
        public async Task<IActionResult> Register(User user)
        {
            // check if all form fields are valid according to our model's [Required]
            if (ModelState.IsValid)
            {
                // check is the usernamne or email already exists in the database to avoid duplicates
                bool userExist = await _context.Users.AnyAsync(u =>
                    u.Username == user.Username || u.EmailAddress == user.EmailAddress);

                if (userExist)
                {
                    // Add a model error message to display in the view
                    ModelState.AddModelError("", "Username or email already exists. Please try again.");
                    return View(user);
                }

                // If the user does not exist, add them to the databse
                _context.Add(user);
                await _context.SaveChangesAsync(); // Save chnages asynchronously

                // Redirect the user to a Success page after registration
                return RedirectToAction("RegisterSuccess");
            }

            // If validation failed, redisplay the form with validation errors
            return View(user);
        }

        // Display a confirmation page after succesful registration
        public IActionResult RegisterSuccess()
        {
            // This view will show a "Registration Successful" message
            return View();
        }

        // ----------------- Login Section ------------------------

        // Display the login form when user visits /Account/Login
        [HttpGet]
        public IActionResult Login()
        {
            // Returns the Login.cshtml form
            return View();
        }

        // Process the login form submission
        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            // Try to find a suer in the database that matches both username and password
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username && u.Password == password);

            // If matching user was found
            if (user != null)
            {
                // Save the username in session so we can identify the logged in user later
                HttpContext.Session.SetString("Username", user.Username);

                // Redirect to the restricted StartGame page (I will create it in future)
                return RedirectToAction("StartGame");
            }

            // If no user was found, show an error message and redisplay fomr
            ViewBag.ErrorMessage = "Invalid username or password.";
            return View();
        } 

        // ---------------- Restricted Page --------------------

        // Page will only be accessible to users who are logged in
        public IActionResult StartGame()
        {
            // Check if the session variable "Username" exists
            var username = HttpContext.Session.GetString("Username");

            // If the session is empty, redirect the user back to login
            if (string.IsNullOrEmpty(username))
            {
                // Prevents unauthorized access
                return RedirectToAction("Login");
            }

            // If the user is logged in, pass the username to the view
            ViewBag.Username = username;

            // Display the StartGame view (placeholder for now)
            return View();
        }

        // Simple logout function for testing. 
        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
