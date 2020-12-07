using EmailManager.Models;
using System.Threading.Tasks;

namespace EmailManager.Contracts
{
    public interface IConnection
    {
        ServerTypes ConnectionType { get; set; }
        Task<object> GetInstance<T>();
        object GetInstance();
        Task DisposeInstance(object instance);
    }
}
