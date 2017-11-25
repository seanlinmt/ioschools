<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<IEnumerable<ioschoolsWebsite.Models.eca.ECA>>" %>
<% foreach (var eca in Model)
   {%>
<tr alt="<%= eca.id %>">
    <td>
        <%= eca.school_name %>
    </td>
    <td>
        <%= eca.name %>
    </td>
    <td class="ar">
        <span class="jqedit">edit</span><span class="jqdelete">delete</span>
    </td>
</tr>
<%} %>
