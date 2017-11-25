<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<IEnumerable<ioschoolsWebsite.Models.admin.ChangeLog>>" %>
<% foreach (var log in Model){%>
<tr alt="<%= log.id %>"><td><%= log.changeDate %></td><td><a href="users/<%= log.creatorid %>" target="_blank"><%= log.creatorname %></a></td><td><%= log.changes %></td></tr>  
<%} %>
