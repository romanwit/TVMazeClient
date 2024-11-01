using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;

namespace MoviesApp
{
    public partial class MainWindow : Window
    {
        private readonly HttpClient _httpClient;

        public MainWindow()
        {
            InitializeComponent();
            _httpClient = new HttpClient();
        }

        private async void OnSearchClick(object sender, RoutedEventArgs e)
        {
            string query = SearchTextBox.Text.Trim();
            if (!string.IsNullOrEmpty(query))
            {
                await LoadMovies(query);
            }
        }

        private async Task LoadMovies(string searchQuery)
        {
            try
            {
                string url = $"http://api.tvmaze.com/search/shows?q={searchQuery}";
                string response = await _httpClient.GetStringAsync(url);
                var movies = JsonConvert.DeserializeObject<List<Movie>>(response);
                MoviesDataGrid.ItemsSource = movies;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }
    }

    public class Movie
    {
        public Show Show { get; set; }
    }

    public class Show
    {
        public string Name { get; set; }
        public string Language { get; set; }
        public string Premiered { get; set; }
        public Image Image { get; set; }
        public string Summary { get; set; }
    }

    public class Image
    {
        public string Medium { get; set; }
    }
}
