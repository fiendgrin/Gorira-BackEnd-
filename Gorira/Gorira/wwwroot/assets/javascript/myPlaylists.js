let sort = document.querySelector("#myPlaylistsMain .sort");
let sortHolder = document.querySelector("#myPlaylistsMain .sortHolder");
let left = document.querySelector("#myPlaylistsMain .left");
let openFilters = document.querySelector("#myPlaylistsMain .openFilters");

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