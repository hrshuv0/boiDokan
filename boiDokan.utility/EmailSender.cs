using Mailjet.Client;
using Mailjet.Client.Resources;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using MimeKit;
using Newtonsoft.Json.Linq;

namespace boiDokan.utility;

public class EmailSender : IEmailSender
{
    private readonly IConfiguration _configuration;
    public MailJetSettings _mailJetOptions;
    
    public EmailSender(IConfiguration configuration)
    {
        _configuration = configuration;
        _mailJetOptions = _configuration.GetSection("MailJet").Get<MailJetSettings>();
    }
    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        // var emailToSend = new MimeMessage();
        // emailToSend.From.Add(MailboxAddress.Parse("email.com"));
        // emailToSend.To.Add(MailboxAddress.Parse(email));
        // emailToSend.Subject = subject;
        // emailToSend.Body = new TextPart(MimeKit.Text.TextFormat.Html){Text = htmlMessage};
        //
        // using (var emailClient = new SmtpClient())
        // {
        //     emailClient.Connect("smtppro.zoho.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
        //     emailClient.Authenticate("username", "password");
        //     emailClient.Send(emailToSend);
        //     emailClient.Disconnect(true);
        // }
        // return Task.CompletedTask;
        
        MailjetClient client = new MailjetClient(_mailJetOptions.ApiKey, _mailJetOptions.SecretKey) {
            Version = ApiVersion.V3_1,
        };
        MailjetRequest request = new MailjetRequest {
                Resource = Send.Resource,
            }
            .Property(Send.Messages, new JArray {
                new JObject {
                    {
                        "From",
                        new JObject {
                            {"Email", "info@boidokan.live"}, 
                            {"Name", "boiDokan"}
                        }
                    }, {
                        "To",
                        new JArray {
                            new JObject {
                                {
                                    "Email",
                                    email
                                }, {
                                    "Name",
                                    "boiDokan"
                                }
                            }
                        }
                    }, {
                        "Subject",
                        subject
                    }, {
                        "HTMLPart",
                        htmlMessage
                    }
                }
            });
        await client.PostAsync(request);
        // if (response.IsSuccessStatusCode) {
        //     Console.WriteLine(string.Format("Total: {0}, Count: {1}\n", response.GetTotal(), response.GetCount()));
        //     Console.WriteLine(response.GetData());
        // } else {
        //     Console.WriteLine(string.Format("StatusCode: {0}\n", response.StatusCode));
        //     Console.WriteLine(string.Format("ErrorInfo: {0}\n", response.GetErrorInfo()));
        //     Console.WriteLine(response.GetData());
        //     Console.WriteLine(string.Format("ErrorMessage: {0}\n", response.GetErrorMessage()));
        // }
    }
}