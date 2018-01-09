using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Management.Automation;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace SpectreCheck
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            using (PowerShell PowerShellInstance = PowerShell.Create())
            {
                // use "AddScript" to add the contents of a script file to the end of the execution pipeline.
                // use "AddCommand" to add individual commands/cmdlets to the end of the execution pipeline.
                PowerShellInstance.AddScript(
                    "$SaveExecutionPolicy = Get-ExecutionPolicy;" +
                    "Set-ExecutionPolicy RemoteSigned -Scope Currentuser;" +
                    "Import-Module .\\SpeculationControl.psd1;" +
                    "Get-SpeculationControlSettings;" +
                    "Set-ExecutionPolicy $SaveExecutionPolicy -Scope Currentuser;"
                );

                // invoke execution on the pipeline (collecting output)
                Collection<PSObject> PSOutput = PowerShellInstance.Invoke();

                // Get Result
                if (PSOutput.Count == 1 && PSOutput[0] != null)
                {
                    Display_Result(PSOutput[0].ToString());
                }
            }
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private void Display_Result(string Output)
        {
            // Convert output to lines[]
            string[] Lines = Output.Replace("@{", "").Replace("}", "").Split(';');

            // Activate Result Screem and Hidden Scanning
            Result.Visibility = Visibility.Visible;
            Scanning.Visibility = Visibility.Collapsed;

            foreach (string Line in Lines)
            {
                string[] data = Line.Trim().Split('=');
                String Name = data[0];
                String Value = data[1];

                if (FindName(Name) != null)
                {
                    // Enable Pannel
                    ((StackPanel)FindName(Name)).Visibility = Visibility.Visible;

                    if (FindName(Name + "_" + Value) != null)
                    {
                        // Enable True/False
                        ((Label)FindName(Name + "_" + Value)).Visibility = Visibility.Visible;
                    }

                    // Suggest BTIHardwarePresent
                    if (Name == "BTIHardwarePresent" && Value == "False")
                    {
                        // Enable Suggest
                        Suggested.Visibility = Visibility.Visible;
                        Suggested_BTIHardwarePresent.Visibility = Visibility.Visible;
                    }

                    // Suggested_Update
                    if (Name == "BTIWindowsSupportPresent" && Value == "False" || Name == "KVAShadowWindowsSupportPresent" && Value == "False")
                    {
                        // Enable Suggest
                        Suggested.Visibility = Visibility.Visible;
                        Suggested_Update.Visibility = Visibility.Visible;
                    }

                    // Suggest Suggested_Support
                    if ((Name == "BTIHardwarePresent" && Value == "True" && Name == "BTIWindowsSupportEnabled" && Value == "False") ||
                        (Name == "KVAShadowRequired" && Value == "True" && Name == "KVAShadowWindowsSupportEnabled" && Value == "False"))
                    {
                        // Enable Suggest
                        Suggested.Visibility = Visibility.Visible;
                        Suggested_Support.Visibility = Visibility.Visible;
                    }
                }
            }
        }
    }
}
