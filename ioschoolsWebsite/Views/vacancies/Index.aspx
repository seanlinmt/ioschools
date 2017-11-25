<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Main.Master" Inherits="System.Web.Mvc.ViewPage<ioschoolsWebsite.Models.pages.PageViewModel>" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    Vacancies
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="body_content">
        <div class="breadcrumb">
            <a href="/">Home</a> / Vacancies
        </div>
        <h1 class="fl">
            Vacancies</h1>
        <% if(Model.canEdit)
           {%>
        <button id="buttonEdit" type="button" class="fr">
            <img class="am" src="/Content/img/icons/edit.png" />
            edit</button>
        <%
           }%>
           <div class="clear"></div>
           <%= Model.content %>
    </div>
    <script type="text/javascript">
        $(document).ready(function () {
            $('#buttonEdit').click(function () {
                window.location = '/vacancies/edit';
            });
        });
    </script>
</asp:Content>
