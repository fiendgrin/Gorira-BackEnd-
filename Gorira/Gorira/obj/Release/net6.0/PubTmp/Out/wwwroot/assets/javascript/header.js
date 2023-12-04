let chevronAcc = document.querySelector(".chevronAcc");
let accountContent = document.querySelector(".accountContent");
let accountHolder = document.querySelector(".accountHolder");

let chevronBasket = document.querySelector(".chevronBasket");
let basketContent = document.querySelector(".basketContent");
let BasketTopIcons = document.querySelector(".BasketTopIcons");

let burger = document.querySelector(".burger");
let navBar = document.querySelector(".navBar");
let removeBaskets = document.querySelectorAll(".removeBasket");



accountHolder.addEventListener("click", (e) => {
  e.stopPropagation();
  chevronBasket.style.transform = "";
  basketContent.classList.remove("active");
  if (window.innerWidth <= 768) {
    navBar.style.display = "none";
  }
  if (chevronAcc.style.transform == "rotate(-180deg)") {
    chevronAcc.style.transform = "";
    accountContent.classList.remove("active");
  } else {
    chevronAcc.style.transform = "rotate(-180deg)";
    accountContent.classList.add("active");
  }
});



burger.addEventListener("click", (e) => {
  e.stopPropagation();
  chevronAcc.style.transform = "";
  accountContent.classList.remove("active");
  chevronBasket.style.transform = "";
  basketContent.classList.remove("active");
  if (navBar.style.display == "flex") {
    navBar.style.display = "none";
  } else {
    navBar.style.display = "flex";
  }
});

window.addEventListener("click", () => {
  accountContent.classList.remove("active");
  basketContent.classList.remove("active");
  chevronBasket.style.transform = "";
  chevronAcc.style.transform = "";
  if (window.innerWidth <= 768) {
    navBar.style.display = "none";
  }
});

window.addEventListener("resize", () => {
  if (window.innerWidth <= 768 && navBar.style.display != "none") {
    navBar.style.display = "none";
  }
  if (window.innerWidth > 768 && navBar.style.display == "none") {
    navBar.style.display = "flex";
  }
});

if (window.innerWidth <= 768) {
    if (navBar.style.display !== "none") {
        navBar.style.display = "none";
    }
}
