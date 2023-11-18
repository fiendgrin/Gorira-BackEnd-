$(document).ready(function () {

    $('.trackPlayBtn').click(async function  (e) {
        e.preventDefault();
        let url = "/Track/PlayCounter/" + $(this).attr("trackId");
        let  response = await fetch(url);
        let data = await response.text();
        return;
    })

    $(document).on('click', '.addToCart', async function (e) {
        e.preventDefault();
        let url = "";
        let id = $(this).attr("trackId")
        console.log(id)
        let checkedValue = $('input[name=radio]:checked').attr('id');
        console.log(checkedValue);
        if (checkedValue == "limited") {
            url = "/Basket/AddBasket/" + id + "?isUnlimited=false";
        } else
        {
            url = "/Basket/AddBasket/" + id + "?isUnlimited=true";
        }

        let response = await fetch(url);
        let data = await response.text();
        $('.basketHolder').html(data);

       chevronBasket = document.querySelector(".chevronBasket");
        basketContent = document.querySelector(".basketContent");
        basketHolder = document.querySelector(".basketHolder");
    });

})