using System.ComponentModel;

namespace ioschoolsWebsite.Library.email
{
    public enum EmailViewType
    {
        [Description("~/Views/email/student/Attendance.ascx")]
        ADMIN_ATTENDANCE,
        [Description("~/Views/email/blog/circular.ascx")]
        CIRCULAR,
        [Description("~/Views/email/system/Feedback.ascx")]
        FEEDBACK,
        [Description("~/Views/email/system/FeedbackVendor.ascx")]
        FEEDBACKVENDOR,
        [Description("~/Views/email/homework/NotificationToTeacher.ascx")]
        HOMEWORK_NOTIFICATION,
        [Description("~/Views/email/leave/LeaveApproval.ascx")]
        LEAVE_APPROVAL,
        [Description("~/Views/email/leave/LeaveUpdated.ascx")]
        LEAVE_UPDATED,
        [Description("~/Views/email/student/NotificationToParent.ascx")]
        PARENT_NOTIFICATION,
        [Description("~/Views/email/system/PasswordReset.ascx")]
        PASSWORD_RESET,
        [Description("~/Views/email/system/Registration.ascx")]
        REGISTRATION,
        [Description("~/Views/email/system/RegistrationNotification.ascx")]
        REGISTRATION_NOTIFICATION,
        [Description("~/Views/email/system/RegistrationUpdate.ascx")]
        REGISTRATION_UPDATE
    }
}