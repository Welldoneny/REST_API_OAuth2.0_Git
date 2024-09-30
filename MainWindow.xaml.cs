using Microsoft.Web.WebView2.Core;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Windows;

namespace Test_GitAPI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string ClientId = "Ov23li3eqlj3Nf6eKjPc"; // Замените на ваш Client ID
        private const string ClientSecret = "3cc10987f3b79218173ecffe61fb52379298ccc9"; // Замените на ваш Client Secret
        private const string RedirectUri = "http://localhost:5000"; // Ваша Redirect URI
        private string _accessToken;


        public MainWindow()
        {
            InitializeComponent();
            InitializeAsync();
        }

        private async void InitializeAsync()
        {
            await webView.EnsureCoreWebView2Async(null);
            webView.CoreWebView2.NavigationCompleted += CoreWebView2_NavigationCompleted;
        }

        private void AuthorizeButton_Click(object sender, RoutedEventArgs e)
        {
            var authorizationUrl = $"https://github.com/login/oauth/authorize?client_id={ClientId}&redirect_uri={RedirectUri}&scope=repo,user";
            webView.Source = new Uri(authorizationUrl);
            webView.Visibility = Visibility.Visible;
        }

        private async void CoreWebView2_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            var uri = webView.Source.ToString();
            if (uri.StartsWith(RedirectUri))
            {
                var query = new Uri(uri).Query;
                var code = System.Web.HttpUtility.ParseQueryString(query).Get("code");
                if (!string.IsNullOrEmpty(code))
                {
                    _accessToken = await GetAccessToken(code);
                    webView.Visibility = Visibility.Collapsed;
                    MessageBox.Show(_accessToken);
                }
            }
        }

        private async Task<string> GetAccessToken(string code)
        {
            using (var httpClient = new HttpClient())
            {
                var request = new HttpRequestMessage(HttpMethod.Post, "https://github.com/login/oauth/access_token");
                request.Content = new StringContent($"client_id={ClientId}&client_secret={ClientSecret}&code={code}");
                request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
                var response = await httpClient.SendAsync(request);
                var responseContent = await response.Content.ReadAsStringAsync();
                return System.Web.HttpUtility.ParseQueryString(responseContent).Get("access_token");
            }
        }

        private async void GetDataButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_accessToken))
            {
                MessageBox.Show("Сначала авторизуйтесь.");
                return;
            }

            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
                //httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("VVV/1.0");
                httpClient.DefaultRequestHeaders.Add("User-Agent", "VVV");
                var response = await httpClient.GetAsync("https://api.github.com/user");
                var content = await response.Content.ReadAsStringAsync();
                outputTextBlock.Text = content;
            }
        }
    }
}