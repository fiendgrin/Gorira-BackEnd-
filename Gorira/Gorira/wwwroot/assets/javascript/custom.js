$(document).ready(function () {
    $('.trackPlayBtn').click(async function  (e) {
        e.preventDefault();
        let url = "/Track/PlayCounter/" + $(this).attr("trackId");
        let  response = await fetch(url);
        let data = await response.text();
        console.log(1);
        return;
    })
})