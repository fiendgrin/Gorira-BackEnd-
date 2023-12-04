let sort = document.querySelector("#playlistsMain .sort");
let sortHolder = document.querySelector("#playlistsMain .sortHolder");
let left = document.querySelector("#playlistsMain .left");
let openFilters = document.querySelector("#playlistsMain .openFilters");

sort.addEventListener("click", (e) => {
  e.stopPropagation();
  sortHolder.classList.toggle("active");
  if (window.innerWidth <= 768) {
    left.style.display = "none";
  }
});

sortHolder.addEventListener("click", (e) => {
  e.stopPropagation();
});

window.addEventListener("click", (e) => {
  sortHolder.classList.remove("active");
});