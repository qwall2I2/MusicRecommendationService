using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicService.Models;

public partial class Track
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string Title { get; set; } = null!;

    [Required]
    public TimeSpan Duration { get; set; }

    [Required]
    [StringLength(255)]
    public string FilePath { get; set; } = null!;

    [Required]
    public int AlbumId { get; set; }

    [Required]
    public int GenreId { get; set; }

    [StringLength(255)]
    public string? CoverPath { get; set; }

    public virtual ICollection<Action> Actions { get; set; } = new List<Action>();

    [ForeignKey("AlbumId")]
    public virtual Album Album { get; set; } = null!;

    [ForeignKey("GenreId")]
    public virtual Genre Genre { get; set; } = null!;

    public virtual ICollection<PlaylistTrack> PlaylistTracks { get; set; } = new List<PlaylistTrack>();

    [NotMapped]
    public string DisplayCoverPath => string.IsNullOrEmpty(CoverPath) ? "/uploads/covers/default_cover.jpg" : CoverPath;
}
