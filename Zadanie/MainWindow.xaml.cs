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
using System.Data.SQLite;
using System.Xml.Linq;

namespace Zadanie
{
    public partial class MainWindow : Window
    {

        // Deklaracja wlasciwosci, ktore przechowuja sciezki do plikow i do bazy danych
        public string FilePath { get; set; }
        public string ItemsPath { get; set; }
        public string DatabasePath { get; set; }

        private readonly DataTable SecondDataTable;

        public MainWindow()
        {
            InitializeComponent();

            SecondDataTable = new DataTable();
            DataContext = this;

            ItemsPath = AppDomain.CurrentDomain.BaseDirectory + @"..\\..\\\data\DocumentItems.csv";
            FilePath = AppDomain.CurrentDomain.BaseDirectory + @"..\\..\\\data\Documents.csv";
            DatabasePath = AppDomain.CurrentDomain.BaseDirectory + @"..\\..\\\data\database.db";
          
            // Wyswietlanie sciezki do pliku Documents
            CurrentDirectory.Text = FilePath.ToString();

        }

        // Metoda obsglugujaca przycisk minimalizowania okna
        private void MinimizeButton_Click (object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        // Metoda obsglugujaca przycisk zamkniecie okna aplikacji
        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        // Metoda obslugujaca zmiane wiersza w "lewej" tabeli
        private void dataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int id = 0;
            if (dataGrid.SelectedItem != null)
            {
                // Wychwytywanie potencjalnego bledu podczas wyboru np. pustego wiersza
                try
                {
                    // Zapisanie do zmiennej zawartosci wybranego wiersza z kolumny 1
                    DataRowView row = (DataRowView)dataGrid.SelectedItem;
                    id = Convert.ToInt32(row[0]);
                }
                catch { };
            }

            // Zmienna do przechowywania wartosci w formie tabelarycznej
            var itemsTable = new DataTable();

            // Polaczenie z baza danych
            using (var connection = new SQLiteConnection($"Data Source={DatabasePath}"))
            {
                connection.Open();
                using (var command = new SQLiteCommand(connection))
                {
                    // Pobranie z bazy danych zawartosci wierszy, ktorych atrybut DocumentId jest rowny wybranemu id wiersza, pomijajac pierwszy wiersz, ktory jest naglowkiem
                    command.CommandText = $"SELECT * FROM Item WHERE DocumentId = {id}";
                    using (var reader = command.ExecuteReader())
                    {
                        var isFirstResult = true;
                        while (reader.Read())
                        {
                            if (isFirstResult)
                            {
                                isFirstResult = false;
                                for (var i = 0; i < reader.FieldCount; i++)
                                {
                                    itemsTable.Columns.Add(new DataColumn(reader.GetName(i).Trim()));
                                }
                            }

                            var row = itemsTable.NewRow();
                            for (var i = 0; i < reader.FieldCount; i++)
                            {
                                row[i] = reader.GetValue(i).ToString().Trim();
                            }
                            itemsTable.Rows.Add(row);
                        }
                    }
                }
            }

            // Zmiana zrodla tabeli 
            detailsGrid.ItemsSource = itemsTable.DefaultView;
        }

        // Metoda obslugujaca przycisk Save
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var existingIds = new HashSet<string>();
            using (var connection = new SQLiteConnection($"Data Source={DatabasePath};Version=3;"))
            {
                connection.Open();
                var selectCommand = new SQLiteCommand("SELECT ID FROM Document", connection);
                using (var reader = selectCommand.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        existingIds.Add(reader.GetValue(0).ToString());
                    }
                }
            }

            using (var streamReader = new StreamReader(FilePath))
            {
                streamReader.ReadLine();

                while (!streamReader.EndOfStream)
                {
                    var line = streamReader.ReadLine();
                    var values = line.Split(';');

                    var id = values[0];
                    var name = values[1];
                    var date = values[2];
                    var firstName = values[3];
                    var lastName = values[4];
                    var city = values[5];

                    if (existingIds.Contains(id))
                    {
                        // Skip adding record if ID already exists
                        continue;
                    }

                    using (var connection = new SQLiteConnection($"Data Source={DatabasePath};Version=3;"))
                    {
                        connection.Open();
                        var command = new SQLiteCommand("INSERT INTO Document VALUES (@id, @name, @date, @firstName, @lastName, @city)", connection);
                        command.Parameters.AddWithValue("@id", id);
                        command.Parameters.AddWithValue("@name", name);
                        command.Parameters.AddWithValue("@date", date);
                        command.Parameters.AddWithValue("@firstName", firstName);
                        command.Parameters.AddWithValue("@lastName", lastName);
                        command.Parameters.AddWithValue("@city", city);
                        command.ExecuteNonQuery();
                    }
                }
            }

            var existingDocumentIds = new HashSet<string>();
            using (var connection = new SQLiteConnection($"Data Source={DatabasePath};Version=3;"))
            {
                connection.Open();
                var selectCommand = new SQLiteCommand("SELECT DocumentId FROM Item", connection);
                using (var reader = selectCommand.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        existingDocumentIds.Add(reader.GetValue(0).ToString());
                    }
                }
            }

            using (var streamReader = new StreamReader(ItemsPath))
            {
                streamReader.ReadLine();

                while (!streamReader.EndOfStream)
                {
                    var line = streamReader.ReadLine();
                    var values = line.Split(';');

                    var documentId = values[0];
                    var ordinal = values[1];
                    var product = values[2];
                    var quantity = values[3];
                    var price = values[4];
                    var taxRate = values[5];

                    if (existingDocumentIds.Contains(documentId))
                    {
                        // Skip adding record if ID already exists
                        continue;
                    }

                    using (var connection = new SQLiteConnection($"Data Source={DatabasePath};Version=3;"))
                    {
                        connection.Open();
                        var command = new SQLiteCommand("INSERT INTO Item VALUES (@DocumentId, @Ordinal, @Product, @Quantity, @Price, @TaxRate)", connection);
                        command.Parameters.AddWithValue("@DocumentId", documentId);
                        command.Parameters.AddWithValue("@Ordinal", ordinal);
                        command.Parameters.AddWithValue("@Product", product);
                        command.Parameters.AddWithValue("@Quantity", quantity);
                        command.Parameters.AddWithValue("@Price", price);
                        command.Parameters.AddWithValue("@TaxRate", taxRate);
                        command.ExecuteNonQuery();
                    }
                }
            }
        }

        // Metoda obslugujaca przycisk Load
        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            // Zmienna do przechowywania wartosci w formie tabelarycznej
            var dataTable = new DataTable();

            // Polaczenie z baza danych
            using (var connection = new SQLiteConnection($"Data Source={DatabasePath};Version=3;"))
            {
                connection.Open();

                // Pobranie zawartosci bazy danych, pomijajac pierwszy wiersz, ktory jest naglowkiem i zapisywanie do zmiennej dataTable
                var command = new SQLiteCommand("SELECT * FROM Document", connection);
                using (var reader = command.ExecuteReader())
                {
                    var isFirstRow = true;
                    while (reader.Read())
                    {
                        if (isFirstRow)
                        {
                            isFirstRow = false;
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                dataTable.Columns.Add(new DataColumn(reader.GetName(i)));
                            }
                        }
                        var row = dataTable.NewRow();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            row[i] = reader.GetValue(i);
                        }
                        dataTable.Rows.Add(row);
                    }
                }
            }
            // Zmiana zrodla tabeli 
            dataGrid.ItemsSource = dataTable.DefaultView;
        }

        // Metoda obslugujaca przycisk Clear data
        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            // Usuwanie zawartosci datagrid
            ((DataView)dataGrid.ItemsSource).Table.Clear();

            // Polaczenie z baza danych i wykonanie polecenia Delete dla obu tabel
            using (var connection = new SQLiteConnection($"Data Source={DatabasePath};Version=3;"))
            {
                connection.Open();

                // Usunięcie wszystkich rekordów z tabeli Document
                var command = new SQLiteCommand("DELETE FROM Document", connection);
                command.ExecuteNonQuery();

                // Usunięcie wszystkich rekordów z tabeli Items
                command = new SQLiteCommand("DELETE FROM Item", connection);
                command.ExecuteNonQuery();

                connection.Close();
            }

        }
    }
}
