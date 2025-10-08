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
        public static string GetThunderbirdTraditionalPath()
        {
            string programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            string traditionalPath = Path.Combine(programFiles, "Mozilla Thunderbird", "thunderbird.exe");
            return traditionalPath;
        }

        public static string GetThunderbirdPath()
        {
            const string StoreAlias = "thunderbird.exe";
            if (CanLaunchExecutable(StoreAlias))
            {
                return StoreAlias;
            }
            string traditionalPath = GetThunderbirdTraditionalPath();
            if (File.Exists(traditionalPath))
            {
                return traditionalPath;
            }
            return "";
        }

        public static bool CanLaunchExecutable(string executableName)
        {
            // If the path is already a full path, check if the file exists directly
            if (Path.IsPathRooted(executableName) && File.Exists(executableName))
            {
                return true;
            }

            // 1. Get the system PATH environment variable
            string? pathEnv = Environment.GetEnvironmentVariable("PATH");
            if (string.IsNullOrEmpty(pathEnv))
            {
                return false;
            }

            // 2. Split the PATH into individual directories (Windows uses ';' as separator)
            string[] directories = pathEnv.Split(Path.PathSeparator);

            // 3. Check each directory for the executable file
            foreach (string dir in directories)
            {
                // Path.Combine is crucial for handling trailing slashes correctly
                string fullPath = Path.Combine(dir, executableName);

                // Check for the file's existence
                if (File.Exists(fullPath))
                {
                    return true;
                }
            }

            // If the loop finishes without finding the file, it's not in the PATH
            return false;
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
                            var thunderbirdPath = ThunderbirdPathProvider.GetThunderbirdPath();

                            if (string.IsNullOrEmpty(thunderbirdPath))
                            {
                                ShowErrorWindow($"Thunderbird not found, please install via Windows Store or at the default location: {ThunderbirdPathProvider.GetThunderbirdTraditionalPath()}");
                                return;
                            }

                            var attachments = string.Join(",", filePaths.Select(f => $"\"{f}\""));
                            var psi = new ProcessStartInfo
                            {
                                FileName = thunderbirdPath,
                                Arguments = $"-compose \"attachment='{attachments}'\"",
                                UseShellExecute = true
                            };
                            Process.Start(psi);
                        }
                        catch (Exception ex)
                        {
                            ShowErrorWindow($"Error processing shared files: {ex.Message}");
                        }
                    }
                }
            } else
            {
                // This is the normal launch path.
                // It will be triggered when the user clicks the app icon.
                ShowErrorWindow("This app is a share target for Thunderbird. To use it, share a file and select this app.");
            }
        }

        private void ShowErrorWindow(string message)
        {
            // Ensure UI updates happen on the UI thread.
            // Although OnActivated is on the UI thread, this is a good practice.
            m_window = new MainWindow();
            if (m_window is MainWindow mainWindow)
            {
                mainWindow.ShowThunderbirdPath(message);
            }
            m_window.Activate();
        }
    }
}
