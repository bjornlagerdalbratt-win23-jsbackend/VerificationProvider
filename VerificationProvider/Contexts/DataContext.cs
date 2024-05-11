using Microsoft.EntityFrameworkCore;
using VerificationProvider.Entities;

namespace VerificationProvider.Contexts;

public class DataContext(DbContextOptions<DataContext> options) : DbContext(options)
{
    public DbSet<VerificationRequest> VerificationRequests { get; set; }
}
