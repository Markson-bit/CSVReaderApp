using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data;
using Microsoft.Win32;
using System.IO;


namespace Zadanie
{
    public partial class MainWindow : Window
    {

        public string FilePath { get; set; }

        public class Person
        {
            public string Id { get; set; }
            public string Type { get; set; }
            public string Date { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string City { get; set; }
        }

        public MainWindow()
        {
            InitializeComponent();
            // DataContext = new MainViewModel();
            DataContext = this;
        }

        private void MinimizeButton_Click (object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void LocationButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                FilePath = openFileDialog.FileName;
                CurrentDirectory.Text = FilePath.ToString();

                var dataTable = new DataTable();
                using (var streamReader = new StreamReader(openFileDialog.FileName))
                {
                    var isFirstLine = true;
                    while (!streamReader.EndOfStream)
                    {
                        var line = streamReader.ReadLine();
                        if (string.IsNullOrWhiteSpace(line)) continue;

                        var values = line.Split(';');
                        if (isFirstLine)
                        {
                            isFirstLine = false;
                            foreach (var value in values)
                            {
                                dataTable.Columns.Add(new DataColumn(value.Trim()));
                            }
                        }
                        else
                        {
                            var row = dataTable.NewRow();
                            for (var i = 0; i < values.Length; i++)
                            {
                                row[i] = values[i].Trim();
                            }
                            dataTable.Rows.Add(row);
                        }
                    }
                }
                dataGrid.ItemsSource = dataTable.DefaultView;

            }
        }

    }
}
