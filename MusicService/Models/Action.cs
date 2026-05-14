using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicService.Models;

public partial class Action
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int AccountId { get; set; }

    [Required]
    public int TrackId { get; set; }

    [Required]
    public bool IsLike { get; set; }

    [Required]
    [Range(0, 1.50)]
    public decimal Weight { get; set; }

    [Required]
    public DateTime UpdatedAt { get; set; }

    [ForeignKey("AccountId")]
    public virtual Account Account { get; set; } = null!;

    [ForeignKey("TrackId")]
    public virtual Track Track { get; set; } = null!;
}
