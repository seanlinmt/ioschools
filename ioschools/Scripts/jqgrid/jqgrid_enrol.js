function reloadEnrolGrid() {
    var status = $('select', '#statusList').val();
    var school = $('select', '#schoolList').val();
    var term = $('#searchbox').val();
    var classyear = $('#classyear').val();
    var year = $('#yearList').val();
    var url = '/enrolment/list?school=' + school + '&status=' + status + '&year=' + year + '&classyear=' + classyear + '&term=' + term;
    $("#enrolGridView").setGridParam({ url: url });
    $("#enrolGridView").trigger("reloadGrid");
}

function enrolBindToGrid() {

    $("#enrolGridView").jqGrid({
        afterInsertRow: function (id, data, element) {

        },
        altRows: true,
        autowidth: true,
        cellEdit: false,
        cellSubmit: 'remote',
        colNames: ['', '', '', 'Date Created', 'Enrolling For', 'Year', 'Status', ''],
        colModel: [
                    { name: 'pid', hidden: true },
                    { name: 'thumbnail', classes: 'jqgrid_thumb', index: 'thumbnail', width: 50, align: 'center', sortable: false },
                    { name: 'name', index: 'name', width: 180, align: 'left', sortable: false },
                    { name: 'date', classes: 'date', index: 'date', width: 100, align: 'left', sortable: false },
                    { name: 'school', index: 'school', width: 80, align: 'left', sortable: false },
                    { name: 'year', index: 'year', width: 50, align: 'left', sortable: false },
                    { name: 'status', index: 'status', width: 100, align: 'left', sortable: false },
                    { name: 'act', width: 100, align: 'left', sortable: false }
                  ],
        datatype: 'json',
        height: '100%',
        hoverrows: false,
        imgpath: '/Content/images',
        loadComplete: function () {
            $('.jqdelete', "#enrolGridView").click(function () {
                var ok = window.confirm('Are you sure? This will delete the current enrolment entry. There is NO UNDO.');
                if (!ok) {
                    return false;
                }
                var id = $(this).parents('tr').find('td:first').text();
                $(this).post('/enrolment/delete/' + id, null, function (json_result) {
                    if (json_result.success) {
                        reloadEnrolGrid();
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

        },
        pager: $('#enrolGridNavigation'),
        rowNum: 50,
        rowList: [10, 50, 100],
        rownumbers: false,
        shrinkToFit: true,
        sortname: 'date',
        sortorder: 'asc',
        url: '/enrolment/list?status=pending',
        viewrecords: true,
        viewsortcols: true
    }).navGrid('#enrolGridNavigation', { search: false, refresh: false, edit: false, add: false, del: false });
}
