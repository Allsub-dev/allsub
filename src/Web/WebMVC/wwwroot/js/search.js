"use strict";

var connection = new signalR.HubConnectionBuilder()
    .withUrl("/searchHub")
    .withAutomaticReconnect()
    .build();

//Disable the send button until connection is established.
document.getElementById("sendButton").disabled = true;

connection.on("ReceiveMessage", function (message) {
    var div = document.createElement("div");
    div.setAttribute('style', 'border-width:1px;border-style:solid;border-color:black; margin: 2px 2px 2px 2px;');

    var divHtml = '<div class="row">' +
        ' <div class="col">' +
        ` <a href="${message.url}" target="_blank">` +
        ` <img src="${message.imageUrl}" alt="${message.type}" style="width:100px;height:100px;">` +
        ' </a>' +
        ' </div>' +
        ' <div class="col">' +
        ` <p>${message.title}</p>` +
        ` <p><i>${message.type} </i> ${ message.description }</p>` +
        ' </div>' +
        ' </div>';
    div.innerHTML = divHtml;

    document.getElementById("searchList").appendChild(div);
});

connection.start().then(function () {
    document.getElementById("sendButton").disabled = false;
}).catch(function (err) {
    return console.error(err.toString());
});

document.getElementById("sendButton").addEventListener("click", function (event) {
    var message = document.getElementById("messageInput").value;
    var useSubs = false;
    var useSubsCheckBox = document.getElementById("usesub");
    if (useSubsCheckBox) {
        useSubs = useSubsCheckBox.checked;
    }

    connection.invoke("Search", message, useSubs).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
});