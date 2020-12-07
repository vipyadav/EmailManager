using EmailManager.Common;
using EmailManager.Contracts;
using EmailManager.Models;
using Limilabs.Client.POP3;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;

namespace EmailManager.Connections
{
    public class Pop3Connection : IConnection
    {
        private readonly Connection connection;
        private static readonly ConcurrentBag<Pop3> instances = new ConcurrentBag<Pop3>();
        static readonly SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);
        private readonly object Lock = new object();

        public Pop3Connection(Connection connection)
        {
            this.connection = connection;
            ConnectionType = ServerTypes.POP3;
        }

        public ServerTypes ConnectionType { get; set; }

        public object GetInstance()
        {
            Pop3 instance = null;

            try
            {
                lock (Lock)
                {
                    instance = CreateInstance(instance);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
            return instance;
        }

        public async Task<object> GetInstance<T>()
        {
            await semaphoreSlim.WaitAsync();
            Pop3 instance = null;

            try
            {
                await Task.Run(() => instance = CreateInstance(instance));
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
            finally
            {
                semaphoreSlim.Release();
            }
            return instance;
        }

        private Pop3 CreateInstance(Pop3 instance)
        {
            if (instances.Count < Constants.MaxServerConnectionOrParallelThreads)
            {
                instance = new Pop3();
                switch (connection.Encryption)
                {
                    case Encryptions.Unencrypted:
                        instance.Connect(connection.Server, connection.Port);
                        instance.Login(connection.Username, connection.Password);
                        break;

                    case Encryptions.SSL_TLS:
                        instance.SSLConfiguration.EnabledSslProtocols = SslProtocols.Tls12;
                        instance.ConnectSSL(connection.Server, connection.Port);
                        instance.UseBestLogin(connection.Username, connection.Password);
                        break;

                    case Encryptions.STARTTLS:
                        instance.SSLConfiguration.EnabledSslProtocols = SslProtocols.Tls12;
                        instance.Connect(connection.Server, connection.Port);
                        instance.StartTLS();
                        instance.UseBestLogin(connection.Username, connection.Password);
                        break;
                }

                instances.Add(instance);
            }

            return instance;
        }

        public async Task DisposeInstance(object instance)
        {
            await semaphoreSlim.WaitAsync();

            var pop3 = instance as Pop3;

            try
            {
                pop3.Dispose();
                instances.TryTake(out pop3);
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }
    }
}
