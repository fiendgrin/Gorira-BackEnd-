//tabs
let profileTab = document.querySelector("#editProfileMain .profileTab");
let credentialsTab = document.querySelector("#editProfileMain .credentialsTab");
let socialMediaTab = document.querySelector("#editProfileMain .socialMediaTab");
//Credentials
let emailChangeForm = document.querySelector(
  "#editProfileMain .emailChangeForm"
);
let usernameChangeForm = document.querySelector(
  "#editProfileMain .usernameChangeForm"
);
let passwordChangeForm = document.querySelector(
  "#editProfileMain .passwordChangeForm"
);
let phoneChangeForm = document.querySelector(
  "#editProfileMain .phoneChangeForm"
);
//Holders
let profileHolder = document.querySelector("#editProfileMain .profileHolder");
let credentialsHolder = document.querySelector(
  "#editProfileMain .credentialsHolder"
);
let socialMediaHolder = document.querySelector(
  "#editProfileMain .socialMediaHolder"
);
//Credential Tabs
let emailBtn = document.querySelector("#editProfileMain .emailBtn");
let usernameBtn = document.querySelector("#editProfileMain .usernameBtn");
let passwordBtn = document.querySelector("#editProfileMain .passwordBtn");
let phoneBtn = document.querySelector("#editProfileMain .phoneBtn");

//========================================================================
//Tab Change
profileTab.addEventListener("click", () => {
  //tabs
  profileTab.classList.add("activeTab");
  credentialsTab.classList.remove("activeTab");
  socialMediaTab.classList.remove("activeTab");
  //holders
  profileHolder.style.display = "flex";
  credentialsHolder.style.display = "none";
  socialMediaHolder.style.display = "none";
  //Credentials
  emailChangeForm.style.display = "none";
  usernameChangeForm.style.display = "none";
  passwordChangeForm.style.display = "none";
  phoneChangeForm.style.display = "none";
});

credentialsTab.addEventListener("click", () => {
  if (
    emailChangeForm.style.display != "inline-block" &&
    usernameChangeForm.style.display != "inline-block" &&
    passwordChangeForm.style.display != "inline-block" &&
    phoneChangeForm.style.display != "inline-block"
  ) {
    //tabs
    credentialsTab.classList.add("activeTab");
    profileTab.classList.remove("activeTab");
    socialMediaTab.classList.remove("activeTab");
    //holders
    profileHolder.style.display = "none";
    credentialsHolder.style.display = "flex";
    socialMediaHolder.style.display = "none";
  }
});

socialMediaTab.addEventListener("click", () => {
  //tabs
  socialMediaTab.classList.add("activeTab");
  credentialsTab.classList.remove("activeTab");
  profileTab.classList.remove("activeTab");
  //holders
  profileHolder.style.display = "none";
  credentialsHolder.style.display = "none";
  socialMediaHolder.style.display = "flex";
  //Credentials
  emailChangeForm.style.display = "none";
  usernameChangeForm.style.display = "none";
  passwordChangeForm.style.display = "none";
  phoneChangeForm.style.display = "none";
});

//====================================================================================

let descs = document.querySelectorAll("#editProfileMain .desc");
let fileButtons = document.querySelectorAll("#editProfileMain .fileUpload");
let imgCover = document.querySelector("#editProfileMain .coverImage");
fileButtons.forEach((filebtn) => {
  filebtn.addEventListener("change", () => {
    let file = filebtn.files[0];
    if (file) {
      let fileName = file.name;
      let lastDotIndex = fileName.lastIndexOf(".");
      if (lastDotIndex !== -1) {
        let fileExtension = fileName.substring(lastDotIndex + 1);
        filebtn.previousElementSibling.textContent = `Your ${fileExtension.toUpperCase()} is acquired`;
        filebtn.previousElementSibling.style.color = "white";
        if (
          fileExtension == "png" ||
          fileExtension == "jpg" ||
          fileExtension == "jpeg"
        ) {
          const reader = new FileReader();

          reader.onload = function () {
            const image = new Image();
            image.src = reader.result;
            image.onload = function () {
              const canvas = document.createElement("canvas");
              const ctx = canvas.getContext("2d");

              const aspectRatio = 1;
              const size = Math.min(image.width, image.height);
              canvas.width = size;
              canvas.height = size;

              ctx.drawImage(
                image,
                (image.width - size) / 2,
                (image.height - size) / 2,
                size,
                size,
                0,
                0,
                size,
                size
              );

              const croppedImage = new Image();
              croppedImage.src = canvas.toDataURL("image/png");

              imgCover.innerHTML = "";
              imgCover.appendChild(croppedImage);
            };
          };

          reader.readAsDataURL(file);
        }
      }
      fileInput.value = "";
    }
  });
});

//=========================================================================
//Credentials btns
emailBtn.addEventListener("click", () => {
  credentialsHolder.style.display = "none";
  emailChangeForm.style.display = "inline-block";
});

usernameBtn.addEventListener("click", () => {
  credentialsHolder.style.display = "none";
  usernameChangeForm.style.display = "inline-block";
});

passwordBtn.addEventListener("click", () => {
  credentialsHolder.style.display = "none";
  passwordChangeForm.style.display = "inline-block";
});

phoneBtn.addEventListener("click", () => {
  credentialsHolder.style.display = "none";
  phoneChangeForm.style.display = "inline-block";
});

let goBackBtns = document.querySelectorAll("#editProfileMain .goBack");

goBackBtns.forEach((back) => {
  back.addEventListener("click", () => {
    emailChangeForm.style.display = "none";
    usernameChangeForm.style.display = "none";
    passwordChangeForm.style.display = "none";
    phoneChangeForm.style.display = "none";
    credentialsHolder.style.display = "flex";
  });
});
