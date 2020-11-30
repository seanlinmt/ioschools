using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ioschools.Models.stats
{
    public class RaceCollection
    {
        public string name { get; set; } // usually class name
        private List<Race> raceList { get; set; }

        public RaceCollection()
        {
            raceList = new List<Race>();
        }

        public void Add(Race race)
        {
            // check if race already exist
            var entry = raceList.Where(x => x.name.ToLower() == race.name.ToLower()).SingleOrDefault();

            if (entry == null)
            {
                raceList.Add(race);
            }
            else
            {
                entry.male += race.male;
                entry.female += race.female;
                entry.unknown += race.unknown;
            }
        }

        public IEnumerable<Race> GetList()
        {
            return raceList;
        }
    }
}