using System.Collections.Generic;

namespace ioschools.Models.subject.JSON
{
    public class SubjectTeacherJSON
    {
        public long id { get; set; }
        public IEnumerable<int> classes { get; set; }
    }
}