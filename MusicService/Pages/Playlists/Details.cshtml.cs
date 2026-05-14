using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MusicService.Models;

namespace MusicService.Pages.Playlists
{
    public class DetailsModel : PageModel
    {
        private readonly MusicServiceContext _context;
        public DetailsModel(MusicServiceContext context) => _context = context;

        public Playlist Playlist { get; set; } = default!;

        public List<int> LikedTrackIds { get; set; } = new();
        public List<int> DislikedTrackIds { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            if (UserSession.CurrentUserId == null) return RedirectToPage("/Accounts/Login");

            var playlist = await _context.Playlists
                .Include(p => p.PlaylistTracks)
                    .ThenInclude(pt => pt.Track)
                    .ThenInclude(t => t.Album)
                    .ThenInclude(a => a.Artist)
                .Include(p => p.PlaylistTracks)
                    .ThenInclude(pt => pt.Track)
                    .ThenInclude(t => t.Genre)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (playlist == null || playlist.AccountId != UserSession.CurrentUserId)
                return NotFound();

            Playlist = playlist;

            var actions = await _context.Actions
                .Where(a => a.AccountId == UserSession.CurrentUserId)
                .ToListAsync();

            LikedTrackIds = actions.Where(a => a.IsLike).Select(a => a.TrackId).ToList();
            DislikedTrackIds = actions.Where(a => !a.IsLike).Select(a => a.TrackId).ToList();

            return Page();
        }

        public async Task<IActionResult> OnPostRemoveTrackAsync(int ptId, int pId)
        {
            var link = await _context.PlaylistTracks.FindAsync(ptId);
            if (link != null)
            {
                _context.PlaylistTracks.Remove(link);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Трек удален из плейлиста";
            }
            return RedirectToPage(new { id = pId });
        }

        public async Task<IActionResult> OnPostToggleLikeAsync(int trackId, int pId)
        {
            var existingLike = await _context.Actions
                .FirstOrDefaultAsync(a => a.AccountId == UserSession.CurrentUserId && a.TrackId == trackId && a.IsLike == true);

            if (existingLike != null)
                await _context.DeleteUserAction(UserSession.CurrentUserId!.Value, trackId);
            else
                await _context.RegisterUserAction(UserSession.CurrentUserId!.Value, trackId, true);

            return RedirectToPage(new { id = pId });
        }

        public async Task<IActionResult> OnPostToggleDislikeAsync(int trackId, int pId)
        {
            var existingDislike = await _context.Actions
                .FirstOrDefaultAsync(a => a.AccountId == UserSession.CurrentUserId && a.TrackId == trackId && a.IsLike == false);

            if (existingDislike != null)
                await _context.DeleteUserAction(UserSession.CurrentUserId!.Value, trackId);
            else
                await _context.RegisterUserAction(UserSession.CurrentUserId!.Value, trackId, false);

            return RedirectToPage(new { id = pId });
        }
    }
}