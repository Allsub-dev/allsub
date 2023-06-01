"use strict";

var searchString = "";
var connection = new signalR.HubConnectionBuilder()
    .withUrl("/searchHub")
    .withAutomaticReconnect()
    .build();

function sendMessage() {
    var message = document.getElementById("searchFormInput").value;
    if (searchString.localeCompare(message) !== 0) {
        searchString = message;
        $("#searchList").empty();
    }
    var useSubs = false;
    //var useSubsCheckBox = document.getElementById("usesub");
    //if (useSubsCheckBox) {
    //    useSubs = useSubsCheckBox.checked;
    //}

    connection.invoke("Search", message, useSubs).catch(function (err) {
        return console.error(err.toString());
    });
}

window.onscroll = function (ev) {
    if (window.innerHeight + window.pageYOffset - document.body.offsetHeight >= 0) {
        sendMessage();
    }
};

document.getElementById("searchForm").addEventListener("submit", function (e) {
    e.preventDefault();
    sendMessage();
});

connection.on("ReceiveMessage", function (message) {
    var messTypeStr = message.type.toString();
    var liHtml =
        ` <li class="grid__item message_type_${messTypeStr}">` +
            ` <a class="video-tile" href="${message.url}" target="_blank" data-toggle="tooltip" data-placement="bottom" title="${message.title} ${message.description}">` + 
                ' <figure class="video-tile__content">' +
                    ` <img class="video-tile__poster" src="${message.imageUrl}" alt="${message.title}">` +
                        ' <figcaption class="video-tile__desc">' +
                            ` <h3 class="video-tile__headline">${message.title.substring(0, 15)} ...</h3>`;
                            if (messTypeStr !== "0"){
                                var messageTypeText = "";
                                switch (messTypeStr) {
                                    case "1":
                                        messageTypeText = '<img class="menu__img" src="assets/service=youtube.svg" width="24" alt="">';
                                        break;
                                    case "2":
                                        messageTypeText = '<img class="menu__img" src="assets/service=vk.svg" width="24" alt="">';
                                        break;
                                    default:
                                        console.log(`Unknown message type: ${messTypeStr}.`);
                                }
                                //<p class="video-tile__author">Dickinson Group</p>
                                liHtml = liHtml + ` <p class="video-tile__meta">${messageTypeText}</p>`;
                            }
    liHtml = liHtml + ' </figcaption>' +
                ' </figure>' +
            ' </a>' +
        ' </li>';

    $("#searchList").append(liHtml);
});

connection.start().then(function () {
    connection.invoke("Search", "", false).catch(function (err) {
        return console.error(err.toString());
    });
}).catch(function (err) {
    return console.error(err.toString());
});
