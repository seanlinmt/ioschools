using System;

namespace ioschools.Data
{
    public enum SorterType
    {
        GREATERTHAN,
        LESSTHAN
    }

    public class Sorter
    {
        public int value { get; set; }
        public SorterType type { get; set; }

        public Sorter(string @string)
        {
            if (@string.Substring(0,1) == ">")
            {
                type = SorterType.GREATERTHAN;
            }
            else if (@string.Substring(0, 1) == "<")
            {
                type = SorterType.LESSTHAN;
            }
            else
            {
                throw new NotImplementedException();
            }

            value = int.Parse(@string.Substring(1));
        }
    }
}