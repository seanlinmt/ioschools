<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Internal.Master" Inherits="System.Web.Mvc.ViewPage" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	An error has occurred
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <h2>An error has occurred</h2>
    <p><%= ViewData["message"] %></p>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="ExtraHeader" runat="server">
</asp:Content>
