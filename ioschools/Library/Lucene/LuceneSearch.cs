using System.Collections.Generic;
using System.Linq;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Util;

namespace ioschools.Library.Lucene
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
            var hits1 = searcher.Search(query, Data.Constants.LUCENE_SEARCH_RESULTS_MAX);

            query = new FuzzyQuery(term2, 0.5f);
            var hits2 = searcher.Search(query, Data.Constants.LUCENE_SEARCH_RESULTS_MAX);

            query = new FuzzyQuery(term3, 0.7f);
            var hits3 = searcher.Search(query, Data.Constants.LUCENE_SEARCH_RESULTS_MAX);

            query = new FuzzyQuery(term4, 0.7f);
            var hits4 = searcher.Search(query, Data.Constants.LUCENE_SEARCH_RESULTS_MAX);

            var analyzer = new StandardAnalyzer(Version.LUCENE_29);
            var parser = new QueryParser(Version.LUCENE_29, "email", analyzer);
            var query_email = parser.Parse(searchterm);
            var hits_email = searcher.Search(query_email, Data.Constants.LUCENE_SEARCH_RESULTS_MAX);

            var ids = new HashSet<LuceneHit>();
            for (int i = 0; i < hits1.TotalHits; i++)
            {
                var scoreDoc = hits1.ScoreDocs[i];
                ids.Add(new LuceneHit() { docId = scoreDoc.Doc, score = scoreDoc.Score});
            }

            for (int i = 0; i < hits2.TotalHits; i++)
            {
                var scoreDoc = hits2.ScoreDocs[i];
                ids.Add(new LuceneHit(){ docId = scoreDoc.Doc, score = scoreDoc.Score });
            }
            
            for (int i = 0; i < hits3.TotalHits; i++)
            {
                var scoreDoc = hits3.ScoreDocs[i];
                ids.Add(new LuceneHit() { docId = scoreDoc.Doc, score = scoreDoc.Score });
            }
            
            for (int i = 0; i < hits4.TotalHits; i++)
            {
                var scoreDoc = hits4.ScoreDocs[i];
                ids.Add(new LuceneHit() { docId = scoreDoc.Doc, score = scoreDoc.Score });
            }

            for (int i = 0; i < hits_email.TotalHits; i++)
            {
                var scoreDoc = hits_email.ScoreDocs[i];
                ids.Add(new LuceneHit() {docId = scoreDoc.Doc, score = scoreDoc.Score});
            }

            searcher.Dispose();
            return ids;
        }
    }
}