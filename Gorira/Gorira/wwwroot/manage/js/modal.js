$(document).ready(function () {

    $(document).on('click', '.setActiveBtn', function (e) {
        e.preventDefault();

        Swal.fire({
            title: 'Are you sure?',
            text: "Account's activity status will be changed!",
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#3085d6',
            cancelButtonColor: '#d33',
            confirmButtonText: 'Yes, change account activity status!',
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
                    text: 'Account activity status has been updated.',
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
                    text: 'Password has been reset.',
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