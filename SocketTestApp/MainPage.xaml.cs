using SocketTestApp.Manager;
using SocketTestApp.Model;
using SocketTestApp.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking;
using Windows.Networking.Connectivity;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using SocketTestApp.Helper;

namespace SocketTestApp
{
	public sealed partial class MainPage : Page
	{
		#region コンストラクタ

		public MainPage()
		{
			try {
				this.InitializeComponent();

				foreach (MeterDataObj objData in DataManager.Instance.MeterDataObjList) {
					this._MeterViewModels.Add(new MeterViewModel(objData));
				}

				NetworkManager.Instance.NotifyConnectionChanged += NetworkManager_NotifyConnectionChanged;
				NetworkManager.Instance.NotifyMessageChanged += NetworkManager_NotifyMessageChanged;

				this.DataContext = this;

			} catch (Exception ex) {

			}
		}

		#endregion //コンストラクタ

		#region プロパティ

		public List<MeterViewModel> MeterViewModels
		{
			get
			{
				return this._MeterViewModels;
			}
		}

		#endregion //プロパティ

		#region イベントハンドラー

		private void ConnectBtn_Click(object sender, RoutedEventArgs e)
		{
			try {
				if (NetworkManager.Instance.IsConnecting == true) {
					NetworkManager.Instance.Disconnect();

				} else {
					NetworkManager.Instance.Connect();
				}

			} catch (Exception ex) {

			}
		}

		private void NetworkManager_NotifyConnectionChanged(object sender, EventArgs e)
		{
			try {
				if (NetworkManager.Instance.IsConnecting == true) {
					this.m_btnConnect.Content = "Disconnect";

				} else {
					this.m_btnConnect.Content = "Connect";

				}

			} catch (Exception ex) {

			}
		}

		private void NetworkManager_NotifyMessageChanged(object sender, StringEventArgs e)
		{
			try {
				if (e != null) {
					this.m_txtMessage.Text = e.Text;
				}

			} catch (Exception ex) {

			}
		}

		#endregion //イベントハンドラー

		#region メソッド

		#endregion //メソッド

		#region メンバー変数

		private List<MeterViewModel> _MeterViewModels = new List<MeterViewModel>();

		#endregion //メンバー変数
	}
}
