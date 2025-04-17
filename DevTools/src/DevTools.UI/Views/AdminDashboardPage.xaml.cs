using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using DevTools.UI.ViewModels;
using DevTools.UI.Models;
using DevTools.UI.Services;
using Microsoft.Extensions.DependencyInjection;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace DevTools.UI.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AdminDashboardPage : Page
    {
        private AdminDashboardViewModel ViewModel => DataContext as AdminDashboardViewModel;

        public AdminDashboardPage()
        {
            this.InitializeComponent();
            DataContext = AppServices.AdminDashboardViewModel;
        }

        private async void UploadTool_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog dialog = new ContentDialog
            {
                Title = "Upload New Tool",
                PrimaryButtonText = "Upload",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.XamlRoot
            };

            // Create the content for the dialog
            StackPanel panel = new StackPanel { Spacing = 10 };

            TextBox nameBox = new TextBox
            {
                Header = "Tool Name",
                PlaceholderText = "Enter tool name"
            };
            panel.Children.Add(nameBox);

            TextBox descriptionBox = new TextBox
            {
                Header = "Description",
                PlaceholderText = "Enter tool description",
                TextWrapping = TextWrapping.Wrap,
                AcceptsReturn = true,
                Height = 100
            };
            panel.Children.Add(descriptionBox);

            ToggleSwitch premiumSwitch = new ToggleSwitch
            {
                Header = "Premium Tool",
                OffContent = "No",
                OnContent = "Yes"
            };
            panel.Children.Add(premiumSwitch);

            Button fileButton = new Button { Content = "Select DLL File" };
            TextBlock fileNameBlock = new TextBlock { Text = "No file selected" };
            panel.Children.Add(fileButton);
            panel.Children.Add(fileNameBlock);

            dialog.Content = panel;

            // Handle file selection (simplified for demo)
            fileButton.Click += (s, args) =>
            {
                fileNameBlock.Text = "example.dll";
            };

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                // Process upload (implementation would need access to actual file stream)
                // This is a placeholder for demonstration
                ContentDialog uploadingDialog = new ContentDialog
                {
                    Title = "Uploading Tool",
                    Content = "Your tool is being uploaded and processed...",
                    XamlRoot = this.XamlRoot
                };

                // Show success message (in production, this would await the actual upload)
                await uploadingDialog.ShowAsync();

                ContentDialog successDialog = new ContentDialog
                {
                    Title = "Success",
                    Content = $"Tool '{nameBox.Text}' has been uploaded successfully.",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };

                await successDialog.ShowAsync();
            }
        }

        private void ToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            if (sender is ToggleSwitch toggle && toggle.DataContext is Tool tool)
            {
                ViewModel.SetToolEnabled(tool);
            }
        }

        private async void EditTool_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Tool tool)
            {
                // Open edit dialog (simplified)
                ContentDialog dialog = new ContentDialog
                {
                    Title = $"Edit Tool: {tool.Name}",
                    PrimaryButtonText = "Save",
                    CloseButtonText = "Cancel",
                    DefaultButton = ContentDialogButton.Primary,
                    XamlRoot = this.XamlRoot
                };

                // Create the content for the dialog
                StackPanel panel = new StackPanel { Spacing = 10 };

                TextBox nameBox = new TextBox
                {
                    Header = "Tool Name",
                    Text = tool.Name
                };
                panel.Children.Add(nameBox);

                TextBox descriptionBox = new TextBox
                {
                    Header = "Description",
                    Text = tool.Description,
                    TextWrapping = TextWrapping.Wrap,
                    AcceptsReturn = true,
                    Height = 100
                };
                panel.Children.Add(descriptionBox);

                ToggleSwitch premiumSwitch = new ToggleSwitch
                {
                    Header = "Premium Tool",
                    IsOn = tool.IsPremium,
                    OffContent = "No",
                    OnContent = "Yes"
                };
                panel.Children.Add(premiumSwitch);

                dialog.Content = panel;

                var result = await dialog.ShowAsync();

                if (result == ContentDialogResult.Primary)
                {
                    // Update tool properties (in a real app, you'd call an API)
                    tool.Name = nameBox.Text;
                    tool.Description = descriptionBox.Text;
                    tool.IsPremium = premiumSwitch.IsOn;

                    // Show success message
                    ContentDialog successDialog = new ContentDialog
                    {
                        Title = "Success",
                        Content = $"Tool '{tool.Name}' has been updated successfully.",
                        CloseButtonText = "OK",
                        XamlRoot = this.XamlRoot
                    };

                    await successDialog.ShowAsync();
                }
            }
        }
    }
}