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