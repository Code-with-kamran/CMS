namespace CMS.Services
{
    public sealed class EmailSettings
    {
        public string Host { get; init; } = "";        // e.g. smtp.yourhost.com
        public int Port { get; init; } = 587;          // 587 = STARTTLS (recommended)
        public string Username { get; init; } = "";    // SMTP username (often the email)
        public string Password { get; init; } = "";    // SMTP password / app password
        public string FromName { get; init; } = "Clinic";
        public string FromAddress { get; init; } = ""; // no-reply@yourhost.com
        public bool EnableSsl { get; init; } = true;   // STARTTLS
        public bool UseDefaultCredentials { get; init; } = false;
    }
}
//cydc tdyz cruw tofe