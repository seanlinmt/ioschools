<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<ioschoolsWebsite.Models.school.SchoolClass>" %>
<%@ Import Namespace="ioschoolsWebsite.Library.Extensions" %>
<tr alt="<%= Model.id %>" class="bg_edit">
    <td>
        <%= Html.DropDownList("year", Model.schoolyearList)%>
    </td>
    <td>
        <%= Html.TextBox("name", Model.currentClass)%>
    </td>
    <td>
        <%= Html.GroupDropDownList("next", Model.nextClassList)%>
    </td>
    <td class="ar">
        <button id="buttonSave" type="button">
            <img class="am" src="/Content/img/icons/save.png" /> save</button>
            <button id="buttonCancel" type="button">
            <img class="am" src="/Content/img/icons/cancel.png" /> cancel
            </button>
    </td>
</tr>
