using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MusicService.Models;

namespace MusicService.Pages.Accounts
{
    public class LoginModel : PageModel
    {
        private readonly MusicServiceContext _context;
        public LoginModel(MusicServiceContext context) => _context = context;

        [BindProperty]
        public string Email { get; set; } = "";
        [BindProperty]
        public string Password { get; set; } = "";
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _context.Accounts.Include(a => a.Role).FirstOrDefaultAsync(a => a.Email == Email && a.Password == Password);

            if (user != null)
            {
                UserSession.CurrentUserId = user.Id;
                UserSession.UserName = user.FirstName;
                UserSession.Role = user.Role.Name;

                return RedirectToPage("/Index");
            }

            ErrorMessage = "Неверный логин или пароль";
            return Page();
        }
    }
}