using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ioschools.DB.repository
{
    public partial class Repository
    {
        public IQueryable<mail> GetMails()
        {
            return db.mails;
        }

        public void DeleteMail(mail entry)
        {
            db.mails.DeleteOnSubmit(entry);
        }
    }
}
