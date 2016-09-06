using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Text.RegularExpressions;
using System.IO;
using System.Speech.Recognition;
using System.Diagnostics;

namespace IntelligensSzotar
{
    /// <summary>
    /// Invented and created by David Bakos
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Init();
        }

        delegate void UpdateProgressBarDelegate(DependencyProperty dp, Object value);

        #region Locals

        private static iDictionary iDictionary;
        private double progbarValue;
        private UpdateProgressBarDelegate updateProgBarFnc;
        private SolidColorBrush green, red, blue;
        private SpeechRecogEngineMS speechRecog;
        private bool speechRecogOn, questioningInProgress;
        private List<WordMeta> foundWords;
        private Questioning questioning;
        private TextToSpeech tts;
        private static Random rnd = new Random();

        #endregion

        #region Properties

        public static iDictionary Dictionary
        {
            get { return iDictionary; }
        }

        public static List<Topic> Topics
        {
            get { return iDictionary.Topics; }
        }

        #endregion

        #region Methods

        private void Init()
        {
            green = new SolidColorBrush(Color.FromArgb(255, 125, 248, 67));
            red = new SolidColorBrush(Colors.Red);
            blue = new SolidColorBrush(Colors.LightBlue);
            questioning = new Questioning(QuestionMode.Random, DisplayAnswerChecked, DisplayNextOrPrevious);
            tts = new TextToSpeech();
        }

        private void LoadDictionary()
        {
            string path = @"topics\";
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            int n = Directory.GetFiles(path).Length;
            progbar_loadingwords.Minimum = 0;
            progbar_loadingwords.Maximum = n;
            progbar_loadingwords.Value = 0;
            progbarValue = 0;
            loadingwords.Visibility = Visibility.Visible;
            updateProgBarFnc = new UpdateProgressBarDelegate(progbar_loadingwords.SetValue);
            iDictionary = new iDictionary(path, WordsLoadingProgress);
            dgrid_stat.ItemsSource = Topics;
            combo_topicsOfWords.ItemsSource = Topics;
            list_topics.ItemsSource = Topics;
            loadingwords.Visibility = Visibility.Collapsed;
        }

        private void LoadWordsToRecognize(List<string> WordsToRecognize)
        {
            if (speechRecog != null) speechRecog.LoadNewWordList(WordsToRecognize);
        }

        private void DisplayAnswerChecked(Result Result, string RightAnswer, int GoodAnswers, int WrongAnswers, int NoAnswers)
        {
            label_noAnsw.Content = NoAnswers;
            switch (Result)
            {
                case Result.Helyes:
                    textb_answer.Background = green;
                    label_goodAnsw.Content = GoodAnswers;
                    label_goood.Visibility = Visibility.Visible;
                    btn_nextQuestion.Focus();
                    break;
                case Result.Hibás:
                    textb_answer.Background = red;
                    label_wrongAnsw.Content = WrongAnswers;
                    textb_rightAnswer.Text = RightAnswer;
                    label_rightAnswer.Visibility = Visibility.Visible;
                    textb_rightAnswer.Visibility = Visibility.Visible;
                    btn_nextQuestion.Focus();
                    break;
                case Result.NincsVálasz:
                    textb_answer.Focus();
                    break;
            }


        }

        private void DisplayNextOrPrevious(string Word, int QuestionIndex, Result Result, string Answer, string RightAnswer)
        {
            label_wordIndex.Content = QuestionIndex;
            switch (Result)
            {
                case Result.NincsVálasz:
                    label_nextWord.Content = Word;
                    textb_answer.Background = blue;
                    textb_answer.Text = String.Empty;
                    textb_answer.Focus();
                    label_rightAnswer.Visibility = Visibility.Hidden;
                    textb_rightAnswer.Visibility = Visibility.Hidden;
                    label_goood.Visibility = Visibility.Hidden;
                    break;
                case Result.Helyes:
                    label_nextWord.Content = Word;
                    textb_answer.Background = green;
                    textb_answer.Text = Answer;
                    label_rightAnswer.Visibility = Visibility.Hidden;
                    textb_rightAnswer.Visibility = Visibility.Hidden;
                    label_goood.Visibility = Visibility.Visible;
                    break;
                case Result.Hibás:
                    label_nextWord.Content = Word;
                    textb_answer.Background = red;
                    textb_rightAnswer.Text = RightAnswer;
                    textb_answer.Text = Answer;
                    label_rightAnswer.Visibility = Visibility.Visible;
                    textb_rightAnswer.Visibility = Visibility.Visible;
                    label_goood.Visibility = Visibility.Hidden;
                    break;
            }
        }

        private void SpeechRecogSwitch()
        {
            if (!questioningInProgress)
            {
                MessageBox.Show("Csak a kikérdezés elindítása után használhatod a beszédfelismerés funkciót!");
                return;
            }
            if (combo_speech.SelectedIndex == -1)
            {
                combo_speech.ItemsSource = SpeechRecogEngineMS.InstalledRecogniserNames;
                if (combo_speech.Items.Count > 0) combo_speech.SelectedIndex = 0;
                else
                {
                    MessageBoxResult result = MessageBox.Show("Nincs telepített beszédfelismerő nyelvcsomag a számítógépen. Szeretnéd feltelepíteni?", "Beszédfelismerő telepítése", MessageBoxButton.YesNo);
                    if (result == MessageBoxResult.Yes)
                    {
                        try
                        {
                            Process.Start("rundll32.exe", "url.dll,FileProtocolHandler wuapp.exe");
                        }
                        catch { MessageBox.Show("Hiba történt! Manuálisan kell megkeresned és feltelepítened a megfelelő nyelvcsomagot a Windows update-el."); }
                    }
                    return;
                }
            }
            if (!speechRecogOn)
            {
                TurnOnSpeechRecognizer();
            }
            else
            {
                TurnOffSpeechRecognizer();
            }
            speechRecogOn = !speechRecogOn;
        }

        private void TurnOnSpeechRecognizer()
        {
            if (speechRecog == null) return;
            stack_speech.Visibility = Visibility.Visible;
            btn_speech.Source = new BitmapImage(new Uri("/img/micro_on.png", UriKind.Relative));

            if (!speechRecog.RecognizingON)
            {
                speechRecog.SetDefaultMicrophone();
                speechRecog.StartRecognising();
            }
        }

        private void TurnOffSpeechRecognizer()
        {
            if (speechRecog == null) return;
            stack_speech.Visibility = Visibility.Collapsed;
            btn_speech.Source = new BitmapImage(new Uri("/img/micro_off.png", UriKind.Relative));
            if (speechRecog.RecognizingON) speechRecog.StopRecognising();
        }

        private void RefreshDataSets()
        {
            list_topics.Items.Refresh();
            dgrid_stat.Items.Refresh();
            RefreshComboBox(combo_topicsOfWords, combo_topicsOfWords_SelectionChanged);
            dgrid_words1.Items.Refresh();
            dgrid_words2.Items.Refresh();
        }

        private void RefreshComboBox(ComboBox Combo, SelectionChangedEventHandler ItemSelectionCallback) // Eventhandler 
        {
            int i = Combo.SelectedIndex;
            Combo.SelectionChanged -= ItemSelectionCallback;
            Combo.SelectedIndex = -1;
            Combo.Items.Refresh();
            Combo.SelectedIndex = i;
            Combo.SelectionChanged += ItemSelectionCallback;
        }

        private void WordsLoadingProgress()
        {
            progbarValue++;
            Dispatcher.Invoke(updateProgBarFnc,
            System.Windows.Threading.DispatcherPriority.Background,
            new object[] { ProgressBar.ValueProperty, progbarValue });
        }

        private void DisplayMessage(string Text)
        {
            MessageBox.Show(Text);
        }

        private void AudioLevelChanged(int Level)
        {
            progbar_audio.Value = Level;
        }

        private void SpeechRecognised(string Text)
        {
            textb_answer.Text = Text;
            questioning.CheckAnswer(Text, true);
        }

        private IList SelectedWords()
        {
            if (dgrid_words1.SelectedIndex != -1) return dgrid_words1.SelectedItems;
            else if (dgrid_words2.SelectedIndex != -1) return dgrid_words2.SelectedItems;
            else return null;
        }

        private void DeleteWords()
        {
            MessageBoxResult result = MessageBox.Show("Tényleg törölni akarod a kiválasztott szót/szavakat?", "Törlés megerősítése", MessageBoxButton.YesNo);
            if (result != MessageBoxResult.Yes) return;
            var selectedItems = SelectedWords();
            if (selectedItems == null) return;
            Topic selectedTopic = combo_topicsOfWords.SelectedItem as Topic;
            foreach (Word w in selectedItems)
            {
                selectedTopic.DeleteWord(w);
            }
            RefreshDataSets();
        }

        #endregion

        #region Window Interface Event Handlers

        private void btn_newTopic_Click(object sender, RoutedEventArgs e)
        {
            NewTopic nt = new NewTopic();
            nt.ShowDialog();
            RefreshDataSets();
        }

        private void btn_addWords_Click(object sender, RoutedEventArgs e)
        {
            AddWords aw = new AddWords();
            aw.ShowDialog();
            RefreshDataSets();
        }

        private void textb_findWord_TextChanged(object sender, TextChangedEventArgs e)
        {
            string keyword = textb_findWord.Text;
            if (foundWords != null) foundWords.Clear();
            if (keyword == String.Empty)
            {
                dgrid_found.Visibility = Visibility.Collapsed;
                return;
            }
            else dgrid_found.Visibility = Visibility.Visible;

            foundWords = iDictionary.Search(keyword);
            dgrid_found.ItemsSource = foundWords;
        }

        private void btn_start_Click(object sender, RoutedEventArgs e)
        {
            if (list_topics.SelectedIndex == -1)
            {
                MessageBox.Show("A kikérdezéshez először témát kell választanod!");
                return;
            }
            if (!questioningInProgress)
            {
                List<Topic> questTopics = new List<Topic>();
                foreach (Topic topic in list_topics.SelectedItems)
                {
                    questTopics.Add(topic);
                }
                questioning.SetQuestionTopics(questTopics);
                if (!questioning.StartQuestioning()) return;

                label_wordCount.Content = questioning.QuestionCount;
                label_noAnsw.Content = questioning.QuestionCount;
                questioningInProgress = true;
                btn_start.Content = "Stop";
                btn_statistics.IsEnabled = true;
                btn_nextQuestion.IsEnabled = true;
                btn_prevQuestion.IsEnabled = true;
                btn_check.IsEnabled = true;
                exp_topics.IsExpanded = false;
                list_topics.IsEnabled = false;
                btn_switchChosenLang.IsEnabled = false;
                label_wordIndex.Content = "1";
                label_goodAnsw.Content = "0";
                label_wrongAnsw.Content = "0";

                LoadWordsToRecognize(questioning.GetWordsToRecognize());
                questioning.NextQuestion();
            }
            else
            {
                questioningInProgress = false;
                btn_start.Content = "Start";
                btn_nextQuestion.IsEnabled = false;
                btn_prevQuestion.IsEnabled = false;
                btn_check.IsEnabled = false;
                list_topics.IsEnabled = true;
                btn_switchChosenLang.IsEnabled = true;
                TurnOffSpeechRecognizer();
            }
        }

        private void btn_nextQuestion_Click(object sender, RoutedEventArgs e)
        {
            questioning.NextQuestion();
        }

        private void btn_prevQuestion_Click(object sender, RoutedEventArgs e)
        {
            questioning.PreviousQuestion();
        }

        private void btn_check_Click(object sender, RoutedEventArgs e)
        {
            questioning.CheckAnswer(textb_answer.Text, false);
        }

        private void textb_answer_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter: questioning.CheckAnswer(textb_answer.Text, false); break;
            }
        }

        private void btn_statistics_Click(object sender, RoutedEventArgs e)
        {
            Statistics stat = new Statistics(questioning.Results);
            stat.Show();
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            LoadDictionary();
        }

        private void btn_speech_MouseDown(object sender, MouseButtonEventArgs e)
        {
            SpeechRecogSwitch();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (speechRecog != null) speechRecog.Dispose();
        }

        private void combo_speech_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (speechRecog != null) speechRecog.Dispose();
            string name = combo_speech.SelectedItem as String;
            speechRecog = new SpeechRecogEngineMS(questioning.GetWordsToRecognize(), name, SpeechRecognised, AudioLevelChanged, DisplayMessage);
            speechRecog.StartRecognising();
        }

        private void btn_switchChosenLang_Click(object sender, RoutedEventArgs e)
        {
            foreach (Topic topic in list_topics.SelectedItems)
            {
                topic.SwitchLanguage();
            }
            list_topics.Items.Refresh();
        }

        private void progbar_audio_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Process.Start("RunDLL32.EXE", "shell32.dll,Control_RunDLL mmsys.cpl,,1");
            }
            catch { MessageBox.Show("Hiba történt. A beállítások elvégzését manuálsan kell elvégeznie."); }
        }

        private void combo_topicsOfWords_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Topic selectedTopic = combo_topicsOfWords.SelectedItem as Topic;
            dgrid_words1.ItemsSource = selectedTopic.StaticLang1Words;
            dgrid_words2.ItemsSource = selectedTopic.StaticLang2Words;
            label_firstLang.Content = selectedTopic.StaticLanguage1 + " nyelv szavai:";
            label_secondLang.Content = selectedTopic.StaticLanguage2 + " nyelv szavai:";
        }

        private void menu_lang_hungarian_Click(object sender, RoutedEventArgs e)
        {
            App.Current.Resources.MergedDictionaries[0].Source = new Uri("/res/lang/Hungarian.xaml", UriKind.Relative);
        }

        private void menu_lang_english_Click(object sender, RoutedEventArgs e)
        {
            App.Current.Resources.MergedDictionaries[0].Source = new Uri("/res/lang/English.xaml", UriKind.Relative);
        }

        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (label_nextWord.Content != null)
                tts.Speak(label_nextWord.Content.ToString());
        }

        private void dgrid_words1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgrid_words1.SelectedIndex != -1) { dgrid_words2.SelectedIndex = -1; btn_deleteWord.IsEnabled = btn_modifyWord.IsEnabled = true; }
            else btn_deleteWord.IsEnabled = btn_modifyWord.IsEnabled = false;
        }

        private void dgrid_words2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgrid_words2.SelectedIndex != -1) { dgrid_words1.SelectedIndex = -1; btn_deleteWord.IsEnabled = btn_modifyWord.IsEnabled = true; }
            else btn_deleteWord.IsEnabled = btn_modifyWord.IsEnabled = false;
        }

        private void btn_deleteWord_Click(object sender, RoutedEventArgs e)
        {
            DeleteWords();
        }

        private void btn_modifyWord_Click(object sender, RoutedEventArgs e)
        {
            var selected = SelectedWords();
            if (selected != null && selected.Count > 0)
            {
                ModifyWord modw = new ModifyWord(selected[0] as Word);
                modw.ShowDialog();
                RefreshDataSets();
            }
        }

        private void DataGrid_Words_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Delete: if (btn_deleteWord.IsEnabled) DeleteWords(); break;
            }
        }

        #endregion
    }

}
