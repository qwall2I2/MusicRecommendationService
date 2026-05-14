using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MusicService.Models;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace MusicService.Pages.Playlists
{
    public class IndexModel : PageModel
    {
        private readonly MusicServiceContext _context;
        public IndexModel(MusicServiceContext context) => _context = context;

        public IList<Playlist> Playlists { get; set; } = new List<Playlist>();

        [BindProperty]
        public string NewPlaylistName { get; set; } = "";

        [BindProperty]
        public IFormFile? UploadedCover { get; set; }

        public string? ErrorMessage { get; set; }

        public async Task OnGetAsync()
        {
            if (UserSession.CurrentUserId == null) return;

            Playlists = await _context.Playlists
                .Where(p => p.AccountId == UserSession.CurrentUserId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            if (UserSession.CurrentUserId == null) return RedirectToPage("/Accounts/Login");
            if (string.IsNullOrWhiteSpace(NewPlaylistName)) return RedirectToPage();

            try
            {
                string? coverPath = null;

                if (UploadedCover != null)
                {
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                    var extension = Path.GetExtension(UploadedCover.FileName).ToLower();

                    if (!allowedExtensions.Contains(extension) || !UploadedCover.ContentType.StartsWith("image/"))
                    {
                        TempData["Error"] = "Ошибка: Файл обложки должен быть изображением (jpg, png, webp).";
                        return RedirectToPage();
                    }

                    string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "covers");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(UploadedCover.FileName);
                    string filePath = Path.Combine(uploadsFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await UploadedCover.CopyToAsync(stream);
                    }
                    coverPath = "/uploads/covers/" + fileName;
                }

                var newPlaylist = new Playlist
                {
                    Title = NewPlaylistName,
                    AccountId = UserSession.CurrentUserId.Value,
                    CoverPath = coverPath,
                    CreatedAt = DateTime.Now.Date
                };

                _context.Playlists.Add(newPlaylist);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Плейлист создан";
            }
            catch (Exception ex)
            {
                string innerError = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                TempData["Error"] = "Детали ошибки: " + innerError;
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var playlist = await _context.Playlists.FindAsync(id);

            if (playlist != null && playlist.AccountId == UserSession.CurrentUserId)
            {
                _context.Playlists.Remove(playlist);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Плейлист удален";
            }

            return RedirectToPage();
        }
    }
}