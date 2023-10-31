using AdaptiveCardViewer.ViewModels;
using Avalonia.Controls;
using System.Diagnostics;

namespace AdaptiveCardViewer.Views
{
    public partial class CompareView : Window
    {
        public CompareView()
        {
            InitializeComponent();
        }

        private void Edit_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var cardModel = ((Button)e.Source).DataContext as CardModel;

            // open cardModel.Uri in default browser
            if (cardModel != null)
            {
                Process.Start(new ProcessStartInfo(cardModel.Uri) { UseShellExecute = true });
            }
        }
    }
}
