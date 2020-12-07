using EmailManager.Common;
using EmailManager.Common.Extensions;
using EmailManager.Contracts;
using EmailManager.Models;
using Limilabs.Client.IMAP;
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
    public class ImapEmailService : IEmailService
    {
        readonly ConcurrentDictionary<long, string> CachedMessages = new ConcurrentDictionary<long, string>();
        readonly ConcurrentBag<Email> emails = new ConcurrentBag<Email>();
        private readonly IConnection connection;

        public ImapEmailService(IConnection connection)
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
                var imap = await connection.GetInstance<Imap>() as Imap;
                imap.SelectInbox();

                List<long> uids = null;

                Log.Information("Starting getting uids");

                await Task.Run(() => { uids = imap.Search(Flag.All); });

                Log.Information("All uids loaded.");

                await connection.DisposeInstance(imap);

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

        private async Task DownloadMessageBodies(List<long> uids)
        {
            try
            {
                var imap = await connection.GetInstance<Imap>() as Imap;
                imap.SelectInbox();

                foreach (var uid in uids)
                {
                    BodyStructure structure = imap.GetBodyStructureByUID(uid);

                    string html = string.Empty;

                    if (structure.Html != null)
                        html = imap.GetTextByUID(structure.Html);

                    CachedMessages.TryAdd(uid, html);

                    Log.Information("Total Message Bodies Downloaded = '{0}' and it's running on thread {1}", CachedMessages.Count, Thread.CurrentThread.ManagedThreadId);
                }

                await connection.DisposeInstance(imap);
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

        private void DownloadEnvelopes(List<long> uids)
        {
            var imap = connection.GetInstance() as Imap;
            try
            {
                imap.SelectInbox();

                List<MessageInfo> infos = imap.GetMessageInfoByUID(uids);

                foreach (MessageInfo info in infos)
                {
                    var email = new Email
                    {
                        UID = info.UID.HasValue ? info.UID.Value.ToString() : string.Empty,
                        Subject = info.Envelope.Subject,
                        Date = info.Envelope.Date,
                    };

                    foreach (var m in info.Envelope.From)
                    {
                        email.From = $"{m.Name} ({ m.Address})";
                    }
                    emails.Add(email);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
            connection.DisposeInstance(imap);
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
            long id = Convert.ToInt64(uid);

            if (CachedMessages.TryGetValue(id, out var emailContent))
            {
                return emailContent;
            }

            return await DownloadEmailBody(uid);
        }

        public async Task<string> DownloadEmailBody<T>(T uid)
        {
            var htmlContent = string.Empty;
            long id = Convert.ToInt64(uid);
            var imap = await connection.GetInstance<Imap>() as Imap;

            await Task.Run(() =>
            {
                imap.SelectInbox();
                var eml = imap.GetMessageByUID(id);
                IMail email = new MailBuilder().CreateFromEml(eml);
               
                htmlContent = email.GetBodyAsHtml();

                CachedMessages.TryAdd(id, htmlContent);
            });

            await connection.DisposeInstance(imap);

            return htmlContent;
        }
    }
}
