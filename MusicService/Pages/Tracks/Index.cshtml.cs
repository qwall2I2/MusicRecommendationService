using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MusicService.Models;
using Npgsql;

namespace MusicService.Pages.Tracks
{
    public class IndexModel : PageModel
    {
        private readonly MusicServiceContext _context;
        public IndexModel(MusicServiceContext context) => _context = context;

        public IList<Track> Track { get; set; } = new List<Track>();
        public List<int> LikedTrackIds { get; set; } = new();
        public List<int> DislikedTrackIds { get; set; } = new();
        public List<Playlist> UserPlaylists { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? SearchString { get; set; }

        public async Task OnGetAsync()
        {
            var tracksQuery = _context.Tracks
                .Include(t => t.Album).ThenInclude(a => a.Artist)
                .Include(t => t.Genre)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(SearchString))
            {
                string lowerSearch = SearchString.Trim().ToLower();
                tracksQuery = tracksQuery.Where(s =>
                    s.Title.ToLower().Contains(lowerSearch) ||
                    s.Album.Artist.Name.ToLower().Contains(lowerSearch) ||
                    s.Album.Title.ToLower().Contains(lowerSearch));
            }
            Track = await tracksQuery.ToListAsync();

            if (UserSession.CurrentUserId != null)
            {
                var actions = await _context.Actions
                    .Where(a => a.AccountId == UserSession.CurrentUserId)
                    .ToListAsync();

                LikedTrackIds = actions.Where(a => a.IsLike).Select(a => a.TrackId).ToList();
                DislikedTrackIds = actions.Where(a => !a.IsLike).Select(a => a.TrackId).ToList();

                UserPlaylists = await _context.Playlists
                    .Where(p => p.AccountId == UserSession.CurrentUserId)
                    .ToListAsync();
            }
        }

        public async Task<IActionResult> OnPostToggleLikeAsync(int trackId, string? SearchString)
        {
            if (UserSession.CurrentUserId == null) return RedirectToPage("/Accounts/Login");

            try
            {
                var existingLike = await _context.Actions
                    .FirstOrDefaultAsync(a => a.AccountId == UserSession.CurrentUserId && a.TrackId == trackId && a.IsLike == true);

                if (existingLike != null)
                    await _context.DeleteUserAction(UserSession.CurrentUserId.Value, trackId);
                else
                    await _context.RegisterUserAction(UserSession.CurrentUserId.Value, trackId, true);
            }
            catch (Npgsql.PostgresException ex) when (ex.SqlState == "23503")
            {
                TempData["ErrorCatalog"] = "Трек недоступен";
            }

            return RedirectToPage(new { SearchString = SearchString });
        }

        public async Task<IActionResult> OnPostToggleDislikeAsync(int trackId, string? SearchString)
        {
            if (UserSession.CurrentUserId == null) return RedirectToPage("/Accounts/Login");
            try
            {
                var existingDislike = await _context.Actions
                    .FirstOrDefaultAsync(a => a.AccountId == UserSession.CurrentUserId && a.TrackId == trackId && a.IsLike == false);

                if (existingDislike != null)
                {
                    await _context.DeleteUserAction(UserSession.CurrentUserId.Value, trackId);
                }
                else
                {
                    await _context.RegisterUserAction(UserSession.CurrentUserId.Value, trackId, false);
                }
            }
            catch (Npgsql.PostgresException ex) when (ex.SqlState == "23503")
            {
                TempData["ErrorCatalog"] = "Трек недоступен";
            }
            catch (Exception ex)
            {
                TempData["ErrorCatalog"] = "Ошибка оценки";
            }

            return RedirectToPage(new { SearchString = SearchString });
        }

        public async Task<IActionResult> OnPostAddToPlaylistAsync(int trackId, int playlistId, string? SearchString)
        {
            if (UserSession.CurrentUserId == null) return RedirectToPage("/Accounts/Login");

            try
            {
                await _context.AddTrackToPlaylist(playlistId, trackId);
                TempData["SuccessCatalog"] = "Трек добавлен в плейлист";
            }
            catch (Npgsql.PostgresException ex)
            {
                if (ex.SqlState == "23503")
                {
                    TempData["ErrorCatalog"] = "Трек недоступен";
                }
                else
                {
                    TempData["ErrorCatalog"] = "Ошибка добавления";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorCatalog"] = "Ошибка добавления";
            }

            return RedirectToPage(new { SearchString = SearchString });
        }

        public async Task<IActionResult> OnPostDeleteTrackAsync(int id)
        {
            if (UserSession.Role != "администратор")
            {
                return RedirectToPage("/Index");
            }

            var track = await _context.Tracks.FindAsync(id);

            if (track != null)
            {
                try
                {
                    _context.Tracks.Remove(track);
                    await _context.SaveChangesAsync();
                    TempData["SuccessCatalog"] = "Трек успешно удален из системы";
                }
                catch (Exception ex)
                {
                    TempData["ErrorCatalog"] = "Ошибка при удалении: " + ex.Message;
                }
            }
            return RedirectToPage(new { SearchString = SearchString });
        }
    }
}