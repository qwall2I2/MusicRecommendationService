using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MusicService.Models;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace MusicService.Pages.Admin
{
    public class UploadModel : PageModel
    {
        private readonly MusicServiceContext _context;
        public UploadModel(MusicServiceContext context) => _context = context;

        [BindProperty] public string TrackTitle { get; set; } = "";
        [BindProperty] public string ArtistName { get; set; } = "";
        [BindProperty] public string AlbumTitle { get; set; } = "";
        [BindProperty] public string GenreName { get; set; } = "";
        [BindProperty] public string DurationStr { get; set; } = "";
        [BindProperty] public IFormFile? TrackFile { get; set; }
        [BindProperty] public IFormFile? TrackCover { get; set; }

        public string? Message { get; set; }

        public IActionResult OnGet()
        {
            if (UserSession.Role != "администратор") return RedirectToPage("/Index");
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!TimeSpan.TryParse(DurationStr, out TimeSpan duration))
            {
                Message = "Ошибка: неверный формат длительности (ЧЧ:ММ:СС)";
                return Page();
            }
            if (TrackFile == null)
            {
                Message = "Ошибка: выберите аудиофайл";
                return Page();
            }

            var allowedExtensions = new[] { ".mp3", ".wav", ".m4a", ".ogg" };
            var extension = Path.GetExtension(TrackFile.FileName).ToLower();

            if (!allowedExtensions.Contains(extension) || !TrackFile.ContentType.StartsWith("audio/"))
            {
                Message = $"Ошибка: файл {TrackFile.FileName} не является допустимым аудиофайлом. Разрешены только: mp3, wav, m4a, ogg.";
                return Page();
            }
            try
            {
                string tracksFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "tracks");
                if (!Directory.Exists(tracksFolder)) Directory.CreateDirectory(tracksFolder);

                string trackFileName = Guid.NewGuid().ToString() + extension;
                string trackPath = Path.Combine(tracksFolder, trackFileName);

                using (var stream = new FileStream(trackPath, FileMode.Create))
                {
                    await TrackFile.CopyToAsync(stream);
                }
                string dbTrackPath = "/uploads/tracks/" + trackFileName;

                string? dbCoverPath = null;
                if (TrackCover != null)
                {
                    var allowedImgExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                    var imgExt = Path.GetExtension(TrackCover.FileName).ToLower();

                    if (!allowedImgExtensions.Contains(imgExt) || !TrackCover.ContentType.StartsWith("image/"))
                    {
                        Message = "Ошибка: файл обложки трека должен быть изображением.";
                        return Page();
                    }
                    string coversFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "covers");
                    if (!Directory.Exists(coversFolder)) Directory.CreateDirectory(coversFolder);
                    string coverFileName = Guid.NewGuid().ToString() + Path.GetExtension(TrackCover.FileName);
                    string coverPath = Path.Combine(coversFolder, coverFileName);

                    using (var stream = new FileStream(coverPath, FileMode.Create))
                    {
                        await TrackCover.CopyToAsync(stream);
                    }
                    dbCoverPath = "/uploads/covers/" + coverFileName;
                }
                await _context.CreateAlbum(AlbumTitle, ArtistName, null);
                await _context.UploadTrack(TrackTitle, ArtistName, AlbumTitle, dbTrackPath, duration, GenreName, dbCoverPath);

                Message = "Трек успешно опубликован";
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