using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace UWPTestApp
{
    /// <summary>
    /// それ自体で使用できる空白ページまたはフレーム内に移動できる空白ページ。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();

            this.Loaded += MainPage_Loaded;
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            this.GetWeatherDataAsync();
        }

        private async void GetWeatherDataAsync()
        {
            try
            {
                // 例：東京の現在の天気を、摂氏で取得する
                var url = "http://api.openweathermap.org/data/2.5/weather?q=Tokyo,jp&units=metric";

                // Web APIを呼び出して、結果をJSONデータで得る
                var hc = new Windows.Web.Http.HttpClient();
                string jsonString = await hc.GetStringAsync(new Uri(url));

                // JSONデータから必要なデータを取り出して、UIのデータコンテキストにセットする
                rootGrid.DataContext = (new WeatherData(jsonString)).Data;

            }
            catch (Exception e)
            {
                
            }
        }
    }
}
