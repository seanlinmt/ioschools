using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Caching;
using clearpixels.Logging;

namespace ioschools.DB.caching
{
    /// <summary>
    ///  requirements on supported queries http://msdn.microsoft.com/en-us/library/ms181122.aspx
    ///  debugging help? http://rusanu.com/2006/06/17/the-mysterious-notification/
    /// </summary>
    public static class CacheHelper
    {
        public static string CreateKey(object[] keys)
        {
            return string.Concat(String.Join(":", keys));
        }
        /// <summary>
        /// Caches Linq query´s that is created for LinqToSql.
        /// Limitations are the same as SqlCacheDependency
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="q">The linq query</param>
        /// <param name="dc">Your LinqToSql DataContext</param>
        /// <param name="CacheId">The unique Id for the cache</param>
        /// <returns></returns>
        public static List<T> LinqCache<T>(this IQueryable<T> q, DataContext dc, string CacheId)
        {
            try
            {
                List<T> objCache = (List<T>)HttpRuntime.Cache.Get(CacheId);

                if (objCache == null)
                {
                    /////////No cache... implement new SqlCacheDependeny//////////
                    //1. Get connstring from DataContext
                    var connStr = Properties.Settings.Default.ioschoolsConnectionString;
                    //2. Get SqlCommand from DataContext and the LinqQuery
                    string sqlCmd = dc.GetCommand(q).CommandText;
                    //3. Create Conn to use in SqlCacheDependency
                    using (SqlConnection conn = new SqlConnection(connStr))
                    {
                        conn.Open();
                        //4. Create Command to use in SqlCacheDependency
                        using (SqlCommand cmd = new SqlCommand(sqlCmd, conn))
                        {
                            //5.0 Add all parameters provided by the Linq Query
                            foreach (System.Data.Common.DbParameter dbp in dc.GetCommand(q).Parameters)
                            {
                                cmd.Parameters.Add(new SqlParameter(dbp.ParameterName, dbp.Value));
                            }
                            //5.1 Enable DB for Notifications... Only needed once per DB...
                            SqlCacheDependencyAdmin.EnableNotifications(connStr);
                            //5.2 Get ElementType for the query
                            var attrib = (TableAttribute)Attribute.GetCustomAttribute(q.ElementType, typeof(TableAttribute));
                            string notificationTable = attrib.Name;

                            //5.3 Enable the elementtype for notification (if not done!)
                            if (!SqlCacheDependencyAdmin.GetTablesEnabledForNotifications(connStr).Contains(notificationTable))
                            {
                                SqlCacheDependencyAdmin.EnableTableForNotifications(connStr, notificationTable);
                            }

                            //6. Create SqlCacheDependency
                            SqlCacheDependency sqldep = new SqlCacheDependency(cmd);
                            // - removed 090506 - 7. Refresh the LinqQuery from DB so that we will not use the current Linq cache
                            // - removed 090506 - dc.Refresh(System.Data.Linq.RefreshMode.OverwriteCurrentValues, q);
                            //8. Execute SqlCacheDepency query...
                            cmd.ExecuteNonQuery();
                            //9. Execute LINQ-query to have something to cache...
                            objCache = q.ToList();
                            //10. Cache the result but use the already created objectCache. Or else the Linq-query will be executed once more...
                            HttpRuntime.Cache.Insert(CacheId, objCache, sqldep);
                        }
                    }
                }
                //Return the created (or cached) List
                return objCache;
            }
            catch (Exception ex)
            {
                Syslog.Write(ex);
                throw;
            }
        }
    }
}
