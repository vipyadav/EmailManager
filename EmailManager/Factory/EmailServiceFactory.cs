using EmailManager.Contracts;
using EmailManager.Models;
using EmailManager.Services;

namespace EmailManager.Factory
{
    public class EmailServiceFactory : IEmailServiceFactory
    {
        public IEmailService GetInstance(IConnection connection)
        {
            IEmailService emailService = null;

            switch (connection.ConnectionType)
            {
                case ServerTypes.IMAP:
                    emailService = new ImapEmailService(connection);
                    break;
                case ServerTypes.POP3:
                    emailService = new Pop3EmailService(connection);
                    break;
            }

            return emailService;
        }
    }
}
