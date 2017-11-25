<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Main.Master" Inherits="System.Web.Mvc.ViewPage<ioschoolsWebsite.Models.blog.BlogSummaryViewData>" %>
<%@ Import Namespace="ioschoolsWebsite.Library" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    News &amp; Events
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="body_content">
        <div class="breadcrumb">
            <a href="/">Home</a> / News &amp; Events
        </div>
        <h1>
            News &amp; Events</h1>
        <div class="col_1">
            <img class="img_border" src="../../Content/img/news/vertical_banner.jpg" />
        </div>
        <div id="events" class="col_1">
            <%
                Html.RenderPartial("~/Views/events/Index.ascx", Model.events);%>
        </div>
        <div id="news" class="col_1">
            <h2 class="bb mb10 bold">
                News</h2>
            <% foreach (var entry in Model.newspanel.news)
               {%>
<div class="item">
    <div class="title">
        <a href="/news/<%= entry.id %>/<%= entry.title.ToSafeUrl() %>">
            <%= entry.title %></a></div>
    <div class="date">
        <%= entry.date %></div>
    <div class="content">
        <%= entry.content %><a class="ml5 smaller" href="/news/<%= entry.id %>/<%= entry.title.ToSafeUrl() %>">more »</a>
    </div>
</div>
<%} %>
<div class="mt20">
<% if (Model.newspanel.hasOlder)
   {%>
<span class="fl"><a id="olderEntries" href="/news?page=<%= Model.newspanel.page + 1 %>">« older</a></span>
<%
   }%>
   <% if (Model.newspanel.hasNewer)
   {%>
<span class="fr"><a id="newerEntries" href="/news?page=<%= Model.newspanel.page - 1 %>">newer »</a></span>
<%
   }%>
</div>
        </div>
    </div>
</asp:Content>
