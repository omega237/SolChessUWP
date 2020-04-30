using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows.Input;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Die Elementvorlage "Leere Seite" ist unter http://go.microsoft.com/fwlink/?LinkId=234238 dokumentiert.

namespace SolitaireChess
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class Menu : Page
    {
        public Menu()
        {
            this.InitializeComponent();
        }

        void Handle_EasyClick(object s, RoutedEventArgs e)
        {
            Controller.GetInstance().NavigateTo(0, (int)((Button)e.OriginalSource).Content);
        }

        void Handle_MediumClick(object s, RoutedEventArgs e)
        {
            Controller.GetInstance().NavigateTo(1, (int)((Button)e.OriginalSource).Content);
        }

        void Handle_HardClick(object s, RoutedEventArgs e)
        {
            Controller.GetInstance().NavigateTo(2, (int)((Button)e.OriginalSource).Content);
        }

        void Handle_ExpertClick(object s, RoutedEventArgs e)
        {
            Controller.GetInstance().NavigateTo(3, (int)((Button)e.OriginalSource).Content);
        }


        private void Page_Loading(FrameworkElement sender, object args)
        {
            for (int i = 1; i <= 100; i++)
            {
                Button easyButton = new Button {
                    Content = i,
                    Background = Controller.GetInstance().IsFinished(0, i) ? new SolidColorBrush(Color.FromArgb(100, 0, 255, 0)) : Background
                };
                easyButton.Click += Handle_EasyClick;

                Button mediumButton = new Button
                {
                    Content = i,
                    Background = Controller.GetInstance().IsFinished(0, i) ? new SolidColorBrush(Color.FromArgb(100, 0, 255, 0)) : Background
                };
                mediumButton.Click += Handle_MediumClick;
                Button hardButton = new Button
                {
                    Content = i,
                    Background = Controller.GetInstance().IsFinished(0, i) ? new SolidColorBrush(Color.FromArgb(100, 0, 255, 0)) : Background
                };
                hardButton.Click += Handle_HardClick;
                Button expertButton = new Button
                {
                    Content = i,
                    Background = Controller.GetInstance().IsFinished(0, i) ? new SolidColorBrush(Color.FromArgb(100, 0, 255, 0)) : Background
                };
                expertButton.Click += Handle_ExpertClick;
                easyList.Items.Add(easyButton);
                mediumList.Items.Add(mediumButton);
                hardList.Items.Add(hardButton);
                expertList.Items.Add(expertButton);
            }
        }

        private void flipView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LevelLabel == null)
                return;
            switch(flipView.SelectedIndex)
            {
                case 0:
                    LevelLabel.Text = "Easy";
                    break;
                case 1:
                    LevelLabel.Text = "Medium";
                    break;
                case 2:
                    LevelLabel.Text = "Hard";
                    break;
                case 3:
                    LevelLabel.Text = "Expert";
                    break;
            }
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            easyList.Items.Clear();
            mediumList.Items.Clear();
            hardList.Items.Clear();
            expertList.Items.Clear();
        }
    }
}
