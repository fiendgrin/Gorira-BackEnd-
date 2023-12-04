let filterBtn = document.querySelector("#musiciansMain .filterBtn");
let filterHolder = document.querySelector("#musiciansMain .filterHolder");

filterBtn.addEventListener("click", (e) => {
  e.stopPropagation();
  if (filterHolder.style.display != "flex") {
    filterHolder.style.display = "flex";
  } else {
    filterHolder.style.display = "none";
  }
});

window.addEventListener("click", () => {
  filterHolder.style.display = "none";
});
filterHolder.addEventListener("click", (e) => {
  e.stopPropagation();
});
