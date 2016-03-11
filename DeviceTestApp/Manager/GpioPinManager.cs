using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Gpio;

namespace PotentiometerSensor.Manager
{
	public class GpioPinManager
	{
		#region コンストラクタ

		private GpioPinManager()
		{

		}

		#endregion //コンストラクタ

		#region プロパティ

		public static GpioPinManager Instance
		{
			get
			{
				if (_Instance == null) {
					_Instance = new GpioPinManager();
				}
				return _Instance;
			}
		}

		#endregion //プロパティ

		#region メソッド

		public void InitDevice()
		{
			var gpio = GpioController.GetDefault();
			if (gpio == null) {
				throw new Exception("There is no GPIO controller on this device");
			}

			_ledPin = gpio.OpenPin(LED_PIN);

			/* GPIO state is initially undefined, so we assign a default value before enabling as output */
			_ledPin.Write(GpioPinValue.High);
			_ledPin.SetDriveMode(GpioPinDriveMode.Output);
		}

		public void LightLED(Boolean bOn)
		{
			if (bOn == true) {
				_ledPin.Write(GpioPinValue.Low);
			} else {
				_ledPin.Write(GpioPinValue.High);
			}
		}

		public void DisposeDevice()
		{
			if (_ledPin != null) {
				_ledPin.Dispose();
			}
		}

		#endregion //メソッド

		#region メンバー変数

		private const int LED_PIN = 4;
		private GpioPin _ledPin;

		private static GpioPinManager _Instance;

		#endregion //メンバー変数
	}
}
