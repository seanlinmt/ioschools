function reloadUsersGrid() {
    // !!!!!! changing the following requires changing UserSearch.cs
    var search = function (school, sclass, attStatus, date, group, discipline, status, term, year, seca, hasIssues) {
        this.school = school;
        this.sclass = sclass;
        this.attStatus = attStatus;
        this.date = date;
        this.group = group;
        this.discipline = discipline;
        this.status = status;
        this.term = term;
        this.year = year;
        this.seca = seca;
        this.hasIssues = hasIssues;
    };
    var school = $('select', '#schoolList').val();
    var form = $('select', '#formList').val();
    var group = $('#groupList').val();
    var discipline = $('#disciplineList').val();
    var term = $('#searchbox').val();
    var attendanceDate = $('#attendanceDate').val();
    var attendanceStatus = $('#attendanceStatus').val();
    var active = $('select', '#activeList').val();
    var year = $('#yearList').val();
    var eca = $('select', '#ecaList').val();
    var hasIssue = $('#hasIssues').is(':checked');

    // the following needs to wait for callback so we check if values are available in the cookie
    var cookiestring = $.cookie('search');
    if ((form == null || eca == null) &&
        cookiestring != '') {
        var searchobj = JSON.parse(cookiestring);
        if (searchobj != null) {
            if (form == null) {
                form = searchobj.sclass;
            }
            if (eca == null) {
                eca = searchobj.seca;
            }
        }
    }

    if (group == undefined || group == "undefined") {
        group = null;
    }
    if (discipline == undefined || discipline == "undefined") {
        discipline = "";
    }

    if (attendanceDate == undefined || attendanceDate == "undefined") {
        attendanceDate = null;
    }

    var current = new search(school, form, attendanceStatus, attendanceDate, group, discipline, active, term, year, eca, hasIssue);

    var jsonstring = JSON.stringify(current);

    $.cookie('search', jsonstring);

    var url = '/users/list?school=' + school + "&form=" + form + '&group=' + group + '&term=' + term +
                '&discipline=' + discipline + '&attendanceDate=' + attendanceDate + '&active=' + active +
                '&year=' + year + '&eca=' + eca + '&attendanceStatus=' + attendanceStatus + '&hasIssues=' + hasIssue;
    $("#usersGridView").setGridParam({ url: url, datatype: 'json' });
    $("#usersGridView").trigger("reloadGrid");
};

function usersBindToGrid() {

    $("#usersGridView").jqGrid({
        afterInsertRow: function (id, data, element) {

        },
        altRows: true,
        autowidth: true,
        cellEdit: false,
        cellSubmit: 'remote',
        colNames: ['', '', 'Name', 'School',  'Contact Information', 'Status',''],
        colModel: [
                    { name: 'pid', hidden: true },
                    { name: 'thumbnail', classes: 'jqgrid_thumb', index: 'thumbnail', width: 70, align: 'center', sortable: false },
                    { name: 'name', index: 'name', width: 180, align: 'left', sortable: true },
                    { name: 'school', index: 'school', width: 80, align: 'left', sortable: false },
                    { name: 'contact', index: 'contact', width: 200, align: 'left', sortable: false },
                    { name: 'status', index: 'status', width: 110, align: 'left', sortable: false },
                    { name: 'act', width: 130, align: 'right', sortable: false }
                  ],
        datatype: 'local',
        height: '100%',
        hoverrows: false,
        imgpath: '/Content/images',
        loadComplete: function () {
            $('.jqatt', "#usersGridView").click(function () {
                var id = $(this).parents('tr').find('td:first').text();
                dialogBox_open('/attendance/add/' + id, 'Student Attendance', 600);
                return false;
            });

            $('.jqdelete', "#usersGridView").click(function () {
                var ok = window.confirm('Are you sure? This will delete all other information belonging to this user. There is NO UNDO.');
                if (!ok) {
                    return false;
                }
                var id = $(this).parents('tr').find('td:first').text();
                $(this).post('/users/delete/' + id, null, function (json_result) {
                    if (json_result.success) {
                        reloadUsersGrid();
                    }
                    $.jGrowl(json_result.message);
                }, 'json');
                return false;
            });
        },
        mtype: 'POST',
        onPaging: function () {
            $.scrollTo("#container", 800);
        },
        onSelectRow: function (id) {

        },
        onSortCol: function (index, colIndex, sortOrder) {
            $('#gview_usersGridView')
                .find(".ui-jqgrid-htable tr > th")
                .removeClass('jqgrid_sorted');
            $('#gview_usersGridView')
                .find(".ui-jqgrid-htable tr > th:eq(" + colIndex + ")")
                .addClass('jqgrid_sorted');
        },
        pager: $('#usersGridNavigation'),
        rowNum: 50,
        rowList: [10, 50, 100],
        rownumbers: false,
        shrinkToFit: true,
        sortname: 'id',
        sortorder: 'desc',
        viewrecords: true,
        viewsortcols: true
    }).navGrid('#usersGridNavigation', { search: false, refresh: false, edit: false, add: false, del: false });
};
