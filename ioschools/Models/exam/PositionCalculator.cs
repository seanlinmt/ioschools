using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ioschools.DB.repository;

namespace ioschools.Models.exam
{
    // key = marks, value = exam_marks id
    public class PositionCalculator
    {
        public SortedDictionary<double, List<long>> table { get; private set; }

        public PositionCalculator()
        {
            table = new SortedDictionary<double, List<long>>();
        }

        private void Add(double mark, long studentid)
        {
            List<long> data;
            if (!table.TryGetValue(mark, out data))
            {
                data = new List<long>();
            }
            data.Add(studentid);
            table[mark] = data;
        }

        public void CalculateAndSaveSubjectPosition(long examid)
        {
            using (var repository = new Repository())
            {
                var exam = repository.GetExam(examid);
                var year = exam.year;

                var marks = exam.exam_marks;
                var classes = exam.exam_classes;
                var subjects = exam.exam_subjects;

                // do it for each class
                foreach (var @class in classes)
                {
                    var studentClass = @class;
                    // then for each subject
                    foreach (var subject in subjects)
                    {
                        var subjectid = subject.id;
                        // go through each student
                        var studentids =
                            marks.SelectMany(
                                x =>
                                x.user.classes_students_allocateds.Where(
                                    y => y.year == year && y.classid == studentClass.classid).Select(y => y.studentid)).Distinct();
                        foreach (var studentid in studentids)
                        {
                            var id = studentid;
                            var mark =
                                marks.Where(x => x.studentid == id && x.exam_subjectid == subjectid).SingleOrDefault();
                            if (mark != null)
                            {
                                // see if we can parse this mark
                                double val;
                                if(double.TryParse(mark.mark, out val))
                                {
                                    Add(val, id);
                                }
                            }
                        }

                        // save position


                        // clear table


                    }
                }

            }




        }
    }
}