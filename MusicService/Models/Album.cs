using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicService.Models;

public partial class Album
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string Title { get; set; } = null!;

    [Required]
    public int ArtistId { get; set; }

    [Required]
    public DateTime ReleaseDate { get; set; }

    [StringLength(255)]
    public string? CoverPath { get; set; }

    [ForeignKey("ArtistId")]
    public virtual Artist Artist { get; set; } = null!;

    public virtual ICollection<Track> Tracks { get; set; } = new List<Track>();
    
    [NotMapped]
    public string DisplayCoverPath => string.IsNullOrEmpty(CoverPath) ? "/uploads/covers/default_cover.jpg" : CoverPath;
}
