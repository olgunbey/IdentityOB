namespace PermiCore.Auth.Entity
{
    public class Outbox
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public string Payload { get; set; }
        public DateTime WriteDateTime { get; set; }
        public bool IsDeleted { get; set; }
    }
}
