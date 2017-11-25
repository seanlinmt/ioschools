namespace ioschools.Data
{
    public class Pair<K, V> 
    {
		public Pair()
		{
		}
 
		public Pair(K first, V second)
		{
			this.first = first;
			this.second = second;
		}
 
		private K first;
		public K First
		{
			get { return first; }
			set { first = value; }
		}
 
		private V second;
		public V Second
		{
			get { return second; }
			set { second = value; }
		}
 
		public override bool Equals(object obj)
		{
			if (this == obj) return true;
			var pair = obj as Pair<K, V>;
			if (pair == null) return false;
			return Equals(first, pair.first) && Equals(second, pair.second);
		}
 
		public override int GetHashCode()
		{
			return (first != null ? first.GetHashCode() : 0) + 29*(second != null ? second.GetHashCode() : 0);
		}

    }
}
