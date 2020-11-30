using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ioschools.Data;
using ioschools.Data.User;
using clearpixels.Logging;
using ioschools.DB.repository;

namespace ioschools.Models.stats
{
    public class ClassStatistics 
    {
        public UserGroup usergroup { get; set; }
        public Schools school { get; set; }
        public int msian_male { get; set; }
        public int msian_female { get; set; }
        public int foreign_male { get; set; }
        public int foreign_female { get; set; }
        public int year { get; set; }
        public List<RaceCollection> collections { get; set; }

        public ClassStatistics()
        {
            collections = new List<RaceCollection>();
        }


        public void CalculateStats(IRepository repository)
        {
            var schoolclasses = repository.GetSchoolClasses().Where(x => x.schoolid == (int)school).OrderBy(x => x.name);
            foreach (var schoolClass in schoolclasses)
            {
                var students = repository.GetUsers(null,null,(int)school, schoolClass.id, usergroup, null, null, null, year, null);
                var racecollection = new RaceCollection { name = schoolClass.name };
                foreach (var student in students)
                {
                    var r = new Race();
                    r.name = student.race ?? "";
                    if (student.gender == Gender.MALE.ToString())
                    {
                        r.male = 1;
                    }
                    else if (student.gender == Gender.FEMALE.ToString())
                    {
                        r.female = 1;
                    }
                    else
                    {
                        Syslog.Write(ErrorLevel.WARNING, "Stats: Unspecified gender: " + student.name);
                        continue;
                    }
                    racecollection.Add(r);

                    // increment citizenship
                    var citizenship = (student.citizenship ?? "").ToLower();
                    if (citizenship.Contains("malay") || citizenship.Contains("m'sian"))
                    {
                        if (r.male == 1)
                        {
                            msian_male += 1;
                        }
                        else
                        {
                            msian_female += 1;
                        }
                    }
                    else
                    {
                        if (r.male == 1)
                        {
                            foreign_male += 1;
                        }
                        else
                        {
                            foreign_female += 1;
                        }
                    }

                }
                collections.Add(racecollection);
            }
        }
    }
}