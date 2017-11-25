<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<IEnumerable<ioschoolsWebsite.Models.exam.templates.ExamSubject>>" %>
<% foreach (var subject in Model)
   { %>
<tr alt="<%= subject.id %>" id="subject_<%= subject.id %>">
    <td>
        <span class="hover_move">
            <%= subject.examsubjectname %></span>
    </td>
    <td>
        <%= subject.code %>
    </td>
    <td>
        <%= subject.subjectname %>
    </td>
    <td>
        <span class="jqdelete">delete</span>
        <span class="jqedit">edit</span>
    </td>
</tr>
<%} %>
