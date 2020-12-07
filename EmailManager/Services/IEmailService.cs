using EmailManager.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EmailManager.Services
{
    public interface IEmailService
    {
        Task<string> GetMessageBody<T>(T uid);
        Task<IEnumerable<Email>> DownloadEmails();
    }
}
