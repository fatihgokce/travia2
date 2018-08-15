import { Room } from "colyseus";

export class CreateOrJoinRoom extends Room<any> {
    maxClients = 4;
  
    onInit (options) {
        console.log("JOINING ROOM"+options);
      
    }

    onJoin (client, options, auth) {
        console.log("CREATING NEW ROOM");
    }

    requestJoin (options, isNewRoom: boolean) {
        //console.log("burada:"+isNewRoom);
        console.log((options.create)
        ? (options.create && isNewRoom)
        : this.clients.length > 0);
        return (options.create)
            ? (options.create && isNewRoom)
            : this.clients.length > 0;
        //return true;
    }

    onMessage (client, message: any) {
        this.broadcast(`(${ client.sessionId }) ${ message }`);
    }

    onLeave (client) {
        this.broadcast(`${ client.sessionId } left.`);
        console.log("ChatRoom:", client.sessionId, "left!");
    }
    onDispose () {
        console.log("Dispose AuthRoom");
    }


}
