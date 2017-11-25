<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<System.Collections.Generic.IEnumerable<ioschoolsWebsite.Models.user.UserBase>>" %>
<% foreach (var usr in Model){%>
  <div class="blockSelectable" alt="<%= usr.id %>">
        <div class="fl w50px ac">
            <%= usr.thumbnailString %>
        </div>
        <div class="content">
        <p>
            <%= usr.name %></p>
        </div>
    </div>
<%} %>
