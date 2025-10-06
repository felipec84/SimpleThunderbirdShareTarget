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

namespace SimpleShareTarget
{
    public static class ThunderbirdPathProvider
    {
        private const string IniFileName = "thunderbird.ini";
        private const string Key = "Path";
        private const string DefaultThunderbirdPath = @"C:\Program Files\Mozilla Thunderbird\thunderbird.exe";

        public static async System.Threading.Tasks.Task<string> GetThunderbirdPathAsync()
        {
            var folder = Windows.ApplicationModel.Package.Current.InstalledLocation;
            var iniFile = await folder.TryGetItemAsync(IniFileName) as StorageFile;
            if (iniFile == null)
            {
                string defaultContent = $"{Key}={DefaultThunderbirdPath}";
                iniFile = await folder.CreateFileAsync(
                    IniFileName,
                    CreationCollisionOption.ReplaceExisting
                );
                await FileIO.WriteTextAsync(iniFile, defaultContent);
                return DefaultThunderbirdPath;
            }


            var lines = await File.ReadAllLinesAsync(iniFile.Path);
            foreach (var line in lines)
            {
                if (line.StartsWith(Key + "=", System.StringComparison.OrdinalIgnoreCase))
                {
                    return line.Substring(Key.Length + 1).Trim();
                }
            }
            return null;
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
            m_window = new MainWindow();

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
                            var thunderbirdPath = await ThunderbirdPathProvider.GetThunderbirdPathAsync();

                            if (string.IsNullOrEmpty(thunderbirdPath) || !File.Exists(thunderbirdPath))
                            {
                                if (m_window is MainWindow mainWindow)
                                {
                                    var folder = Windows.ApplicationModel.Package.Current.InstalledLocation;
                                    mainWindow.ShowThunderbirdPath($"Thunderbird path not found, ini searched on {folder.Path}.");
                                }
                            }
                            else
                            {
                                if (m_window is MainWindow mainWindow)
                                {
                                    mainWindow.ShowThunderbirdPath(thunderbirdPath);
                                }
                            }

                            var argsList = string.Join(" ", filePaths.Select(f => $"\"{f}\""));
                            var psi = new ProcessStartInfo
                            {
                                FileName = thunderbirdPath,
                                Arguments = $"-compose \"attachment='{argsList}'\"",
                                UseShellExecute = true
                            };
                            Process.Start(psi);

                            var firstFile = storageItems.FirstOrDefault();

                            if (argsList != null)
                            {
                                // We have the file! Pass its name to our main window.
                                // We cast the generic Window to our specific MainWindow type.
                                if (m_window is MainWindow mainWindow)
                                {
                                    mainWindow.ShowSharedFileName(argsList);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            // Handle potential errors, e.g., access denied.
                            // For this simple app, we can just show the error message.
                            if (m_window is MainWindow mainWindow)
                            {
                                mainWindow.ShowSharedFileName($"Error: {ex.Message}");
                            }
                        }
                    }
                }
            }

            m_window.Activate();
        }
    }
}
