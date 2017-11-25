<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<System.Collections.Generic.IEnumerable<ioschoolsWebsite.Models.school.SchoolYear>>" %>
<% foreach (var schoolYear in Model)
           { %>
<tr alt="<%= schoolYear.id %>">
    <td>
        <%= schoolYear.name %>
    </td>
    <td>
        <%= schoolYear.gradingmethodName %>
    </td>
    <td class="ar">
        <span class="jqedit">edit</span><span class="jqdelete">delete</span>
    </td>
</tr>
<%  } %>
