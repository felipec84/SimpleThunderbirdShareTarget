using Microsoft.UI.Xaml;
using Microsoft.Windows.AppLifecycle;
using System;
using System.Linq;
using Windows.ApplicationModel.Activation;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SimpleShareTarget
{
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
                            var firstFile = storageItems.FirstOrDefault();

                            if (firstFile != null)
                            {
                                // We have the file! Pass its name to our main window.
                                // We cast the generic Window to our specific MainWindow type.
                                if (m_window is MainWindow mainWindow)
                                {
                                    mainWindow.ShowSharedFileName(firstFile.Name);
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
