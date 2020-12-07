using EmailManager.Connections;
using EmailManager.Contracts;
using EmailManager.Models;

namespace EmailManager.Factory
{
    public class ConnectionFactory : IConnectionFactory
    {
        public IConnection GetInstance(Connection connectionInfo)
        {
            IConnection connection = null;

            switch (connectionInfo.ServerType)
            {
                case ServerTypes.IMAP:
                    connection =  new ImapConnection(connectionInfo);
                    break;
                case ServerTypes.POP3:
                    connection = new Pop3Connection(connectionInfo);
                    break;
            }

            return connection;
        }
    }
}
