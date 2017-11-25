<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<ioschoolsWebsite.Models.notifications.NotificationSendViewModel>" %>
<p>Dear <%= Model.receiver %>,</p>
<p><%= Model.message %></p>
<p>
Regards,
<br />
 School
</p>
