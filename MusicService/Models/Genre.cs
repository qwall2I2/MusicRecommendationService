using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MusicService.Models;

public partial class Genre
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string Name { get; set; } = null!;

    public virtual ICollection<Track> Tracks { get; set; } = new List<Track>();
}
