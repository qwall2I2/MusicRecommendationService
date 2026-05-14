using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MusicService.Models;

namespace MusicService.Pages.Tracks
{
    public class LikedModel : PageModel
    {
        private readonly MusicServiceContext _context;
        public LikedModel(MusicServiceContext context) => _context = context;

        public List<Track> LikedTracks { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            if (UserSession.CurrentUserId == null) return RedirectToPage("/Accounts/Login");

            LikedTracks = await _context.Actions
                .Where(a => a.AccountId == UserSession.CurrentUserId && a.IsLike).Include(a => a.Track).ThenInclude(t => t.Album)
                .ThenInclude(alb => alb.Artist)
                .Include(a => a.Track)
                .ThenInclude(t => t.Genre)
                .Select(a => a.Track)
                .ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostUnlikeAsync(int trackId)
        {
            if (UserSession.CurrentUserId == null) return RedirectToPage("/Accounts/Login");

            await _context.DeleteUserAction(UserSession.CurrentUserId.Value, trackId);

            return RedirectToPage();
        }
    }
}