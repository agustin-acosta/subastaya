using Application.Interfaces;

namespace Infrastructure;

public class UnitOfWork : IUnitOfWork
{
    private readonly SubastaYaDbContext _context;

    public UnitOfWork(SubastaYaDbContext context)
    {
        _context = context;
    }

    public Task<int> SaveChangesAsync() => _context.SaveChangesAsync();
}