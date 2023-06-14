"use strict";

var searchString = "";
var connection = new signalR.HubConnectionBuilder()
    .withUrl("/searchHub")
    .withAutomaticReconnect()
    .build();
var lastSendTicks = 0;

function sendMessage() {
    var ticks = Date.now();
    if ((ticks - lastSendTicks) > 500) {
        lastSendTicks = ticks;

        var message = document.getElementById("searchFormInput").value;
        if (!message || searchString.localeCompare(message) !== 0) {
            searchString = message;
            $("#load-more").hide();
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
    var type = $('a.menu__link--active').data("type");
    var displayStyle = ' style="display: none"'
    if (type.toString() === messTypeStr || messTypeStr === "0" || type.toString() === "0") {
        displayStyle = ' style="display: block"'
    }

    var liHtml =
        ` <li class="grid__item message_type_${messTypeStr}" ${displayStyle}>` +
            ` <a class="video-tile" href="${message.url}" target="_blank" data-toggle="tooltip" data-placement="bottom" title="${message.title}">` + 
                ' <figure class="video-tile__content">' +
                    ` <img class="video-tile__poster" src="${message.imageUrl}" alt="${message.title}">` +
                        ' <figcaption class="video-tile__desc">' +
                                ` <h3 class="video-tile__headline">${message.title}</h3>`;
                            if (message.ownerTitle) {
                                liHtml = liHtml + ` <p class="video-tile__author">${message.ownerTitle}</p>`;
                            }
                            if (messTypeStr !== "0") {
                                var messageTypeText = "";
                                switch (messTypeStr) {
                                    case "1":
                                        messageTypeText = '<img src="assets/service=youtube.svg" alt="">';
                                        break;
                                    case "2":
                                        messageTypeText = '<img src="assets/service=vk.svg" alt="">';
                                        break;
                                    default:
                                        console.log(`Unknown message type: ${messTypeStr}.`);
                                }
                                liHtml = liHtml + ` <p class="video-tile__meta">${messageTypeText} ${message.metaData}</p>`;
                            }
                            else if (message.metaData){
                                liHtml = liHtml + ` <p class="video-tile__meta">${message.metaData}</p>`;
                            }
                            liHtml = liHtml + ' </figcaption>' +
                ' </figure>' +
            ' </a>' +
        ' </li>';

    $("#searchList").append(liHtml);
    if ($("#searchList").children().length > 20) {
        $("#load-more").show();
    }
});

connection.start()
    .then(function () {
        sendMessage();
    })
    .catch(function (err) {
        return console.error(err.toString());
    });

$('a.menu__link:not(.menu__link--disabled)').on('click', function (event) {
    $('a.menu__link--active').removeClass("menu__link--active")
    $(this).addClass("menu__link--active");
    // Get type
    var type = $(this).data("type");
    if (type === 0) {
        // show all types
        $('li[class^="grid__item message_type_"]').show();
    }
    else {
        $(`li[class^="grid__item message_type_"]`).hide();

        $(`li[class="grid__item message_type_0"]`).show();
        $(`li[class="grid__item message_type_${type}"]`).show();
    }

    //event.stopPropagation();
    //event.stopImmediatePropagation();
});

$('#load-more').on('click', function (event) {
    sendMessage();
});
