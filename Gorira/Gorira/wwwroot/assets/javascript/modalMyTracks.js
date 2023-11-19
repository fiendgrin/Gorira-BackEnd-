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


closeModals.forEach(closeModal => {
    closeModal.addEventListener("click", () => {
        dialogs.forEach(dialog => {
        if (closeModal.getAttribute("trackId") == dialog.getAttribute("trackId")) {
            dialog.close();
        }
        })
    })

})
