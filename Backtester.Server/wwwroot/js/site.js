// Write your Javascript code.
function showBootstrapAlert(level, subject) {
    $('#alertholder').after(
        '<div class="alert alert-' + level + ' alert-dismissable">' +
        '<button type="button" class="close" data-dismiss="alert" aria-label="Close">' +
        '<span aria-hidden="true">&times;</span>' +
        '</button>' +
        subject +
        '</div>');
}

function exportToExcel(jsonJobs) {
    $.ajax({
        type: 'POST',
        url: '/JobGroups/ExportListToExcel',
        data: JSON.stringify(jsonJobs),
        dataType: 'json',
        contentType: 'application/json; charset=utf-8',
        success: function (data) {
            console.log(data);

            //get the file name for download
            if (data.fileName) {
                //use window.location.href for redirect to download action for download the file
                window.location.href = '/JobGroups/DownloadExcelList/?fileName=' + data.fileName;
            }
        }
    });
}
