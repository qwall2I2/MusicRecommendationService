using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicService.Models;

public partial class Account
{
    [Key]
    public int Id { get; set; }

    [Required]
    [EmailAddress]
    [StringLength(50)]
    public string Email { get; set; } = null!;

    [Required]
    [StringLength(255)]
    public string Password { get; set; } = null!;

    [Required]
    [StringLength(50)]
    public string FirstName { get; set; } = null!;

    [Required]
    [StringLength(50)]
    public string LastName { get; set; } = null!;

    [StringLength(50)]
    public string? Patronymic { get; set; }

    [Required]
    public int RoleId { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; }

    [ForeignKey("RoleId")]
    public virtual Role Role { get; set; } = null!;

    public virtual ICollection<Action> Actions { get; set; } = new List<Action>();

    public virtual ICollection<Playlist> Playlists { get; set; } = new List<Playlist>();
}
