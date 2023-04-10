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
        public string ItemsPath { get; set; }

        private DataTable SecondDataTable;

        public MainWindow()
        {
            InitializeComponent();
            SecondDataTable = new DataTable();
            DataContext = this;
            ItemsPath = AppDomain.CurrentDomain.BaseDirectory + @"/DocumentItems.csv";
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

        private void dataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int id = 0;
            if (dataGrid.SelectedItem != null)
            {
                DataRowView row = (DataRowView)dataGrid.SelectedItem;
                id = Convert.ToInt32(row[0]);
            }

            CurrentRow.Text = id.ToString();

            var itemsTable = new DataTable();
            using (var streamReader = new StreamReader(ItemsPath))
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
                            itemsTable.Columns.Add(new DataColumn(value.Trim()));
                        }
                    }
                    else
                    {
                        var row = itemsTable.NewRow();
                        if (Convert.ToInt32(values[0].Trim()) == id) // dodaj warunek
                        {
                            for (var i = 0; i < values.Length; i++)
                            {
                                row[i] = values[i].Trim();
                            }
                            itemsTable.Rows.Add(row);
                        }
                    }
                }
            }

            detailsGrid.ItemsSource = itemsTable.DefaultView;

        }
    }
}
