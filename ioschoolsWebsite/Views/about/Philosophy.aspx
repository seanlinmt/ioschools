<%@ Page Title="" Language="C#" Inherits="System.Web.Mvc.ViewPage" MasterPageFile="~/Views/Shared/Main.Master" %>

<asp:Content runat="server" ID="Title" ContentPlaceHolderID="TitleContent">
    Philosophy</asp:Content>
<asp:Content runat="server" ID="ExtraHeader" ContentPlaceHolderID="ExtraHeader">
</asp:Content>
<asp:Content runat="server" ID="Main" ContentPlaceHolderID="MainContent">
    <div class="nav_side mt20">
        <ul>
            <li><a href="/about">About </a></li>
            <li><a href="/about/history">History</a></li>
            <li><a class="selected" href="/about/philosophy">Philosophy</a></li>
            <li><a href="/about/vision">Vision</a></li>
            <li><a href="/about/mission">Mission</a></li>
            <li><a href="/about/people">People</a></li>
            <li><a href="/about/spirit">Spirit</a></li>
        </ul>
    </div>
    <div class="col_2 ml20 mt10">
        <div class="breadcrumb">
            <a href="/">Home</a> / Philosophy
        </div>
       
    </div>
    <script type="text/javascript">
        $(document).ready(function () {
            $('.slider').nivoSlider({
                controlNav: false,
                directionNav: false,
                pauseTime: 5000,
                pauseOnHover: false
            });
        });
    </script>
</asp:Content>
