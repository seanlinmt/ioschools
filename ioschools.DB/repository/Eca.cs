using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ioschools.Data;

namespace ioschools.DB.repository
{
    public partial class Repository
    {
        public void AddEca(eca eca)
        {
            db.ecas.InsertOnSubmit(eca);
        }

        public void DeleteEca(long id)
        {
            var exist = GetEca(id);
            if (exist != null)
            {
                var studentecas = db.eca_students.Where(x => x.ecaid == exist.id);
                db.eca_students.DeleteAllOnSubmit(studentecas);
                db.ecas.DeleteOnSubmit(exist);
                Save();
            }
        }

        public void DeleteStudentEca(long id)
        {
            var exist = db.eca_students.SingleOrDefault(x => x.id == id);
            if (exist != null)
            {
                var attendances = db.attendances.Where(x => x.ecaid.HasValue && x.ecaid.Value == id);
                db.attendances.DeleteAllOnSubmit(attendances);
                db.eca_students.DeleteOnSubmit(exist);
                Save();
            }
        }

        public eca GetEca(long id)
        {
            return db.ecas.SingleOrDefault(x => x.id == id);
        }

        public eca_student GetStudentEca(long id)
        {
            return db.eca_students.SingleOrDefault(x => x.id == id);
        }

        public IEnumerable<eca> GetViewableEcas(Schools? userschool)
        {
            if (!userschool.HasValue)
            {
                return db.ecas;
            }
            switch (userschool)
            {
                case Schools.International:
                    return db.ecas.Where(x => x.schoolid == (int) Schools.International);
                case Schools.Kindergarten:
                case Schools.Primary:
                case Schools.Secondary:
                    return db.ecas.Where(x => x.schoolid != (int)Schools.International);
                default:
                    throw new NotImplementedException();
            }
        }

        public IEnumerable<eca> GetEcas(int? schoolid)
        {
            if (!schoolid.HasValue)
            {
                return db.ecas;
            }
            return db.ecas.Where(x => x.schoolid == schoolid);
        }
    }
}
