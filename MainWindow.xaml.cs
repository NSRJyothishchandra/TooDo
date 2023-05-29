using System;
using System.Windows;
using TooDo.ViewModels;

namespace TooDo
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new TaskViewModel();
        }

        private void InitializeComponent()
        {
            throw new NotImplementedException();
        }
    }
}
