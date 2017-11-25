<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<System.Collections.Generic.IEnumerable<ioschoolsWebsite.Models.leave.LeaveTaken>>" %>
<%@ Import Namespace="ioschools.Data.Leave" %>
<% foreach (var leave in Model)
   {%>
<tr alt="<%= leave.id %>">
    <td>
        <%= leave.id %>
    </td>
    <td>
            <ul>
            <li class="bold">
                <%= leave.name %></li>
            <li>
                <%= leave.start_date %><span class="smaller font_grey">(<%= leave.start_time %>)</span> -
                <%= leave.end_date %><span class="smaller font_grey">(<%= leave.end_time %>)</span> </li>
            <% if (!string.IsNullOrEmpty(leave.description))
               {%>
            <li class="ff_serif font_darkblue bg_lightblue p5">
                <%= leave.description %></li>
            <% } %>
            <% if (!string.IsNullOrEmpty(leave.reason))
               {%>
            <li class="font_red ff_serif p5 bg_lightred">
                <%= leave.reason %></li>
            <% } %>
        </ul>
           
       </td> 
 
    <td>
        <%= leave.days %>
    </td>
    <td>
        <div class="mb10">
            <%= leave.status %></div>
        <% if (leave.showReview)
           { %>
        <span class="jqedit">review</span>
        <% } %>
        <% if (leave.showDelete)
           { %>
        <span class="jqdelete">cancel</span>
        <% } %>
    </td>
</tr>
<% } %>