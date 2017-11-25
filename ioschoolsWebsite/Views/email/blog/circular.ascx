<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>
<p>Dear all,</p>
<p><a href="<%= ViewData["link"] %>"><strong><%= ViewData["title"] %></strong></a></p>
<p><%= ViewData["content"] %></p>
<p>&nbsp;</p>
<p><%= ViewData["sender"] %></p>
