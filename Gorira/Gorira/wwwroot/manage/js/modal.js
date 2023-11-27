$(document).ready(function () {

    $(document).on('click', '.setActiveBtn', function (e) {
        e.preventDefault();

        Swal.fire({
            title: 'Are you sure?',
            text: "You won't be able to revert this!",
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#3085d6',
            cancelButtonColor: '#d33',
            confirmButtonText: 'Yes, Change Account Activity Status!',
            customClass: {
                popup: 'myCustomModal'
            }
        }).then((result) => {
            if (result.isConfirmed) {
                let url = $(this).attr('href');
                changeAccountStatus(url);
            }
        });
    });

    function changeAccountStatus(url) {
        fetch(url)
            .then(response => response.text())
            .then(data => {
                Swal.fire({
                    title: 'Changed!',
                    text: 'Your Account Activity Status has been updated.',
                    icon: 'success',
                    customClass: {
                        popup: 'myCustomModal'
                    }
                }).then(() => {
                    location.reload();
                });
            })
    }


    $(document).on('click', '.resetPasswordBtn', function (e) {
        e.preventDefault();

        Swal.fire({
            title: 'Are you sure?',
            text: "You won't be able to revert this!",
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#3085d6',
            cancelButtonColor: '#d33',
            confirmButtonText: 'Yes, reset it!',
            customClass: {
                popup: 'myCustomModal'
            }
        }).then((result) => {
            if (result.isConfirmed) {
                let url = $(this).attr('href');
                resetPassword(url);
            }
        });
    });

    function resetPassword(url) {
        fetch(url)
            .then(res => res.text())
            .then((data) => {
                Swal.fire({
                    title: 'Reset!',
                    text: 'Your Password has been Reset.',
                    icon: 'success',
                    customClass: {
                        popup: 'myCustomModal'
                    }
                })
                    .then(() => {
                        location.reload()
                    });
            })
    }

    $('.roleSelector').removeAttr('multiple');

});