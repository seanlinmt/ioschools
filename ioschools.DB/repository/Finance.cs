using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ioschools.Data;

namespace ioschools.DB.repository
{
    public partial class Repository
    {
        public IQueryable<fee> GetFees()
        {
            return db.fees;
        }

    }
}
