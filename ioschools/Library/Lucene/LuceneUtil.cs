using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using clearpixels.Logging;
using ioschools.DB;
using ioschools.DB.repository;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Store;
using Version = Lucene.Net.Util.Version;

namespace ioschools.Library.Lucene
{
    public class LuceneUtil
    {
        const string LUCENE_INDEX_LOCATION = "LuceneIndex";

        public static void ThreadProcBuild()
        {
            var myLock = new object();
            lock (myLock)
            {
                try
                {
                    var writer = CreateWriter(true);
                    using (var repository = new Repository())
                    {
                        var contacts = repository.GetUsers();
                        foreach (var contact in contacts)
                        {
                            try
                            {
                                var doc = IndexContact(contact);
                                writer.AddDocument(doc);
                            }
                            catch (Exception ex)
                            {
                                Syslog.Write(ex);
                            }

                        }
                    }
                    writer.Optimize();
                    writer.Dispose();
                }
                catch (Exception ex)
                {
                    Syslog.Write(ex);
                }
                finally
                {
                    Syslog.Write(ErrorLevel.INFORMATION, "Lucene Indexing Complete");
                }
            }
        }
        public static void DeleteLuceneIndex(long id)
        {
            ModifyLuceneIndex(null, id, true);
        }
        public static void UpdateLuceneIndex(user data)
        {
            try
            {
                ModifyLuceneIndex(data, null, false);
            }
            catch (Exception ex)
            {
                Syslog.Write(ex);
            }
        }

        private static void ModifyLuceneIndex(user usr, long? id, bool deleteOnly)
        {
            var writer = CreateWriter(false);
            Term term;
            string termID;
            user contact = null;
            if (id.HasValue)
            {
                termID = id.Value.ToString();
            }
            else
            {
                contact = usr;
                termID = contact.id.ToString();
            }
            term = new Term("id", termID);
            writer.DeleteDocuments(term);
            if (!deleteOnly)
            {
                var doc = IndexContact(contact);
                writer.AddDocument(doc);
            }
            writer.Optimize();
            writer.Dispose();
        }

        private static IndexWriter CreateWriter(bool create)
        {
            var analyzer = new StandardAnalyzer(Version.LUCENE_29);
            var fsdir = GetDirectoryInfo();
            var writer = new IndexWriter(fsdir, analyzer, create, new IndexWriter.MaxFieldLength(5000));
            return writer;
        }

        public static FSDirectory GetDirectoryInfo()
        {
            DirectoryInfo dir;
            // add contacts index
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, LUCENE_INDEX_LOCATION);
            if (!System.IO.Directory.Exists(path))
            {
                dir = System.IO.Directory.CreateDirectory(path);
            }
            else
            {
                dir = new DirectoryInfo(path);
            }
            return FSDirectory.Open(dir);
        }

        private static Document IndexContact(user u)
        {
            var doc = new Document();

            doc.Add(new Field("id", u.id.ToString(), Field.Store.YES, Field.Index.NO));

            var namefield = new Field("name", u.name.ToLowerInvariant(), Field.Store.YES, Field.Index.ANALYZED);
            doc.Add(namefield);

            var namesegments = u.name.ToLowerInvariant().Split(new[] {' '});

            var shortname = string.Join(" ", namesegments.Take(2).ToArray());
            var shortnamefield = new Field("2names", shortname, Field.Store.YES, Field.Index.NOT_ANALYZED);
            doc.Add(shortnamefield);

            shortname = string.Join(" ", namesegments.Take(3).ToArray());
            var fullname_analysed_field = new Field("3names", shortname, Field.Store.YES, Field.Index.NOT_ANALYZED);
            doc.Add(fullname_analysed_field);

            var fullnamefield = new Field("fullname", u.name.ToLowerInvariant(), Field.Store.YES, Field.Index.NOT_ANALYZED);
            doc.Add(fullnamefield);

            // index email
            if (!string.IsNullOrEmpty(u.email))
            {
                var emailfield = new Field("email", u.email, Field.Store.YES, Field.Index.NOT_ANALYZED);
                doc.Add(emailfield);
            }

            return doc;
        }
    }
}

