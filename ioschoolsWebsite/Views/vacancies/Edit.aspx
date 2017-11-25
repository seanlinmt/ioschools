<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Main.Master" Inherits="System.Web.Mvc.ViewPage<ioschoolsWebsite.Models.pages.PageViewModel>" %>
<%@ Import Namespace="ioschoolsWebsite.Library.Helpers" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	Edit Page
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<h2>Edit Page: Vacancies</h2>
<form id="editorform" method="post" action="/vacancies/save">
<textarea id="content" name="content" class="w700px h400px"><%= Model.content %></textarea>
<div class="mt20">
    <span class="mr10">
        <button type="button" id="buttonSave" class="ajax">
            <img class="am" src="/Content/img/icons/save.png" /> save</button>
            <a href="/vacancies">cancel</a>
    </span>
</div>
</form>
<script type="text/javascript">
    $(document).ready(function () {
        $('#buttonSave', '#editorform').click(function () {
            $('#editorform').trigger('submit');
        });

        $('#content', '#editorform').wysiwyg({
            autoGrow: false,
            autoload: true,
            controls: {
                insertImage: { visible: false }
            },
            css: '/Content/css/common.css?v=3'
        });
    });


    $('#editorform').submit(function () {
        var action = $(this).attr("action");


        var serialized = $(this).serialize();

        // post form
        $('#buttonSave', '#editorform').post(action, serialized, function (json_data) {
            if (json_data.success) {
                window.location = '/vacancies';
            }
        }, 'json');
        return false;
    });
</script>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="ExtraHeader" runat="server">
<%= Html.JavascriptInclude("/Scripts/ajaxUpload/fileuploader.js")%>
<link media="screen, print" type="text/css" rel="stylesheet" href="/editor/jquery.wysiwyg.css" />
<%= Html.JavascriptInclude("/editor/jquery.wysiwyg.js")%>
<%= Html.JavascriptInclude("/editor/jquery.autoload.js")%>
<%= Html.JavascriptInclude("/editor/controls/wysiwyg.table.js")%>
<%= Html.JavascriptInclude("/editor/controls/wysiwyg.link.js")%>
<%= Html.JavascriptInclude("/editor/plugins/wysiwyg.autoload.js")%>
<%= Html.JavascriptInclude("/editor/plugins/wysiwyg.rmFormat.js")%>
</asp:Content>
