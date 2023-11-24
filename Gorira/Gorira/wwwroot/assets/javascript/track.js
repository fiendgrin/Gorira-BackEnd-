const ap = new APlayer({
    container: document.querySelector("#trackMain #aplayer"),
    theme: "#a210f7",
    audio: [
        {
            name: document.querySelector("#trackMain #aplayer").getAttribute("title"),
            artist: document.querySelector("#trackMain #aplayer").getAttribute("name"),
            url: document.querySelector("#trackMain #aplayer").getAttribute("track"),
            cover: document.querySelector("#trackMain #aplayer").getAttribute("cover"),
        },
    ],
});

let playAplayer = document.querySelector("#trackMain #aplayer .aplayer-play");
let trackId = document.querySelector("#trackMain #aplayer").getAttribute("trackId");
playAplayer.classList.add("trackPlayBtn");
playAplayer.setAttribute("trackId", trackId)


let thePrice = document.querySelector("#trackMain .thePrice");
let radios = document.querySelectorAll("#trackMain .licenses .radio");

radios.forEach((r) => {
    if (r.firstElementChild.checked) {
        thePrice.textContent = r.getAttribute("price");
    }
    r.addEventListener("click", () => {
        if (r.firstElementChild.checked) {
            thePrice.textContent = r.getAttribute("price");
        }
    });
});

let myInfoTab = document.querySelector("#trackMain .info");
let myTracksTab = document.querySelector("#trackMain .tracks");
let myProfileTracks = document.querySelector("#trackMain .right");
let myProfile = document.querySelector("#trackMain .left");

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
        myProfile.style.display == "flex" &&
        myProfileTracks.style.display == "flex"
    ) {
        myInfoTab.classList.add("tabActive");
        myTracksTab.classList.remove("tabActive");
        myProfile.style.display = "flex";
        myProfileTracks.style.display = "none";
    }
});

let myNumberElements = document.querySelectorAll("#trackMain .numberElement");

myNumberElements.forEach((numberElement) => {
    let number = parseInt(numberElement.textContent);

    let formattedNumber = number.toLocaleString();

    numberElement.textContent = formattedNumber;
});

if (window.innerWidth >= 1360) {
    myProfile.style.display = "flex";
    myProfileTracks.style.display = "flex";
} else if (window.innerWidth < 1360) {
    myInfoTab.classList.add("tabActive");
    myTracksTab.classList.remove("tabActive");
    myProfile.style.display = "flex";
    myProfileTracks.style.display = "none";
}

let PostComment = document.querySelector("#trackMain .postComment");

PostComment.addEventListener("submit", (e) => {

    let commentText = document.querySelector("#trackMain .commentText");
    e.preventDefault();
    let url = PostComment.getAttribute('action'); 
    let textValue = PostComment.querySelector('input[name="text"]').value;

    let urlParts = url.split('/');
    let id = urlParts[urlParts.length - 1];
    fetch(url, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
            'X-Requested-With': 'XMLHttpRequest',
            'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
        },
        body: `Id=${id}&text=${encodeURIComponent(textValue)}`
    })
        .then(res => res.text())
        .then(data => { document.querySelector("#trackMain .theComments").innerHTML = data })

    commentText.value = "";

});