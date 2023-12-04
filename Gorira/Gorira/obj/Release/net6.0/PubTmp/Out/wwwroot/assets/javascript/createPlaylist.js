let filebtn = document.querySelector("#createPlaylistMain .fileUpload");
let imgCover = document.querySelector("#createPlaylistMain .coverImage");

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
