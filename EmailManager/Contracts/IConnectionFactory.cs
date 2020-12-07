using EmailManager.Models;

namespace EmailManager.Contracts
{
    public interface IConnectionFactory
    {
        IConnection GetInstance(Connection connectionInfo);
    }
}
