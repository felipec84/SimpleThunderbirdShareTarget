using Microsoft.UI.Xaml;
using Microsoft.Windows.AppLifecycle;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Windows.ApplicationModel.Activation;
using Windows.Storage;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SimpleThunderbirdShareTarget
{
    public static class ThunderbirdPathProvider
    {
        private const string DefaultThunderbirdPath = @"C:\Program Files\Mozilla Thunderbird\thunderbird.exe";

        public static string GetThunderbirdPath()
        {
            return DefaultThunderbirdPath;
        }
    }

    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        private Window? m_window;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override async void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            // This is the modern way to get activation arguments in WinUI 3.
            var activatedArgs = AppInstance.GetCurrent().GetActivatedEventArgs();

            // Check if the activation kind is ShareTarget.
            if (activatedArgs.Kind == ExtendedActivationKind.ShareTarget)
            {
                // Cast the arguments to the specific type for share targets.
                var shareArgs = activatedArgs.Data as ShareTargetActivatedEventArgs;
                if (shareArgs != null)
                {
                    // The ShareOperation contains the data being shared.
                    var dataPackageView = shareArgs.ShareOperation.Data;

                    // Check if the shared data contains files (StorageItems).
                    if (dataPackageView.Contains(Windows.ApplicationModel.DataTransfer.StandardDataFormats.StorageItems))
                    {
                        try
                        {
                            // Asynchronously get the list of shared files.
                            var storageItems = await dataPackageView.GetStorageItemsAsync();
                            var filePaths = new List<string>();
                            foreach (var item in storageItems.OfType<StorageFile>())
                            {
                                filePaths.Add(item.Path);
                            }
                            // Read Thunderbird path from .ini
                            var thunderbirdPath = ThunderbirdPathProvider.GetThunderbirdPath();

                            if (string.IsNullOrEmpty(thunderbirdPath) || !File.Exists(thunderbirdPath))
                            {
                                m_window = new MainWindow();
                                if (m_window is MainWindow mainWindow)
                                {
                                    mainWindow.ShowThunderbirdPath($"Thunderbird not found at the default location: {thunderbirdPath}");
                                }
                                m_window.Activate();
                                return;
                            }

                            var argsList = string.Join(",", filePaths.Select(f => $"\"{f}\""));
                            var psi = new ProcessStartInfo
                            {
                                FileName = thunderbirdPath,
                                Arguments = $"-compose \"attachment='{argsList}'\"",
                                UseShellExecute = true
                            };
                            Process.Start(psi);
                        }
                        catch (Exception ex)
                        {
                            m_window = new MainWindow();
                            if (m_window is MainWindow mainWindow)
                            {
                                mainWindow.ShowSharedFileName($"Error: {ex.Message}");
                            }
                            m_window.Activate();
                            return;
                        }
                    }
                }
            } else
            {
                // Only show the window for normal launches (not share target)
                m_window = new MainWindow();
                m_window.Activate();
            }
        }
    }
}
