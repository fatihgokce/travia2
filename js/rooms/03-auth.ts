import { Room, Client } from "colyseus";
import * as request from "superagent";

const FACEBOOK_APP_TOKEN = "1990687700962815|d0571d7ae11f0c986791179fa56f63c8";

export class AuthRoom extends Room {
    onInit (options) {
        console.log("StateHandlerRoom created!", options);
        this.setState({});
        
    }

    // async onAuth (options: any) {
    //     //console.log("burada :"+options);
    //     const response = await request.get(`https://graph.facebook.com/debug_token`).
    //         query({
    //             input_token: options.accessToken,
    //             access_token: FACEBOOK_APP_TOKEN
    //         }).
    //         set('Accept', 'application/json');
    //     console.log(response.body.data);
    //     return response.body.data.is_valid;
    // }
    onAuth (options: any) {
        console.log(options);
        return true;
    }


    onJoin (client:Client) {
        
        console.log(client.sessionId, "joined successfully");
    }

    onLeave (client) {
        console.log(client.sessionId, "left");
    }

    onMessage (client, data) {
        console.log("AuthRoom received message from", client.sessionId, ":", data);
    }

    onDispose () {
        console.log("Dispose AuthRoom");
    }

}
