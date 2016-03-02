using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PotentiometerSensor.Helper;
using Windows.Networking;
using Windows.Networking.Connectivity;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace PotentiometerSensor.Manager
{
	public class SocketManager
	{
		#region コンストラクタ

		private SocketManager()
		{

		}

		#endregion //コンストラクタ

		#region プロパティ

		public static SocketManager Instance
		{
			get
			{
				if (_Instance == null) {
					_Instance = new SocketManager();
				}
				return _Instance;
			}
		}

		#endregion //プロパティ

		#region イベント

		public event EventHandler<StringEventArgs> NotifyMessageChanged;

		#endregion //イベント

		#region メソッド

		public async void InitServer()
		{
			try {
				_localHost = NetworkInformation.GetHostNames().Where(q => q.Type == HostNameType.Ipv4).First();

				_listener = new StreamSocketListener();

				_listener.ConnectionReceived += (ss, ee) => {
					_serverSocket = ee.Socket;
					Debug.WriteLine("connected {0}", _serverSocket.Information.RemoteAddress);
					this.UpdateStatusMessage(String.Format("connected {0}", _serverSocket.Information.RemoteAddress));
					_writer = new DataWriter(_serverSocket.OutputStream);
				};

				await _listener.BindEndpointAsync(_localHost, _port);

				Debug.WriteLine("listen...");
				this.UpdateStatusMessage("listen...");

			} catch (Exception ex) {
				Debug.WriteLine("InitServer Failure : {0}", ex.ToString());
			}
		}

		public async void SendData(String strData)
		{
			try {
				if (_writer != null) {
					_writer.WriteUInt32(_writer.MeasureString(strData));
					_writer.WriteString(strData);
					await _writer.StoreAsync();

					Debug.WriteLine(String.Format("server send : {0}", strData));
				}

			} catch (Exception ex) {
				Debug.WriteLine("SendData Failure : {0}", ex.ToString());
			}
		}


		public void UpdateStatusMessage(String strMessage)
		{
			if (this.NotifyMessageChanged != null) {
				this.NotifyMessageChanged(this, new StringEventArgs(strMessage));
			}
		}

		#endregion //メソッド

		#region メンバー変数

		private static SocketManager _Instance;

		StreamSocket _serverSocket;

		StreamSocketListener _listener;

		HostName _localHost;

		string _port = "5000";

		private DataWriter _writer;

		#endregion //メンバー変数
	}
}
