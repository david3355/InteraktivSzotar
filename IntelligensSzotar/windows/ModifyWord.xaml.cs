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
    /// Interaction logic for ModifyWord.xaml
    /// </summary>
    public partial class ModifyWord : Window
    {
        public ModifyWord(Word WordToModify)
        {
            InitializeComponent();
            database = DatabaseHandler.GetInstance();
            meaningsToDelete = new List<int>();
            LoadWord(WordToModify);
        }
        private Word w;
        private DatabaseHandler database;
        private List<int> meaningsToDelete;

        private void LoadWord(Word Word)
        {
            w = Word;
            txt_word.Text = w.WordName;
            for (int i = 0; i < w.MeaningIDs.Count; i++)
            {
                list_meanings.Items.Add(new MeaningTemplate() { Meaning = w.Meanings[i], MeaningID = w.MeaningIDs[i] });
            }

        }

        private void DeleteMeaning_click(object sender, MouseButtonEventArgs e)
        {
            int si = list_meanings.SelectedIndex;
            if (si != -1)
            {
                if (list_meanings.Items.Count == 1)
                {
                    MessageBox.Show("A szónak legalább egy jelentéssel rendelkeznie kell!");
                    return;
                }
                meaningsToDelete.Add((list_meanings.SelectedItem as MeaningTemplate).MeaningID);
                list_meanings.Items.RemoveAt(si);
            }
        }

        private void btn_save_Click(object sender, RoutedEventArgs e)
        {
            if (w.WordName != txt_word.Text) w.Topic.ModifyWord(w, txt_word.Text);
            if (meaningsToDelete.Count > 0) w.Topic.ModifyWord(w, meaningsToDelete);
            this.Close();
        }
    }
}
