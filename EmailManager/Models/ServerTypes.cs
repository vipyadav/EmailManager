using System.ComponentModel;

namespace EmailManager.Models
{
    public enum ServerTypes
    {
        [Description("IMAP")]
        IMAP,

        [Description("POP3")]
        POP3
    }
}
