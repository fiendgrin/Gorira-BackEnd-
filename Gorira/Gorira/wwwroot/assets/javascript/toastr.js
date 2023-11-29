$(document).ready(function () {


    toastr.options = {
        "closeButton": true,
        "debug": false,
        "newestOnTop": true,
        "progressBar": true,
        "positionClass": "toast-top-left",
        "preventDuplicates": false,
        "showDuration": "300",
        "hideDuration": "1000",
        "timeOut": "10000",
        "extendedTimeOut": "2000",
        "showEasing": "swing",
        "hideEasing": "linear",
        "showMethod": "fadeIn",
        "hideMethod": "fadeOut"
    }



    let infoToaster = $('#infoToaster');
    if (infoToaster.val() != undefined && infoToaster.val().length > 0) {
        toastr["info"](infoToaster.val());
    }

    let warningToaster = $('#warningToaster');
    if (warningToaster.val() != undefined && warningToaster.val().length > 0) {
        toastr["warning"](warningToaster.val());
    }

    let successToaster = $('#successToaster');
    if (successToaster.val() != undefined && successToaster.val().length > 0) {
        toastr["success"](successToaster.val());
    }


})