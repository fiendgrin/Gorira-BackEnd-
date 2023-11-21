let deleteTrack = document.querySelectorAll(".deleteTrack");
let dialogs = document.querySelectorAll(".modal");
let closeModals = document.querySelectorAll(".closeModal");

deleteTrack.forEach(dt => {
    dt.addEventListener("click", () => {

        dialogs.forEach(dialog => {
            if (dt.getAttribute("trackId") == dialog.getAttribute("trackId")) {
                dialog.showModal();
            }
        })
    });

})

dialogs.forEach(dialog => {
    dialog.addEventListener("click", e => {
        const dialogDimensions = dialog.getBoundingClientRect()
        if (
            e.clientX < dialogDimensions.left ||
            e.clientX > dialogDimensions.right ||
            e.clientY < dialogDimensions.top ||
            e.clientY > dialogDimensions.bottom
        ) {
            dialog.close()
        }
    })

})

closeModals.forEach(closeModal => {
    closeModal.addEventListener("click", () => {
        dialogs.forEach(dialog => {
        if (closeModal.getAttribute("trackId") == dialog.getAttribute("trackId")) {
            dialog.close();
        }
        })
    })

})
