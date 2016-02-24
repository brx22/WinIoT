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


namespace SocketTestApp
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            this.DataContext = this;

            InitClientConnection();
        }

        private async void InitClientConnection()
        {
            try
            {
                clientSocket = new StreamSocket();
                await clientSocket.ConnectAsync(localHost, port);
                Debug.WriteLine("connected!");



            }  catch (Exception exception)
            {
                // If this is an unknown status, 
                // it means that the error is fatal and retry will likely fail.
                if (SocketError.GetStatus(exception.HResult) == SocketErrorStatus.Unknown)
                {
                    throw;
                }

                //StatusText.Text = "Connect failed with error: " + exception.Message;

                clientSocket.Dispose();
                clientSocket = null;
            }
        }

        private async void RecvData()
        {
            if (_reader == null)
            {
                _reader = new DataReader(clientSocket.InputStream);
                _reader.InputStreamOptions = InputStreamOptions.Partial;
                await _reader.LoadAsync(sizeof(uint));
            }
            

            ReadBuffer();
        }

        private async void ReadBuffer()
        {
            while (_reader.UnconsumedBufferLength > 0)
            {
                uint sizeFieldCount = await _reader.LoadAsync(sizeof(uint));

                uint size = _reader.ReadUInt32();

                uint sizeFieldCount2 = await _reader.LoadAsync(size);

                var str = _reader.ReadString(sizeFieldCount2);

                Debug.WriteLine("client receive {0}", str);

                this.m_tbTemp.Text = str;
                this.m_prgTemp.Value = Convert.ToDouble(str);
            }
        }


        StreamSocket clientSocket;

        HostName localHost = new HostName("192.168.0.7");

        string port = "5000";

        DataReader _reader;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.RecvData();
        }
    }
}
