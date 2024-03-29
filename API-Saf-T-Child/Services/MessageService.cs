using System;
using Microsoft.AspNetCore.Mvc;
using API_Saf_T_Child.Models;
using API_Saf_T_Child.Services;
using System.Net.Mail;
using Microsoft.Extensions.Options;

namespace API_Saf_T_Child.Services
{
    public class MessageService
    {
        private readonly MailSettings _mailSettings;

        public MessageService(IOptions<MailSettings> mailSettingsOptions)
        {
            _mailSettings = mailSettingsOptions.Value;
        }

        public async Task<bool> SendEmail(string to, string subject, string body)
        {
            var message = new MailMessage(_mailSettings.Email, to, subject, body);
            message.IsBodyHtml = true;

            SmtpClient gmailer = new SmtpClient {
                    Host = _mailSettings.Host,
                    Port = _mailSettings.Port,
                    UseDefaultCredentials = false,
                    EnableSsl = _mailSettings.EnableSSL,
                    Credentials = new System.Net.NetworkCredential(_mailSettings.Email, _mailSettings.Password)
                };

            try
            {
                await gmailer.SendMailAsync(message);
                gmailer.Dispose();
                return true;
            }
            catch
            {
                gmailer.Dispose();
                return false;
            }
        }
    }
}
