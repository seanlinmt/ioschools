<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<IEnumerable<ioschoolsWebsite.Models.leave.AdminLeave>>" %>
<% foreach (var leave in Model)
   {%>
<tr alt="<%= leave.id %>">
    <td>
        <%= leave.name %>
    </td>
    <td>
        <%= leave.annualTotal %>
    </td>
    <td class="ar">
        <span class="jqedit">edit</span><span class="jqdelete">delete</span>
    </td>
</tr>
<%} %>
