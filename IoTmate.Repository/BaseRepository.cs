using IoTmate.Model;

namespace IoTmate.Repository
{
    public abstract class BaseRepository(ApplicationDbContext dbContext)
    {
        protected readonly ApplicationDbContext DbContext = dbContext;
    }
}
