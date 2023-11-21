let numberElements = document.querySelectorAll("#profileMain .numberElement");

numberElements.forEach((numberElement) => {
    let number = parseInt(numberElement.textContent);

    let formattedNumber = number.toLocaleString();

    numberElement.textContent = formattedNumber;
});

let reportContent = document.querySelector("#profileMain .reportContent");
let reportHolder = document.querySelector("#profileMain .reportHolder");

reportHolder.addEventListener("click", () => {
    if (reportContent.style.display != "flex") {
        reportContent.style.display = "flex";
    } else {
        reportContent.style.display = "none";
    }
});

let followBtn = document.querySelector("#profileMain .follow");
if (followBtn != null) {

    followBtn.addEventListener("click", (e) => {
        e.preventDefault();
        let url = followBtn.getAttribute('href');
        fetch(url)

        followBtn.classList.toggle("followActive");
    });
}

let infoTab = document.querySelector("#profileMain .info");
let tracksTab = document.querySelector("#profileMain .tracks");
let profileTracks = document.querySelector("#profileMain .right");
let profile = document.querySelector("#profileMain .left");

infoTab.addEventListener("click", () => {
    infoTab.classList.add("tabActive");
    tracksTab.classList.remove("tabActive");
    profile.style.display = "flex";
    profileTracks.style.display = "none";
});
tracksTab.addEventListener("click", () => {
    tracksTab.classList.add("tabActive");
    infoTab.classList.remove("tabActive");
    profileTracks.style.display = "flex";
    profile.style.display = "none";
});

window.addEventListener("resize", () => {
    if (window.innerWidth >= 1360) {
        profile.style.display = "flex";
        profileTracks.style.display = "flex";
    } else if (
        window.innerWidth < 1360 &&
        profile.style.display != "none" &&
        profileTracks.style.display != "none"
    ) {
        infoTab.classList.add("tabActive");
        tracksTab.classList.remove("tabActive");
        profile.style.display = "flex";
        profileTracks.style.display = "none";
    }
});


if (window.innerWidth < 1360) {
    infoTab.classList.remove("tabActive");
    tracksTab.classList.add("tabActive");
    profile.style.display = "none";
    profileTracks.style.display = "flex";
}


let plays = document.querySelectorAll("#profileMain .myCard .play");
let pauses = document.querySelectorAll("#profileMain .myCard .pause");
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
                container: document.querySelector("#profileMain #aplayer"),
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
                .querySelector("#profileMain #aplayer")
                .classList.remove("aplayer-narrow");
            document
                .querySelector("#profileMain #aplayer")
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
