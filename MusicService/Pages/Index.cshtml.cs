using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MusicService.Models;

namespace MusicService.Pages
{
    public class IndexModel : PageModel
    {
        private readonly MusicServiceContext _context;
        public IndexModel(MusicServiceContext context) => _context = context;

        public List<Track> Recommendations { get; set; } = new();

        public List<int> LikedTrackIds { get; set; } = new();
        public List<int> DislikedTrackIds { get; set; } = new();

        public async Task OnGetAsync()
        {
            if (UserSession.CurrentUserId != null)
            {
                Recommendations = await _context.GetRecommendations(UserSession.CurrentUserId.Value);

                var actions = await _context.Actions
                    .Where(a => a.AccountId == UserSession.CurrentUserId)
                    .ToListAsync();

                LikedTrackIds = actions.Where(a => a.IsLike).Select(a => a.TrackId).ToList();
                DislikedTrackIds = actions.Where(a => !a.IsLike).Select(a => a.TrackId).ToList();
            }
        }

        public async Task<IActionResult> OnPostToggleLikeAsync(int trackId)
        {
            if (UserSession.CurrentUserId == null) return RedirectToPage("/Accounts/Login");

            var existingLike = await _context.Actions
                .FirstOrDefaultAsync(a => a.AccountId == UserSession.CurrentUserId && a.TrackId == trackId && a.IsLike == true);

            if (existingLike != null)
            {
                await _context.DeleteUserAction(UserSession.CurrentUserId.Value, trackId);
            }
            else
            {
                await _context.RegisterUserAction(UserSession.CurrentUserId.Value, trackId, true);
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostToggleDislikeAsync(int trackId)
        {
            if (UserSession.CurrentUserId == null) return RedirectToPage("/Accounts/Login");

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

            return RedirectToPage();
        }

        public IActionResult OnPostLogout()
        {
            UserSession.Logout();
            return RedirectToPage("/Index");
        }
    }
}