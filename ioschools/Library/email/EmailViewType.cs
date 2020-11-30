using System.ComponentModel;

namespace ioschools.Library.email
{
    public enum EmailViewType
    {
        [Description("~/Views/email/student/Attendance.cshtml")]
        ADMIN_ATTENDANCE,
        [Description("~/Views/email/blog/circular.cshtml")]
        CIRCULAR,
        [Description("~/Views/email/system/Feedback.cshtml")]
        FEEDBACK,
        [Description("~/Views/email/system/FeedbackVendor.cshtml")]
        FEEDBACKVENDOR,
        [Description("~/Views/email/homework/NotificationToTeacher.cshtml")]
        HOMEWORK_NOTIFICATION,
        [Description("~/Views/email/leave/LeaveApproval.cshtml")]
        LEAVE_APPROVAL,
        [Description("~/Views/email/leave/LeaveUpdated.cshtml")]
        LEAVE_UPDATED,
        [Description("~/Views/email/student/NotificationToParent.cshtml")]
        PARENT_NOTIFICATION,
        [Description("~/Views/email/system/PasswordReset.cshtml")]
        PASSWORD_RESET,
        [Description("~/Views/email/system/Registration.cshtml")]
        REGISTRATION,
        [Description("~/Views/email/system/RegistrationNotification.cshtml")]
        REGISTRATION_NOTIFICATION,
        [Description("~/Views/email/system/RegistrationUpdate.cshtml")]
        REGISTRATION_UPDATE
    }
}