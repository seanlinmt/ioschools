using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ioschools.Data;

namespace ioschools.DB.repository
{
    public partial class Repository
    {
        public void AddRegistration(registration registration)
        {
            db.registrations.InsertOnSubmit(registration);
        }

        public void DeleteRegistrationNotificationByUser(long userid)
        {
            var exist = db.registration_notifications.Where(x => x.userid == userid).SingleOrDefault();
            if (exist != null)
            {
                db.registration_notifications.DeleteOnSubmit(exist);
            }
            Save();
        }

        public void DeleteRegistraion(registration registration)
        {
            db.registrations.DeleteOnSubmit(registration);
            Save();
        }

        public registration GetRegistration(long id)
        {
            return db.registrations.Where(x => x.id == id).SingleOrDefault();
        }

        public IQueryable<registration> GetRegistration(int? school, RegistrationStatus? status, int? year, int? classyear)
        {
            var registrations = db.registrations.AsQueryable();
            if (school.HasValue)
            {
                registrations = registrations.Where(x => x.schoolid == school.Value);
            }

            if (status.HasValue)
            {
                registrations = registrations.Where(x => x.status == status.ToString());
            }

            if (year.HasValue)
            {
                registrations = registrations.Where(x => x.enrollingYear.HasValue && x.enrollingYear.Value == year.Value);
            }

            if (classyear.HasValue)
            {
                registrations = registrations.Where(x => x.schoolyearid.HasValue && x.schoolyearid.Value == classyear);
            }

            return registrations;
        }

        public IEnumerable<registration_notification> GetRegistrationNotifications()
        {
            return db.registration_notifications;
        }
    }
}
