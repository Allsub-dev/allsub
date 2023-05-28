"use strict";

var searchString = "";
var connection = new signalR.HubConnectionBuilder()
    .withUrl("/searchHub")
    .withAutomaticReconnect()
    .build();

//Disable the send button until connection is established.
document.getElementById("sendButton").disabled = true;

connection.on("ReceiveMessage", function (message) {
    var div = document.createElement("div");
    div.setAttribute('style', 'border-width:1px;border-style:solid;border-color:black; margin: 2px 2px 2px 2px;');
    div.className = "col-2";

    var divHtml = ` <a href="${message.url}" target="_blank" data-toggle="tooltip" data-placement="bottom" title="${message.title} ${message.description}">` +
        ` <img src="${message.imageUrl}" alt="${message.type}" style="width:100px;height:100px;">` +
        ' </a>' +
        ` <p><i>${message.type} </i> ${message.title.substring(0, 13) } ...</p>`;
    div.innerHTML = divHtml;

    document.getElementById("searchList").appendChild(div);
});

connection.start().then(function () {
    document.getElementById("sendButton").disabled = false;
    connection.invoke("Search", "", false).catch(function (err) {
        return console.error(err.toString());
    });
}).catch(function (err) {
    return console.error(err.toString());
});

document.getElementById("sendButton").addEventListener("click", function (e) {
    sendMessage(e);
});

document.getElementById("messageInput").addEventListener("keydown", function (e) {
    if (e.code === "Enter") {  //checks whether the pressed key is "Enter"
        sendMessage(e);
    }
});

function sendMessage (event) {
    var message = document.getElementById("messageInput").value;
    if (searchString.localeCompare(message) !== 0) {
        searchString = message;
        document.getElementById("searchList").innerHTML = "";
    }
    var useSubs = false;
    var useSubsCheckBox = document.getElementById("usesub");
    if (useSubsCheckBox) {
        useSubs = useSubsCheckBox.checked;
    }

    connection.invoke("Search", message, useSubs).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
}