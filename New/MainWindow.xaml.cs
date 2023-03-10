using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
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
using static System.Reflection.Metadata.BlobBuilder;

namespace New
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        SqlConnection? connection = null;
        SqlDataAdapter? adapter = null;
        DataTable? table = null;

        public MainWindow()
        {
            InitializeComponent();
            Configure();


        }

        private void Configure()
        {
            var configuration = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory() + "../../../../").AddJsonFile("appsettings.json").Build();

            connection = new SqlConnection();
            connection.ConnectionString = configuration.GetConnectionString("System.Data.SqlClient");
            adapter = new SqlDataAdapter(string.Empty, connection);
            table = new();

            table.Columns.Add("Id");
            table.Columns.Add("Name");
            table.Columns.Add("Pages");
            table.Columns.Add("YearPress");
            table.Columns.Add("Id_Author");
            table.Columns.Add("Id_Themes");
            table.Columns.Add("Id_Category");
            table.Columns.Add("Id_Press");
            table.Columns.Add("Comment");
            table.Columns.Add("Quantity");
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SqlDataReader? result = null;


            try
            {
                connection?.Open();

                using SqlCommand command = new SqlCommand("WAITFOR DELAY '00:00:04'; SELECT * FROM Authors;", connection);
                result = await command.ExecuteReaderAsync();

                while (result.Read())
                {

                    int? id = result["Id"] as int?;
                    string? firstName = result["FirstName"] as string;
                    string? lastName = result["LastName"] as string;

                    Authors.Items.Add(id + " " + firstName + " " + lastName);
                }

                MessageBox.Show("Authors Loaded");

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                connection?.Close();
                result?.Close();
            }
        }

        private async void Authors_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!Categories.IsEnabled)
                Categories.IsEnabled = !Categories.IsEnabled;

            Categories.Items.Clear();

            SqlDataReader? result = null;

            try
            {
                connection?.Open();

                var id = Authors.SelectedItem.ToString()?.Split(' ')[0];

                if (id is null)
                    return;

                using SqlCommand command = new SqlCommand($"WAITFOR DELAY '00:00:02'; SELECT DISTINCT Categories.[Name] FROM Categories\r\nJOIN Books ON Id_Category = Categories.Id\r\nJOIN Authors ON Id_Author = Authors.Id\r\nWHERE Authors.Id = {id}", connection);
                result = await command.ExecuteReaderAsync();

                while (result.Read())
                    Categories.Items.Add(result["Name"] as string);

                MessageBox.Show("Categories Loaded");

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                connection?.Close();
                result?.Close();
            }
        }

        private async void Categories_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Categories.Items.IsEmpty || adapter is null)
                return;
            SqlDataReader? result = null;

            try
            {
                connection?.Open();

                var id = Authors.SelectedItem.ToString()?.Split(' ')[0];
                var name = Categories.SelectedItem.ToString();

                using SqlCommand command = new SqlCommand($"WAITFOR DELAY '00:00:02'; SELECT Books.Id, Books.Name, Pages, YearPress, Id_Themes, Id_Category, Id_Author, Id_Press, Comment, Quantity FROM Books\r\nJOIN Categories ON Categories.Id = Id_Category \r\nJOIN Authors ON Authors.Id = Id_Author \r\nWHERE Categories.Name = '{name}' AND Id_Author = {id}\r\n", connection);

                result = await command.ExecuteReaderAsync();

                while (result.Read())
                {
                    var row = table?.NewRow();

                    if (row != null)
                    {
                        row["Id"] = result["Id"];
                        row["Name"] = result["Name"];
                        row["Pages"] = result["Pages"];
                        row["YearPress"] = result["YearPress"];
                        row["Id_Author"] = result["Id_Author"];
                        row["Id_Themes"] = result["Id_Themes"];
                        row["Id_Category"] = result["Id_Category"];
                        row["Id_Press"] = result["Id_Press"];
                        row["Comment"] = result["Comment"];
                        row["Quantity"] = result["Quantity"];

                        table?.Rows.Add(row);
                    }
                }

                Books.ItemsSource = table?.AsDataView();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                connection?.Close();
                result?.Close();
            }
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SearchTxt.Text) || connection?.State is ConnectionState.Open)
                return;

            SqlDataReader? result = null;

            table?.Rows.Clear();

            try
            {
                connection?.Open();

                using SqlCommand command = new SqlCommand($"WAITFOR DELAY '00:00:02'; SELECT * FROM Books\r\nWHERE Name LIKE '%{SearchTxt.Text}%'", connection);
                result = await command.ExecuteReaderAsync();

                while (result.Read())
                {
                    var row = table?.NewRow();

                    if (row != null)
                    {
                        row["Id"] = result["Id"];
                        row["Name"] = result["Name"];
                        row["Pages"] = result["Pages"];
                        row["YearPress"] = result["YearPress"];
                        row["Id_Author"] = result["Id_Author"];
                        row["Id_Themes"] = result["Id_Themes"];
                        row["Id_Category"] = result["Id_Category"];
                        row["Id_Press"] = result["Id_Press"];
                        row["Comment"] = result["Comment"];
                        row["Quantity"] = result["Quantity"];
                        table?.Rows.Add(row);
                    }

                }
                Books.ItemsSource = table?.AsDataView();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                connection?.Close();
                result?.Close();
            }
        }
    }
}
