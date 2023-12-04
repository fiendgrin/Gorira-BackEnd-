let plays = document.querySelectorAll("#myTracksMain .singleTrack .play");
let pauses = document.querySelectorAll("#myTracksMain .singleTrack .pause");
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
      ap = new APlayer({
        container: document.querySelector("#myTracksMain #aplayer"),
        autoplay: false,
        loop: "all",
        preload: "auto",
        volume: 0.7,
        mutex: true,
        listFolded: true,
        fixed: true,
        theme: "#a210f7",
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
        .querySelector("#myTracksMain #aplayer")
        .classList.remove("aplayer-narrow");
      document
        .querySelector("#myTracksMain #aplayer")
        .classList.remove("aplayer-arrow");
      document.querySelector(".aplayer-miniswitcher").remove();
      let next = document.querySelector("#aplayer .aplayer-icon-forward");
      let prev = document.querySelector("#aplayer .aplayer-icon-back");
      document.querySelector("#aplayer .aplayer-icon-play").remove();
      document.querySelector("#aplayer .aplayer-icon-lrc").remove();
      document.querySelector(
        "#aplayer .aplayer-icon-volume-down"
      ).style.display = "inline-block";
      document.querySelector("#aplayer .aplayer-volume-bar").style.zIndex =
        "10";
      document.querySelector("#aplayer .aplayer-volume-wrap").style.height =
        "15px";
      document.querySelector("#aplayer .aplayer-volume-wrap").style.width =
        "15px";
      next.remove();
      prev.remove();
      document.querySelector("#aplayer .aplayer-music").style.width = "100%";
      ap.play();
    }

 
  });
});