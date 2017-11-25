<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Main.Master" Inherits="System.Web.Mvc.ViewPage" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	Contact Us
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<div class="body_content">
<div class="breadcrumb">
<a href="/">Home</a> / Contact Us
</div>
<h1>Contact Us</h1>

<div class="w200px fl">

</div>
<div id="map" class="fl bg_lightgrey" style="width:700px;height:400px;">
<img src="/Content/img/loading.gif" /> Loading map    

</div>
</div>
<script src="http://maps.google.com/maps?file=api&amp;v=2&amp;sensor=false&amp;key=<%= ioschools.Data.Constants.GOOGLEMAP_APIKEY %>" type="text/javascript"></script>
<script type="text/javascript">
    $(document).ready(function () {
        var latitude =;
        var longtitude =;
        var zoomLevel = 15;
        var center = new GLatLng(latitude, longtitude);
        var map = new GMap2(document.getElementById("map"));
        map.addControl(new GSmallMapControl());
        map.addControl(new GMapTypeControl());
        map.setCenter(center, zoomLevel);
        map.setZoom(zoomLevel);

        var marker = new GMarker(center, { draggable: false });
        map.addOverlay(marker);
        map.panTo(center);
        GEvent.addListener(marker, "click", function () {
            marker.openInfoWindowHtml(""); 
        });
    });
</script>
</asp:Content>
