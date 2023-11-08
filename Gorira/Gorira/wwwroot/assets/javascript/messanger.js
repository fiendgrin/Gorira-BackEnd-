let meaasagingBox = document.querySelector(".meaasagingBox");

meaasagingBox.scrollTop =
  meaasagingBox.scrollHeight - meaasagingBox.clientHeight;

let left = document.querySelector("#messangerMain .left");
let people = document.querySelector("#messangerMain .people");

people.addEventListener("click", (e) => {
  e.stopPropagation();
  if (left.style.display != "flex") {
    left.style.display = "flex";
  } else {
    left.style.display = "none";
  }
});

left.addEventListener("click", (e) => {
  e.stopPropagation();
});

window.addEventListener("click", () => {
  left.style.display = "none";
});

window.addEventListener("resize", () => {
  if (window.innerWidth > 768) {
    left.style.display = "flex";
  } else {
    left.style.display = "none";
  }
});
