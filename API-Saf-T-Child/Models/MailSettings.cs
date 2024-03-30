namespace API_Saf_T_Child.Models
{
    public class MailSettings
    {
        public string Host { get; set; } = null!;
        public int Port { get; set; }
        public string SenderName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public bool EnableSSL { get; set; }

    }
}
