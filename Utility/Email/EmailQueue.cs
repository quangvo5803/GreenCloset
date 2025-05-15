using System.Collections.Concurrent;

namespace Utility.Email
{
    public class EmailQueue : IEmailQueue
    {
        private readonly ConcurrentQueue<(
            string Email,
            string Subject,
            string HtmlMessage
        )> _queue = new();

        public void QueueEmail(string email, string subject, string htmlMessage) =>
            _queue.Enqueue((email, subject, htmlMessage));

        public bool TryDequeue(out (string Email, string Subject, string HtmlMessage) emailData) =>
            _queue.TryDequeue(out emailData);
    }
}
