using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IntelligensSzotar
{
    public enum QuestionMode { Random, DifficultyBased }
    public delegate void DisplayAnswerChecked(Result Result, string RightAnswer, int GoodAnswers, int WrongAnswers, int NoAnswers);
    public delegate void DisplayNextOrPrevious(string Word, int QuestionIndex, Result Result, string Answer, string RightAnswer);

    /// <summary>
    /// Procedure:
    /// Create Questioning instance >>
    /// Call SetQuestionTopics() function >>
    /// Call StartQuestioning() function
    /// </summary>
    class Questioning
    {
        public Questioning(QuestionMode QuestionMode, DisplayAnswerChecked DisplayAnswerCheckedFunction, DisplayNextOrPrevious DisplayNextWordFunction)
        {
            questions = new List<Question>();
            questWords = new List<Word>();
            mode = QuestionMode;
            displayAC = DisplayAnswerCheckedFunction;
            displayNoP = DisplayNextWordFunction;
        }

        private List<Topic> questTopics;
        private List<Word> questWords;
        private List<Question> questions;
        private int questIndex;
        private int goodAnsw, wrongAnsw, noAnsw;
        private static Random rnd = new Random();
        private QuestionMode mode;
        private DisplayAnswerChecked displayAC;
        private DisplayNextOrPrevious displayNoP;

        public int QuestionCount { get { return questions.Count; } }
        public int QuestionIndex { get { return questIndex; } }
        public List<Question> Results { get { return questions; } }

        public void SetQuestionTopics(List<Topic> QuestionTopics)
        {
            questTopics = QuestionTopics;
        }

        /// <summary>
        /// You must call SetQuestionTopics(List&lt;Topic&gt; QuestionTopics) for at least once before calling StartQuestioning()
        /// </summary>
        public bool StartQuestioning()
        {
            if (questTopics == null) throw new Exception("You must call SetQuestionTopics() before calling this!");
            if (questTopics.Count == 0) return false;
            goodAnsw = wrongAnsw = 0;
            questIndex = -1;
            if (questWords.Count > 0) questWords.Clear();
            foreach (Topic topic in questTopics)
            {
                questWords.AddRange(topic.DynamicLang1Words);
            }
            //LoadWordsToRecognise(); // from questWords
            switch (mode)
            {
                case QuestionMode.Random: LoadQuestionsRandomly(); break;
                case QuestionMode.DifficultyBased: LoadQuestionsBasedOnDifficulty(); break;
            }
            noAnsw = questions.Count;
            return true;
        }

        public void StopQuestioning()
        {

        }

        public void NextQuestion()
        {
            questIndex++;
            if (questIndex > questions.Count - 1) return;
            Question q = questions[questIndex];
            displayNoP(q.Word, questIndex + 1, q.Result, q.Answer, q.RightAnswer);
        }

        public void PreviousQuestion()
        {
            questIndex--;
            if (questIndex < 0) return;
            Question q = questions[questIndex];
            displayNoP(q.Word, questIndex + 1, q.Result, q.Answer, q.RightAnswer);
        }

        public void CheckAnswer(string Answer, bool SpeechRecognition)
        {
            Question q = questions[questIndex];
            if (q.Answered) return;
            q.AnswerQuestion(Answer, SpeechRecognition);
            if (q.Answered) noAnsw--;
            switch (q.Result)
            {
                case Result.Helyes: goodAnsw++; break;
                case Result.Hibás: wrongAnsw++; break;
            }
            displayAC(q.Result, q.RightAnswer, goodAnsw, wrongAnsw, noAnsw);
        }

        public List<string> GetWordsToRecognize()
        {
            List<string> wtr = new List<string>();
            foreach (Question q in questions)
            {
                wtr.AddRange(q.GetWord.Meanings);
            }
            return wtr;
        }

        private void LoadQuestionsRandomly()
        {
            if (questions.Count > 0) questions.Clear();
            int idx;
            while (questWords.Count > 0)
            {
                idx = rnd.Next(questWords.Count);
                questions.Add(new Question(questWords[idx]));
                questWords.RemoveAt(idx);
            }
        }

        private void LoadQuestionsBasedOnDifficulty()
        {

        }
    }
}
