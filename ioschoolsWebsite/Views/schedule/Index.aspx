<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Internal.Master" Inherits="System.Web.Mvc.ViewPage" %>
<asp:Content runat="server" ID="Title" ContentPlaceHolderID="TitleContent">Schedule</asp:Content>
<asp:Content runat="server" ID="Main" ContentPlaceHolderID="MainContent">
<div id="content_result">
<% Html.RenderAction("ScheduleContent");%>
<script type="text/javascript">
    $(document).ready(function () {
        ioschools.util.setNavigation('#nschedule', 'selected');
    });
</script>
</div>
</asp:Content>
