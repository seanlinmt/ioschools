using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ioschools.Data;

namespace ioschools.DB.repository
{
    public partial class Repository
    {
        public IQueryable<school> GetSchools()
        {
            return db.schools;
        }

        public IQueryable<school> GetSchools(Schools? myschool)
        {
            if (!myschool.HasValue)
            {
                return db.schools;
            }
            switch (myschool.Value)
            {
                case Schools.International:
                    return db.schools.Where(x => x.id == (int) Schools.International);
                case Schools.Primary:
                case Schools.Secondary:
                case Schools.Kindergarten:
                    return db.schools.Where(x => x.id != (int)Schools.International);
                default:
                    throw new NotImplementedException();
            }
        }

        public IQueryable<school_year> GetSchoolYears()
        {
            return db.school_years;
        }

        public IEnumerable<school_term> GetSchoolTerms()
        {
            return db.school_terms;
        }
    }
}
