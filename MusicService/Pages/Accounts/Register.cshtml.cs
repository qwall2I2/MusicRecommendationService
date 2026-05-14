using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MusicService.Models;
using System.ComponentModel.DataAnnotations;

namespace MusicService.Pages.Accounts
{
    public class RegisterModel : PageModel
    {
        private readonly MusicServiceContext _context;
        public RegisterModel(MusicServiceContext context) => _context = context;

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public class InputModel
        {
            [Required(ErrorMessage = "Введите имя")]
            public string FirstName { get; set; } = "";

            [Required(ErrorMessage = "Введите фамилию")]
            public string LastName { get; set; } = "";

            [Display(Name = "Отчество")]
            [StringLength(50, ErrorMessage = "Отчество не должно превышать 50 символов")]
            public string? Patronymic { get; set; }

            [Required(ErrorMessage = "Введите почту"), EmailAddress(ErrorMessage = "Некорректный формат")]
            public string Email { get; set; } = "";

            [Required(ErrorMessage = "Введите пароль"), DataType(DataType.Password)]
            public string Password { get; set; } = "";

            [Required(ErrorMessage = "Подтвердите пароль")]
            [Compare("Password", ErrorMessage = "Пароли не совпадают")]
            [DataType(DataType.Password)]
            public string ConfirmPassword { get; set; } = "";
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            if (_context.Accounts.Any(a => a.Email == Input.Email))
            {
                ModelState.AddModelError("Input.Email", "Почта уже занята");
                return Page();
            }

            var account = new Account
            {
                FirstName = Input.FirstName,
                LastName = Input.LastName,
                Patronymic = Input.Patronymic,
                Email = Input.Email,
                Password = Input.Password,
                RoleId = 2,
                CreatedAt = DateTime.Now.Date
            };

            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Login");
        }
    }
}