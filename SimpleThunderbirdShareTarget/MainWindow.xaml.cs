using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SimpleThunderbirdShareTarget
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Title = "Simple Share Target";
        }
        /// <summary>
        /// Public method that allows other parts of the app (like App.xaml.cs)
        /// to update the TextBlock in this window.
        /// </summary>
        /// <param name="fileName">The name of the file to display.</param>
        public void ShowSharedFileName(string fileName)
        {
            FileNameTextBlock.Text = $"Received File: {fileName}";
        }
        public void ShowErrorMessage(string message)
        {
            ErrorMessageTextBlock.Text = message;
        }
    }
}
