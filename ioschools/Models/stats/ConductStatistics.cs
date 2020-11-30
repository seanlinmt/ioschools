using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ioschools.Data;
using ioschools.Library.Helpers;
using ioschools.Models.discipline;
using ioschools.DB.repository;

namespace ioschools.Models.stats
{
    public class ConductStatistics
    {
        public string school { get; set; }
        public string from { get; set; }
        public string to { get; set; }
        public List<ConductStatistic> merits { get; set; }
        public List<ConductStatistic> demerits { get; set; }

        public ConductStatistics()
        {
            merits = new List<ConductStatistic>();
            demerits = new List<ConductStatistic>();
        }

        public void PopulateStats(DateTime start, DateTime end, Schools school)
        {
            ioschools.DB.students_discipline[] rows;
            using (var repository = new Repository())
            {
                rows = repository.GetDisciplines()
                                .Where(x => x.created >= start &&
                                            x.created <= end &&
                                            x.user.classes_students_allocateds
                                            .Count(y => y.school_class.schoolid == (int)school && y.year == start.Year) != 0).ToArray();
            }

            foreach (DisciplineType entry in Enum.GetValues(typeof(DisciplineType)))
            {
                var enumvalue = (int)entry;
                var stat = new ConductStatistic
                {
                    name = entry.ToDescriptionString(),
                    totalIncidents = rows.Count(x => x.type.HasValue && x.type.Value == enumvalue),
                    totalStudents = rows.Where(x => x.type.HasValue && x.type.Value == enumvalue)
                                        .GroupBy(x => x.studentid).Count()
                };

                if (entry.IsMerit())
                {
                    merits.Add(stat);
                }
                else
                {
                    demerits.Add(stat);
                }
            }
        }
    }
}