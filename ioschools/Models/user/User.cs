using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using ioschools.Data;
using ioschools.Data.User;
using ioschools.Models.homework.viewmodels;
using ioschools.Models.leave;
using ioschools.Models.subject.viewmodels;
using ioschools.Library;
using ioschools.Library.Helpers;
using ioschools.Library.Imaging;
using ioschools.Models.attendance;
using ioschools.Models.classes;
using ioschools.Models.discipline;
using ioschools.Models.eca;
using ioschools.Models.enrolment;
using ioschools.Models.photo;
using ioschools.Models.user.staff;
using clearpixels.Models.jqgrid;

namespace ioschools.Models.user
{
    public class User : UserBase
    {
        public string designation { get; set; }

        // lists
        public SelectList designationList { get; set; }
        public SelectList schoolsList { get; set; }
        public IEnumerable<SelectListItem> dayList { get; set; }
        public IEnumerable<SelectListItem> monthList { get; set; }
        public SelectList maritalStatusList { get; set; }

        public string rank { get; set; }
        public string marital_status { get; set; }  
        public bool active { get; set; }
        public long? thumbnailid { get; set; }
        public Photo profilePhoto { get; set; }
        public UserGroup usergroup { get; set; }
        public Schools? school { get; set; }
        public string schoolname { get; set; }
        public Gender gender { get; set; }
        public string race { get; set; }
        public string dialect { get; set; }
        public DateTime? dob { get; set; }
        public string pob { get; set; }
        public string citizenship { get; set; }
        public string birthcertno { get; set; }
        public string passport { get; set; }
        public bool? bumi { get; set; }
        public string religion { get; set; }
        public string nric_new { get; set; }

        public long? nextStudentInClassID { get; set; }
        public bool canGoToNext { get; set; }
        
        // permissions
        public Permission permission { get; set; }
        public bool canModify { get; set; }
        public bool canModifyStaff { get; set; }
        public bool canModifyLeave { get; set; }

        // contact
        public string email { get; set; }
        public string homephone { get; set; }
        public string cellphone { get; set; }
        public string address { get; set; }

        // staff
        public UserStaff staff { get; set; }
        public IEnumerable<StaffLeave> staffLeaves { get; set; } 

        // teachers
        public IEnumerable<AllocatedTeacher> allocatedTeacherClasses { get; set; }
        public List<SubjectsTeaching> subjectsTeaching { get; set; } 

        // parents/guardians
        public string employer { get; set; }
        public string occupation { get; set; }
        public string officephone { get; set; }

        // students
        public IEnumerable<AllocatedStudent> allocatedStudentClasses { get; set; }
        public IEnumerable<UserParent> parents { get; set; }
        public IEnumerable<UserChild> children { get; set; }
        public AttendanceViewModel attendance { get; set; }
        public DisciplineViewModel discipline { get; set; }
        public ECAStudentViewModel eca { get; set; }
        public IEnumerable<Enrolment> enrolments { get; set; }
        public HomeworkStudentViewModel homework { get; set; }

        // etc
        public string notes { get; set; }

        public User()
        {
            allocatedTeacherClasses = Enumerable.Empty<AllocatedTeacher>();
            subjectsTeaching = new List<SubjectsTeaching>();
            allocatedStudentClasses = Enumerable.Empty<AllocatedStudent>();
            parents = Enumerable.Empty<UserParent>();
            children = Enumerable.Empty<UserChild>();
            attendance = new AttendanceViewModel();
            homework = new HomeworkStudentViewModel();
            active = true;
            dayList = DateHelper.GetDayList();
            monthList = DateHelper.GetMonthList();
            enrolments = Enumerable.Empty<Enrolment>();

            staff = new UserStaff();
            staffLeaves = Enumerable.Empty<StaffLeave>();
        }
    }

    public static class UserHelper
    {
        // leave commented lines in (easier to see what has been disabled by default)
        public static Permission GetDefaultPermission(UserGroup usergroup)
        {
            var perms = Permission.NONE;

            switch (usergroup)
            {
                case UserGroup.NONE:
                    break;
                case UserGroup.ADMIN: // done
                    perms = 
                       Permission.ATTENDANCE_CREATE |
                       Permission.ATTENDANCE_NOTIFY |
                       Permission.CONDUCT_ADMIN |
                       Permission.CONDUCT_CREATE |
                       Permission.CONDUCT_NOTIFY |
                       Permission.CALENDAR_ADMIN |
                       Permission.ECA_ADMIN |
                       Permission.ECA_CREATE |
                       Permission.ENROL_CREATE |
                       Permission.ENROL_ADMIN |
                       Permission.EXAM_CREATE |
                       Permission.EXAM_VIEW |
                       Permission.EXAM_ADMIN |
                       Permission.EXAM_EDIT |
                       Permission.FEES_ADMIN |
                       Permission.FEES_UPDATE_STATUS |
                       //Permissions.HOMEWORK_CREATE |
                       Permission.LEAVE_ADMIN |
                       //Permissions.LEAVE_REVIEW |
                       Permission.LEAVE_APPLY |
                       Permission.NEWS_CREATE |
                       Permission.NEWS_ADMIN |
                       Permission.NEWS_BROADCAST |
                       Permission.SETTINGS_ADMIN |
                       Permission.STATS_VIEW |
                       Permission.TRANSCRIPTS_CREATE |
                       Permission.TRANSCRIPTS_EDIT |
                       Permission.USERS_VIEW_STUDENTS |
                       Permission.USERS_VIEW_PARENTS |
                       Permission.USERS_VIEW_STAFF |
                       Permission.USERS_CREATE |
                       Permission.USERS_EDIT_OWN |
                       Permission.USERS_EDIT_STUDENTS |
                       Permission.USERS_EDIT_PARENTS |
                       Permission.USERS_EDIT_STAFF |
                       Permission.WEBSITE_EDIT;
                    break;
                case UserGroup.DIRECTOR: // done
                    perms =
                       Permission.ATTENDANCE_CREATE |
                       Permission.ATTENDANCE_NOTIFY |
                       Permission.CONDUCT_ADMIN |
                       Permission.CONDUCT_CREATE |
                       Permission.CONDUCT_NOTIFY |
                       Permission.CALENDAR_ADMIN |
                       Permission.ECA_ADMIN |
                       Permission.ECA_CREATE |
                       Permission.ENROL_CREATE |
                       Permission.ENROL_ADMIN |
                       Permission.EXAM_CREATE |
                       Permission.EXAM_VIEW |
                       Permission.EXAM_ADMIN |
                       Permission.EXAM_EDIT |
                       Permission.FEES_ADMIN |
                       Permission.FEES_UPDATE_STATUS |
                        //Permissions.HOMEWORK_CREATE |
                       Permission.LEAVE_ADMIN |
                       Permission.LEAVE_REVIEW |
                       Permission.LEAVE_APPLY |
                       Permission.NEWS_CREATE |
                       Permission.NEWS_ADMIN |
                       Permission.NEWS_BROADCAST |
                       Permission.SETTINGS_ADMIN |
                       Permission.STATS_VIEW |
                       Permission.TRANSCRIPTS_CREATE |
                       Permission.TRANSCRIPTS_EDIT |
                       Permission.USERS_VIEW_STUDENTS |
                       Permission.USERS_VIEW_PARENTS |
                       Permission.USERS_VIEW_STAFF |
                       Permission.USERS_CREATE |
                       Permission.USERS_EDIT_OWN |
                       Permission.USERS_EDIT_STUDENTS |
                       Permission.USERS_EDIT_PARENTS |
                       Permission.USERS_EDIT_STAFF |
                       Permission.WEBSITE_EDIT;
                    break;
                case UserGroup.HEAD: // done
                    perms =
                       Permission.ATTENDANCE_CREATE |
                       Permission.ATTENDANCE_NOTIFY |
                       Permission.CONDUCT_ADMIN |
                       Permission.CONDUCT_CREATE |
                       Permission.CONDUCT_NOTIFY |
                       Permission.CALENDAR_ADMIN |
                       Permission.ECA_ADMIN |
                       Permission.ECA_CREATE |
                       Permission.ENROL_CREATE |
                       Permission.ENROL_ADMIN |
                       Permission.EXAM_CREATE |
                       Permission.EXAM_VIEW |
                       Permission.EXAM_ADMIN |
                       Permission.EXAM_EDIT |
                       Permission.FEES_ADMIN |
                       Permission.FEES_UPDATE_STATUS |
                       Permission.HOMEWORK_CREATE |
                       Permission.LEAVE_ADMIN |
                        //Permissions.LEAVE_REVIEW |
                       Permission.LEAVE_APPLY |
                       Permission.NEWS_CREATE |
                       Permission.NEWS_ADMIN |
                       Permission.NEWS_BROADCAST |
                       Permission.SETTINGS_ADMIN |
                       Permission.STATS_VIEW |
                       Permission.TRANSCRIPTS_CREATE |
                       Permission.TRANSCRIPTS_EDIT |
                       Permission.USERS_VIEW_STUDENTS |
                       Permission.USERS_VIEW_PARENTS |
                       Permission.USERS_VIEW_STAFF |
                       Permission.USERS_CREATE |
                       Permission.USERS_EDIT_OWN |
                       Permission.USERS_EDIT_STUDENTS |
                       Permission.USERS_EDIT_PARENTS |
                       Permission.USERS_EDIT_STAFF |
                       Permission.WEBSITE_EDIT;
                    break;
                case UserGroup.TEACHER: // done
                    perms =
                        Permission.ATTENDANCE_CREATE |
                        Permission.ATTENDANCE_NOTIFY |
                        //Permission.CONDUCT_ADMIN |
                        Permission.CONDUCT_CREATE |
                        Permission.CONDUCT_NOTIFY |
                        //Permission.CALENDAR_ADMIN |
                        //Permission.ECA_ADMIN |
                        Permission.ECA_CREATE |
                        //Permission.ENROL_CREATE |
                        //Permission.ENROL_ADMIN |
                        Permission.EXAM_CREATE |
                        Permission.EXAM_VIEW |
                        //Permission.EXAM_ADMIN |
                        Permission.EXAM_EDIT |
                        //Permission.FEES_ADMIN |
                        //Permission.FEES_UPDATE_STATUS |
                        Permission.HOMEWORK_CREATE |
                        //Permission.LEAVE_ADMIN |
                        //Permission.LEAVE_REVIEW |
                        Permission.LEAVE_APPLY |
                        //Permission.NEWS_CREATE |
                        //Permission.NEWS_ADMIN |
                        //Permission.NEWS_BROADCAST |
                        //Permission.SETTINGS_ADMIN |
                        Permission.STATS_VIEW |
                        Permission.TRANSCRIPTS_CREATE |
                        Permission.TRANSCRIPTS_EDIT |
                        Permission.USERS_VIEW_STUDENTS |
                        //Permission.USERS_VIEW_PARENTS |
                        //Permission.USERS_VIEW_STAFF |
                        //Permission.USERS_CREATE |
                        Permission.USERS_EDIT_OWN 
                        //Permission.USERS_EDIT_STUDENTS |
                        //Permission.USERS_EDIT_PARENTS |
                        //Permission.USERS_EDIT_STAFF |
                        //Permission.WEBSITE_EDIT
                        ;
                    break;
                case UserGroup.FINANCE:// done
                    perms =
                        //Permission.ATTENDANCE_CREATE |
                        //Permission.ATTENDANCE_NOTIFY |
                        //Permission.CONDUCT_ADMIN |
                        //Permission.CONDUCT_CREATE |
                        //Permission.CONDUCT_NOTIFY |
                        //Permission.CALENDAR_ADMIN |
                        //Permission.ECA_ADMIN |
                        //Permission.ECA_CREATE |
                        //Permission.ENROL_CREATE |
                        Permission.ENROL_ADMIN |
                        //Permission.EXAM_CREATE |
                        //Permission.EXAM_VIEW |
                        //Permission.EXAM_ADMIN |
                        //Permission.EXAM_EDIT |
                        Permission.FEES_ADMIN |
                        Permission.FEES_UPDATE_STATUS |
                        //Permission.HOMEWORK_CREATE |
                        Permission.LEAVE_ADMIN |
                        //Permission.LEAVE_REVIEW |
                        Permission.LEAVE_APPLY |
                        //Permission.NEWS_CREATE |
                        //Permission.NEWS_ADMIN |
                        //Permission.NEWS_BROADCAST |
                        Permission.SETTINGS_ADMIN |
                        Permission.STATS_VIEW |
                        //Permission.TRANSCRIPTS_CREATE |
                        //Permission.TRANSCRIPTS_EDIT |
                        Permission.USERS_VIEW_STUDENTS |
                        Permission.USERS_VIEW_PARENTS |
                        Permission.USERS_VIEW_STAFF |
                        //Permission.USERS_CREATE |
                        Permission.USERS_EDIT_OWN |
                        //Permission.USERS_EDIT_STUDENTS |
                        //Permission.USERS_EDIT_PARENTS |
                        Permission.USERS_EDIT_STAFF 
                        //Permission.WEBSITE_EDIT
                        ;
                    break;
                case UserGroup.CLERK:// done
                    perms =
                        Permission.ATTENDANCE_CREATE |
                        Permission.ATTENDANCE_NOTIFY |
                        //Permission.CONDUCT_ADMIN |
                        //Permission.CONDUCT_CREATE |
                        //Permission.CONDUCT_NOTIFY |
                        //Permission.CALENDAR_ADMIN |
                        //Permission.ECA_ADMIN |
                        //Permission.ECA_CREATE |
                        //Permission.ENROL_VIEW |
                        //Permission.ENROL_CREATE |
                        //Permission.ENROL_ADMIN |
                        //Permission.EXAM_CREATE |
                        //Permission.EXAM_VIEW |
                        //Permission.EXAM_ADMIN |
                        //Permission.EXAM_EDIT |
                        //Permission.FEES_ADMIN |
                        Permission.FEES_UPDATE_STATUS |
                        //Permission.HOMEWORK_CREATE |
                        //Permission.LEAVE_ADMIN |
                        //Permission.LEAVE_REVIEW |
                        Permission.LEAVE_APPLY |
                        //Permission.NEWS_CREATE |
                        //Permission.NEWS_ADMIN |
                        //Permission.NEWS_BROADCAST |
                        //Permission.SETTINGS_ADMIN |
                        Permission.STATS_VIEW |
                        //Permission.TRANSCRIPTS_CREATE |
                        //Permission.TRANSCRIPTS_EDIT |
                        Permission.USERS_VIEW_STUDENTS |
                        Permission.USERS_VIEW_PARENTS |
                        Permission.USERS_VIEW_STAFF |
                        //Permission.USERS_CREATE |
                        Permission.USERS_EDIT_OWN 
                        //Permission.USERS_EDIT_STUDENTS |
                        //Permission.USERS_EDIT_PARENTS |
                        //Permission.USERS_EDIT_STAFF
                        //Permission.WEBSITE_EDIT
                        ;
                    break;
                case UserGroup.SUPPORT:// done
                    perms =
                        //Permission.ATTENDANCE_CREATE |
                        //Permission.ATTENDANCE_NOTIFY |
                        //Permission.CONDUCT_ADMIN |
                        //Permission.CONDUCT_CREATE |
                        //Permission.CONDUCT_NOTIFY |
                        //Permission.CALENDAR_ADMIN |
                        //Permission.ECA_ADMIN |
                        //Permission.ECA_CREATE |
                        //Permission.ENROL_VIEW |
                        //Permission.ENROL_CREATE |
                        //Permission.ENROL_ADMIN |
                        //Permission.EXAM_CREATE |
                        //Permission.EXAM_VIEW |
                        //Permission.EXAM_ADMIN |
                        //Permission.EXAM_EDIT |
                        //Permission.FEES_ADMIN |
                        //Permission.FEES_UPDATE_STATUS |
                        //Permission.HOMEWORK_CREATE |
                        //Permission.LEAVE_ADMIN |
                        //Permission.LEAVE_REVIEW |
                        Permission.LEAVE_APPLY 
                        //Permission.NEWS_CREATE |
                        //Permission.NEWS_ADMIN |
                        //Permission.NEWS_BROADCAST |
                        //Permission.SETTINGS_ADMIN |
                        //Permission.STATS_VIEW |
                        //Permission.TRANSCRIPTS_CREATE |
                        //Permission.TRANSCRIPTS_EDIT |
                        //Permission.USERS_VIEW_STUDENTS |
                        //Permission.USERS_VIEW_PARENTS |
                        //Permission.USERS_VIEW_STAFF |
                        //Permission.USERS_CREATE |
                        //Permission.USERS_EDIT_OWN
                        //Permission.USERS_EDIT_STUDENTS |
                        //Permission.USERS_EDIT_PARENTS |
                        //Permission.USERS_EDIT_STAFF
                        //Permission.WEBSITE_EDIT
                        ;
                    break;
                case UserGroup.STUDENT:
                case UserGroup.GUARDIAN:
                    break;
                case UserGroup.SUPERUSER: // done
                    perms =
                       Permission.ATTENDANCE_CREATE |
                       Permission.ATTENDANCE_NOTIFY |
                       Permission.CONDUCT_ADMIN |
                       Permission.CONDUCT_CREATE |
                       Permission.CONDUCT_NOTIFY |
                       Permission.CALENDAR_ADMIN |
                       Permission.ECA_ADMIN |
                       Permission.ECA_CREATE |
                       Permission.ENROL_CREATE |
                       Permission.ENROL_ADMIN |
                       Permission.EXAM_CREATE |
                       Permission.EXAM_VIEW |
                       Permission.EXAM_ADMIN |
                       Permission.EXAM_EDIT |
                       Permission.FEES_ADMIN |
                       Permission.FEES_UPDATE_STATUS |
                       Permission.HOMEWORK_CREATE |
                       Permission.LEAVE_ADMIN |
                       Permission.LEAVE_REVIEW |
                       Permission.LEAVE_APPLY |
                       Permission.NEWS_CREATE |
                       Permission.NEWS_ADMIN |
                       Permission.NEWS_BROADCAST |
                       Permission.SETTINGS_ADMIN |
                       Permission.STATS_VIEW |
                       Permission.TRANSCRIPTS_CREATE |
                       Permission.TRANSCRIPTS_EDIT |
                       Permission.USERS_VIEW_STUDENTS |
                       Permission.USERS_VIEW_PARENTS |
                       Permission.USERS_VIEW_STAFF |
                       Permission.USERS_CREATE |
                       Permission.USERS_EDIT_OWN |
                       Permission.USERS_EDIT_STUDENTS |
                       Permission.USERS_EDIT_PARENTS |
                       Permission.USERS_EDIT_STAFF |
                       Permission.WEBSITE_EDIT;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("usergroup");
            }

            return perms;
        }

        public static int SetFlag(this ioschools.DB.user row, UserSettings setting)
        {
            return row.settings | (int) setting;
        }

        public static int UnsetFlag(this ioschools.DB.user row, UserSettings setting)
        {
            return row.settings & ~(int) setting;
        }

        public static JqgridTable ToUsersJqGrid(this IEnumerable<ioschools.DB.user> rows, int year)
        {
            var grid = new JqgridTable();
            foreach (var row in rows)
            {
                var entry = new JqgridRow();
                entry.id = row.id.ToString();
                entry.cell = new object[]
                                 {
                                     row.id,
                                     row.photo.HasValue
                                         ? Img.by_size(row.user_image.url, Imgsize.USER_THUMB).ToHtmlImage()
                                         : Img.PHOTO_NO_THUMBNAIL.ToHtmlImage(),
                                     row.ToJqName(),
                                     row.ToSchoolClass(year),
                                     row.ToContactInfo(),
                                     row.ToStatus(year),
                                     RenderActionLinks(row.id, row.usergroup == (int)UserGroup.STUDENT)
                                 };
                grid.rows.Add(entry);
            }
            return grid;
        }

        private static string RenderActionLinks(long id, bool isStudent)
        {
            var sb = new StringBuilder();
            sb.Append("<ul class='action_links'>");
            sb.AppendFormat("<li><a class='jqedit' href='/users/edit/{0}'>edit</a></li>", id);
            sb.Append("<li class='jqdelete'>delete</li>");
            if (isStudent)
            {
                sb.Append("<li class='jqatt'>attendance</li>");
            }
            sb.Append("</ul>");
            return sb.ToString();
        }

        public static string ToName(this ioschools.DB.user row, bool includeDesignation = true)
        {
            var sb = new StringBuilder();

            // add name
            if (string.IsNullOrEmpty(row.designation) || !includeDesignation)
            {
                sb.Append(row.name);
            }
            else
            {
                sb.AppendFormat("{0} {1}", row.designation, row.name);
            }

            return sb.ToString();
        }

        public static string ToJqName(this ioschools.DB.user row)
        {
            var sb = new StringBuilder();
            sb.AppendFormat("<a href='/users/{0}'>{1}</a><br/>", row.id, row.ToName());

            // add usergroup
            switch ((UserGroup)row.usergroup)
            {
                case UserGroup.ADMIN:
                    sb.Append("<span class='tag_red'>sysadmin</span>");
                    break;
                case UserGroup.CLERK:
                    sb.Append("<span class='tag_brown'>clerk</span>");
                    break;
                case UserGroup.DIRECTOR:
                    sb.Append("<span class='tag_orange'>director</span>");
                    break;
                case UserGroup.FINANCE:
                    sb.Append("<span class='tag_brown'>finance</span>");
                    break;
                case UserGroup.GUARDIAN:
                    sb.Append("<span class='tag_purple'>parent/guardian</span>");
                    break;
                case UserGroup.HEAD:
                    sb.Append("<span class='tag_orange'>headmaster/headmistress</span>");
                    break;
                case UserGroup.STUDENT:
                    sb.Append("<span class='tag_blue'>student</span>");
                    break;
                case UserGroup.TEACHER:
                    sb.Append("<span class='tag_green'>teacher</span>");
                    break;
                case UserGroup.SUPPORT:
                    sb.Append("<span class='tag_grey'>support</span>");
                    break;
                default:
                    throw new NotImplementedException();
            }

            return sb.ToString();
        }

        public static string ToClassName(this ioschools.DB.user row)
        {
            var classs = row.classes_students_allocateds.SingleOrDefault(x => x.year == Utility.GetDBDate().Year);
            if (classs == null)
            {
                return "";
            }
            return classs.school_class.name;
        }

        public static string ToRankStudent(this ioschools.DB.user row)
        {
            ioschools.DB.user parent = null;
            var father = row.students_guardians
                .FirstOrDefault(x => x.type == (byte) GuardianType.FATHER);

            if (father != null)
            {
                parent = father.user1;
            }

            if (parent == null)
            {
                // try mum
                var mother = row.students_guardians
                    .FirstOrDefault(x => x.type == (byte) GuardianType.MOTHER);
                if (mother != null)
                {
                    parent = mother.user1;
                }
            }
            if (parent != null)
            {
                var siblings = parent
                .students_guardians1
                .Where(x => (x.user.settings & (int)UserSettings.INACTIVE) == 0 && x.user.dob.HasValue)
                .Select(x => x.user)
                .OrderBy(y => y.dob.Value);

                var count = 1;
                foreach (var sibling in siblings)
                {
                    if (sibling.id == row.id)
                    {
                        return count.ToString();
                    }
                    count++;
                }
            }

            return "";
        }

        public static string ToReferenceNumber(this ioschools.DB.user row, int year)
        {
            return string.Format("{0}{1}", row.id.ToString("D8"), year);
        }

        public static string ToSchoolClass(this ioschools.DB.user row, int year)
        {
            var sb = new StringBuilder();
            if (row.usergroup == (int)UserGroup.STUDENT)
            {
                var classname =
                    row.classes_students_allocateds.FirstOrDefault(x => x.year == year);
                if (classname != null)
                {
                    sb.AppendFormat("<div>{0}</div><div>{1}</div>", classname.school_class.school.name, classname.school_class.name);
                }
            }
            else if (row.usergroup == (int)UserGroup.TEACHER)
            {
                if (row.schoolid.HasValue)
                {
                    // teacher can be teaching more than one class
                    sb.AppendFormat("<div>{0}</div>", row.school.name);
                }
            }
            return sb.ToString();
        }

        private static string ToStatus(this ioschools.DB.user row, int year)
        {
            var sb = new StringBuilder();
            // show discipline points
            // add usergroup
            // DONT FORGET TO ADD TABLE TO CACHE DEPENDENCIES
            switch ((UserGroup)row.usergroup)
            {
                case UserGroup.STUDENT:
                    var discipline = row.students_disciplines.Where(x => x.created.Year == year).Sum(x => x.points);
                    if (discipline != 0)
                    {
                        sb.AppendFormat("<div>Merit: {0}</div>", discipline);
                    }

                    // show late/absent count
                    var incidentsArray = row.attendances.ToNumberThisWeek(Utility.GetDBDate());
                    if (incidentsArray[0] != 0)
                    {
                        sb.AppendFormat("<div class='font_red'><strong>{0}</strong> late</div>", incidentsArray[0]);
                    }
                    if (incidentsArray[1] != 0)
                    {
                        sb.AppendFormat("<div class='font_red'><strong>{0}</strong> absent</div>", incidentsArray[1]);
                    }
                    break;
                default:
                    break;
            }
            if ((row.settings & (int)UserSettings.INACTIVE) != 0)
            {
                sb.Append("<div class='font_red'>INACTIVE</div>");
            }

            return sb.ToString();
        }

        private static string ToContactInfo(this ioschools.DB.user row)
        {
            var sb = new StringBuilder();

            if (!string.IsNullOrEmpty(row.email))
            {
                sb.AppendFormat("<div><a href='mailto:{0}'>{0}</a></div>", row.email);
            }
            if (!string.IsNullOrEmpty(row.phone_home))
            {
                sb.AppendFormat("<div class='icon_home' title='home phone'>{0}</div>", row.phone_home);
            }
            if (!string.IsNullOrEmpty(row.phone_cell))
            {
                sb.AppendFormat("<div class='icon_mobile' title='mobile phone'>{0}</div>", row.phone_cell);
            }
            if (row.user_parents != null && !string.IsNullOrEmpty(row.user_parents.phone_office))
            {
                sb.AppendFormat("<div class='icon_server' title='office phone'>{0}</div>", row.user_parents.phone_office);
            }
            
            return sb.ToString();
        }

        public static string ToContactString(this ioschools.DB.user row)
        {
            var contacts = new List<string>();

            if (!string.IsNullOrEmpty(row.phone_home))
            {
                contacts.Add("H:" + row.phone_home);
            }
            if (!string.IsNullOrEmpty(row.phone_cell))
            {
                contacts.Add("M:" + row.phone_cell);
            }
            if (row.user_parents != null && !string.IsNullOrEmpty(row.user_parents.phone_office))
            {
                contacts.Add("O:" + row.user_parents.phone_office);
            }
            return string.Join(", ", contacts.ToArray());
        }

        // extracts DOB from new NRIC
        public static DateTime? ToDOB(this string nric)
        {
            nric = nric.Replace("-", "");
            if (nric.Length != 12)
            {
                // some unknown NRIC length
                return null;
            }

            // remove last 2 digits as year can be 10234, ie. 101th century
            var year = DateTime.Now.Year;
            var thiscentury = year.ToString().Substring(0, year.ToString().Length - 2);
            var lastcentury = (year - 100).ToString().Substring(0, (year - 100).ToString().Length - 2);
            
            // try previous century, if date is 100 years older then use new one
            var month = int.Parse(nric.Substring(2, 2));
            var day = int.Parse(nric.Substring(4, 2));
            var ic_last = new DateTime(int.Parse(lastcentury + nric.Substring(0, 2)), month, day);
            var ic_this = new DateTime(int.Parse(thiscentury + nric.Substring(0, 2)), month, day);
            if (DateTime.Now.AddYears(-100) < ic_last)
            {
                return ic_last;
            }
            return ic_this;
        }

        public static User ToModel(this ioschools.DB.user u, long viewer_id, UserAuth viewer_auth, int year)
        {
            var usr = new User();
            usr.canModify = u.GetCanEdit(viewer_id, viewer_auth);
            usr.canModifyStaff = viewer_auth.perms.HasFlag(Permission.USERS_EDIT_STAFF);
            usr.canModifyLeave = viewer_auth.perms.HasFlag(Permission.LEAVE_ADMIN);
            
            // normal properties
            usr.gender = !string.IsNullOrEmpty(u.gender)?u.gender.ToEnum<Gender>():Gender.MALE;
            usr.id = u.id.ToString();
            usr.name = u.name;
            usr.email = u.email;
            usr.race = u.race;
            usr.dialect = u.dialect;
            usr.dob = u.dob;
            usr.pob = u.pob;
            usr.citizenship = u.citizenship;
            usr.birthcertno = u.birthcertno;
            usr.passport = u.passportno;
            usr.bumi = u.isbumi;
            usr.religion = u.religion;
            usr.nric_new = u.nric_new;
            usr.homephone = u.phone_home;
            usr.cellphone = u.phone_cell;
            usr.address = u.address;
            usr.notes = u.notes;
            usr.school = u.schoolid.HasValue ? (Schools)u.schoolid.Value : (Schools?)null;

            if (usr.school.HasValue)
            {
                usr.schoolname = u.school.name;
            }

            // don't move active as rank checking depends on this value being already set
            usr.active = (u.settings & (int)UserSettings.INACTIVE) == 0;

            // thumbnails
            usr.thumbnailid = u.photo;
            if (u.photo.HasValue)
            {
                usr.profilePhoto = u.user_image.ToModel(Imgsize.USER_PROFILE);
            }
            usr.designation = u.designation;
            usr.usergroup = (UserGroup) u.usergroup;

            // handle marital status
            usr.marital_status = u.marital_status;
            usr.maritalStatusList = typeof(MaritalStatus).ToSelectList(false, null, null, string.IsNullOrEmpty(usr.marital_status)?MaritalStatus.SINGLE.ToString():usr.marital_status);

            // handle supergroups
            if (UserSuperGroups.STAFF.HasFlag(usr.usergroup))
            {
                usr.staffLeaves = u.leaves_allocateds.OrderBy(x => x.leave.name).ToModel();
                if (usr.canModifyStaff)
                {
                    usr.staff = u.user_staffs.ToModel();
                    usr.permission = (Permission)u.permissions;
                }
            }

            // group specific properties
            switch ((UserGroup)u.usergroup)
            {
                case UserGroup.ADMIN:
                    break;
                case UserGroup.CLERK:
                    break;
                case UserGroup.DIRECTOR:
                    break;
                case UserGroup.FINANCE:
                    break;
                case UserGroup.GUARDIAN:
                    usr.employer = u.user_parents.employer;
                    usr.officephone = u.user_parents.phone_office;
                    usr.occupation = u.user_parents.occupation;
                    usr.children = u.students_guardians1.ToChildrenModel(year);
                    break;
                case UserGroup.HEAD:
                    break;
                case UserGroup.STUDENT:
                    // get allocated classes
                    usr.allocatedStudentClasses = u.classes_students_allocateds.OrderByDescending(x => x.year).ToModel();

                    // get parents
                    usr.parents = u.students_guardians.Select(x => new UserParent()
                                                                       {
                                                                           contactInfo = x.user1.ToContactInfo(), 
                                                                           name = x.user1.ToName(), 
                                                                           id = x.user1.id.ToString(),
                                                                           linkid = x.id,
                                                                           relationship = x.type.HasValue ? ((GuardianType)x.type.Value).ToString() : ""
                                                                       });

                    if (usr.active)
                    {
                        usr.rank = u.ToRankStudent();
                    }

                    // show attendance information
                    usr.attendance = new AttendanceViewModel();
                    usr.attendance.Initialise(u, viewer_auth.group, year);

                    // show discipline information
                    usr.discipline = new DisciplineViewModel(u, viewer_id, viewer_auth.group, year);

                    // show eca information
                    usr.eca = new ECAStudentViewModel(u, viewer_auth.perms, year);

                    // show enrolment information
                    usr.enrolments = u.registrations.ToModel();

                    // next student in class only if student already has been allocated to a class
                    if (usr.allocatedStudentClasses.Count(x => x.year == year) != 0)
                    {
                        var commonclass = u.classes_students_allocateds.Single(x => x.year == year)
                            .school_class.classes_students_allocateds.FirstOrDefault(x => x.studentid > u.id && x.year == year);
                        if (commonclass != null)
                        {
                            usr.nextStudentInClassID = commonclass.studentid;
                        }
                        else
                        {
                            commonclass =
                                u.classes_students_allocateds.Single(x => x.year == year)
                                    .school_class.classes_students_allocateds.FirstOrDefault(x => x.studentid < u.id && x.year == year);
                            if (commonclass != null)
                            {
                                usr.nextStudentInClassID = commonclass.studentid;
                            }
                        }
                    }

                    // show homework information
                    usr.homework = new HomeworkStudentViewModel(u.id, u.classes_students_allocateds.AsQueryable(), year, false);

                    if (!(UserGroup.STUDENT | UserGroup.GUARDIAN).HasFlag(viewer_auth.group) &&
                        usr.nextStudentInClassID.HasValue)
                    {
                        usr.canGoToNext = true;
                    }
                    break;
                case UserGroup.TEACHER:
                    // get subjects teaching
                    var teaching = u.subject_teachers.OrderBy(x => x.year).GroupBy(x => x.year);
                    foreach (var yearteaching in teaching)
                    {
                        var subjects = yearteaching.GroupBy(x => x.subject);
                        foreach (var subject in subjects)
                        {
                            var item = new SubjectsTeaching();
                            item.year = yearteaching.Key;
                            item.subjectname = subject.Key.name;
                            item.school =
                                u.subject_teachers.First(x => x.subjectid == subject.Key.id && x.year == yearteaching.Key).
                                    school_class.school.name;
                            item.classesTeaching = string.Join(", ", u.subject_teachers.Where(
                                x => x.subjectid == subject.Key.id && x.year == yearteaching.Key).Select(
                                    x => x.school_class.name));
                            usr.subjectsTeaching.Add(item);
                        }
                    }

                    // get allocated classes
                    usr.allocatedTeacherClasses = u.classes_teachers_allocateds
                        .OrderByDescending(x => x.year)
                        .ThenBy(x => x.day)
                        .ThenBy(x => x.time_start)
                        .ToModel();

                    break;
                case UserGroup.SUPPORT:
                    break;
                
                default:
                    throw new NotImplementedException();
            }

            return usr;
        }

        public static IEnumerable<User> ToModel(this IEnumerable<ioschools.DB.user> rows, long viewer_id, UserAuth viewer_auth, int year)
        {
            foreach (var row in rows)
            {
                yield return row.ToModel(viewer_id, viewer_auth, year);
            }
        }


    }
}