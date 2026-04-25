namespace YarpExample.Gateway.Entity
{
    public class Outbox
    {
        public int Id { get; set; }
        public Guid IdempotencyId { get; set; }

        public string InboxOutboxType { get; set; }
        public string Payload { get; set; }

        public DateTime WriteDateTime { get; set; }
        public bool IsDeleted { get; set; }
    }

}
