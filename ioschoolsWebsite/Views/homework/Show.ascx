<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<ioschoolsWebsite.Models.homework.viewmodels.HomeworkStudentViewModel>" %>

<div class="mt10" id="homework_year_selector_<%= Model.studentid %>">
    <%=Html.DropDownList("homework_year_select", Model.years)%>
</div>
<table id="homeworkTable" class="table_brown mt10">
    <tbody id="homework_results_<%= Model.studentid %>">
        <% Html.RenderPartial("~/Views/Homework/StudentRows.ascx", Model.homeworks); %>
    </tbody>
</table>
<script type="text/javascript">
    $('select', '#homework_year_selector_<%= Model.studentid %>').bind('change', function () {
        var selected = $(this).val();
        if (selected != '') {
            $(this).post('/homework/homeworkcontent/<%= Model.studentid %>', { year: selected }, function (result) {
                $('#homework_results_<%= Model.studentid %>').html(result);
            });
        }
        return false;
    });
</script>