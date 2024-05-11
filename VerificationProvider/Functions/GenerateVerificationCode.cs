using System;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VerificationProvider.Entities;
using VerificationProvider.Models;

namespace VerificationProvider.Functions;

public class GenerateVerificationCode(ILogger<GenerateVerificationCode> logger, IServiceProvider serviceProvider)
{
    private readonly ILogger<GenerateVerificationCode> _logger = logger;
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    [Function(nameof(GenerateVerificationCode))]
    [ServiceBusOutput("email_request", Connection = "ServiceBusConnection")]
    public async Task Run([ServiceBusTrigger("verification_request", Connection = "ServiceBusConnection")] ServiceBusReceivedMessage message, ServiceBusMessageActions messageActions)
    {
        try
        {
            var verificationRequest = UnpackVerificationRequest(message);
            if (verificationRequest != null)
            {
                var code = GenerateCode();
                if (!string.IsNullOrEmpty(code))
                {
                    var emailRequest = GenerateEmailRequest(verificationRequest, code);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR : GenerateVerificationCode.Run() :: {ex.Message}");
        }
    }

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

    private EmailRequestModel GenerateEmailRequest(VerificationRequestModel verificationRequest, string code)
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
                    <html lang='en'>
                        <head>
                            <meta charset='UTF-8'>
                            <meta name='viewport' content='width=device-width, initial-scale=1.0'>
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
                        <div style='max-width: 600px; margin: 20px auto; padding: 20px; background-color: #dfe3eb; border-radius: 5px; box-shadow: 0 0 10px rgba(0, 0, 0, 0.1); text-align: center;'>
                            <h2 style='color: white; background-color: blueviolet; padding: 30px; border-radius: 5px;'>Verification Code</h1>
                            <p>Dear User,</p>
                            <p>Here is your verification code:</p>
                            <h1 style='font-size: 42px; font-weight: bold; color: black; margin-bottom: 20px; padding: 30px;'>{code}</h1> <!-- Använder en h2 för kodtexten -->
                            <p>Please use this code to verify your email address.</p>
                            <p style='color: #999; font-style: italic; margin-top: 10px;'>For your security, if you did not request this code, someone may be attempting to access your Silicon account.</p>
                        </div>
                        </body>
                    </html>
                    ",
                    
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR : GenerateEmailRequest :: {ex.Message}");
        }

        return null!;
    }

}


