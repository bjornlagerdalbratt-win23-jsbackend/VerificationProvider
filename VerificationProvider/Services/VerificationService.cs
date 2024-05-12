using Azure.Messaging.ServiceBus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VerificationProvider.Contexts;
using VerificationProvider.Entities;
using VerificationProvider.Models;

namespace VerificationProvider.Services;

public class VerificationService(ILogger<VerificationService> logger, IServiceProvider serviceProvider) : IVerificationService
{
    private readonly ILogger<VerificationService> _logger = logger;
    private readonly IServiceProvider _serviceProvider = serviceProvider;



    public VerificationRequestModel UnpackVerificationRequest(ServiceBusReceivedMessage message)
    {
        try
        {
            var verificationRequest = JsonConvert.DeserializeObject<VerificationRequestModel>(message.Body.ToString());
            if (verificationRequest != null && !string.IsNullOrEmpty(verificationRequest.Email))
                return verificationRequest;
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR : GenerateVerificationCode.Run() :: {ex.Message}");
        }

        return null!;
    }

    public string GenerateCode()
    {
        try
        {
            var rnd = new Random();
            var code = rnd.Next(100000, 999999);

            return code.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR : GenerateCode :: {ex.Message}");
        }

        return null!;
    }

    public async Task<bool> SaveVerificationRequest(VerificationRequestModel verificationRequest, string code)
    {
        try
        {
            using var context = _serviceProvider.GetRequiredService<DataContext>();

            var existingRequest = await context.VerificationRequests.FirstOrDefaultAsync(x => x.Email == verificationRequest.Email);
            if (existingRequest != null)
            {
                existingRequest.Code = code;
                existingRequest.ExpiryDate = DateTime.Now.AddMinutes(5);
                context.Entry(existingRequest).State = EntityState.Modified;
            }
            else
            {
                context.VerificationRequests.Add(new VerificationRequest() { Email = verificationRequest.Email, Code = code });
            }

            await context.SaveChangesAsync();
            return true;

        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR : SaveVerificationRequest() :: {ex.Message}");
        }

        return false;
    }

    public EmailRequestModel GenerateEmailRequest(VerificationRequestModel verificationRequest, string code)
    {
        try
        {
            if (!string.IsNullOrEmpty(verificationRequest.Email) && !string.IsNullOrEmpty(code))
            {
                var emailRequest = new EmailRequestModel()
                {
                    To = verificationRequest.Email,
                    Subject = $"Verification Code {code}",
                    HtmlBody = $@"
                    <html lang=""en"">
                    <head>
                        <meta charset=""UTF-8"">
                        <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                        <title>Silicon - Verification Code</title>
                        <style>
                            body {{
                                font-family: Arial, sans-serif;
                                margin: 0;
                                padding: 0;
                                background-color: #f4f4f4;
                            }}
                            .container {{
                                max-width: 600px;
                                margin: 20px auto;
                                padding: 20px;
                                background-color: #dfe3eb; /* Mjuk blå färg */
                                border-radius: 5px;
                                box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);
                                text-align: center; /* Centrerar innehållet */
                            }}
                            h2 {{
                                color:white;
                                background-color: blueviolet; /* Mjuk grå färg */
                                padding: 30px;
                                border-radius: 5px;
                            }}
                            p {{
                                color: #666;
                                font-size: 14px; /* Mindre textstorlek för varningen */
                            }}
                            .code {{
                                font-size: 42px;
                                font-weight: bold;
                                color: black;
                                margin-bottom: 20px;
                                padding: 30px;
                            }}
                            .warning {{
                                color: #999;
                                font-style: italic;
                                margin-top: 10px; /* Mindre avstånd till kodtexten */
                            }}
                        </style>
                    </head>
                    <body>
                        <div class=""container"">
                            <h2>Verification Code</h2>
                            <p>Dear User,</p>
                            <p>Here is your verification code:</p>
                            <h1 class=""code"">{code}</h1> <!-- Använder en h2 för kodtexten -->
                            <p>Please use this code to verify your email address.</p>
                            <p class=""warning"">For your security, if you did not request this code, someone may be attempting to access your Silicon account.</p>
                        </div>
                    </body>
                </html>
                ",
                PlainText = $@"Please use this code to verify your email address: {code}. For your security, if you did not request this code, someone may be attempting to access your Silicon account."
                };

                return emailRequest;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR : GenerateEmailRequest :: {ex.Message}");
        }

        return null!;
    }

    public string GenerateServiceBusEmailRequest(EmailRequestModel emailRequest)
    {
        try
        {
            var payload = JsonConvert.SerializeObject(emailRequest);
            if (!string.IsNullOrEmpty(payload))
            {
                return payload;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR : GenerateServiceBusEmailRequest.Run() :: {ex.Message}");
        }

        return null!;
    }

}
