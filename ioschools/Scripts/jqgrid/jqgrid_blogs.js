function reloadBlogsGrid() {
    var type = $('select', '#typeList').val();
    var year = $('select', '#yearList').val();

    var url = '/blog/list?type=' + type + '&year=' + year;
    $("#blogsGridView").setGridParam({ url: url });
    $("#blogsGridView").trigger("reloadGrid");
}

function blogsBindToGrid() {

    $("#blogsGridView").jqGrid({
        afterInsertRow: function (id, data, element) {

        },
        altRows: true,
        autowidth: true,
        cellEdit: false,
        cellSubmit: 'remote',
        colNames: ['', 'Title', 'Status', ''],
        colModel: [
                    { name: 'pid', hidden: true },
                    { name: 'title', index: 'title', width: 470, align: 'left', sortable: false },
                    { name: 'status', index: 'status', width: 130, align: 'left', sortable: false },
                  { name: 'act', width: 70, align: 'left',  sortable: false }
                  ],
        datatype: 'json',
        height: '100%',
        hoverrows: false,
        imgpath: '/Content/images',
        loadComplete: function () {
            // bind edit button
            $('.jqedit', "#blogsGridView").click(function () {
                var id = $(this).parents('tr').find('td:first').text();
                window.location = '/blog/edit/' + id;
                return false;
            });

            // bind delete button
            $('.jqdelete', "#blogsGridView").click(function () {
                var ok = window.confirm('Are you sure? There is NO UNDO.');
                if (!ok) {
                    return false;
                }
                var id = $(this).parents('tr').find('td:first').text();
                $(this).post('/blog/delete/' + id, null, function (json_result) {
                    if (json_result.success) {
                        reloadBlogsGrid();
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
        pager: $('#blogsGridNavigation'),
        rowNum: 50,
        rowList: [10, 50, 100],
        rownumbers: false,
        shrinkToFit: true,
        sortname: 'date',
        sortorder: 'asc',
        url: '/blog/list',
        viewrecords: true,
        viewsortcols: true
    }).navGrid('#blogsGridNavigation', { search: false, refresh: false, edit: false, add: false, del: false });
}
