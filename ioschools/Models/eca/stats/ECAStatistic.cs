using System.Collections.Generic;
using System.Linq;
using ioschools.Data;
using ioschools.DB.repository;

namespace ioschools.Models.eca.stats
{
    public class ECAStatistic
    {
        public Schools school { get; set; }
        public int year { get; set; }
        public List<Entry> entries { get; set; }

        public ECAStatistic()
        {
            entries = new List<Entry>();
        }

        public void CalculateStats(IRepository repository)
        {
            var ecas = repository.GetEcas((int) school);
            foreach (var eca in ecas)
            {
                var entry = new Entry();
                var usrs = eca.eca_students.Where(x => x.year == year).Select(x => x.user);
                entry.name = eca.name;
                entry.female = usrs.Where(x => x.gender == Gender.FEMALE.ToString()).Count();
                entry.male = usrs.Where(x => x.gender == Gender.MALE.ToString()).Count();
                entries.Add(entry);
            }
        }
    }
}