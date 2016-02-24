using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UWPTestApp
{
    public class WeatherData
    {
        // 画面に表示するための匿名型データ
        public object Data { get; private set; }

        // コンストラクターはJSONフォーマットの文字列を受け付けられるようにする
        public WeatherData() // XAMLにバインドするため引数なしのコンストラクターも必要
        {
        }
        public WeatherData(string json)
        {
            LoadData(json);
        }

        // JSONフォーマットの文字列から匿名型データを作る
        private void LoadData(string json)
        {
            // JSONフォーマットの文字列からJsonObjectオブジェクトを作る
            var data = new Windows.Data.Json.JsonObject();
            bool success
                = Windows.Data.Json.JsonObject.TryParse(json, out data);

            if (success)
            {
                // JsonObjectオブジェクトから画面表示に必要なデータを取り出し、
                // 匿名型のオブジェクトに詰め込む
                Windows.Data.Json.JsonObject weather
                  = data.GetNamedArray("weather", null)?.GetObjectAt(0);
                Windows.Data.Json.JsonObject main
                  = data.GetNamedObject("main", null);
                string iconId
                  = weather?.GetNamedString("icon", string.Empty);
                Data = new
                {
                    Main = weather?.GetNamedString("main", "(不明)"),
                    Temp = main?.GetNamedNumber("temp", -999),
                    TempMin = main?.GetNamedNumber("temp_min", -999),
                    TempMax = main?.GetNamedNumber("temp_max", -999),
                    Place = data.GetNamedString("name", "(不明)"),
                    IconUrl = iconId != null
                            ? $"http://openweathermap.org/img/w/{iconId}.png"
                            : null,
                };
            }
        }
    }
}
