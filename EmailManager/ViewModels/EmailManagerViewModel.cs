using EmailManager.Common.BaseViewModels;
using EmailManager.Contracts;
using EmailManager.Models;
using EmailManager.Services;
using MvvmCross.Commands;
using Serilog;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

namespace EmailManager.ViewModels
{
    public class EmailManagerViewModel : BaseViewModel
    {
        private IEmailService emailService;
        private readonly IConnectionFactory connectionFactory;
        private readonly IEmailServiceFactory emailServiceFactory;

        static SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);
        public EmailManagerViewModel(IConnectionFactory connectionFactory, IEmailServiceFactory emailServiceFactory)
        {
            this.connectionFactory = connectionFactory;
            this.emailServiceFactory = emailServiceFactory;
            InitializeDefaults();
        }

        #region Properties

        private ServerTypes serverType;
        public ServerTypes ServerType
        {
            get => serverType;
            set { serverType = value; RaisePropertyChanged(() => ServerType); ClearOrReset(); }
        }

        private Encryptions encryption;
        public Encryptions Encryption
        {
            get => encryption;
            set { encryption = value; RaisePropertyChanged(() => Encryption); }
        }

        private string server;
        public string Server
        {
            get => server;
            set { server = value; RaisePropertyChanged(() => Server); }
        }

        private int port;
        public int Port
        {
            get => port;
            set { port = value; RaisePropertyChanged(() => Port); }
        }

        private string username;
        public string Username
        {
            get => username;
            set { username = value; RaisePropertyChanged(() => Username); }
        }

        private string password;
        public string Password
        {
            get => password;
            set { password = value; RaisePropertyChanged(() => Password); }
        }

        private string emailContent;
        public string EmailContent
        {
            get => emailContent;
            set { emailContent = value; RaisePropertyChanged(() => EmailContent); }
        }

        private ObservableCollection<Email> emails = new ObservableCollection<Email>();
        public ObservableCollection<Email> Emails
        {
            get => emails;
            set { emails = value; RaisePropertyChanged(() => Emails); }
        }

        private Email selectedEmail;
        public Email SelectedEmail
        {
            get => selectedEmail;
            set { selectedEmail = value; RaisePropertyChanged(() => SelectedEmail); }
        }

        #endregion

        #region Commands
        public IMvxAsyncCommand StartCommand => new MvxAsyncCommand(OnStart, CanExcuteStart);

        private bool CanExcuteStart()
        {
            return !(string.IsNullOrEmpty(Username) ||
                   string.IsNullOrEmpty(Password) || 
                   string.IsNullOrEmpty(Server));
        }

        public IMvxAsyncCommand<Email> SelectedItemChangedCommand => new MvxAsyncCommand<Email>(OnSelectionChanged);

        private async Task OnStart()
        {
            Log.Information("Start Command has been Sent.");

            emailService = GetEmailService();

            var emailData = await emailService.DownloadEmails();

            Emails = new ObservableCollection<Email>(emailData);
        }

        private async Task OnSelectionChanged(Email email)
        {
            await semaphoreSlim.WaitAsync();

            try
            {
                EmailContent = "Loading...";

                Log.Information("Selected Email's body Downloading command has been sent.");

                if (email != null && !string.IsNullOrEmpty(email.UID))
                {
                    EmailContent = await emailService.GetMessageBody(email.UID);
                }
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }

        #endregion

        private IEmailService GetEmailService()
        {
            var con = new Connection
            {
                ServerType = ServerType,
                Server = Server,
                Encryption = Encryption,
                Port = Port,
                Password = Password,
                Username = Username
            };

            IConnection connection = connectionFactory.GetInstance(con);

            return emailServiceFactory.GetInstance(connection);
        }

        private void InitializeDefaults()
        {
            ServerType = ServerTypes.IMAP;
            Server = "imap.gmail.com";
            Port = 993;
            Encryption = Encryptions.SSL_TLS;
        }

        private void ClearOrReset()
        {
            Server = string.Empty;
            Emails.Clear();
            EmailContent = string.Empty;
        }
    }
}
