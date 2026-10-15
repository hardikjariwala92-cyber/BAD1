namespace PROJECT_SBV1.Models
{
    public class NotificationMessage
    {
        public int MessageId { get; set; }
        public string Sender { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public DateTime SentDate { get; set; } = DateTime.Now;

        public NotificationMessage() { }

        public NotificationMessage(int messageId, string sender, string subject, string content, bool isRead, DateTime sentDate)
        {
            MessageId = messageId;
            Sender = sender;
            Subject = subject;
            Content = content;
            IsRead = isRead;
            SentDate = sentDate;
        }
    }
}
