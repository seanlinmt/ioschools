<%@ Page Title="" Language="C#" Inherits="System.Web.Mvc.ViewPage<ioschoolsWebsite.Models.user.student.GuardianViewModel>" MasterPageFile="~/Views/Shared/Internal.Master" %>
<asp:Content runat="server" ID="Title" ContentPlaceHolderID="TitleContent"> Information System</asp:Content>
<asp:Content runat="server" ID="ExtraHeader" ContentPlaceHolderID="ExtraHeader"></asp:Content>
<asp:Content runat="server" ID="Main" ContentPlaceHolderID="MainContent">
<div id="content_result">
<%if (!Model.students.Any())
  {%>
  <p class="mt10">No children enrolled.</p>
<%
    }
  else
  {%>
  <div id="children_tabs">
    <ul class="hidden">
    <% foreach (var child in Model.students){%>
        <li><a href="#<%= child.id %>_child"><%= child.name %></a></li>
        <%}%>
    </ul>
    <% foreach (var child in Model.students){%>
    <div id="<%= child.id %>_child" class="hidden">
        <% if (child.enrolments != null && 
               child.enrolments.Count() != 0 && 
               child.enrolments.Any(x => x.status.Contains("accepted")))
           {%>
           <div class="mt10 mb10">
        <button type="button" onclick="dialogBox_open('/feedback/student/<%= child.id %>','Contact School', 600);"><img class="am" src="/Content/img/icons/send.png" /> contact teacher or principal</button>
   </div>
    <% } %>
    <% Html.RenderPartial("~/Views/Users/SingleContent.ascx",child); %>
    </div>
    <%}%>
    </div>
  <%
  }%>
</div>
<script type="text/javascript">
    $(document).ready(function () {
        ioschools.util.setNavigation('#nhome', 'selected_dark');
        $('#children_tabs').tabs();
    });
</script>
</asp:Content>
