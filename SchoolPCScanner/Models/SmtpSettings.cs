namespace SchoolPCScanner.Models
{
    // SmtpSettings class to store the SMTP settings
    public class SmtpSettings
    {
        public string Host { get; set; }
        public int Port { get; set; } 
        public string Username { get; set; }
        public string Password { get; set; }
        public bool EnableSsl { get; set; }
        public string From { get; set; }
        public string To { get; set; }
    }

}
