using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Text;

namespace ioschools.DB
{
    public static class EntityLogging
    {
        /// <summary>
        /// Audits Changes to a Specific Entity
        /// </summary>
        /// <typeparam name="T">The Data Entity Type</typeparam>
        /// <param name="dataContext">The Data Context</param>
        /// <param name="modifiedEntity">The Entity To Audit</param>
        /// <param name="target"></param>
        /// <param name="userid"></param>
        public static void LogChanges<T>(ioschoolsDBDataContext dataContext, T modifiedEntity, string target, long userid) where T : class
        {
            if (dataContext == null || modifiedEntity == null)
                return;

            var sb = new StringBuilder();
            sb.AppendFormat("{0}: ", target);
            foreach (ModifiedMemberInfo modifiedProperty in dataContext.GetTable<T>().GetModifiedMembers(modifiedEntity))
            {
                //log changes
                // field[original:new] field[original:new]
                sb.AppendFormat("{0}[{1} -> {2}] ", modifiedProperty.Member.Name, modifiedProperty.OriginalValue,
                                modifiedProperty.CurrentValue);
            }

            var change = new changelog
                             {
                                 changes = sb.ToString(), 
                                 created = DateTime.Now, 
                                 userid = userid
                             };

            dataContext.changelogs.InsertOnSubmit(change);
            dataContext.SubmitChanges();
        }
    }
}
