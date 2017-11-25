<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Internal.Master" Inherits="System.Web.Mvc.ViewPage<ioschoolsWebsite.Models.fees.viewmodel.ParentFinanceViewModel>" %>
<%@ Import Namespace="ioschools.Data" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	Finance
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<div id="finance_tabs">
    <ul class="hidden">
         <li><a href="#fees_statement">Statement</a></li>
        <li><a href="#fees_unpaid">Payments Outstanding</a></li>
    </ul>
    <div id="fees_unpaid" class="hidden pt10">
        <% Html.RenderPartial("~/Views/schoolfees/overduefees.ascx", Model.alert); %>
    </div>
    <div id="fees_statement" class="hidden pt10">
        <% Html.RenderPartial("~/Views/schoolfees/statement.ascx", Model.statement); %>
    </div>
</div>
<script type="text/javascript">
    $(document).ready(function () {
        ioschools.util.setNavigation('#nfinance', 'selected_dark');
        $('#finance_tabs').tabs();
    });
    </script>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="ExtraHeader" runat="server">
</asp:Content>
