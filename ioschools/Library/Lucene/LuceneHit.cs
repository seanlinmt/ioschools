using System;
namespace ioschools.Library.Lucene
{
    public class LuceneHit
    {
        public long docId { get; set; }
        public float score { get; set; }

        public override int GetHashCode()
        {
            return docId.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var other = obj as LuceneHit;
            if (other == null)
            {
                return false;
            }
            return docId.Equals(other.docId);
        }
    }
}