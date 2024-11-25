namespace AuthService.Helpers;

using MailKit.Net.Smtp;
using MimeKit;

public interface ISendMail
{
    Task SendVerifyEmailAsync(string email, string token);
    Task SendResetPasswordEmailAsync(string email, string token);
}

public class SendMail : ISendMail
{
    private readonly ILogger<SendMail> _logger;
    private readonly IConfiguration _configuration;
    private readonly string _smtpHost;
    private readonly int _smtpPort;
    private readonly string _smtpUsername;
    private readonly string _smtpPassword;
    private readonly string _clientUrl;
    public SendMail(IConfiguration configuration, ILogger<SendMail> logger)
    {
        _configuration = configuration;
        _logger = logger;
        _smtpHost = _configuration["Smtp:Host"];
        _smtpPort = int.Parse(_configuration["Smtp:Port"]);
        _smtpUsername = _configuration["Smtp:Username"];
        _smtpPassword = _configuration["Smtp:Password"];
        _clientUrl = _configuration["Smtp:ClientUrl"];
    }

    public async Task SendResetPasswordEmailAsync(string email, string token)
    {
         try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("System", "noreply-system-authentication@localhost.local"));
            message.To.Add(new MailboxAddress("User", email));
            message.Subject = "Reset Password";
            message.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = $"<p>Click this link to Reset Password : <a href='{this._clientUrl}/api/auth/reset-password/{token}'>Click</a></p>"
            };
            using var client = new SmtpClient();
            client.Connect(host: _smtpHost, port: _smtpPort, useSsl: false);
            client.Authenticate(_smtpUsername, _smtpPassword);
            await client.SendAsync(message);
            await client.DisconnectAsync(quit: true);
            this._logger.LogInformation($"Email sent {email}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            throw;
        }
    }

    public async Task SendVerifyEmailAsync(string email, string token)
    {
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("System", "noreply-system-authentication@localhost.local"));
            message.To.Add(new MailboxAddress("User", email));
            message.Subject = "Verify Email";
            message.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = $"<p>Click this link to verify your email : <a href='{this._clientUrl}/api/auth/verify-email/{token}'>Click</a></p>"
            };
            using var client = new SmtpClient();
            client.Connect(host: _smtpHost, port: _smtpPort, useSsl: false);
            client.Authenticate(_smtpUsername, _smtpPassword);
            await client.SendAsync(message);
            await client.DisconnectAsync(quit: true);
            this._logger.LogInformation($"Email sent {email}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            throw;
        }
    }
}