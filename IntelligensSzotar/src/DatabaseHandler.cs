using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;

namespace IntelligensSzotar
{
    class DatabaseHandler
    {
        private DatabaseHandler() { }

        private static DatabaseHandler self;
        private string path;
        private XmlDocument database;
        private ProgressFunction progress;
        private enum WordAttributes { ID, Word, Language }
        private string lastTopic;

        private const string rootTag = "Topic";
        private const string wordTag = "W";
        private const string meanTag = "M";
        private const string atrTitle = "title";
        private const string atrLang1 = "language1";
        private const string atrLang2 = "language2";
        private const string atrLastID = "lastID";
        private const string atrID = "id";
        private const string atrWName = "word";
        private const string atrWLang = "lang";
        private const string defExt = "idt";

        public static DatabaseHandler GetInstance()
        {
            if (self == null) self = new DatabaseHandler();
            return self;
        }

        public void DatabaseInit(string DatabasePath, ProgressFunction ProgressCallback)
        {
            CreateDBFolderIfNotExists(DatabasePath);
            path = DatabasePath;
            lastTopic = String.Empty;
            progress = ProgressCallback;
            database = new XmlDocument();
        }

        private void CreateDBFolderIfNotExists(string Folder)
        {
            if (!Directory.Exists(Folder)) Directory.CreateDirectory(Folder);
        }

        private void SaveTopic(string TopicFileName)
        {
            try
            {
                database.Save(path + '\\' + TopicFileName + '.' + defExt);
            }
            catch { }
        }

        private void LoadTopic(string TopicFileName)
        {
            try
            {
                if (lastTopic != TopicFileName)
                {
                    database.Load(path + '\\' + TopicFileName + '.' + defExt);
                    lastTopic = TopicFileName;
                }
            }
            catch { }
        }

        public void AddNewTopicFile(string TopicName, string Language1, string Language2)
        {
            database = new XmlDocument();
            XmlElement root = database.CreateElement(rootTag);
            root.SetAttribute(atrTitle, TopicName);
            root.SetAttribute(atrLang1, Language1);
            root.SetAttribute(atrLang2, Language2);
            root.SetAttribute(atrLastID, "0");
            database.AppendChild(root);
            SaveTopic(TopicName);
        }

        public void AddNewWord(Word Word, Topic Topic)
        {
            LoadTopic(Topic.TopicTitle);
            XmlElement topic = database.DocumentElement;
            XmlElement word = database.CreateElement(wordTag);
            word.SetAttribute(atrID, Word.ID.ToString());
            word.SetAttribute(atrWName, Word.WordName);
            word.SetAttribute(atrWLang, Word.Language);
            XmlElement meaning;
            foreach (int m in Word.MeaningIDs)
            {
                meaning = database.CreateElement(meanTag);
                meaning.SetAttribute(atrID, m.ToString());
                word.AppendChild(meaning);
            }
            topic.AppendChild(word);
            topic.SetAttribute(atrLastID, Word.ID.ToString());
            SaveTopic(Topic.TopicTitle);
        }

        public void AddNewWords(List<Word> Words, Topic Topic)
        {
            foreach (Word w in Words)
            {
                AddNewWord(w, Topic);
            }
        }

        public void AddNewMeaningsToWord(Topic Topic, int WordID, List<int> MeaningWordIDs)
        {
            LoadTopic(Topic.TopicTitle);
            XmlElement topic = database.DocumentElement;
            XmlNodeList words = topic.ChildNodes;
            bool meaningExists;
            foreach (XmlNode word in words)
            {
                if (int.Parse(word.Attributes.Item((int)WordAttributes.ID).Value) == WordID)
                {
                    foreach (int mi in MeaningWordIDs)
                    {
                        meaningExists = false;
                        foreach(XmlElement meaning in word.ChildNodes)
                        {
                            if (meaning.Attributes.Item(0).Value == mi.ToString()) { meaningExists = true; break; }
                        }
                        if (!meaningExists)
                        {
                            XmlElement newMeaning = database.CreateElement(meanTag);
                            newMeaning.SetAttribute(atrID, mi.ToString());
                            word.AppendChild(newMeaning);
                        }
                    }
                }
            }
            SaveTopic(Topic.TopicTitle);
        }

        private Topic ReadFromTopicFile(string FileName)
        {
            database.Load(FileName);
            XmlElement root = database.DocumentElement;
            XmlAttributeCollection ats = root.Attributes;
            string topicName, lang1, lang2, lastID;
            topicName = root.Attributes.Item(0).Value;
            lang1 = root.Attributes.Item(1).Value;
            lang2 = root.Attributes.Item(2).Value;
            lastID = root.Attributes.Item(3).Value;
            Topic topic = new Topic(topicName, lang1, lang2, int.Parse(lastID));
            XmlNodeList words = root.ChildNodes;
            string wname, wlang;
            List<int> mIndexes;
            foreach (XmlNode word in words)
            {
                int id = int.Parse(word.Attributes.Item((int)WordAttributes.ID).Value);
                wname = word.Attributes.Item((int)WordAttributes.Word).Value;
                wlang = word.Attributes.Item((int)WordAttributes.Language).Value;
                XmlNodeList meanings = word.ChildNodes;
                mIndexes = new List<int>();
                foreach (XmlNode meaning in meanings)
                {
                    mIndexes.Add(int.Parse(meaning.Attributes.Item(0).Value));
                }
                Word word2add = new Word(id, wname, wlang, topic, mIndexes);
                topic.AddWordFromDatabase(word2add);
            }

            return topic;
        }

        public List<Topic> ReadAllTopics(string TopicDirectory)
        {
            string[] files = Directory.GetFiles(TopicDirectory);
            List<Topic> topics = new List<Topic>();
            foreach (string topicFile in files)
            {
                topics.Add(ReadFromTopicFile(topicFile));
                progress();
            }
            return topics;
        }

        public void DeleteWord(string TopicName, int WordID)
        {
            LoadTopic(TopicName);
            XmlElement topic = database.DocumentElement;
            XmlNodeList words = topic.ChildNodes;
            List<int> mIDs = new List<int>();
            foreach (XmlNode word in words)
            {
                if ((int.Parse(word.Attributes.Item((int)WordAttributes.ID).Value) == WordID))
                {
                    XmlNodeList meanings = word.ChildNodes;
                    foreach (XmlNode meaning in meanings) mIDs.Add(int.Parse(meaning.Attributes.Item(0).Value));
                    topic.RemoveChild(word);
                    break;
                }
            }
            foreach (int mid in mIDs) DeleteMeaning(mid, WordID);
            SaveTopic(TopicName);
        }

        private void DeleteMeaning(int WordID, int MeaningID)
        {
            DeleteMeanings(WordID, new List<int>() { MeaningID });
        }

        private void DeleteMeanings(int WordID, List<int> MeaningIDsToDelete)
        {
            XmlElement topic = database.DocumentElement;
            XmlNodeList words = topic.ChildNodes;
            foreach (XmlNode word in words)
            {
                if ((int.Parse(word.Attributes.Item((int)WordAttributes.ID).Value) == WordID))
                {
                    XmlNodeList meanings = word.ChildNodes;
                    foreach (int MeaningIDToDelete in MeaningIDsToDelete)
                    {
                        foreach (XmlNode meaning in meanings)
                        {
                            if ((int.Parse(meaning.Attributes.Item(0).Value) == MeaningIDToDelete && MeaningIDToDelete != WordID))
                            {
                                word.RemoveChild(meaning);
                                DeleteMeaning(MeaningIDToDelete, WordID);
                                if (word.ChildNodes.Count == 0) DeleteWord(lastTopic, WordID);
                                break;
                            }
                        }
                    }
                    break;
                }
            }
        }

        public void DeleteMeanings(string TopicName, int WordID, List<int> MeaningIDs)
        {
            LoadTopic(TopicName);
            DeleteMeanings(WordID, MeaningIDs);
            SaveTopic(TopicName);
        }

        public void ModifyWord(string TopicName, int WordID, string NewWord)
        {
            LoadTopic(TopicName);
            XmlElement topic = database.DocumentElement;
            XmlNodeList words = topic.ChildNodes;
            foreach (XmlNode word in words)
            {
                if ((int.Parse(word.Attributes.Item((int)WordAttributes.ID).Value) == WordID))
                {
                    word.Attributes.Item((int)WordAttributes.Word).Value = NewWord;
                }
            }
            SaveTopic(TopicName);
        }
    }
}
