namespace byte_hunt.Services;

using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using MimeKit;

/// <summary>
///     Serviço de envio de emails através de SMTP.
///     Implementa a interface IEmailSender para integração com o sistema de identidade.
/// </summary>
public class SmtpEmailSender : IEmailSender
{
    private readonly IConfiguration _config;

    /// <summary>
    ///     Construtor do serviço de envio de emails SMTP.
    /// </summary>
    /// <param name="config">Configuração da aplicação para acesso às definições do servidor SMTP.</param>
    public SmtpEmailSender(IConfiguration config)
    {
        _config = config;
    }

    /// <summary>
    ///     Envia um email através do servidor SMTP configurado.
    /// </summary>
    /// <param name="email">Endereço de email do destinatário.</param>
    /// <param name="subject">Assunto do email.</param>
    /// <param name="htmlMessage">Conteúdo do email em formato HTML.</param>
    /// <returns>Uma tarefa que representa a operação assíncrona de envio de email.</returns>
    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        var settings = _config.GetSection("EmailSettings");

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(settings["SenderName"], settings["SenderEmail"]));
        message.To.Add(MailboxAddress.Parse(email));
        message.Subject = subject;

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = htmlMessage
        };
        message.Body = bodyBuilder.ToMessageBody();

        using var client = new SmtpClient();
        client.ServerCertificateValidationCallback = (s, c, h, e) => true; // Ignora validação de certificado
        await client.ConnectAsync(settings["SmtpServer"], int.Parse(settings["SmtpPort"]), SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(settings["Username"], settings["Password"]);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}
