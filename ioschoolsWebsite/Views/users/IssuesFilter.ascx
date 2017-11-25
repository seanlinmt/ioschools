<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>
<div class="filter">
        <%= Html.CheckBox("hasIssues", false) %><label for="hasIssues">only problem rows</label>
    </div>
