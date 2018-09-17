using Colyseus;
using GameDevWare.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Linq;
enum PlayerState:byte
{
    Waiting,PutMoney,TenderIsOver
}
class Player
{
    public string SessionId { get; set; }
    public Dictionary<string, int> putMoney { get; set; }
    public Dictionary<string, bool> answer { get; set; }
    public string name { get; set; }  
    public Player()
    {
        putMoney = new Dictionary<string, int>();
        answer = new Dictionary<string, bool>();
    }
}
public class GameControl : MonoBehaviour {
    //


    Client client;
    Room room;
    public Text txtPlayerName;
    public Text txtOpponentName;
    public Text txtMessage;
    public Text TxtMessagePanel2;
    public Text txtMessagePanel3;
    public Text txtTimerPane3;
    public Text txtQuestion;
    public string serverName = "192.168.1.33";//"localhost";
    public string port = "2658";
    public string roomName = "chat";//"chat";
    public GameObject question;
    public GameObject canvas;
    public List<GameObject> listPanels;
    //public Sprite[] moneySprites;
    public GameObject ImgPanel2Player;
    public GameObject ImgPanel2Rival;
    public GameObject ImgFooter;
    
   
    Dictionary<string, Player> players = new Dictionary<string, Player>();
    //public float waitTime = 1f;
   
    //public int timeLeft = 60; //Seconds Overall
    //

    public Text txtTimer;
    PlayerState playerState = PlayerState.Waiting;
    float currCountdownValue;
    int clientCount = 0;
    Assets.Question questionTxt;
    float distanceLoadingPanel;
    List<GameObject> _listQuestions;
    int questionCount=0;
    void initBeforeQuestion(){
        var rs = ImgFooter.GetComponent<RectTransform>();
       
        ImgFooter.transform.localPosition = new Vector3(ImgFooter.transform.localPosition.x,-565.7f, 0);

        rs.sizeDelta = new Vector2(rs.sizeDelta.x, 65.5f);
        listPanels[0].SetActive(true);
        listPanels[1].SetActive(false);
        listPanels[2].SetActive(false);
        //room.Send(new { nextQuestion = true });
        txtQuestion.text="Önce para sonra soru!";
      
    }
    void Start()
    {
        _listQuestions = new List<GameObject>();
        Vector3 vec2 = ImgFooter.transform.localPosition;
        vec2.y = 169.6f;
        distanceLoadingPanel = Vector3.Distance(ImgFooter.transform.localPosition,vec2)-65.5f/2;       
       
        txtPlayerName.text = PlayerPrefs.GetString("userName");
     
        var lists = GameObject.FindGameObjectsWithTag("btnPutMoney");
        foreach(var go in lists)
        {
            go.GetComponent<Button>().onClick.AddListener(BtnPutMoney);
        }
        //Sprite myFruit = Resources.Load<Sprite>("images/15");
     
       
        StartCoroutine("AddListeners");
    }

    // Update is called once per frame
    void Update()
    {     
    }
    void BtnQuestionsToogleEnable(bool enable){
        // var lists = GameObject.FindGameObjectsWithTag("btnPutMoney");
        // foreach(var go in lists)
        // {
        //     go.GetComponent<Button>().enabled=enable;
        // }
        foreach(var btn in _listQuestions){
           btn.GetComponent<Button>().enabled=enable;
        }
    }
    void BtnPutMoney()
    {
        var go = EventSystem.current.currentSelectedGameObject;
        var s= go.transform.transform.GetChild(0).GetComponent<Text>().text;
        playerState = PlayerState.PutMoney;
        room.Send(new { put_money=int.Parse(s)});
       
        initBeforeQuestion();
      
    }
    public IEnumerator StartCountdown(Text textBox,float countdownValue = 10)
    {
        currCountdownValue = countdownValue;
        while (currCountdownValue > 0)
        {
           
            var rs = ImgFooter.GetComponent<RectTransform>();
            //rT.sizeDelta = new Vector2(rT.sizeDelta.x, rT.sizeDelta.y + 0.5f);
            float incAmount = distanceLoadingPanel/countdownValue;
            ImgFooter.transform.localPosition = new Vector3(ImgFooter.transform.localPosition.x, ImgFooter.transform.localPosition.y + (incAmount / 2), 0);

            rs.sizeDelta = new Vector2(rs.sizeDelta.x, rs.sizeDelta.y + incAmount);

            textBox.text = currCountdownValue.ToString();
            currCountdownValue--;
            yield return new WaitForSeconds(1.0f);
        }
        initBeforeQuestion();
    }
    IEnumerator AddListeners()
    {

        Debug.Log("start");
      
        String uri = "ws://" + serverName + ":" + port;
        //Debug.Log(uri);

        client = new Client(uri);
        client.OnOpen += OnOpenHandler;
        client.OnClose += (object sender, EventArgs e) => Debug.Log("CONNECTION CLOSED");

        Debug.Log("Let's connect the client!");
        //yield return StartCoroutine(client.Connect());
        StartCoroutine(client.Connect());
        Debug.Log("Let's join the room!");
        room = client.Join(roomName, new Dictionary<string, object>()
        {
            { "name", txtPlayerName.text }
        });
       
        room.OnReadyToConnect += (sender, e) =>
        {
            Debug.Log("Ready to connect to room!");
            StartCoroutine(room.Connect());
        };
        room.OnJoin += OnRoomJoined;
        room.OnStateChange += OnStateChangeHandler;

        room.Listen("players/:id", this.OnPlayerChange);
        room.Listen("players/:id/:prop", this.OnPlayerMove);
        room.Listen("players/:id/:prop/:q",this.OnPlayerChangeQuestion);
        //room.Listen("messages/:number", this.OnMessageAdded);
        //room.Listen(this.OnChangeFallback);

        room.OnMessage += OnMessage;

        int i = 0;

        while (true)
        {
            client.Recv();

            i++;

            if (i % 50 == 0)
            {
                //room.Send("some_command");
            }

            yield return 0;
        }
    }
    void OnOpenHandler(object sender, EventArgs e)
    {
        Debug.Log("Connected to server. Client id: " + client.id);
    }

    void OnRoomJoined(object sender, EventArgs e)
    {
        Debug.Log("Joined room successfully.");

    }

    void OnMessage(object sender, MessageEventArgs e)
    {
        if(e.message==null)
           Debug.Log("null deger geldi");
        var message = (IndexedDictionary<string, object>) e.message;
        if(message==null)
           return;
        Debug.Log("messa");
        Debug.Log(message.Keys[0]);
        Debug.Log(e.message);
        foreach(var k in message.Keys)
        {
          
            if (k == Assets.Keys.UserName)
            {
                txtOpponentName.text = message[k].ToString();//e.message.ToString();
            }
            if (k == Assets.Keys.Message)
            {
                txtMessage.text = message[k].ToString();
            }
            if (k == Assets.Keys.Question)
            {
                questionTxt = JsonUtility.FromJson<Assets.Question>(message[k].ToString());
                
                Debug.Log(questionTxt.question);
                int amount = 0;//-130;
                int index = 1;
                foreach(var btn in _listQuestions){
                    Destroy(btn);                    
                }
                _listQuestions=new List<GameObject>();
                foreach(var q in questionTxt.suggestions)
                {
                   
                    Vector3 pos = new Vector3(1, -107+amount,0);
                    var btnQ = Instantiate(question);
                    btnQ.GetComponent<RectTransform>().SetParent(canvas.transform);
                    btnQ.GetComponent<RectTransform>().localPosition=pos;                    
                    int index2 = index;
                    btnQ.GetComponent<Button>().onClick.AddListener(delegate {
                        
                        BtnQuestionClick(index2,btnQ);
                    }
                    );
                        index++;
                    btnQ.transform.GetChild(0).GetComponent<Text>().text = q;
                    _listQuestions.Add(btnQ);
                    amount -= 130;
                   
                }
            }
        }
        //		Debug.Log(data);
        //text1.text = e.message.ToString() + "\n";
      
    }
    void BtnQuestionClick(int answer,GameObject btn)
    {
        Debug.Log(answer);
        btn.GetComponent<Image>().color=Color.green;
        if (answer == questionTxt.correct)
        {
            txtMessagePanel3.text = "doğru cevap";
        }
        else
        {
            txtMessagePanel3.text = "yanlış cevap";
        }
        room.Send(new { answer = answer == questionTxt.correct });
        BtnQuestionsToogleEnable(false);
    }
    void OnStateChangeHandler(object sender, RoomUpdateEventArgs e)
    {
        // Setup room first state
        if (e.isFirstState)
        {
            IndexedDictionary<string, object> players = (IndexedDictionary<string, object>)e.state["players"];
        }
    }

    void OnPlayerChange(DataChange change)
    {
        //Debug.Log("OnPlayerChange");
        //Debug.Log(change.operation);
        //Debug.Log(change.path["id"]);
        //Debug.Log("client id:"+client.id);
        
        if (change.operation == "add")
        {
            //IndexedDictionary<string, object> value = (IndexedDictionary<string, object>)change.value;

            //GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);

            //cube.transform.position = new Vector3(Convert.ToSingle(value["x"]), Convert.ToSingle(value["y"]), 0);

            // add "player" to map of players by id.
            Player player = new Player { SessionId = change.path["id"] };
           
            //player.putMoney[this.questionCount.ToString()]=10;          
            players.Add(change.path["id"], player);
            Debug.Log("set put deneme");
            // var ply=players[change.path["id"]];
            // if(ply==null){
            //     Debug.Log("null ply");
            // }
            //ply.putMoney["0"]=20;
            //players[change.path["id"]].putMoney[this.questionCount.ToString()]=20;
            clientCount++;
        }
        else if (change.operation == "remove")
        {
            // remove player from scene
            //GameObject cube;
            //players.TryGetValue(change.path["id"], out cube);
            //Destroy(cube);

            players.Remove(change.path["id"]);
            clientCount--;
            //game over
        }
    }
    void OnPlayerChangeQuestion(DataChange change)
    {
        Debug.Log("OnPlayerChangeQuestion");
        Debug.Log(change.path["id"]);
        Debug.Log(change.path["prop"]);
        Debug.Log(change.value);
        //playerState == PlayerState.PutMoney 
        if (change.path["prop"]=="putMoney" && players.Count==2)
        {
            //ihale bittimi kontrol ediliyor
          
            int v=int.Parse(change.value.ToString());
          
            players[change.path["id"]].putMoney[this.questionCount.ToString()]=v;//(int)change.value;
            bool tenderOver = true;
            
            foreach (var kvp in players)
            {
                Debug.Log("into put money cc:"+kvp.Key);
                var player= kvp.Value;
                if(player.putMoney.ContainsKey(this.questionCount.ToString())){
                    if (player.putMoney[questionCount.ToString()] == 0){
                        tenderOver = false;
                        break;
                    }
                }else{
                    tenderOver=false;
                    break;
                }
             
              
            }
            if (tenderOver)
            {
                Debug.Log("ihale bitti");
                playerState = PlayerState.TenderIsOver;
                listPanels[0].SetActive(false);
                listPanels[1].SetActive(true);
                Player player = players[client.id];
                Sprite myFruit = Resources.Load<Sprite>("images/"+player.putMoney);
                ImgPanel2Player.GetComponent<Image>().sprite = myFruit;
                ImgPanel2Player.GetComponent<Image>().GetComponentInChildren<Text>().text = player.putMoney.ToString();
                Player rival = FindRivalPlayer();
                Sprite myFruit2 = Resources.Load<Sprite>("images/" + rival.putMoney);
                ImgPanel2Rival.GetComponent<Image>().sprite = myFruit2;
                ImgPanel2Rival.GetComponent<Image>().GetComponentInChildren<Text>().text = rival.putMoney.ToString();
                Player mostPutMoneyPlayer = FindMostPutMoneyPlayer();
                TxtMessagePanel2.text = "En çok parayı "+ mostPutMoneyPlayer.name+" koydu!";
              
                StartCoroutine(ActivePanel3());
            }
        }
        else{
            if(change.path["prop"]=="putMoney")
            {
                var player=players[change.path["id"]];
                if(player!=null)
                {
                    player.putMoney[this.questionCount.ToString()]=(int)change.value;
                    Debug.Log("set player");
                    Debug.Log(player);
                }
                
            }
        }
    }
    void OnPlayerMove(DataChange change)
    {
        Debug.Log("OnPlayerMove");
        Debug.Log(change.path["id"]);
        Debug.Log(change.path["prop"]);
        Debug.Log(change.value);
        string prop= change.path["prop"];
        if(!String.IsNullOrEmpty(prop) && prop=="name")
        {
            if (change.operation == "add")
            {
                Player player = players[change.path["id"]];           
                SetObjectProperty(change.path["prop"], change.value, player);
            }
            else if (change.operation == "replace")
            {
                Player player = players[change.path["id"]];
                SetObjectProperty(change.path["prop"], change.value, player);
            }
        }
        //Debug.Log("OnPlayerMove");
        //Debug.Log("playerId: " + change.path["id"] + ", prop: " + change.path["prop"]+" operation:"+change.operation);
        //Debug.Log(change.value);
        // if (change.operation == "add")
        // {
        //     Player player = players[change.path["id"]];           
        //     SetObjectProperty(change.path["prop"], change.value, player);
        // }
        // else if (change.operation == "replace")
        // {
        //     Player player = players[change.path["id"]];
        //     SetObjectProperty(change.path["prop"], change.value, player);
        // }
        // if (playerState == PlayerState.PutMoney && players.Count==2)
        // {
        //     //ihale bittimi kontrol ediliyor
        //     Debug.Log("into put money cc:"+players.Count);
        //     bool tenderOver = true;
            
        //     foreach (var kvp in players.ToArray())
        //     {
        //         var player= kvp.Value;
        //         if (player.putMoney[questionCount.ToString()] == 0){
        //             tenderOver = false;
        //             break;
        //         }
        //     }
        //     if (tenderOver)
        //     {
        //         playerState = PlayerState.TenderIsOver;
        //         listPanels[0].SetActive(false);
        //         listPanels[1].SetActive(true);
        //         Player player = players[client.id];
        //         Sprite myFruit = Resources.Load<Sprite>("images/"+player.putMoney);
        //         ImgPanel2Player.GetComponent<Image>().sprite = myFruit;
        //         ImgPanel2Player.GetComponent<Image>().GetComponentInChildren<Text>().text = player.putMoney.ToString();
        //         Player rival = FindRivalPlayer();
        //         Sprite myFruit2 = Resources.Load<Sprite>("images/" + rival.putMoney);
        //         ImgPanel2Rival.GetComponent<Image>().sprite = myFruit2;
        //         ImgPanel2Rival.GetComponent<Image>().GetComponentInChildren<Text>().text = rival.putMoney.ToString();
        //         Player mostPutMoneyPlayer = FindMostPutMoneyPlayer();
        //         TxtMessagePanel2.text = "En çok parayı "+ mostPutMoneyPlayer.name+" koydu!";
        //         Debug.Log("neden calling");
        //         StartCoroutine(ActivePanel3());
        //     }
        // }
       
    }
    public IEnumerator ActivePanel3()
    {
        Debug.Log("neden called");
        yield return new WaitForSeconds(2.0f);
        listPanels[1].SetActive(false);
        listPanels[2].SetActive(true);
        Player mostPutMoneyPlayer = FindMostPutMoneyPlayer();
        txtMessagePanel3.text = mostPutMoneyPlayer.name + " için soru geliyor...";
        txtQuestion.text = questionTxt.question;
        StartCoroutine(StartCountdown(txtTimerPane3, 20));
    }
    private Player FindMostPutMoneyPlayer()
    {
        Player cplayer = players[client.id];
        foreach (var kvp in players.ToArray())
        {
            var player = kvp.Value;
            if (player.putMoney[questionCount.ToString()] > cplayer.putMoney[questionCount.ToString()])
            {
                return player;
            }
        }
        return cplayer;
    }
    private Player FindRivalPlayer()
    {
        Player rival=null;
        foreach (var kvp in players.ToArray())
        {
            var player = kvp.Value;
            if (player.SessionId != client.id)
            {
                rival = player;
                break;
            }
        }
        return rival;
    }
    private void SetObjectProperty(string propertyName, object value, object obj)
    {
        PropertyInfo propertyInfo = obj.GetType().GetProperty(propertyName);
        // make sure object has the property we are after
        if (propertyInfo != null)
        {
            propertyInfo.SetValue(obj, value, null);
        }
    }
    void OnPlayerRemoved(DataChange change)
    {
        //		Debug.Log ("OnPlayerRemoved");
        //		Debug.Log (change.path);
        //		Debug.Log (change.value);
    }

    void OnMessageAdded(DataChange change)
    {
        //		Debug.Log ("OnMessageAdded");
        //		Debug.Log (change.path["number"]);
        //Debug.Log (change.value);
    }

    void OnChangeFallback(PatchObject change)
    {
        //		Debug.Log ("OnChangeFallback");
        //		Debug.Log (change.operation);
        //		Debug.Log (change.path);
        //		Debug.Log (change.value);
    }

    void OnApplicationQuit()
    {
        // Make sure client will disconnect from the server
        if (client != null)
        {
            room.Leave();
            client.Close();

        }

    }
}
