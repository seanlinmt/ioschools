namespace ioschools.Models.fees.statistics
{
    public class StatRow
    {
        public string schoolyear { get; set; }
        public int paid { get; set; }
        public int unpaid { get; set; }
        public int overdue { get; set; }
    }
}