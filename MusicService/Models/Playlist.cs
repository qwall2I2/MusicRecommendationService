using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicService.Models;

public partial class Playlist
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string Title { get; set; } = null!;

    [StringLength(255)]
    public string? CoverPath { get; set; }

    [Required]
    public int AccountId { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; }

    [ForeignKey("AccountId")]
    public virtual Account Account { get; set; } = null!;

    public virtual ICollection<PlaylistTrack> PlaylistTracks { get; set; } = new List<PlaylistTrack>();

    [NotMapped]
    public string DisplayCoverPath => string.IsNullOrEmpty(CoverPath) ? "/uploads/covers/default_cover.jpg" : CoverPath;
}
