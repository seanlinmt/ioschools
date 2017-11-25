<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<System.Collections.Generic.IEnumerable<ioschoolsWebsite.Models.user.staff.EmploymentPeriod>>" %>
<% foreach (var period in Model)
   { %>
<tr alt="<%= period.id %>">
    <td>
        <%= period.startDate %>
    </td>
    <td>
        <%= period.endDate %>
    </td>
    <td>
        <%= period.remarks %>
    </td>
    <td class="ar">
        <span class="jqedit">edit</span> <span class="assign_del">delete</span>
    </td>
</tr>
<%} %>
