/// <reference types="@microsoft/signalr" />
var connection = new signalR.HubConnectionBuilder()
    .withUrl("/specifEventHub")
    .build();
connection.on("SpecIfEvent", function () {
    console.debug("SpecIF Event received ***");
    //let messages = document.createElement("div");
    location.reload(true);
});
connection.start()["catch"](function (err) { return document.write(err); });
