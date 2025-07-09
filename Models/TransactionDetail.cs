using System;
using System.Collections.Generic;

namespace P2P.Models;

public partial class TransactionDetail
{
    public int Id { get; set; }

    public int? TransactionId { get; set; }

    public string? Action { get; set; }

    public int? PerformedBy { get; set; }

    public DateTime? PerformedDate { get; set; }

    public string? Note { get; set; }
}
