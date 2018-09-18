using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets
{
    class Keys
    {
        public static  readonly string Message="message";
        public static  readonly string UserName = "userName";
        public static readonly string Question = "question";
        public static readonly string QuestionCount="question_count";
    }
    [Serializable]
    public class Question
    {
        //    Id:number;
        //order:number;
        //qustion:string;
        //suggestion:Array<string>=[];   
        //correct:number; 
        public string question;
        public int Id;
        public int order;
        //public List<string> suggestion { get; set; }
        public List<string> suggestions;
        public int correct;
    }
}
