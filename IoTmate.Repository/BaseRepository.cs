using IoTmate.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace IoTmate.Repository
{
    public abstract class BaseRepository(ApplicationDbContext dbContext)
    {
        protected ApplicationDbContext DbContext = dbContext;
    }
}
