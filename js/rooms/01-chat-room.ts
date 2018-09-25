import { Room, Client, Clock,Delayed } from "colyseus";
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
const MAX_QUESTION=5;
export class ChatRoom extends Room {
    // this room supports only 4 clients connected
    maxClients = 2;
    name:string="";
    questions:Array<Question>;
    private countQuestion:number=1;    
    private questionSend=false;
    private askedQuestionIds=[];
    private answredQuestions={};
    private moveTimeOut:Delayed;
    private tempTimeOut:Delayed;
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
            putMoney:{},
            money:5000           
          };
       //this.nextQuestion(2000);
        if(this.clients.length == this.maxClients){        
            this.questionSend=true;
            this.sendMessageToOpponent({userName:options.name},client);
            this.sendQuestion();
        }
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
        //console.log(data);
        if(data.hasOwnProperty('message')){
            //Send a message to a particular client.
            this.send(this.findOpponent(client),{message:data.message});
            //this.broadcast(`${this.state.players[client.sessionId].name} `+data.message);
        }
        if(data.hasOwnProperty('put_money')){
            let qc=data.question_count;   
            console.log("put money for:"+this.state.players[client.id].name+" qc:"+qc+" time:"+new Date());    
            this.state.players[client.id].putMoney[qc]=data.put_money;
            if(this.allPlayerSetMoneyForQuestion(this.countQuestion)){
              
                if(this.answredQuestions.hasOwnProperty(`${this.countQuestion-1}`)){
                    //check did you answer the previous question
                    // added 3 seconds for informatio about which one win
                    console.log("next wait 18000");
                    this.nextQuestion(18000);
                }else{
                    //added 2 second information 
                    console.log("next wait 15000");
                    this.nextQuestion(17000);  
                }
               
            }
                       
        }
        if(data.hasOwnProperty('answer')){          
            let qc=data.question_count;
            console.log("answer  for:"+this.state.players[client.id].name+" qc:"+qc+" question count:"+this.countQuestion+" time:"+new Date());    
            this.answredQuestions[qc]=qc;
            let multiply = data["answer"]==true ? 1 : -1;
            let money = this.state.players[client.id].money;
            const putMoney=this.state.players[client.id].putMoney[qc];
            money = money + putMoney * multiply ;
            this.state.players[client.id].money = money ;
            if(qc==this.countQuestion){
                this.countQuestion=this.countQuestion+1;
                let that = this;
                if(this.tempTimeOut)
                   this.tempTimeOut.clear();
                this.tempTimeOut = this.clock.setTimeout(()=>{
                    console.log("question count increase :"+that.countQuestion);               
                    that.sendQuestion(); 
                },3000);              
               
               
                     
            }
            this.state.players[client.id].answer[qc]=data.answer;
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
    private allPlayerSetMoneyForQuestion(questionCount:number){
        let res=true;
        for(let i=0;i<this.clients.length;i++){
            let p=this.clients[i];
            if(!this.state.players[p.id].putMoney.hasOwnProperty(questionCount)){
                res=false;
                break;
            }
        }
        return res;
    }
    private nextQuestion(time:number){
        let that=this;
        if(this.moveTimeOut)
            this.moveTimeOut.clear();
        this.moveTimeOut=this.clock.setTimeout(function(){
            //that.state.questionCount=1;
            //before asked question make control for answered 
            let putMoneyFor=that.allPlayerSetMoneyForQuestion(that.countQuestion);
            console.log("control for next question:"+that.countQuestion+" time:"+new Date());
            console.log("allPlayerSetMoneyForQuestion:"+putMoneyFor);
            console.log("allPlayerSetMoneyForQuestion:"+putMoneyFor);
            if(!that.answredQuestions.hasOwnProperty(`${that.countQuestion}`) && putMoneyFor==true){
                that.countQuestion+=1;
                that.sendQuestion();
                console.log("timer runned");
            }
           
        },time); 
    }
    private findOpponent(currentClient:Client):Client{
        return this.clients.find(cli=>{
            return cli.id!=currentClient.id
        });
    }
    private sendMessageToOpponent(message,currentClient:Client){
        let oponentClient:Client=this.findOpponent(currentClient);
        this.send(oponentClient,message);
      
        this.send(currentClient,{userName:this.state.players[oponentClient.id].name});
    }
    private sendQuestion(){
        //let oponentClient:Client=this.findOpponent(currentClient);
        //this.broadcast({question:this.questions[this.countQuestion]});
        console.log("send question:"+this.countQuestion+" time:"+new Date());
        let id=this.countQuestion;
        if(id>=5){
            id=0;
        }        
        this.broadcast({question:JSON.stringify(this.questions[id]),question_count:this.countQuestion});
       
    }
    private loadQuestions(){
        var contents = fs.readFileSync("questions/questions.json");
        // Define to JSON type
        var jsonContent:Array<Question> = JSON.parse(contents);
        this.questions=jsonContent;
        //console.log(this.questions);
    }

}
