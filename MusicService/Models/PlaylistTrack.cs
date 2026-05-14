using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicService.Models;

public partial class PlaylistTrack
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int PlaylistId { get; set; }

    [Required]
    public int TrackId { get; set; }

    [Required]
    public DateTime AddedAt { get; set; }

    [ForeignKey("PlaylistId")]
    public virtual Playlist Playlist { get; set; } = null!;

    [ForeignKey("TrackId")]
    public virtual Track Track { get; set; } = null!;
}
