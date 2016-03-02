using System;
using System.Threading.Tasks;
using Windows.Devices.Gpio;
using Windows.Devices.Spi;
using Windows.Devices.Enumeration;
using Windows.UI.Core;
using System.Threading;
using PotentiometerSensor.Helper;

namespace PotentiometerSensor.Manager
{
	public class ADCManager
	{
		#region コンストラクタ

		private ADCManager()
		{

		}

		#endregion //コンストラクタ

		#region プロパティ

		public static ADCManager Instance
		{
			get
			{
				if (_Instance == null) {
					_Instance = new ADCManager();
				}
				return _Instance;
			}
		}

		#endregion //プロパティ

		#region イベント

		public event EventHandler<StringEventArgs> NotifyMessageChanged;
		public event EventHandler<Int32EventArgs> NotifyDataChanged;

		#endregion //イベント

		#region イベントハンドラー

		private void Timer_Tick(object state)
		{
			try {
				this.ReadADC();

			} catch (Exception ex) {

			}
		}

		#endregion //イベントハンドラー

		#region メソッド

		#region Initメソッド

		public async void InitDevice()
		{
			if (ADC_DEVICE == AdcDevice.NONE) {
				this.UpdateStatusMessage("Please change the ADC_DEVICE variable to either MCP3002 or MCP3208, or MCP3008");
				return;
			}

			try {
				await InitSPI();

			} catch (Exception ex) {
				this.UpdateStatusMessage(ex.Message);
				return;
			}
			
			_periodicTimer = new Timer(this.Timer_Tick, null, 0, 1000);

			this.UpdateStatusMessage("Status: Running");
		}

		private async Task InitSPI()
		{
			try {
				var settings = new SpiConnectionSettings(SPI_CHIP_SELECT_LINE);
				settings.ClockFrequency = 500000;   /* 0.5MHz clock rate                                        */
				settings.Mode = SpiMode.Mode0;      /* The ADC expects idle-low clock polarity so we use Mode0  */

				string spiAqs = SpiDevice.GetDeviceSelector(SPI_CONTROLLER_NAME);
				var deviceInfo = await DeviceInformation.FindAllAsync(spiAqs);
				_SpiADC = await SpiDevice.FromIdAsync(deviceInfo[0].Id, settings);

			} catch (Exception ex) {
				throw new Exception("SPI Initialization Failed", ex);
			}
		}

		#endregion //Initメソッド

		public void ReadADC()
		{
			byte[] readBuffer = new byte[3];
			byte[] writeBuffer = new byte[3] { 0x00, 0x00, 0x00 };

			/* Setup the appropriate ADC configuration byte */
			switch (ADC_DEVICE) {
				case AdcDevice.MCP3002:
					writeBuffer[0] = MCP3002_CONFIG;
					break;
				case AdcDevice.MCP3208:
					writeBuffer[0] = MCP3208_CONFIG;
					break;
				case AdcDevice.MCP3008:
					writeBuffer[0] = MCP3008_CONFIG[0];
					writeBuffer[1] = MCP3008_CONFIG[1];
					break;
			}

			this._SpiADC.TransferFullDuplex(writeBuffer, readBuffer); /* Read data from the ADC                           */
			this._adcValue = ADCManager.ConvertToInt(ADC_DEVICE, readBuffer);

			if (this.NotifyDataChanged != null) {
				this.NotifyDataChanged(this, new Int32EventArgs(this._adcValue));
			}
		}

		#region Staticメソッド

		public static int ConvertToInt(AdcDevice device, byte[] data)
		{
			int result = 0;
			switch (device) {
				case AdcDevice.MCP3002:
					result = data[0] & 0x03;
					result <<= 8;
					result += data[1];
					break;
				case AdcDevice.MCP3208:
					result = data[1] & 0x0F;
					result <<= 8;
					result += data[2];
					break;
				case AdcDevice.MCP3008:
					result = data[1] & 0x03;
					result <<= 8;
					result += data[2];
					break;
			}
			return result;
		}

		#endregion //Staticメソッド

		public void UpdateStatusMessage(String strMessage)
		{
			if (this.NotifyMessageChanged != null) {
				this.NotifyMessageChanged(this, new StringEventArgs(strMessage));
			}
		}

		public void DisposeDevice()
		{
			if (_SpiADC != null) {
				_SpiADC.Dispose();
			}
		}

		#endregion //メソッド

		#region メンバー変数

		/* Important! Change this to either AdcDevice.MCP3002, AdcDevice.MCP3208 or AdcDevice.MCP3008 depending on which ADC you chose     */
		private static readonly AdcDevice ADC_DEVICE = AdcDevice.MCP3008;

		private const string SPI_CONTROLLER_NAME = "SPI0";  /* Friendly name for Raspberry Pi 2 SPI controller          */
		private const Int32 SPI_CHIP_SELECT_LINE = 0;       /* Line 0 maps to physical pin number 24 on the Rpi2        */
		private SpiDevice _SpiADC;

		private const byte MCP3002_CONFIG = 0x68; /* 01101000 channel configuration data for the MCP3002 */
		private const byte MCP3208_CONFIG = 0x06; /* 00000110 channel configuration data for the MCP3208 */
		private readonly byte[] MCP3008_CONFIG = { 0x01, 0x80 }; /* 00000001 10000000 channel configuration data for the MCP3008 */

		private Timer _periodicTimer;
		private Int32 _adcValue;

		private static ADCManager _Instance;

		#endregion //メンバー変数

		public enum AdcDevice { NONE, MCP3002, MCP3208, MCP3008 };
	}
}
