using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ioschools.Data;

namespace ioschools.Models.homework
{
    public class HomeworkStudent
    {
        public string created { get; set; }
        public IEnumerable<IdNameUrl> files { get; set; }
        public IEnumerable<IdName> classes { get; set; }
        public string message { get; set; }
        public string title { get; set; }
        public string id { get; set; }
        public IEnumerable<HomeworkAnswerFile> answers { get; set; }
        public bool canUploadAnswer { get; set; }
        public string subjectname { get; set; }
    }

    public static class HomeworkStudentHelper
    {
        public static HomeworkStudent ToModel(this ioschools.DB.homework_student row, bool canUpload)
        {
            return new HomeworkStudent()
                       {
                           created = row.homework.created.ToString(Constants.DATETIME_STANDARD),
                           id = row.id.ToString(),
                           classes = row.homework.homework_students.GroupBy(x => x.school_class).Select(
                               x => new IdName(x.Key.id, x.Key.name)),
                           files =
                               row.homework.homework_files.Select(
                                   x => new IdNameUrl() {id = x.id.ToString(), name = x.filename, url = x.url}),
                           message = row.homework.message,
                           title = row.homework.title,
                           answers = row.homework_answers.Select(x => new HomeworkAnswerFile()
                                                                          {
                                                                              url = x.url,
                                                                              id = x.id.ToString(),
                                                                              name = x.filename
                                                                          }),
                           subjectname = row.homework.subject.name,
                           canUploadAnswer = canUpload
                       };
        }

        public static IEnumerable<HomeworkStudent> ToModel(this IEnumerable<ioschools.DB.homework_student> rows, bool canUpload)
        {
            foreach (var row in rows)
            {
                yield return row.ToModel(canUpload);
            }
        }
    }
}