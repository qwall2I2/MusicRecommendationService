using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MusicService.Models;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace MusicService.Pages.Admin
{
    public class CreateAlbumModel : PageModel
    {
        private readonly MusicServiceContext _context;
        public CreateAlbumModel(MusicServiceContext context) => _context = context;

        [BindProperty]
        public string Title { get; set; } = "";

        [BindProperty]
        public string ArtistName { get; set; } = "";

        [BindProperty]
        public DateTime ReleaseDate { get; set; } = DateTime.Now;

        [BindProperty]
        public IFormFile? UploadedCover { get; set; }

        public string? Message { get; set; }

        public IActionResult OnGet()
        {
            if (UserSession.Role != "администратор") return RedirectToPage("/Index");
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrWhiteSpace(Title) || string.IsNullOrWhiteSpace(ArtistName))
            {
                Message = "Название и исполнитель обязательны.";
                return Page();
            }

            try
            {
                string? finalCoverPath = null;

                if (UploadedCover != null)
                {
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                    var extension = Path.GetExtension(UploadedCover.FileName).ToLower();

                    if (!allowedExtensions.Contains(extension) || !UploadedCover.ContentType.StartsWith("image/"))
                    {
                        Message = "Ошибка: Выбранный файл не является поддерживаемым изображением.";
                        return Page();
                    }

                    string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "covers");
                    if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(UploadedCover.FileName);
                    string filePath = Path.Combine(uploadsFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await UploadedCover.CopyToAsync(stream);
                    }
                    finalCoverPath = "/uploads/covers/" + fileName;
                }

                await _context.CreateAlbum(Title, ArtistName, finalCoverPath, ReleaseDate.Date);

                Message = "Альбом успешно создан";
            }
            catch (Exception ex)
            {
                string inner = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                Message = "Ошибка: " + inner;
            }

            return Page();
        }
    }
}