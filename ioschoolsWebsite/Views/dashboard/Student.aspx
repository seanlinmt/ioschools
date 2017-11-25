<%@ Page Title="" Language="C#" Inherits="System.Web.Mvc.ViewPage<ioschoolsWebsite.Models.user.student.StudentViewModel>"
    MasterPageFile="~/Views/Shared/Internal.Master" %>
<%@ Import Namespace="ioschoolsWebsite.Library.Helpers" %>
<asp:Content runat="server" ID="Title" ContentPlaceHolderID="TitleContent">
 Information System
</asp:Content>
<asp:Content runat="server" ID="ExtraHeader" ContentPlaceHolderID="ExtraHeader">
</asp:Content>
<asp:Content runat="server" ID="Main" ContentPlaceHolderID="MainContent">
    <%= Html.JavascriptInclude("/Scripts/ajaxUpload/fileuploader.js")%>
    <div id="student_tabs">
        <ul class="hidden">
            <li><a href="#homework">Homework</a></li>
            <li><a href="#exam">Results</a></li>
            <li><a href="#timetable">Timetable</a></li>
            <li><a href="#discipline">Conduct</a></li>
        </ul>
        <div id="homework" class="hidden">
            <% Html.RenderPartial("~/Views/homework/IndexStudent.ascx", Model.homework); %>
        </div>
        <div id="exam" class="hidden">
            <% Html.RenderPartial("~/Areas/exams/Views/Exams/Show.ascx", Model.exam); %>
        </div>
        <div id="timetable" class="hidden">
            <% Html.RenderAction("ScheduleContent", "Schedule"); %>
        </div>
            <div id="discipline" class="hidden">
        <% Html.RenderPartial("~/Views/Discipline/Show.ascx", Model.discipline); %>
    </div>
    </div>
    <script type="text/javascript">
        $(document).ready(function () {
            $('#student_tabs').tabs();
        });
    </script>
</asp:Content>
