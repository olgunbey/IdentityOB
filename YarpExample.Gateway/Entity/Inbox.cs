namespace YarpExample.Gateway.Entity
{
    public class Inbox
    {
        public int Id { get; set; }
        public Guid IdempotencyId { get; set; }
        public string InboxOutboxType { get; set; }
        public string Payload { get; set; }
        public DateTime WriteDateTime { get; set; }
        public DateTime? ProcessedDateTime { get; set; }
    }
}
