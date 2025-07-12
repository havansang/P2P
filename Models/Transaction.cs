using System;
using System.Collections.Generic;

namespace P2P.Models;

public partial class Transaction
{
    public int Id { get; set; }

    public string TransactionId { get; set; } = null!;

    public int SenderId { get; set; }

    public int ReceiverId { get; set; }

    public decimal Amount { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdateDate { get; set; }

    public string? Description { get; set; }

    public int Status { get; set; }

    public string? PaymentMethod { get; set; }

    public string? BankCode { get; set; }

    public string? AccountNumber { get; set; }

    public int? FeeSend { get; set; }

    public int? FeeReceive { get; set; }
}
