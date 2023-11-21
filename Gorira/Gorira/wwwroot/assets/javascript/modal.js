const addToPlaylist = document.querySelector(".addToPlaylist");
const dialog = document.getElementById("modal");
const closeModal = document.querySelector(".closeModal");

addToPlaylist.addEventListener("click", (e) => {

  dialog.showModal();
});

closeModal.addEventListener("click", (e) => {
   
  dialog.close();
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


let addBtn = document.querySelectorAll("dialog .editBtn");
const awaiting = document.querySelectorAll("dialog .awaiting");
const done = document.querySelectorAll(".done");

addBtn.forEach((a) => {
  a.addEventListener("click", (e) => {
    e.preventDefault();
      let url = a.getAttribute('href');
      fetch(url)
      a.classList.toggle("active");
    
  });
});
