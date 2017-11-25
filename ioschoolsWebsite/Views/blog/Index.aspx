<%@ Page Title="" Language="C#" Inherits="System.Web.Mvc.ViewPage" MasterPageFile="~/Views/Shared/Internal.Master" %>
<asp:Content runat="server" ID="Content1" ContentPlaceHolderID="TitleContent">Newsletters / Circulars</asp:Content>
<asp:Content runat="server" ID="ExtraHeader" ContentPlaceHolderID="ExtraHeader">
    <script type="text/javascript" src="/js/jqgrid.js"></script>
</asp:Content>
<asp:Content runat="server" ID="Main" ContentPlaceHolderID="MainContent">
<div class="mt10">
<div class="breadcrumb"><a href="/admin">Admin</a> > Circulars / News</div>
<h2 class="icon_blog">Circulars / News</h2> 
<div class="content_filter mt10 ">
<button id="blogAdd" type="button"><img class="am" src="/Content/img/icons/plus.png" /> create new entry</button>
    <div id="typeList" class="filter mt10">
        <h4 class="acc">
            Type</h4>
        <select>
<option value=''>All</option>
<option value="public">Public</option>
<option value="internal">Unpublished</option>
</select>
    </div>
    <div id="yearList" class="filter">
        <h4 class="acc">
            Year</h4>
        <select>
        <option value=''>All</option>
<option value="2011">2011</option>
<option value="2012">2012</option>
</select>
    </div>
</div>
<div class="main_columnright">
    <div id="grid_content">
        <table id="blogsGridView" class="scroll">
        </table>
        <div id="blogsGridNavigation" class="scroll ac">
        </div>
    </div>
</div>
</div>
<script type="text/javascript">
    $(document).ready(function () {
        ioschools.util.setNavigation('#nadmin', 'selected_link');
        pageInit();
        blogsBindToGrid();
    });

    function pageInit() {
        // add button
        $('#blogAdd').click(function () {
            window.location = '/blog/add';
            return false;
        });

        ////////////// category
        // bind side filter click events
        $('select', '.content_filter').bind("change", function () {
            reloadBlogsGrid();
        });
    };
</script>
</asp:Content>
