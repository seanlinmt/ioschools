<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<System.Collections.Generic.IEnumerable<ioschoolsWebsite.Models.school.SchoolTermsViewModel>>" %>
<% foreach (var entry in Model)
   {%>
  <% Html.RenderPartial("SchoolDays", entry); %>
 <%   } %>
