using IoTmate.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace IoTmate.Repository
{
    public abstract class BaseRepository
    {
        protected ApplicationDbContext DbContext;

        public BaseRepository(ApplicationDbContext dbContext)
        {
            DbContext = dbContext;
        }
    }
}
