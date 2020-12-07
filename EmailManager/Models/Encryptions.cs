using System.ComponentModel;

namespace EmailManager.Models
{
    public enum Encryptions
    {
        [Description("Unencrypted")]
        Unencrypted,

        [Description("SSL/TLS")]
        SSL_TLS,

        [Description("STARTTLS")]
        STARTTLS
    }
}
