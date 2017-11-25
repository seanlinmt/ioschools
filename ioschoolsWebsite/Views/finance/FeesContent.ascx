<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<System.Collections.Generic.IEnumerable<ioschoolsWebsite.Models.fees.FeePayable>>" %>
<%@ Import Namespace="ioschoolsWebsite.Models.fees" %>
<% foreach (var row in Model)
   {%>
<tr alt="<%= row.id %>">
    <td>
        <%= row.school_name %>
    </td>
    <td>
        <%= row.name %>
    </td>
    <td class="ar">
        <%= row.amount %>
    </td>
    <td class="ar">
<span class="jqedit">edit</span><span class="jqdelete">delete</span>
    </td>
</tr>
<%} %>
