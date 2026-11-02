
using Microsoft.EntityFrameworkCore;

public class BibliothequeDbContext : DbContext
{
    public BibliothequeDbContext(
        DbContextOptions<BibliothequeDbContext> options)
        : base(options)
    {
    }
}
