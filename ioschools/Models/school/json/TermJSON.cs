namespace ioschools.Models.school.json
{
    public class TermJSON
    {
        public short termid { get; set; }
        public int startday { get; set; }
        public int startmonth { get; set; }
        public int endday { get; set; }
        public int endmonth { get; set; }
        public int? entryid { get; set; }
        public int total { get; set; }
    }
}