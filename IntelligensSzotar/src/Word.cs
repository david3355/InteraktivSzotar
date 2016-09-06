using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IntelligensSzotar
{
    public class Word
    {
        public Word(int ID, string Word, string Language, Topic OwnTopic, List<int> MeaningsIndexes, List<string> Meanings)
        {
            id = ID; // Next Gen
            word = Word;
            language = Language;
            ownTopic = OwnTopic;
            meanings = Meanings;
            mIndexes = MeaningsIndexes;
        }

        public Word(int ID, string Word, string Language, Topic OwnTopic, List<int> MeaningsIndexes)
            : this(ID, Word, Language, OwnTopic, MeaningsIndexes, new List<string>()) { }

        public Word(int ID, string Word, string Language, Topic OwnTopic) : this(ID, Word, Language, OwnTopic, new List<int>(), new List<string>()) { }

        private int id; // Next Gen
        private string word;
        private string language;
        private List<string> meanings;
        private List<int> mIndexes;
        private Topic ownTopic; // Next Gen

        public int ID // Next Gen
        {
            get { return id; }
        }

        public string WordName
        {
            get { return word; }
            set { word = value; }
        }

        public string Language
        {
            get { return language; }
        }

        public List<string> Meanings
        {
            get { return meanings; }
        }

        public List<int> MeaningIDs
        {
            get { return mIndexes; }
        }

        public Topic Topic
        {
            get { return ownTopic; }
        }

        public string MeaningManifest
        {
            get
            {
                StringBuilder stb = new StringBuilder();
                int count = meanings.Count;
                for (int i = 0; i < count; i++)
                {
                    stb.Append(meanings[i]);
                    if (i < count - 1) stb.Append(", ");
                }
                return stb.ToString();
            }
        }

        public void AddNewMeaning(int WordIndex, string Meaning)
        {
            if (!mIndexes.Contains(WordIndex))
            {
                mIndexes.Add(WordIndex);
                meanings.Add(Meaning);
            }
        }

        public void AddNewMeanings(List<int> WordIndexes, List<string> Meaning)
        {
            for (int i = 0; i < WordIndexes.Count; i++)
            {
                AddNewMeaning(WordIndexes[i], Meaning[i]);
            }
        }

        public void DeleteMeaning(int MeaningIndex)
        {
            for (int i = 0; i < mIndexes.Count; i++)
            {
                if (mIndexes[i] == MeaningIndex)
                {
                    mIndexes.RemoveAt(i);
                    meanings.RemoveAt(i);
                    return;
                }
            }
        }

        public string GetWordByID(int WordID)
        {
            return ownTopic.GetWordByID(WordID);
        }

        public void SetMeaningsFromIndexes()
        {
            foreach (int midx in mIndexes)
            {
                meanings.Add(GetWordByID(midx));
            }
        }

        public override string ToString()
        {
            return word;
        }
    }
}
