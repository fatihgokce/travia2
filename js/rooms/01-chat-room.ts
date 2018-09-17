import { Room, Client } from "colyseus";
import * as fs from 'fs';
const Keys={
    userName:'userName',
    question:'question',
    message:'message',
}
class Question{
    Id:number;
    order:number;
    question:string;
    suggestions:Array<string>=[];   
    correct:number; 
}
export class ChatRoom extends Room {
    // this room supports only 4 clients connected
    maxClients = 2;
    name:string="";
    questions:Array<Question>;
    private countQuestion:number=0;
    private questionSend=false;
    private askedQuestions=[];
    onInit (options) {
       //console.log("BasicRoom created!", options);
        
        //console.log(this.name)
        this.setState({
            players: {},
            winGame:null,
            questionCount:0
          });
        this.loadQuestions();
        this.metadata=new Date();
    }

    onJoin (client:Client,options) {
        //this.broadcast(`${ client.sessionId } joined.`)
        // if(this.name){
        //     this.broadcast(`${ this.name }`)
        // }
        //this.state.players[client.id].name=options.name;
        this.state.players[client.id] = {
            name: options.name,
            answer: {},
            putMoney:{}           
          };
        // let that=this;
        // this.clock.setTimeout(function(){
        //     that.state.questionCount=1;
        //     console.log("timer runned");
        // },2000); 
        if(this.clients.length==2){        
            this.questionSend=true;
           
            this.sendMessageToOpponent({userName:options.name},client);
            this.sendQuestion(client);
            
        }
           
        
    }
    nextQuestion(){

    }
    requestJoin (options: any) {
        //console.log("requst join");
        // Prevent the client from joining the same room from another browser tab
        return this.clients.filter(c => c.id === options.clientId).length === 0;
    }

    onLeave (client:Client) {
        this.broadcast(`${ client.sessionId } left`)
        delete this.state.players[client.id];
        console.log(`${client.id} leave roo`);
        //this.disconnect();
       
    }

    onMessage (client:Client, data:object) {
        //console.log("BasicRoom received message from", client.sessionId, ":", data);
        //console.log(this.state);
        //this.broadcast(`(${ client.sessionId }) ${ data.message }`);
       console.log(data);
        if(data.hasOwnProperty('message')){
            //Send a message to a particular client.
            this.send(this.findOpponent(client),{message:data.message});
            //this.broadcast(`${this.state.players[client.sessionId].name} `+data.message);
        }
        if(data.hasOwnProperty('put_money')){   
                  
            this.state.players[client.id].putMoney[this.countQuestion]=data.put_money;
            console.log(this.state.players[client.id]);
        }
        if(data.hasOwnProperty('answer')){
            console.log("içerde");
            console.log(this.state.players[client.id]);
            this.state.players[client.id].answer=data.answer;
        }
        if(data.hasOwnProperty('nextQuestion')){            
            this.countQuestion+=1;
            if(this.questions.length==this.countQuestion)
                this.countQuestion=0;
            this.broadcast({question:JSON.stringify(this.questions[this.countQuestion])});
        }
       
    }
  
    onDispose () {        
        console.log("Dispose BasicRoom1");
    }
    findOpponent(currentClient:Client):Client{
        return this.clients.find(cli=>{
            return cli.id!=currentClient.id
        });
    }
    private sendMessageToOpponent(message,currentClient:Client){
        let oponentClient:Client=this.findOpponent(currentClient);
        this.send(oponentClient,message);
      
        this.send(currentClient,{userName:this.state.players[oponentClient.id].name});
    }
    private sendQuestion(currentClient:Client){
        let oponentClient:Client=this.findOpponent(currentClient);
        //this.broadcast({question:this.questions[this.countQuestion]});
        this.broadcast({question:JSON.stringify(this.questions[this.countQuestion]),question_count:this.countQuestion});
    }
    private loadQuestions(){
        var contents = fs.readFileSync("questions/questions.json");
        // Define to JSON type
        var jsonContent:Array<Question> = JSON.parse(contents);
        this.questions=jsonContent;
        //console.log(this.questions);
    }

}
