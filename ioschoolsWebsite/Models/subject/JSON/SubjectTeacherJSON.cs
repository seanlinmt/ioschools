using System.Collections.Generic;

namespace ioschoolsWebsite.Models.subject.JSON
{
    public class SubjectTeacherJSON
    {
        public long id { get; set; }
        public IEnumerable<int> classes { get; set; }
    }
}