<%@ Page Title="" Language="C#" Inherits="System.Web.Mvc.ViewPage<ioschoolsWebsite.Models.enrolment.EnrolmentViewModel>" MasterPageFile="~/Views/Shared/Internal.Master" %>
<asp:Content runat="server" ID="Content1" ContentPlaceHolderID="TitleContent">Edit Enrolment</asp:Content>
<asp:Content runat="server" ID="ExtraHeader" ContentPlaceHolderID="ExtraHeader">
</asp:Content>
<asp:Content runat="server" ID="Main" ContentPlaceHolderID="MainContent">
    <div class="pb100 mt10">
        <div class="breadcrumb"><a href="/enrolment">Enrolments</a> > Update Enrolment Record</div>
        <h2 class="font_red"><%= Model.enrolment.student_name %> Enrolment Record</h2>
        <% Html.RenderPartial("EditCommon", Model.enrolment); %>
    </div>
<div class="buttonRow_bottom">
    <span class="mr10">
        <button type="button" id="buttonSave" class="ajax large">
           <img class="am" src="/Content/img/icons/save.png" /> save</button>
    </span>
</div>
<script type="text/javascript">
    $(document).ready(function () {
        ioschools.util.setNavigation('#nenrolment', 'selected');
        if ($('#id').val() != '') {
            $('#message_area').hide();
        }

        $('#enrol_school').bind('change', function () {
            $('#enrol_schoolyear').html('');
            var id = $(this).val();
            if (id == '') {
                return false;
            }

            $(this).post('/schools/years/' + id, null, function (json_result) {
                if (json_result.success) {
                    $('#enrol_schoolyear').html('<option value="">select year</option>');
                    $.each(json_result.data, function () {
                        var option = ["<option value='", this.id, "'>", this.name, "</option>"];
                        $('#enrol_schoolyear').append(option.join(''));
                    });
                }
                else {
                    $.jGrowl(json_result.message);
                }
            }, 'json');
        });
    });

    $('#buttonSave').click(function () {
        $('#enrolForm').trigger('submit');
    });

    $('#enrolForm').submit(function () {
        var serialized = $(this).serialize();
        var action = $(this).attr("action");

        // post form
        $('#buttonSave').post(action, serialized, function (json_result) {
            if (json_result.success) {
                window.location = '/enrolment';
            }
            $.jGrowl(json_result.message);
        }, 'json');
        return false;
    });
</script>
</asp:Content>
