using System;
using System.Windows;
using Encryptor.Presentation.ViewModel;

namespace Encryptor.Presentation.Views
{
    /// <summary>
    /// Interaction logic for MainView.xaml
    /// </summary>
    public partial class MainView : Window
    {
        public MainView()
        {
            this.InitializeComponent();
        }

        private void MainWindow_OnDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                var viewModel = (MainViewModel)DataContext;
                viewModel.DropFiles(files);
                viewModel.ShowDropPanel = false;
            }
        }

        private void MainWindow_OnDragEnter(object sender, DragEventArgs e)
        {
            var viewModel = (MainViewModel)DataContext;
            viewModel.ShowDropPanel = true;
        }

        private void MainWindow_OnDragLeave(object sender, DragEventArgs e)
        {
            var viewModel = (MainViewModel)DataContext;
            viewModel.ShowDropPanel = false;
        }
    }
}
