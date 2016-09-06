using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace IntelligensSzotar
{
    public delegate void ProgressFunction();

    public class iDictionary
    {
        public iDictionary(string DataBasePath, ProgressFunction ProgressFnc)
        {
            path = DataBasePath;
            database = DatabaseHandler.GetInstance();
            database.DatabaseInit(path, ProgressFnc);
            topics = new List<Topic>();
            LoadTopics();
        }

        private DatabaseHandler database;
        private List<Topic> topics;
        private string path;

        public List<Topic> Topics
        {
            get { return topics; }
        }

        public int TopicCount
        {
            get { return topics.Count; }
        }

        public bool AddNewTopic(String Name, string Language1, string Language2)
        {
            Topic t = new Topic(Name, Language1, Language2);
            if (!topics.Contains(t))
            {
                topics.Add(t);
                return true;
            }
            return false;
        }

        public void LoadTopics()
        {
            try
            {
                topics = database.ReadAllTopics(path);
                foreach (Topic t in topics) t.RenderMeaningsToWords();
            }
            catch { }
        }

        public List<WordMeta> Search(string keyword)
        {
            List<WordMeta> foundWords = new List<WordMeta>();

            for (int i = 0; i < topics.Count; i++)
            {
                foreach (Word w in topics[i].DynamicLang1Words)
                {
                    if (Regex.IsMatch(w.WordName, '^' + keyword))
                    {
                        WordMeta wm = new WordMeta(w.WordName, w.Language, w.Meanings);
                        foundWords.Add(wm);
                    }
                }
                foreach (Word w in topics[i].DynamicLang2Words)
                {
                    if (Regex.IsMatch(w.WordName, '^' + keyword))
                        if (Regex.IsMatch(w.WordName, keyword))
                        {
                            WordMeta wm = new WordMeta(w.WordName, w.Language, w.Meanings);
                            foundWords.Add(wm);
                        }
                }
            }
            return foundWords;
        }

    }
}
