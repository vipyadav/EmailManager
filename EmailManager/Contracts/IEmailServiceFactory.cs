using EmailManager.Services;

namespace EmailManager.Contracts
{
    public interface IEmailServiceFactory
    {
        IEmailService GetInstance(IConnection connection);
    }
}
