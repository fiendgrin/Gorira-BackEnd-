let myInfoTab = document.querySelector("#myProfileMain .info");
let myTracksTab = document.querySelector("#myProfileMain .tracks");
let myProfileTracks = document.querySelector("#myProfileMain .right");
let myProfile = document.querySelector("#myProfileMain .left");

myInfoTab.addEventListener("click", () => {
  myInfoTab.classList.add("tabActive");
  myTracksTab.classList.remove("tabActive");
  myProfile.style.display = "flex";
  myProfileTracks.style.display = "none";
});
myTracksTab.addEventListener("click", () => {
  myTracksTab.classList.add("tabActive");
  myInfoTab.classList.remove("tabActive");
  myProfileTracks.style.display = "flex";
  myProfile.style.display = "none";
});

window.addEventListener("resize", () => {
  if (window.innerWidth >= 1360) {
    myProfile.style.display = "flex";
    myProfileTracks.style.display = "flex";
  } else if (
    window.innerWidth < 1360 &&
    myProfile.style.display != "none" &&
    myProfileTracks.style.display != "none"
  ) {
    myInfoTab.classList.add("tabActive");
    myTracksTab.classList.remove("tabActive");
    myProfile.style.display = "flex";
    myProfileTracks.style.display = "none";
  }
});


if (window.innerWidth < 1360) {
  myInfoTab.classList.add("tabActive");
  myTracksTab.classList.remove("tabActive");
  myProfile.style.display = "flex";
  myProfileTracks.style.display = "none";
}


let myNumberElements = document.querySelectorAll("#myProfileMain .numberElement");

myNumberElements.forEach((numberElement) => {
  let number = parseInt(numberElement.textContent);

  let formattedNumber = number.toLocaleString();

  numberElement.textContent = formattedNumber;
});

let plays = document.querySelectorAll("#myProfileMain .myCard .play");
let pauses = document.querySelectorAll("#myProfileMain .myCard .pause");
let ap = null;

plays.forEach((play) => {
  play.addEventListener("click", () => {
    //pause or new handle
    if (
      ap != null &&
      ap.audio.getAttribute("src") == play.getAttribute("track")
    ) {
      ap.play();
    } else {
      pauses.forEach((pause) => {
        pause.style.display = "none";
        pause.previousElementSibling.style.display = "inline-block";
      });
      ap = new APlayer({
        container: document.querySelector("#myProfileMain #aplayer"),
        autoplay: false,
        loop: "all",
        preload: "auto",
        volume: 0.7,
        mutex: true,
        listFolded: true,
        fixed: true,
        theme:"#a210f7",
        audio: [
          {
            name: play.getAttribute("name"),
            artist: play.getAttribute("artist"),
            url: play.getAttribute("track"),
            cover: play.getAttribute("cover"),
          },
        ],
      });
      document
        .querySelector("#myProfileMain #aplayer")
        .classList.remove("aplayer-narrow");
      document
        .querySelector("#myProfileMain #aplayer")
        .classList.remove("aplayer-arrow");
      document.querySelector(".aplayer-miniswitcher").remove();
      let next = document.querySelector("#aplayer .aplayer-icon-forward");
      let prev = document.querySelector("#aplayer .aplayer-icon-back");
      document.querySelector("#aplayer .aplayer-icon-play").remove();
      next.remove();
      prev.remove();
      document.querySelector("#aplayer .aplayer-music").style.width = "100%";
      ap.play();
    }

    // pause play btn display handel
    play.style.display = "none";
    play.nextElementSibling.style.display = "inline-block";
    ap.on("pause", function () {
      if (ap.audio.paused) {
        play.style.display = "inline-block";
        play.nextElementSibling.style.display = "none";
      }
    });
    ap.on("play", function () {
      if (!ap.audio.paused) {
        play.style.display = "none";
        play.nextElementSibling.style.display = "inline-block";
      }
    });
  });
});

pauses.forEach((pause) => {
  pause.addEventListener("click", () => {
    ap.pause();
    pause.previousElementSibling.style.display = "inline-block";
    pause.style.display = "none";
  });
});
