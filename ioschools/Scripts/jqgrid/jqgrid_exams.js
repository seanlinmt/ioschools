function reloadExamsGrid() {
    var school = $('select', '#schoolList').val();
    var form = $('select', '#formList').val();
    var year = $('#year', '#yearList').val();

    if (year == '' || isNaN(year)) {
        year = new Date().getFullYear();
    }

    var url = '/exams/list?school=' + school + "&form=" + form + '&year=' + year;
    $("#examsGridView").setGridParam({ url: url, datatype: 'json' });
    $("#examsGridView").trigger("reloadGrid");
}

function examsBindToGrid() {
    $("#examsGridView").jqGrid({
        afterInsertRow: function (id, data, element) {

        },
        altRows: true,
        autowidth: true,
        cellEdit: false,
        cellSubmit: 'remote',
        colNames: ['','ID', 'Exam/Test', 'Details', ''],
        colModel: [
                    { name: 'pid', hidden: true, sortable: false },
                    { name: 'id', index: 'id', width: 40, align: 'center', sortable: false },
                    { name: 'name', index: 'name', width: 290, align: 'left', sortable: false },
                    { name: 'classes', index: 'classes', width: 150, align: 'left', sortable: false },
                  { name: 'act', width: 50, align: 'left', sortable: false }
                  ],
        datatype: 'local',
        height: '100%',
        hoverrows: false,
        imgpath: '/Content/images',
        loadComplete: function () {
            // bind edit button
            $('.jqdelete', "#examsGridView").click(function () {
                var ok = window.confirm('Are you sure? There is NO UNDO.');
                if (!ok) {
                    return false;
                }
                var id = $(this).parents('tr').find('td:first').text();
                $(this).post('/exams/delete/' + id, null, function (json_result) {
                    if (json_result.success) {
                        reloadExamsGrid();
                    }
                    $.jGrowl(json_result.message);
                }, 'json');
                return false;
            });

            $('#jqgh_id').addClass('ac');
        },
        mtype: 'POST',
        onPaging: function () {
            $.scrollTo("#container", 800);
        },
        onSelectRow: function (id) {

        },
        onSortCol: function (index, colIndex, sortOrder) {

        },
        pager: $('#examsGridNavigation'),
        rowNum: 50,
        rowList: [10, 50, 100],
        rownumbers: false,
        shrinkToFit: true,
        sortname: 'date',
        sortorder: 'asc',
        viewrecords: true,
        viewsortcols: true
    }).navGrid('#examsGridNavigation', { search: false, refresh: false, edit: false, add: false, del: false });
}
