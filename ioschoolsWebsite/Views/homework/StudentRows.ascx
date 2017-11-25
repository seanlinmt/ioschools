<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<IEnumerable<ioschoolsWebsite.Models.homework.HomeworkStudent>>" %>
<% foreach (var homework in Model)
{%>
<% Html.RenderPartial("~/Views/homework/studentsingle.ascx", homework); %>
<%
}%>