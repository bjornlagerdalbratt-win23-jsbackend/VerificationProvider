using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using VerificationProvider.Services;

namespace VerificationProvider.Functions;

public class VerificationCleaner(ILogger<VerificationCleaner> logger, ICleanerService cleanerService)
{
    private readonly ILogger<VerificationCleaner> _logger = logger;
    private readonly ICleanerService _cleanerService = cleanerService;

    [Function("VerificationCleaner")]
    public async Task Run([TimerTrigger("0 */1 * * * *")] TimerInfo myTimer)
    {
        try
        {
            await _cleanerService.RemoveExpiredRecordsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR : CleanerService.TimerTrigger() :: {ex.Message}");
        }
    }
}
