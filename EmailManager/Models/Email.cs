using System;

namespace EmailManager.Models
{
    public class Email 
    {
        public string UID { get; set; }
        public DateTime? Date { get; set; }
        public string Subject { get; set; }
        public string From { get; set; }
    }
}
