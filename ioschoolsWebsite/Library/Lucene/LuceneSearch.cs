using System.Collections.Generic;
using System.Linq;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Util;

namespace ioschoolsWebsite.Library.Lucene
{
    public class LuceneSearch
    {
        public IEnumerable<LuceneHit> UserSearch(string searchterm)
        {
            var searcher = new IndexSearcher(LuceneUtil.GetDirectoryInfo(), true);
            
            var term1 = new Term("name", searchterm);
            var term2 = new Term("fullname", searchterm);
            var term3 = new Term("2names", searchterm);
            var term4 = new Term("3names", searchterm);
            
            var query = new FuzzyQuery(term1, 0.6f);
            var hits1 = searcher.Search(query);

            query = new FuzzyQuery(term2, 0.5f);
            var hits2 = searcher.Search(query);

            query = new FuzzyQuery(term3, 0.7f);
            var hits3 = searcher.Search(query);

            query = new FuzzyQuery(term4, 0.7f);
            var hits4 = searcher.Search(query);

            var analyzer = new StandardAnalyzer(Version.LUCENE_29);
            var parser = new QueryParser(Version.LUCENE_29, "email", analyzer);
            var query_email = parser.Parse(searchterm);
            var hits_email = searcher.Search(query_email);

            var ids = new HashSet<LuceneHit>();
            for (int i = 0; i < hits1.Length(); i++)
            {
                Document doc = hits1.Doc(i);
                ids.Add(new LuceneHit() { id = doc.Get("id"), score = hits1.Score(i)});
            }

            for (int i = 0; i < hits2.Length(); i++)
            {
                Document doc = hits2.Doc(i);
                ids.Add(new LuceneHit(){ id = doc.Get("id"), score = hits2.Score(i)});
            }
            
            for (int i = 0; i < hits3.Length(); i++)
            {
                Document doc = hits3.Doc(i);
                ids.Add(new LuceneHit() { id = doc.Get("id"), score = hits3.Score(i) });
            }
            
            for (int i = 0; i < hits4.Length(); i++)
            {
                Document doc = hits4.Doc(i);
                ids.Add(new LuceneHit() { id = doc.Get("id"), score = hits4.Score(i) });
            }

            for (int i = 0; i < hits_email.Length(); i++)
            {
                Document doc = hits_email.Doc(i);
                ids.Add(new LuceneHit() {id = doc.Get("id"), score = hits_email.Score(i)});
            }

            searcher.Close();
            return ids;
        }
    }
}