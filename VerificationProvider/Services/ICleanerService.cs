
namespace VerificationProvider.Services
{
    public interface ICleanerService
    {
        Task RemoveExpiredRecordsAsync();
    }
}