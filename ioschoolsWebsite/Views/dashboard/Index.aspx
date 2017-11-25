<%@ Page Title="" Language="C#" Inherits="System.Web.Mvc.ViewPage<ioschoolsWebsite.Models.dashboard.DashboardViewModel>" MasterPageFile="~/Views/Shared/Internal.Master" %>
<asp:Content runat="server" ID="Title" ContentPlaceHolderID="TitleContent">Dashboard</asp:Content>
<asp:Content runat="server" ID="ExtraHeader" ContentPlaceHolderID="ExtraHeader">
</asp:Content>
<asp:Content runat="server" ID="Main" ContentPlaceHolderID="MainContent">
<div id="content_result">
      <%  Html.RenderPartial("search", Model.yearList); %>
</div>
</asp:Content>
