namespace EmailManager.Models
{
    public class Connection
    {
        public ServerTypes ServerType { get; set; }
        public Encryptions Encryption { get; set; }
        public string Server { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
