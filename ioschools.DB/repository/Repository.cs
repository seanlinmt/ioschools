using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Text;
using clearpixels.Logging;

namespace ioschools.DB.repository
{
    public partial class Repository : IRepository, IDisposable
    {
        private readonly ioschoolsDBDataContext db;

        public Repository()
        {
            db = new ioschoolsDBDataContext();
        }

        public Repository(ioschoolsDBDataContext db)
        {
            this.db = db;
        }

        public void Save()
        {
            try
            {
                db.SubmitChanges();
            }
            catch (ChangeConflictException cce)
            {
                var sb = new StringBuilder();
                sb.AppendLine(cce.Message);

                foreach (ObjectChangeConflict occ in db.ChangeConflicts)
                {
                    var metatable = db.Mapping.GetTable(occ.Object.GetType());
                    sb.AppendFormat("\nTable name: {0}\n", metatable.TableName);
                    foreach (MemberChangeConflict mcc in occ.MemberConflicts)
                    {
                        sb.AppendFormat("Member: {0}", mcc.Member);
                        sb.AppendFormat("\tCurrent  value: {0}", mcc.CurrentValue);
                        sb.AppendFormat("\tOriginal value: {0}", mcc.OriginalValue);
                        sb.AppendFormat("\tDatabase value: {0}", mcc.DatabaseValue);
                    }
                }
                Syslog.Write(ErrorLevel.CRITICAL, sb.ToString());
            }
            catch (Exception ex)
            {
                Syslog.Write(ex);
                throw;
            }
        }

        public IEnumerable<int> GetOperationalYears()
        {
            // gets allocated years from student and teacher tables
            var studentyears = db.classes_students_allocateds.Select(x => x.year).Distinct().ToArray();
            var teacheryears = db.classes_teachers_allocateds.Select(x => x.year).Distinct().ToArray();
            var years = new HashSet<int>(studentyears);
            years.UnionWith(teacheryears);
            return years;
        }

        public void Dispose()
        {
            if (db != null)
            {
                db.Dispose();
            }
        }
    }
}
