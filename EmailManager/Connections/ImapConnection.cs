using EmailManager.Common;
using EmailManager.Contracts;
using EmailManager.Models;
using Limilabs.Client.IMAP;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;

namespace EmailManager.Connections
{
    public class ImapConnection : IConnection
    {
        private readonly Connection connection;
        private static readonly ConcurrentBag<Imap> instances = new ConcurrentBag<Imap>();
        static SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);
        private readonly object Lock = new object();

        public ImapConnection(Connection connection)
        {
            this.connection = connection;
            ConnectionType = ServerTypes.IMAP;
        }

        public ServerTypes ConnectionType { get; set; }

        public object GetInstance()
        {
            Imap instance = null;

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
            Imap instance = null;

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

        private Imap CreateInstance(Imap instance)
        {
            try
            {
                if (instances.Count < Constants.MaxServerConnectionOrParallelThreads)
                {
                    instance = new Imap();
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
            }
            catch (System.Net.Sockets.SocketException e)
            {
                Log.Error(e.Message);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }

            return instance;
        }

        public async Task DisposeInstance(object instance)
        {
            await semaphoreSlim.WaitAsync();

            var imap = instance as Imap;

            try
            {
                imap.Dispose();
                instances.TryTake(out imap);
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }
    }
}
