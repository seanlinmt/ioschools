using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ioschools.Data;
using ioschools.Data.User;

namespace ioschools.DB
{
    public partial class user
    {
        public bool GetCanEdit(long viewerid, UserAuth auth)
        {
            var canedit = false;
            var rowGroup = (UserGroup)usergroup;

            // only staff can do something
            if (UserSuperGroups.STAFF.HasFlag(auth.group))
            {
                // permission based
                if ((rowGroup == UserGroup.STUDENT && auth.perms.HasFlag(Permission.USERS_EDIT_STUDENTS)) ||
                    (rowGroup == UserGroup.GUARDIAN && auth.perms.HasFlag(Permission.USERS_EDIT_PARENTS)) ||
                    (UserSuperGroups.STAFF.HasFlag(rowGroup) && auth.perms.HasFlag(Permission.USERS_EDIT_STAFF)) ||
                    (id == viewerid && auth.perms.HasFlag(Permission.USERS_EDIT_OWN)))
                {
                    canedit = true;
                }
            }

            return canedit;
        }

        public bool GetCanView(long viewerid, UserAuth auth)
        {
            var rowGroup = (UserGroup)usergroup;

            var canview = false;
            if (UserSuperGroups.STAFF.HasFlag(auth.group))
            {
                // permission based
                if ((rowGroup == UserGroup.STUDENT && auth.perms.HasFlag(Permission.USERS_VIEW_STUDENTS)) ||
                    (rowGroup == UserGroup.GUARDIAN && auth.perms.HasFlag(Permission.USERS_VIEW_PARENTS)) ||
                    (UserSuperGroups.STAFF.HasFlag(rowGroup) && auth.perms.HasFlag(Permission.USERS_VIEW_STAFF)) ||
                    (id == viewerid))
                {
                    canview = true;
                }
            }
            else
            {
                switch (auth.group)
                {
                    case UserGroup.STUDENT:
                        // can only view self
                        if (id == viewerid)
                        {
                            canview = true;
                        }
                        break;
                    case UserGroup.GUARDIAN:
                        // restrict to children only
                        if (rowGroup == UserGroup.STUDENT)
                        {
                            if (students_guardians.Count(x => x.parentid == viewerid) != 0)
                            {
                                canview = true;
                            }
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return canview;
        }


        public UserIssue GetIssues(int year, bool returnOnFirstIssueDetected = true)
        {
            var issue = UserIssue.NONE;
            var _ugroup = (UserGroup)usergroup;
            var isActive = !((UserSettings)settings).HasFlag(UserSettings.INACTIVE);

            if (UserSuperGroups.STAFF.HasFlag(_ugroup))
            {
                // staff
                if (!employments.Any())
                {
                    issue |= UserIssue.STAFF_NOEMPLOYMENT;
                    if (returnOnFirstIssueDetected)
                    {
                        return issue;
                    }
                }
                else
                {
                    if (isActive && !employments.Any(x => x.start_date.HasValue))
                    {
                        issue |= UserIssue.USER_NOSTARTDATE;
                        if (returnOnFirstIssueDetected)
                        {
                            return issue;
                        }
                    }

                    if (!isActive)
                    {
                        var activeRow = employments.Where(x => x.start_date.HasValue)
                                            .OrderBy(x => x.start_date).LastOrDefault();
                        if (activeRow != null && !activeRow.end_date.HasValue && DateTime.Now > activeRow.start_date.Value)
                        {
                            issue |= UserIssue.USER_NOLEAVINGDATE;
                            if (returnOnFirstIssueDetected)
                            {
                                return issue;
                            }
                        }

                    }
                }
            }

            switch (_ugroup)
            {
                case UserGroup.NONE:
                case UserGroup.DIRECTOR:
                case UserGroup.HEAD:
                case UserGroup.ADMIN:
                case UserGroup.FINANCE:
                case UserGroup.CLERK:
                case UserGroup.SUPPORT:
                    break;
                case UserGroup.TEACHER:
                    if (isActive && !subject_teachers.Any(x => x.year == year))
                    {
                        if (schoolid.HasValue && schoolid.Value == (int)Schools.Kindergarten)
                        {
                            // ignore for kindy teachers
                        }
                        else
                        {
                            issue |= UserIssue.TEACHER_NOSUBJECT;
                            if (returnOnFirstIssueDetected)
                            {
                                return issue;
                            }
                        }
                    }
                    break;
                case UserGroup.STUDENT:
                    if (isActive && !classes_students_allocateds.Any(x => x.year == year))
                    {
                        issue |= UserIssue.STUDENT_NOCLASS;
                        if (returnOnFirstIssueDetected) 
                        {
                            return issue;
                        }
                    }

                    if (!dob.HasValue)
                    {
                        issue |= UserIssue.STUDENT_NOBIRTHDAY;
                        if (returnOnFirstIssueDetected)
                        {
                            return issue;
                        }
                    }

                    var registration = registrations.LastOrDefault();
                    if (registration == null)
                    {
                        issue |= UserIssue.STUDENT_NOENROLMENT;
                        if (returnOnFirstIssueDetected)
                        {
                            return issue;
                        }
                    }
                    else
                    {
                        // ignore if not accepted
                        if (registration.status == RegistrationStatus.ACCEPTED.ToString())
                        {
                            if (isActive && !registrations.Any(x => x.admissionDate.HasValue))
                            {
                                issue |= UserIssue.USER_NOSTARTDATE;
                                if (returnOnFirstIssueDetected)
                                {
                                    return issue;
                                }
                            }

                            if (!isActive)
                            {
                                var activeRow = registrations.Where(x => x.admissionDate.HasValue)
                                                    .OrderBy(x => x.admissionDate).LastOrDefault();
                                if (activeRow != null && !activeRow.leftDate.HasValue && DateTime.Now > activeRow.admissionDate.Value)
                                {
                                    issue |= UserIssue.USER_NOLEAVINGDATE;
                                    if (returnOnFirstIssueDetected)
                                    {
                                        return issue;
                                    }
                                }

                            }
                        }

                        
                    }

                    if (students_guardians.Count(y => y.type.HasValue && y.type.Value == (byte)GuardianType.FATHER) > 1 ||
                        students_guardians.Count(y => y.type.HasValue && y.type.Value == (byte)GuardianType.MOTHER) > 1)
                    {
                        issue |= UserIssue.STUDENT_MANYPARENTS;
                        if (returnOnFirstIssueDetected)
                        {
                            return issue;
                        }
                    }
                    

                    break;
                case UserGroup.GUARDIAN:
                    if (!students_guardians1.Any())
                    {
                        issue |= UserIssue.GUARDIAN_NOCHILD;
                        if (returnOnFirstIssueDetected)
                        {
                            return issue;
                        }
                    }
                    break;
                default:
                    throw new NotImplementedException();
            }

            return issue;
        }

        // used to prevent information from appearing after leaving date
        public int? FinalYear
        {
            get
            {
                var latestEnrolled = registrations.OrderByDescending(x => x.id).FirstOrDefault();
                if (latestEnrolled != null && latestEnrolled.leftDate.HasValue)
                {
                    return latestEnrolled.leftDate.Value.Year;
                }
                return null;
            }
        }

        public int? GetNewSchoolID()
        {
            if (usergroup == (int)UserGroup.STUDENT)
            {
                var classname =
                    classes_students_allocateds.FirstOrDefault(x => x.year == DateTime.Now.Year);
                if (classname != null)
                {
                    return classname.school_class.school.id;
                }
            }
            else if (usergroup == (int)UserGroup.TEACHER)
            {
                // check subject teacher entries first
                var subject = subject_teachers.OrderByDescending(x => x.year).FirstOrDefault();
                if (subject != null)
                {
                    return subject.school_class.schoolid;
                }

                var classname = classes_teachers_allocateds.OrderByDescending(x => x.year).FirstOrDefault();
                if (classname != null)
                {
                    return classname.school.id;
                }
            }
            return schoolid;
        }

        public string ToName(bool includeDesignation = true)
        {
            var sb = new StringBuilder();

            // add name
            if (string.IsNullOrEmpty(designation) || !includeDesignation)
            {
                sb.Append(name);
            }
            else
            {
                sb.AppendFormat("{0} {1}", designation, name);
            }

            return sb.ToString();
        }
    }


    
}
