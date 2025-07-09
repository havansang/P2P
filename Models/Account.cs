using System;
using System.Collections.Generic;

namespace P2P.Models;

public partial class Account
{
    public int Id { get; set; }

    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public int Role { get; set; }

    public string? FullName { get; set; }

    public string? PhoneNumber { get; set; }

    public string? AvatarUrl { get; set; }

    public decimal? Wallet { get; set; }

    public bool? IsDelete { get; set; }

    public DateTime? CreatedDate { get; set; }
}
