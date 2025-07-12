namespace P2P.Models.DTOs
{
    public class ClaimViewModel
    {
        public string TransactionId { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public string SenderName { get; set; }
    }
}
