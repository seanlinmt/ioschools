<%@ Page Title="" Language="C#" Inherits="System.Web.Mvc.ViewPage<ioschoolsWebsite.Models.enrolment.AdmissionViewModel>" MasterPageFile="~/Views/Shared/Internal.Master" %>
<asp:Content runat="server" ID="Content1" ContentPlaceHolderID="TitleContent">Add Enrolment</asp:Content>
<asp:Content runat="server" ID="ExtraHeader" ContentPlaceHolderID="ExtraHeader">
</asp:Content>
<asp:Content runat="server" ID="Main" ContentPlaceHolderID="MainContent">
    <div class="mt10">
    <div class="breadcrumb"><a href="/enrolment">Enrolments</a> > New Student Enrolment</div>
<h2>New Student Enrolment</h2>
<div class="info">System will match any existing students or parents/guardian already in the system by their NRIC or passport number. </div>
<form id="registerForm" action="/enrolment/save" method="post" enctype="multipart/form-data" class="pb100">
<% Html.RenderPartial("~/Views/admission/FormCommon.ascx", Model); %>
<div class="mt20">
<button id="buttonRegister" type="submit" class="large"><img class="am" src="/Content/img/icons/save.png" /> submit registration</button>
</div>
</form>
</div>
<script type="text/javascript">
    $(document).ready(function () {
        init_inputSelectors();
        ioschools.util.setNavigation('#nenrolment', 'selected');
        $('#registerForm').validate({
            rules: {
                child_photo: {
                    accept: "jpg|png|gif|jpeg"
                }
            },
            messages: {
                child_photo: {
                    accept: "Please select a valid image file"
                }
            }
        });

        $.validator.addClassRules('required_group', {
            email: true
        });
    });
</script>
</asp:Content>