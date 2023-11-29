$(document).ready(function () {


    function handleBasketClick(e) {
        e.stopPropagation();
        $('.accountContent').removeClass('active');
    let chevronAcc = document.querySelector(".chevronAcc");
        if (window.innerWidth <= 768) {
            $('.navBar').hide();
        }
        chevron = $('.chevronBasket');
        if (chevron.css('transform') === 'none' || chevron.css('transform') === 'matrix(1, 0, 0, 1, 0, 0)') {
            chevron.css('transform', 'rotate(-180deg)');
            $('.basketContent').addClass('active');
        } else {
            chevron.css('transform', "");
            $('.basketContent').removeClass('active');
        }
        chevronAcc.style.transform = ""
    }


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
        if (data != null && data != "") {
            $('.basketHolder').html(data);
        }


        chevronBasket = document.querySelector(".chevronBasket");
        basketContent = document.querySelector(".basketContent");
        BasketTopIcons = document.querySelector(".BasketTopIcons");
        removeBaskets = document.querySelectorAll(".removeBasket");
        chevronAcc = document.querySelector(".chevronAcc");
        chevron = $('.chevronBasket');
        $('.BasketTopIcons').click(handleBasketClick);

    });

    $(document).on('click', '.removeBasket, .removeCart', function (e) {
        console.log(1)
        e.preventDefault();
        let urlArr = $(this).attr('href').split('/');
        let trackId = urlArr[urlArr.length - 1]
        console.log(urlArr);
        console.log(trackId);
        let basketUrl = '/Basket/RemoveBasket/' + trackId

        fetch(basketUrl)
            .then(res => res.text())
            .then(data => {

                if (data != null && data != "") {
                    $('.basketHolder').html(data);
                }
                $('.BasketTopIcons').click(handleBasketClick);
            })


        let cartUrl = 'Cart/RemoveCart/' + trackId
        fetch(cartUrl)
            .then(res => res.text())
            .then(data => {
                if (data != null && data != "") {
                    $('#cartBody').html(data);
                }

                $('.BasketTopIcons').click(handleBasketClick);

            })

        $('.BasketTopIcons').click(handleBasketClick);
    });
    $('.BasketTopIcons').click(handleBasketClick);

    let myCurrentUrl = window.location.href;

    const hub = new signalR.HubConnectionBuilder()
        .withUrl("/chat")
        .build();
    function showMessageNotification(message, name) {
        toastr["info"](message, name);
    }

    toastr.options = {
        "closeButton": true,
        "debug": false,
        "newestOnTop": true,
        "progressBar": true,
        "positionClass": "toast-top-left",
        "preventDuplicates": false,
        "showDuration": "300",
        "hideDuration": "1000",
        "timeOut": "10000",
        "extendedTimeOut": "2000",
        "showEasing": "swing",
        "hideEasing": "linear",
        "showMethod": "fadeIn",
        "hideMethod": "fadeOut"
    }

    hub.start().then(() => {
        console.log("Connection established.");
    }).catch(err => console.error(err.toString()));

    // Check if the URL contains "Messenger" (case-sensitive)
    if (myCurrentUrl.includes("Messenger")) {
        let meaasagingBox = document.querySelector(".meaasagingBox");
        let chatId = meaasagingBox.getAttribute("data-chatid")
        let userId = meaasagingBox.getAttribute("data-userId");
        let userName = meaasagingBox.getAttribute("userName");
        let pfp = meaasagingBox.getAttribute("pfp");
        let searchMessangers = document.querySelector(".searchMessangers");
        let singleMessangers = document.querySelectorAll(".singleMessanger");

        let myUrlParts = myCurrentUrl.split("/");
        let myId = myUrlParts[myUrlParts.length - 1];
   
        let sendBtn = document.querySelector(".sendBtn");
        let messageInput1 = document.querySelector(".messageInput");
        if (myId == '' || myId.toLowerCase() == "messenger" || myId.toLowerCase() == "index") {
          
            sendBtn.classList.add("invis")
            messageInput1.setAttribute("disabled", true)
        }
        
        searchMessangers.addEventListener("keyup", (e) => {
            singleMessangers.forEach(sm => {
                let text = sm.lastElementChild.firstElementChild.textContent;
        
                if (!text.toLowerCase().includes(e.target.value.toLowerCase())) {
                    sm.style.display = "none";
                } else {
                    sm.style.display = "flex"; 
                }
            })

        })

        function SendMessage(user, message, chatId, pfp, theUsersName) {
            hub.invoke("SendMessage", user, message, chatId, pfp, theUsersName)
                .then(() => {
                    let url = '/Messenger/GetUpdatedChatList/' + chatId;
                    // Fetch the updated chat list
                    fetch(url)
                        .then(res => res.text())
                        .then(data => {
                            $('.messangers').html(data);
                        });
                })
                .catch(err => console.error(err.toString()));

        }
        if (sendBtn != null) {
            let sendForm = document.querySelector(".send");
            sendForm.addEventListener('submit', function (e) {
                e.preventDefault()
                let messageInput = document.querySelector(".messageInput");
                let myMessageDiv = ` <div class="singleMessage me">
                        <p class="theMessage">${messageInput.value}</p>
                    </div>`
                let chatArea = document.querySelector(".meaasagingBox");
                let userName2 = meaasagingBox.getAttribute("userName");
                chatArea.innerHTML += myMessageDiv;
                SendMessage(userId, messageInput.value, chatId, pfp, userName2);
                messageInput.value = "";

                meaasagingBox.scrollTop =
                    meaasagingBox.scrollHeight - meaasagingBox.clientHeight;
            })
        }


        hub.on("ReceiveMessage", (message, pfp, theUsersName, chatId) => {
            const chatArea = document.querySelector(".meaasagingBox");
            let messageDiv = ` <div class="singleMessage interlocutor">
                        <img class="pfpMsg" src="/assets/images/pfp/${pfp}" />
                        <div class="interlocutorMessages">
                            <p class="theMessage">
                               ${message}
                            </p>
                        </div>`

            const currentUrl = window.location.href;
            const urlParts = currentUrl.split("/");
            const id = urlParts[urlParts.length - 1];

            if (chatId === id) {
                chatArea.innerHTML += messageDiv;

                markMessagesAsSeen(chatId, function () {
                    let url = '/Messenger/GetUpdatedChatList/' + chatId;
                 
                    fetch(url)
                        .then(res => res.text())
                        .then(data => {
                            $('.messangers').html(data);
                        });
                    let urlMsgCount = "/Messenger/GetMessageCount"
                    fetch(urlMsgCount).then(res => res.text())
                        .then(data => {
                            $('.messegeCounter').html(data);
                        });
                });
            }
            let url = '/Messenger/GetUpdatedChatList/' + chatId

            fetch(url)
                .then(res => res.text())
                .then(data => {

                    $('.messangers').html(data);

                })
            meaasagingBox.scrollTop =
                meaasagingBox.scrollHeight - meaasagingBox.clientHeight;

        });




        meaasagingBox.scrollTop =
            meaasagingBox.scrollHeight - meaasagingBox.clientHeight;


        let left = document.querySelector("#messangerMain .left");
        let people = document.querySelector("#messangerMain .people");

        people.addEventListener("click", (e) => {
            e.stopPropagation();
            if (left.style.display != "flex") {
                left.style.display = "flex";
            } else {
                left.style.display = "none";
            }
        });

        left.addEventListener("click", (e) => {
            e.stopPropagation();
        });

        window.addEventListener("click", () => {
            if (window.innerWidth < 768) {
                left.style.display = "none";
            }

        });

        window.addEventListener("resize", () => {
            if (window.innerWidth > 768) {
                left.style.display = "flex";
            } else {
                left.style.display = "none";
            }
        });
    }
    let meaasagingBox = document.querySelector(".meaasagingBox");
    let chatId2 = null;
    if (meaasagingBox != null) {

        chatId2 = meaasagingBox.getAttribute("data-chatid");
    }
    hub.on("ReceiveMessage", (message, pfp, theUsersName, chatId1) => {
        if (!myCurrentUrl.includes("Messenger") || (chatId2 != null && chatId1 != chatId2)) {
            showMessageNotification(message, theUsersName)
            var audio = document.getElementById("notificationSound");
            if (audio) {
                audio.play();
            }
            let urlMsgCount = "/Messenger/GetMessageCount"
            fetch(urlMsgCount).then(res => res.text())
        .then(data => {
            $('.messegeCounter').html(data);
        });
        }
    });

    function markMessagesAsSeen(chatId, callback) {
        hub.invoke("MarkMessagesAsSeen", chatId).catch(err => console.error(err.toString()));
        callback();
    }



    let infoToaster = $('#infoToaster');
    if (infoToaster.val() != undefined && infoToaster.val().length > 0) {
        toastr["info"](infoToaster.val());
    }

    let warningToaster = $('#warningToaster');
    if (warningToaster.val() != undefined && warningToaster.val().length > 0) {
        toastr["warning"](warningToaster.val());
    }

    let successToaster = $('#successToaster');
    if (successToaster.val() != undefined && successToaster.val().length > 0) {
        toastr["success"](successToaster.val());
    }


})