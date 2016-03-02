using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
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
using SocketTestApp.Data;

namespace SocketTestApp.Manager
{
	public class NetworkManager
	{
		#region コンストラクタ

		private NetworkManager()
		{

		}

		#endregion //コンストラクタ

		#region プロパティ

		public static NetworkManager Instance
		{
			get
			{
				if (_Instance == null) {
					_Instance = new NetworkManager();
				}
				return _Instance;
			}
		}

		public Boolean IsConnecting
		{
			get
			{
				return this._IsConnecting;
			}
			private set
			{
				this._IsConnecting = value;
				if (this.NotifyConnectionChanged != null) {
					this.NotifyConnectionChanged(this, null);
				}
			}
		}

		#endregion //プロパティ

		#region イベント

		public event EventHandler NotifyConnectionChanged;
		public event EventHandler<StringEventArgs> NotifyMessageChanged;
		public event EventHandler<DataRecievedEventArgs> NotifyDataReceived;

		#endregion //イベント

		#region イベントハンドラー

		#endregion //イベントハンドラー

		#region メソッド

		public void Connect()
		{
			InitClientConnection();
		}

		public void Disconnect()
		{
			_clientSocket.Dispose();
			_clientSocket = null;
			this.IsConnecting = false;

			DataManager.Instance.InitData();
			this.UpdateStatusMessage("Disconnected");
		}

		private async void InitClientConnection()
		{
			try {
				_clientSocket = new StreamSocket();
				await _clientSocket.ConnectAsync(_localHost, _port);
				Debug.WriteLine("connected!");
				this.UpdateStatusMessage(String.Format("Connected to {0}, {1}", this._localHost.ToString(), this._port.ToString()));
				this.IsConnecting = true;

				this.RecvData();

			} catch (Exception exception) {
				// If this is an unknown status, 
				// it means that the error is fatal and retry will likely fail.
				if (SocketError.GetStatus(exception.HResult) == SocketErrorStatus.Unknown) {
					throw;
				}

				Debug.WriteLine("Connect failed with error: " + exception.Message);

				_clientSocket.Dispose();
				_clientSocket = null;
				this.IsConnecting = false;
			}
		}

		private async void RecvData()
		{
			try {
				if (_reader == null) {
					_reader = new DataReader(_clientSocket.InputStream);
					_reader.InputStreamOptions = InputStreamOptions.Partial;
					await _reader.LoadAsync(sizeof(uint));
				}


				this.ReadBuffer();

			} catch (Exception ex) {
				Debug.WriteLine(String.Format("RecvData Failure : {0}", ex.ToString()));

			}
		}

		private async void ReadBuffer()
		{
			try {
				while (_reader.UnconsumedBufferLength > 0) {
					uint sizeFieldCount = await _reader.LoadAsync(sizeof(uint));

					uint size = _reader.ReadUInt32();

					uint sizeFieldCount2 = await _reader.LoadAsync(size);

					var str = _reader.ReadString(sizeFieldCount2);

					Debug.WriteLine(String.Format("client receive {0}", str));

					List<String> lstData = new List<String>();
					lstData.Add(str);

					if (this.NotifyDataReceived != null) {
						this.NotifyDataReceived(this, new DataRecievedEventArgs(lstData));
					}
				}

			} catch (Exception ex) {
				Debug.WriteLine(String.Format("ReadBuffer Failure : {0}", ex.ToString()));
			}
		}

		public void UpdateStatusMessage(String strMessage)
		{
			if (this.NotifyMessageChanged != null) {
				this.NotifyMessageChanged(this, new StringEventArgs(strMessage));
			}
		}

		#endregion メソッド

		#region メンバー変数

		private StreamSocket _clientSocket;

		private HostName _localHost = new HostName(DataDef.IP_DEFAULT);

		private string _port = DataDef.PORT_DEFAULT;

		private DataReader _reader;

		private static NetworkManager _Instance;

		private Boolean _IsConnecting = false;

		#endregion //メンバー変数
	}
}
