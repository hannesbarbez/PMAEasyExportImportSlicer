using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace PMAEasyExportImportSlicer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml - Took 2 hours to write this app so it can most definitely be improved.
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string EXTENSION = ".sql";
        private const string FILEFILTER = "SQL files (*" + EXTENSION + ")| *" + EXTENSION + "|All files (*.*)|*.*";
        private const string SPLITON = ");\n";//Part of SQL to Split To
        private const int MAXBYTES = 2048000; //Split in files up to 2MB
        private List<string> ResultingSet = new List<string>();
        private int currentIndex = 0;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnOpenSqlFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = FILEFILTER
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string combo = "";
                int fileNumber = 0;
                ResultingSet = new List<string>();
                currentIndex = 0;
                string path = openFileDialog.FileName;
                tbFinished.Text = "";
                if (File.Exists(path))
                {
                    string file = File.ReadAllText(openFileDialog.FileName);
                    string[] lines = file.Split(new string[] { SPLITON }, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0; i < lines.Length; i++)
                    {
                        lines[i] = lines[i] + SPLITON; //Correct syntax lost when we were splitting file up
                        if (lines.Length == i + 1)
                        {   //Save the last round also even though may be far from mayBytes
                            if (combo.Length > 0)
                            {
                                ResultingSet.Add(combo);
                                File.WriteAllText(path.Substring(0, path.Length - 4) + fileNumber + "-" + DateTime.Now.Hour + DateTime.Now.Minute + DateTime.Now.Second + EXTENSION, combo);
                            }
                        }
                        else if (sizeof(char) * (combo.Length + lines[i].Length) < MAXBYTES)
                            combo += lines[i];
                        else
                        {
                            string filename = path.Substring(0, path.Length - 4) + fileNumber + "-" + DateTime.Now.Hour + DateTime.Now.Minute + DateTime.Now.Second + EXTENSION;
                            if (combo.Length > 0)
                            {
                                ResultingSet.Add(combo);
                                File.WriteAllText(filename, combo);
                            }
                            fileNumber++;
                            combo = lines[i];
                        }
                    }
                    tbFinished.Visibility = Visibility.Visible;
                    btnCopyContents.Visibility = Visibility.Visible;
                    btnCopyContents.Content = "Copy contents of file 1 (of " + ResultingSet.Count + ") to clipboard";
                }
            }
        }

        private void btnCopyContents_Click(object sender, RoutedEventArgs e)
        {
            string finishedMsg = "That was the last file! Well done, no more files to copy to clipboard.";
            string finishedTtl = "Finished!";
            try
            {
                if (cbShowBelow.IsChecked == true) tbFile.Text = ResultingSet[currentIndex];
                Clipboard.SetText(ResultingSet[currentIndex]);
                currentIndex++;
                if (currentIndex + 1 <= ResultingSet.Count)
                    btnCopyContents.Content = "Copy contents of file " + (currentIndex + 1) + " (of " + ResultingSet.Count + ") to clipboard.";
                else
                {
                    MessageBox.Show(finishedMsg, finishedTtl, MessageBoxButton.OK, MessageBoxImage.Information);
                    btnCopyContents.IsEnabled = false;
                }
            }
            catch
            {
                MessageBox.Show(finishedMsg, finishedTtl, MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void lblAbout_MouseDown(object sender, MouseButtonEventArgs e)
        {
            RunExternalProcess("explorer.exe", "http://" + lblAbout.Content);
        }

        private void lblAbout_MouseDown(object sender, TouchEventArgs e)
        {
            RunExternalProcess("explorer.exe", "http://" + lblAbout.Content);
        }

        private static void RunExternalProcess(string appPath, string args)
        {
            ProcessStartInfo info = new ProcessStartInfo(appPath, @args)
            {
                UseShellExecute = false
            };

            Process p = new Process
            {
                StartInfo = info
            };

            p.Start();
        }

        private void cbShowBelow_Checked(object sender, RoutedEventArgs e)
        {
            if (cbShowBelow.IsChecked == true && ResultingSet.Count > 0 && ResultingSet.Count >= currentIndex)
                tbFile.Text = ResultingSet[currentIndex];
        }
    }
}
