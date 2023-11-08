const addToPlaylist = document.querySelector(".addToPlaylist");
const dialog = document.getElementById("modal");
const closeModal = document.querySelector(".closeModal");

addToPlaylist.addEventListener("click", () => {
  dialog.showModal();
});

closeModal.addEventListener("click", () => {
  dialog.close();
});

let addBtn = document.querySelectorAll("dialog .editBtn");
const awaiting = document.querySelectorAll("dialog .awaiting");
const done = document.querySelectorAll(".done");

addBtn.forEach((a) => {
  a.addEventListener("click", (e) => {
    e.preventDefault();
    a.classList.add("active");
  });
});
