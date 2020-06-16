/// import * as signalR from "@microsoft/signalr";

const connection = new signalR.HubConnectionBuilder()
    .withUrl("/specifEventHub")
    .build();

connection.on("SpecIfEvent", () => {

    console.debug("SpecIF Event received ***");
    //let messages = document.createElement("div");

    location.reload(true);
});

connection.start().catch(err => document.write(err));