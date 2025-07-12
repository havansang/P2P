using System;
using System.Collections.Generic;

namespace P2P.Models;

public partial class Dispute
{
    public int Id { get; set; }

    public string? TransactionId { get; set; }

    public int? DisputeBy { get; set; }

    public int? DisputeTo { get; set; }

    public string? Description { get; set; }

    public int? Status { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }
}
