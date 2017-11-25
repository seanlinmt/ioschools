<%@ Page Title="" Language="C#" Inherits="System.Web.Mvc.ViewPage" MasterPageFile="~/Views/Shared/Main.Master" %>

<asp:Content runat="server" ID="Title" ContentPlaceHolderID="TitleContent">
    Extracurricular Activities</asp:Content>
<asp:Content runat="server" ID="ExtraHeader" ContentPlaceHolderID="ExtraHeader">
    <style type="text/css">
        .style1 {
            background-color: #F7F7F7;
        }
    </style>
</asp:Content>
<asp:Content runat="server" ID="Main" ContentPlaceHolderID="MainContent">
    <div class="nav_side mt20">
        <ul>
            <li><a href="/primary">About</a></li>
            <li><a class="selected" href="/primary/eca">Extracurricular Activities</a></li>
        </ul>
    </div>
    <div class="col_2 ml20 mt10">
        <div class="breadcrumb">
            <a href="/">Home</a> / <a href="/Schools">Schools</a> / <a href="/primary/">Primary</a>
            / Extracurricular Activities
        </div>
        <h1>
            Extracurricular Activities</h1>
        
    </div>
</asp:Content>
