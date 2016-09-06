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
    /// Interaction logic for NewTopic.xaml
    /// </summary>
    public partial class NewTopic : Window
    {
        public NewTopic()
        {
            InitializeComponent();
        }

        private void btn_addNewTopic_Click(object sender, RoutedEventArgs e)
        {
            string name = textb_newTopicName.Text;
            string lang1 = textb_lang1.Text;
            string lang2 = textb_lang2.Text;
            if (name == String.Empty || lang1 == String.Empty || lang2 == String.Empty) return;
            bool success = MainWindow.Dictionary.AddNewTopic(name, lang1, lang2);
            if (success)
            {
                textb_newTopicName.Text = String.Empty;
                textb_lang1.Text = String.Empty;
                textb_lang2.Text = String.Empty;
                //animáció
            }
            else MessageBox.Show("Már létezik ilyen téma!");
        }
    }
}
