// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Threading;
using System.Threading.Tasks;
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
using Windows.Devices.Gpio;
using Windows.Devices.Spi;
using Windows.Devices.Enumeration;
using Windows.Networking.Sockets;
using Windows.Networking;
using System.Diagnostics;
using Windows.Storage.Streams;
using Windows.Networking.Connectivity;
using Windows.UI.Core;
using PotentiometerSensor.Manager;
using PotentiometerSensor.Helper;

namespace PotentiometerSensor
{
	//http://ms-iot.github.io/content/en-US/win10/samples/Potentiometer.htm
	public sealed partial class MainPage : Page
	{
		#region コンストラクタ

		public MainPage()
		{
			try {
				this.InitializeComponent();

				this.Unloaded += MainPage_Unloaded;

				ADCManager.Instance.NotifyMessageChanged += Instance_NotifyMessageChanged;
				SocketManager.Instance.NotifyMessageChanged += Instance_NotifyMessageChanged;
				ADCManager.Instance.NotifyDataChanged += ADCManager_NotifyDataChanged;

				ADCManager.Instance.InitDevice();
				GpioPinManager.Instance.InitDevice();

				SocketManager.Instance.InitServer();

			} catch (Exception ex) {

			}
		}

		#endregion //コンストラクタ

		#region イベントハンドラー

		private void MainPage_Unloaded(object sender, object args)
		{
			try {
				ADCManager.Instance.DisposeDevice();
				GpioPinManager.Instance.DisposeDevice();

			} catch (Exception ex) {

			}
		}

		private void Instance_NotifyMessageChanged(object sender, StringEventArgs e)
		{
			try {
				this.ShowMessage(e.Text);

			} catch (Exception ex) {

			}
		}

		private void ADCManager_NotifyDataChanged(object sender, Helper.Int32EventArgs e)
		{
			try {
				this.RefreshData(e.Data);

				bLedOn = !bLedOn;
				GpioPinManager.Instance.LightLED(bLedOn);

			} catch (Exception ex) {

			}
		}

		#endregion //イベントハンドラー

		#region メソッド

		private async void ShowMessage(String strMessage)
		{
			await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => { this.m_MessageText.Text = strMessage; });
		}

		private void RefreshData(Int32 nAdcValue)
		{
			try {
				var task = this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => {

					//変換
					//http://store.techshare.jp/html/page113.html
					Double volt = nAdcValue * 3.3 / 1023.0;//               # converte data to Voltage
					Double temp = (volt * 1000.0 - 500.0) / 10.0;// # convertr volt to temp


					this.m_textPlaceHolder.Text = temp.ToString("F1");
					this.m_prog.Value = temp;

					SocketManager.Instance.SendData(temp.ToString("F1"));
				});

			} catch (Exception ex) {

			}
		}

		#endregion //メソッド

		#region メンバー変数

		Boolean bLedOn = false;

		#endregion //メンバー変数
	}
}
