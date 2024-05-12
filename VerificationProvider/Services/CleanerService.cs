using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VerificationProvider.Contexts;

namespace VerificationProvider.Services;

public class CleanerService(ILogger<CleanerService> logger, DataContext context) : ICleanerService
{
    private readonly ILogger<CleanerService> _logger = logger;
    private readonly DataContext _context = context;

    public async Task RemoveExpiredRecordsAsync()
    {
        try
        {
            var expired = await _context.VerificationRequests.Where(x => x.ExpiryDate <= DateTime.Now).ToListAsync();
            _context.RemoveRange(expired);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR : CleanerService.RemoveExpiredRecordsAsync() :: {ex.Message}");
        }
    }
}
