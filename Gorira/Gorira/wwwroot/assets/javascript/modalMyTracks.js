const deleteTrack = document.querySelectorAll(".deleteTrack");
const dialog = document.getElementById("modal");
const closeModal = document.querySelector(".closeModal");

deleteTrack.forEach(dt => {
    dt.addEventListener("click", () => {
            dialog.showModal();
        });

})

closeModal.addEventListener("click", () => {
    dialog.close();
});



