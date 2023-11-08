let addBtn = document.querySelector("#uploadMain .addBtn");
let tagsinputField = document.querySelector("#uploadMain .tagsInputField");
let tagHolder = document.querySelector("#uploadMain .tagHolder");
let tagsAndBtn = document.querySelector("#uploadMain .tagsAndBtn");
let inputTags = document.querySelectorAll("#uploadMain .singleTag");
let theTags = document.querySelectorAll("#uploadMain .theTag");
let inputIcon = document.querySelector("#uploadMain .inputIcon");
if (inputTags.length >= 3) {
  addBtn.setAttribute("disabled", "");
}

function inputCreator(tagToPass) {
  return `<input class="singleTag" type="text" hidden value="${tagToPass
    .toLowerCase()
    .trim()}"/>`;
}

function tagCreator(tagToPass) {
  return ` <div class="theTag">
  ${tagToPass.toLowerCase().trim()}
    <img class="close" id=${tagToPass
      .toLowerCase()
      .trim()
      .replaceAll(" ", "#")} src="./assets/images/icons/plus.svg" alt="" />
  </div>`;
}

tagsinputField.addEventListener("input", () => {
  inputIcon.textContent = `${tagsinputField.value.length}/25`;
});

addBtn.addEventListener("click", (e) => {
  let tagAlreadyExists = false;
  if (tagsinputField.value.length > 1 && tagsinputField.value.length <= 25) {
    if (inputTags.length > 0) {
      inputTags.forEach((tags) => {
        if (tags.value == tagsinputField.value) {
          tagAlreadyExists = true;
        }
      });
      if (!tagAlreadyExists) {
        tagHolder.innerHTML += inputCreator(tagsinputField.value);
        tagHolder.innerHTML += tagCreator(tagsinputField.value);
      }
    } else if (inputTags.length == 0) {
      tagHolder.innerHTML += inputCreator(tagsinputField.value);
      tagHolder.innerHTML += tagCreator(tagsinputField.value);
    }
    tagsinputField.value = "";
    inputTags = document.querySelectorAll("#uploadMain .singleTag");
    theTags = document.querySelectorAll("#uploadMain .theTag");
    if (inputTags.length >= 3) {
      addBtn.setAttribute("hidden", "");
    }
  }
  let tagClose = document.querySelectorAll("#uploadMain .theTag .close");
  tagClose.forEach((tc) => {
    tc.addEventListener("click", () => {
      inputTags.forEach((it) => {
        theTags.forEach((tt) => {
          if (
            it.value == tc.id.replaceAll("#", " ") &&
            tt.textContent.trim() == tc.id.replaceAll("#", " ")
          ) {
            it.remove();
            tt.remove();
            inputTags = document.querySelectorAll("#uploadMain .singleTag");
            if (inputTags.length < 3) {
              addBtn.removeAttribute("hidden");
            }
          }
        });
      });
    });
  });
  if (inputTags.length >= 3) {
    addBtn.setAttribute("hidden", "");
  }
  inputIcon.textContent = `${tagsinputField.value.length}/25`;
});

let descs = document.querySelectorAll("#uploadMain .desc");
let fileButtons = document.querySelectorAll("#uploadMain .fileUpload");
let imgCover = document.querySelector("#uploadMain .coverImage");
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
      filebtn.value = "";
    }
  });
});

let numberInputs = document.querySelectorAll(".numberInput");
numberInputs.forEach((numberInput) => {
  numberInput.addEventListener("input", function () {
    let inputValue = numberInput.value.replace(/\D/g, "");
    if (inputValue > 9999) {
      numberInput.value = inputValue.substring(0, 4);
    } else {
      numberInput.value = inputValue;
    }
  });
});

