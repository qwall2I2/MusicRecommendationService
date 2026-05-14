using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MusicService.Models;

public partial class Role
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string Name { get; set; } = null!;
}
