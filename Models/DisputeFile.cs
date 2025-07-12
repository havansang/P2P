using System;
using System.Collections.Generic;

namespace P2P.Models;

public partial class DisputeFile
{
    public int Id { get; set; }

    public int DisputeId { get; set; }

    public string? FilePath { get; set; }

    public string? FileName { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? CreatedBy { get; set; }
}
