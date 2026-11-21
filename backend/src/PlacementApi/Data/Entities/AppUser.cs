using System;
using System.Collections.Generic;

namespace PlacementApi.Data.Entities;

public partial class AppUser
{
    public int UserId { get; set; }

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string Role { get; set; } = null!;

    public int? StudentId { get; set; }

    public virtual Student? Student { get; set; }
}
