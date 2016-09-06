using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace IntelligensSzotar
{
    public class Topic
    {
        public Topic(string TopicName, string Language1, string Language2)
            : this(TopicName, Language1, Language2, 0)
        {
            database.AddNewTopicFile(TopicName, Language1, Language2);
        }

        public Topic(string TopicName, string Language1, string Language2, int LastWordID)
        {
            database = DatabaseHandler.GetInstance();
            L1Words = new List<Word>();
            L2Words = new List<Word>();
            lang1 = Language1;
            lang2 = Language2;
            langSwitcher = true;
            topicName = TopicName;
            lastWordID = LastWordID;
        }

        private string lang1, lang2;
        private List<Word> L1Words;
        private List<Word> L2Words;
        private string topicName;
        private bool langSwitcher;
        private int lastWordID;
        private DatabaseHandler database;

        public string TopicTitle
        {
            get { return topicName; }
        }

        public List<Word> StaticLang1Words { get { return L1Words; } }
        public List<Word> StaticLang2Words { get { return L2Words; } }
        public string StaticLanguage1 { get { return lang1; } }
        public string StaticLanguage2 { get { return lang2; } }
        public int StaticLang1Count { get { return L1Words.Count; } }
        public int StaticLang2Count { get { return L2Words.Count; } }

        public List<Word> DynamicLang1Words
        {
            get
            {
                if (langSwitcher) return L1Words;
                else return L2Words;
            }
        }

        public List<Word> DynamicLang2Words
        {
            get
            {
                if (langSwitcher) return L2Words;
                else return L1Words;
            }
        }

        public string DynamicLanguage1
        {
            get
            {
                if (langSwitcher) return lang1;
                else return lang2;
            }
        }

        public string DynamicLanguage2
        {
            get
            {
                if (langSwitcher) return lang2;
                else return lang1;
            }
        }

        public int DynamicLang1Count
        {
            get
            {
                if (langSwitcher) return L1Words.Count;
                else return L2Words.Count;
            }
        }

        public int DynamicLang2Count
        {
            get
            {
                if (langSwitcher) return L2Words.Count;
                else return L1Words.Count;
            }
        }

        public void AddNewWord(List<string> StaticL1NewWords, List<string> StaticL2NewWords)
        {
            List<Word> newL1words = new List<Word>();
            List<Word> newL2words = new List<Word>();
            List<Word> existingL1words = new List<Word>();
            List<Word> existingL2words = new List<Word>();
            foreach (string l1word in StaticL1NewWords)
            {
                Word existing = StaticLang1Words.Find(word => word.WordName.ToLower() == l1word.ToLower());
                if (existing != null) existingL1words.Add(existing);
                else newL1words.Add(new Word(++lastWordID, l1word, StaticLanguage1, this));
            }
            foreach (string l2word in StaticL2NewWords)
            {
                Word existing = StaticLang2Words.Find(word => word.WordName.ToLower() == l2word.ToLower());
                if (existing != null) existingL2words.Add(existing);
                else newL2words.Add(new Word(++lastWordID, l2word, StaticLanguage2, this));
            }

            foreach (Word w in newL1words)
            {
                foreach (Word m in newL2words) w.AddNewMeaning(m.ID, m.WordName);
                foreach (Word m in existingL2words) w.AddNewMeaning(m.ID, m.WordName);
                StaticLang1Words.Add(w);
                database.AddNewWord(w, this);
            }
            foreach (Word w in newL2words)
            {
                foreach (Word m in newL1words) w.AddNewMeaning(m.ID, m.WordName);
                foreach (Word m in existingL1words) w.AddNewMeaning(m.ID, m.WordName);
                StaticLang2Words.Add(w);
                database.AddNewWord(w, this);
            }

            //Ezeknél csak a jelentéseket kell hozzáadni, a szót nem kell elmenteni
            // Ha pl a wait szó már fel van véve, és felvesszük a vár szó új jelentéseként a waitet, ezt adatbázisban is kell ellenőrizni
            List<int> mids = new List<int>();
            foreach (Word w in existingL1words)
            {
                foreach (Word m in newL2words) { w.AddNewMeaning(m.ID, m.WordName); mids.Add(m.ID); }
                foreach (Word m in existingL2words) { w.AddNewMeaning(m.ID, m.WordName); mids.Add(m.ID); }
                database.AddNewMeaningsToWord(this, w.ID, mids);
            }
            mids.Clear();
            foreach (Word w in existingL2words)
            {
                foreach (Word m in newL1words) { w.AddNewMeaning(m.ID, m.WordName); mids.Add(m.ID); }
                foreach (Word m in existingL1words) { w.AddNewMeaning(m.ID, m.WordName); mids.Add(m.ID); }
                database.AddNewMeaningsToWord(this, w.ID, mids);
            }
        }

        public void AddWordFromDatabase(Word Word)
        {
            if (Word.Language == this.lang1) L1Words.Add(Word);
            else L2Words.Add(Word);
        }

        public void DeleteWord(Word Word)
        {
            List<Word> find = (Word.Language == this.lang1) ? StaticLang1Words : StaticLang2Words;
            List<Word> oppositeLangWords = (Word.Language == this.lang2) ? StaticLang1Words : StaticLang2Words;
            foreach (Word wordToDelete in find)
            {
                if (wordToDelete.ID == Word.ID)
                {
                    foreach (Word possibleMeaning in oppositeLangWords)
                    {
                        foreach (int mi in wordToDelete.MeaningIDs)
                        {
                            if (possibleMeaning.ID == mi) possibleMeaning.DeleteMeaning(wordToDelete.ID);
                            //if (possibleMeaning.MeaningIDs.Count == 0) { possibleMeaning.Topic.DeleteWord(possibleMeaning); break; }
                        }
                    }
                    find.Remove(wordToDelete); break;
                }
            }
            database.DeleteWord(this.TopicTitle, Word.ID);
        }

        public void ModifyWord(Word Word, string NewName)
        {
            Word.WordName = NewName;
            List<Word> occur = Word.Language == this.StaticLanguage1 ? StaticLang2Words : StaticLang1Words;
            foreach (Word w in occur)
            {
                for (int i = 0; i < w.MeaningIDs.Count; i++)
                {
                    if (Word.ID == w.MeaningIDs[i]) w.Meanings[i] = Word.WordName;
                }
            }
            database.ModifyWord(this.TopicTitle, Word.ID, Word.WordName);
        }

        public void ModifyWord(Word Word, List<int> MeaningsToDelete)
        {
            foreach (int mi in MeaningsToDelete)
            {
                DeleteMeaning(Word, mi);
            }
            database.DeleteMeanings(this.TopicTitle, Word.ID, MeaningsToDelete);
        }

        private void DeleteMeaning(Word Word, int mi)
        {
            int idx = Word.MeaningIDs.IndexOf(mi);
            if (idx == -1) return;
            Word.MeaningIDs.RemoveAt(idx);
            Word.Meanings.RemoveAt(idx);
            if (Word.MeaningIDs.Count == 0)
            {
                this.DeleteWord(Word);
                return;
            }
            List<Word> occur = Word.Language == this.StaticLanguage1 ? StaticLang2Words : StaticLang1Words;
            int i = 0;
            while (i < occur.Count)
            {
                if (occur[i].ID == mi && occur[i].MeaningIDs.Contains(Word.ID)) DeleteMeaning(occur[i], Word.ID);
                else i++;
            }
        }


        public void RenderMeaningsToWords()
        {
            foreach (Word w in L1Words)
                w.SetMeaningsFromIndexes();
            foreach (Word w in L2Words)
                w.SetMeaningsFromIndexes();
        }

        public string GetWordByID(int WordID)
        {
            foreach (Word w in StaticLang1Words)
            {
                if (w.ID == WordID) return w.WordName;
            }
            foreach (Word w in StaticLang2Words)
            {
                if (w.ID == WordID) return w.WordName;
            }
            return String.Empty;
        }

        public override string ToString()
        {
            return topicName + ": " + DynamicLanguage1 + " (" + DynamicLang1Count + ") >> " + DynamicLanguage2 + " (" + DynamicLang2Count + ')';
        }

        public override bool Equals(object obj)
        {
            Topic t = obj as Topic;
            if (t == null) return false;
            return (this.topicName.ToLower() == t.topicName.ToLower() &&
                this.DynamicLanguage1.ToLower() == t.DynamicLanguage1.ToLower() &&
                this.DynamicLanguage2.ToLower() == t.DynamicLanguage2.ToLower());
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public void SwitchLanguage()
        {
            langSwitcher = !langSwitcher;
        }
    }
}
