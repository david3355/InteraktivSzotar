using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace IntelligensSzotar
{
    /// <summary>
    /// Interaction logic for AddWords.xaml
    /// </summary>
    public partial class AddWords : Window
    {
        public AddWords()
        {
            InitializeComponent();
            LoadTopics();
        }

        #region Methods

        private void LoadTopics()
        {
            combo_topics.ItemsSource = MainWindow.Dictionary.Topics;
        }

        private void AddWord()
        {
            int idx = combo_topics.SelectedIndex;
            if (idx == -1)
            {
                MessageBox.Show("A szavak hozzáadásához előbb válassz témát!");
                return;
            }
            List<string> words1 = new List<string>();
            List<string> words2 = new List<string>();

            foreach (TextBox txtb in stack_words1.Children)
            {
                if (txtb.Text != String.Empty) words1.Add(txtb.Text);
            }

            foreach (TextBox txtb in stack_words2.Children)
            {
                if (txtb.Text != String.Empty) words2.Add(txtb.Text);
            }

            MainWindow.Dictionary.Topics[idx].AddNewWord(words1, words2);

            RefreshComboBox();
            RefreshPanel(stack_words2);
            RefreshPanel(stack_words1).Focus();
        }

        private TextBox RefreshPanel(StackPanel Panel)
        {
            var words = Panel.Children;
            if (words.Count > 1) words.RemoveRange(1, words.Count - 1);
            TextBox firstTxt = words[0] as TextBox;
            firstTxt.Text = String.Empty;
            return firstTxt;
        }

        private void RefreshComboBox()
        {
            int i = combo_topics.SelectedIndex;
            combo_topics.SelectionChanged -= combo_topics_SelectionChanged;
            combo_topics.SelectedIndex = -1;
            combo_topics.Items.Refresh();
            combo_topics.SelectedIndex = i;
            combo_topics.SelectionChanged += combo_topics_SelectionChanged;
        }

        private void AddNewMeaning(int lang)
        {
            TextBox textb = new TextBox();
            textb.Margin = new Thickness(10, 0, 15, 5);
            StackPanel s; ScrollViewer sv;
            if (lang == 1) s = stack_words1; else s = stack_words2;
            if (lang == 1) sv = scroll_words1; else sv = scroll_words2;
            s.Children.Add(textb);
            sv.ScrollToBottom();
            textb.Focus();
        }

        #endregion

        #region EventHandlers

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter: AddWord(); break;
                case Key.OemPlus:
                    if (stack_words1.IsFocused) AddNewMeaning(1);
                    if (stack_words2.IsFocused) AddNewMeaning(2);
                    break;
            }
        }

        private void btn_addWord_Click(object sender, RoutedEventArgs e)
        {
            AddWord();
        }

        private void btn_ready_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void combo_topics_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (combo_topics.SelectedIndex == -1) return;
            Topic t = (combo_topics.SelectedItem as Topic);
            label_lang1.Content = t.StaticLanguage1;
            label_lang2.Content = t.StaticLanguage2;
        }

        private void btn_newMeaning1_Click(object sender, RoutedEventArgs e)
        {
            AddNewMeaning(1);
        }

        private void btn_newMeaning2_Click(object sender, RoutedEventArgs e)
        {
            AddNewMeaning(2);
        }

        #endregion
    }
}
