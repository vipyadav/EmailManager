using EmailManager.Common;
using EmailManager.Common.Extensions;
using EmailManager.Contracts;
using EmailManager.Models;
using Limilabs.Client.POP3;
using Limilabs.Mail;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Log = Serilog.Log;

namespace EmailManager.Services
{
    public class Pop3EmailService : IEmailService
    {
        readonly ConcurrentDictionary<string, string> CachedMessages = new ConcurrentDictionary<string, string>();
        readonly ConcurrentBag<Email> emails = new ConcurrentBag<Email>();
        private readonly IConnection connection;

        public Pop3EmailService(IConnection connection)
        {
            this.connection = connection;
        }

        /// <summary>
        /// It downloads just the message envelopes/headers for all mail in the inbox, and store header infos including 'From', 'Subject' and 'Date'
        /// into 'emails' collection. 
        /// While downloading the envelopes, it also be downloading the message bodies (the actual HTML/Text) of the downloaded envelopes in separate threads,
        ///  and storing them into 'CachedMessages' collection. so they're ready to be viewed when a message is selected.
        /// </summary>
        /// <returns> IEnumerable of Emails </returns>
        public async Task<IEnumerable<Email>> DownloadEmails()
        {
            try
            {
                List<string> uids = null;

                Log.Information("Starting getting uids");

                var pop3 = await connection.GetInstance<Pop3>() as Pop3;

                await Task.Run(() => uids = pop3.GetAll());

                await connection.DisposeInstance(pop3);

                Log.Information("All uids loaded.");

                var listPartitions = uids?.Split(Constants.MaxServerConnectionOrParallelThreads - 1);

                Log.Information("Download Envelopes Started...");

                await Task.Run(() => Parallel.ForEach(listPartitions, x => DownloadEnvelopes(x)));

                Log.Information("Download Envelopes Finished...");

                Log.Information("Download Message Bodies Started...");

                Parallel.ForEach(listPartitions, x =>
                {
                    Task.Factory.StartNew(() => DownloadMessageBodies(x), TaskCreationOptions.LongRunning);
                });

                Log.Information("Download Message Bodies Finished...");
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }

            return emails.AsEnumerable();
        }

        private async Task DownloadMessageBodies(List<string> uids)
        {
            try
            {
                MailBuilder builder = new MailBuilder();
                var pop3 = await connection.GetInstance<Pop3>() as Pop3;

                foreach (string uid in uids)
                {
                    IMail email = builder.CreateFromEml(pop3.GetMessageByUID(uid));

                    CachedMessages.TryAdd(uid, email.Html);

                    Log.Information("Total Message Bodies Downloaded = '{0}' and it's running on thread {1}", CachedMessages.Count, Thread.CurrentThread.ManagedThreadId);
                }

                await connection.DisposeInstance(pop3);
            }
            catch (System.Net.Sockets.SocketException e)
            {
                Log.Error(e.Message);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
        }

        private void DownloadEnvelopes(List<string> uids)
        {
            var pop3 = connection.GetInstance() as Pop3;
            try
            {
                MailBuilder builder = new MailBuilder();

                foreach (string uid in uids)
                {
                    var headers = pop3.GetHeadersByUID(uid);
                    IMail email = builder.CreateFromEml(headers);

                    var emailHeaderInfo = new Email
                    {
                        UID = uid,
                        Subject = email.Subject,
                        Date = email.Date,
                    };

                    foreach (var m in email.From)
                    {
                        emailHeaderInfo.From = $"{m.Name} ({ m.Address})";
                    }
                    emails.Add(emailHeaderInfo);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }

            connection.DisposeInstance(pop3);
        }

        /// <summary>
        /// This method is used to Get or Download Email's Message Body on Demand. If the body is already downloaded once then 
        /// it will return from cached List else will download from Email Server. 
        /// </summary>
        /// <typeparam name="T">type of uid</typeparam>
        /// <param name="uid">unique Id of message</param>
        /// <returns>It returns selected email's body as HTML</returns>
        public async Task<string> GetMessageBody<T>(T uid)
        {
            string id = Convert.ToString(uid);

            if (CachedMessages.TryGetValue(id, out var emailContent))
            {
                return emailContent;
            }

            return await DownloadEmailBody(uid);
        }

        public async Task<string> DownloadEmailBody<T>(T uid)
        {
            var htmlContent = string.Empty;

            var pop3 = await connection.GetInstance<Pop3>() as Pop3;

            await Task.Run(() =>
            {
                MailBuilder builder = new MailBuilder();

                IMail email = builder.CreateFromEml(pop3.GetMessageByUID(uid.ToString()));

                CachedMessages.TryAdd(uid.ToString(), email.Html);

                htmlContent = email.Html;
            });

            await connection.DisposeInstance(pop3);

            return htmlContent;
        }
    }
}
