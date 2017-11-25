<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<IEnumerable<ioschoolsWebsite.Models.calendar.admin.CalendarAdminEntry>>" %>
<%@ Import Namespace="ioschools.Data" %>
<% foreach (var entry in Model)
   {%>
<tr alt="<%= entry.id %>">
    <td class="ar">
        <%= entry.date.ToString(Constants.DATEFORMAT_DATEPICKER)%>
    </td>
    <td>
        <%= entry.description %>
    </td>
    <td>
        <%= entry.isHoliday?"Yes":"No" %>
    </td>
    <td class="ar">
        <span class="jqedit">edit</span><span class="jqdelete">delete</span>
    </td>
</tr>
<%} %>
