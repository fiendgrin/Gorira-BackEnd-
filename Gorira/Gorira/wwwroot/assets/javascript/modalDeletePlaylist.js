let deletePlaylist = document.querySelector(".deletePlaylist");
let dialog = document.querySelector(".modal");
let closeModal = document.querySelector(".closeModal");

deletePlaylist.addEventListener("click", () => {
    dialog.showModal();

});



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



closeModal.addEventListener("click", () => {

    dialog.close();


})


