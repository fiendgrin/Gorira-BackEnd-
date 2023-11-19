$(document).ready(function () {

    $('.trackPlayBtn').click(async function (e) {
        e.preventDefault();
        let url = "/Track/PlayCounter/" + $(this).attr("trackId");
        let response = await fetch(url);
        let data = await response.text();
        return;
    })
    var chevron = $('.chevronBasket');
    $(document).on('click', '.addToCart', async function (e) {
        e.preventDefault();
        let url = "";
        let id = $(this).attr("trackId")
        console.log(id)
        let checkedValue = $('input[name=radio]:checked').attr('id');
        console.log(checkedValue);
        if (checkedValue == "limited") {
            url = "/Basket/AddBasket/" + id + "?isUnlimited=false";
        } else {
            url = "/Basket/AddBasket/" + id + "?isUnlimited=true";
        }

        let response = await fetch(url);
        let data = await response.text();
        $('.basketHolder').html(data);

      chevronBasket = document.querySelector(".chevronBasket");
       basketContent = document.querySelector(".basketContent");
        BasketTopIcons = document.querySelector(".BasketTopIcons");
        removeBaskets = document.querySelectorAll(".removeBasket");
        chevron = $('.chevronBasket');
        $('.BasketTopIcons').click(function (e) {
            e.stopPropagation();
            $('.accountContent').removeClass('active');
            if (window.innerWidth <= 768) {
                $('.navBar').hide();
            }
            chevron = $('.chevronBasket');
            if (chevron.css('transform') === 'none' || chevron.css('transform') === 'matrix(1, 0, 0, 1, 0, 0)') {
                chevron.css('transform', 'rotate(-180deg)');
                $('.basketContent').addClass('active');
            } else {
                chevron.css('transform', '');
                $('.basketContent').removeClass('active');
            }
        });

    });

    $('.BasketTopIcons').click(function (e) {
        e.stopPropagation();
        $('.accountContent').removeClass('active');
        if (window.innerWidth <= 768) {
            $('.navBar').hide();
        }
      chevron = $('.chevronBasket');
        if (chevron.css('transform') === 'none' || chevron.css('transform') === 'matrix(1, 0, 0, 1, 0, 0)') {
            chevron.css('transform', 'rotate(-180deg)');
            $('.basketContent').addClass('active');
        } else {
            chevron.css('transform', '');
            $('.basketContent').removeClass('active');
        }
    });
    $(document).on('click', '.removeBasket', async function (e) {
        console.log(1)
        e.preventDefault();
         let urlArr = $(this).attr('href').split('/');
        let trackId = urlArr[urlArr.length - 1]
        console.log(urlArr);
        console.log(trackId);
        let basketUrl = '/Basket/RemoveBasket/' + trackId

        let response = await fetch(basketUrl);
        let data = await response.text();
        $('.basketHolder').html(data);
      /*  let cartUrl = '/Cart/RemoveCart/' + Id*/
        //fetch(cartUrl)
        //    .then(res => res.text())
        //    .then(data => {
        //        $('#cartBody').html(data);
        //    });


        $('.BasketTopIcons').click(function (e) {
            e.stopPropagation();
            $('.accountContent').removeClass('active');
            if (window.innerWidth <= 768) {
                $('.navBar').hide();
            }
            chevron = $('.chevronBasket');
            if (chevron.css('transform') === 'none' || chevron.css('transform') === 'matrix(1, 0, 0, 1, 0, 0)') {
                chevron.css('transform', 'rotate(-180deg)');
                $('.basketContent').addClass('active');
            } else {
                chevron.css('transform', '');
                $('.basketContent').removeClass('active');
            }
        });
    });

        
})