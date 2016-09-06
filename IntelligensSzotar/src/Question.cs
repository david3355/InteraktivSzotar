using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IntelligensSzotar
{
    public enum Result { Hibás, Helyes, NincsVálasz }

    public class Question
    {
        public Question(Word WordToAsk)
        {
            word = WordToAsk;
            result = Result.NincsVálasz;
            answer = String.Empty;
            answered = false;
        }

        private Result result;
        private Word word;
        private string answer;
        private bool answered;

        public string AnswerResult
        {
            get
            {
                switch (result)
                {
                    case Result.Helyes: return "Helyes";
                    case Result.Hibás: return "Hibás";
                    case Result.NincsVálasz: return "Nincs válasz";
                    default: return String.Empty;
                }
            }
        }

        public string Word { get { return word.WordName; } }
        public string Answer { get { return answer; } }
        public bool Answered { get { return answered; } }
        public Result Result { get { return result; } }
        public Word GetWord { get { return word; } }

        public string RightAnswer
        {
            get
            {
                if (!answered) return String.Empty;
                StringBuilder stb = new StringBuilder();
                int count = word.Meanings.Count;
                for (int i = 0; i < count; i++)
                {
                    stb.Append(word.Meanings[i]);
                    if (i < count - 1) stb.Append(", ");
                }
                return stb.ToString();
            }
        }

        public void AnswerQuestion(string TheAnswer, bool SpeechRecognition)
        {
            if (TheAnswer == String.Empty) return;
            bool right = word.Meanings.Contains(TheAnswer);

            if (right)
            {
                result = Result.Helyes;
                answer = TheAnswer;
                answered = true;
            }
            else if (!right && !SpeechRecognition)
            {
                result = Result.Hibás;
                answer = TheAnswer;
                answered = true;
            }
        }
    }
}
