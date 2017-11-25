<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<IEnumerable<ioschoolsWebsite.Models.homework.Homework>>" %>
<% foreach (var homework in Model)
{%>
    <% Html.RenderPartial("~/Views/homework/single.ascx", homework); %>
    <%
}%>
