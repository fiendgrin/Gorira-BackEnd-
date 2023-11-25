let meaasagingBox = document.querySelector(".meaasagingBox");
let chatId = meaasagingBox.getAttribute("data-chatid")
let userId = meaasagingBox.getAttribute("data-userId");
let pfp = meaasagingBox.getAttribute("pfp");
console.log(chatId)
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

const hub = new signalR.HubConnectionBuilder()
    .withUrl("/chat")
    .build();

hub.start().then(() => {
    console.log("Connection established.");
}).catch(err => console.error(err.toString()));

function SendMessage(user,message,chatId,pfp) {
    hub.invoke("SendMessage", user, message, chatId, pfp).catch(err => console.error(err.toString()));
}


hub.on("ReceiveMessage", (message,pfp) => {
    const chatArea = document.querySelector(".meaasagingBox");
    let messageDiv = ` <div class="singleMessage interlocutor">
                        <img class="pfpMsg" src="/assets/images/pfp/${pfp}" />
                        <div class="interlocutorMessages">
                            <p class="theMessage">
                               ${message}
                            </p>
                        </div>`
    chatArea.innerHTML += messageDiv;
});

let sendBtn = document.querySelector(".sendBtn");

sendBtn.addEventListener('click', function () {
    let messageInput = document.querySelector(".messageInput");
    let myMessageDiv = ` <div class="singleMessage me">
                        <p class="theMessage">${messageInput.value}</p>
                    </div>`
    const chatArea = document.querySelector(".meaasagingBox");
    chatArea.innerHTML += myMessageDiv;
    SendMessage(userId, messageInput.value, chatId, pfp);
    messageInput.value = "";
})


