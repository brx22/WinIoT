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

namespace PotentiometerSensor
{
    //http://ms-iot.github.io/content/en-US/win10/samples/Potentiometer.htm
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();

            Unloaded += MainPage_Unloaded;

            InitAll();
        }

        private async void InitAll()
        {
            if (ADC_DEVICE == AdcDevice.NONE)
            {
                m_StatusText.Text = "Please change the ADC_DEVICE variable to either MCP3002 or MCP3208, or MCP3008";
                return;
            }

            try
            {
                InitGpio();         /* Initialize GPIO to toggle the LED                          */
                await InitSPI();    /* Initialize the SPI bus for communicating with the ADC      */

            }
            catch (Exception ex)
            {
                m_StatusText.Text = ex.Message;
                return;
            }

            /* Now that everything is initialized, create a timer so we read data every 500mS */
            periodicTimer = new Timer(this.Timer_Tick, null, 0, 100);

            m_StatusText.Text = "Status: Running";


            this.InitServer();
        }

        private async Task InitSPI()
        {
            try
            {
                var settings = new SpiConnectionSettings(SPI_CHIP_SELECT_LINE);
                settings.ClockFrequency = 500000;   /* 0.5MHz clock rate                                        */
                settings.Mode = SpiMode.Mode0;      /* The ADC expects idle-low clock polarity so we use Mode0  */

                string spiAqs = SpiDevice.GetDeviceSelector(SPI_CONTROLLER_NAME);
                var deviceInfo = await DeviceInformation.FindAllAsync(spiAqs);
                SpiADC = await SpiDevice.FromIdAsync(deviceInfo[0].Id, settings);
            }

            /* If initialization fails, display the exception and stop running */
            catch (Exception ex)
            {
                throw new Exception("SPI Initialization Failed", ex);
            }
        }

        private void InitGpio()
        {
            var gpio = GpioController.GetDefault();

            /* Show an error if there is no GPIO controller */
            if (gpio == null)
            {
                throw new Exception("There is no GPIO controller on this device");
            }

            ledPin = gpio.OpenPin(LED_PIN);

            /* GPIO state is initially undefined, so we assign a default value before enabling as output */
            ledPin.Write(GpioPinValue.High);
            ledPin.SetDriveMode(GpioPinDriveMode.Output);
        }

        /* Turn on/off the LED depending on the potentiometer position    */
        private void LightLED()
        {
            int adcResolution = 0;

            switch (ADC_DEVICE)
            {
                case AdcDevice.MCP3002:
                    adcResolution = 1024;
                    break;
                case AdcDevice.MCP3208:
                    adcResolution = 4096;
                    break;
                case AdcDevice.MCP3008:
                    adcResolution = 1024;
                    break;
            }

            /* Turn on LED if pot is rotated more halfway through its range */
            if (adcValue > adcResolution / 2)
            {
                ledPin.Write(GpioPinValue.Low);
            }
            /* Otherwise turn it off                                        */
            else
            {
                ledPin.Write(GpioPinValue.High);
            }
        }

        /* Read from the ADC, update the UI, and toggle the LED */
        private void Timer_Tick(object state)
        {
            ReadADC();
            LightLED();
        }

        public void ReadADC()
        {
            byte[] readBuffer = new byte[3]; /* Buffer to hold read data*/
            byte[] writeBuffer = new byte[3] { 0x00, 0x00, 0x00 };

            /* Setup the appropriate ADC configuration byte */
            switch (ADC_DEVICE)
            {
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

            SpiADC.TransferFullDuplex(writeBuffer, readBuffer); /* Read data from the ADC                           */
            adcValue = convertToInt(readBuffer);                /* Convert the returned bytes into an integer value */

            /* UI updates must be invoked on the UI thread */
            var task = this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {

                //変換
                //http://store.techshare.jp/html/page113.html
                Double volt = adcValue * 3.3 / 1023.0;//               # converte data to Voltage
                Double temp = (volt * 1000.0 - 500.0) / 10.0;// # convertr volt to temp



                textPlaceHolder.Text = temp.ToString("F1");         /* Display the value on screen                      */

                this.m_prog.Value = temp;

                this.SendData(temp.ToString("F1"));
            });
        }


        public int convertToInt(byte[] data)
        {
            int result = 0;
            switch (ADC_DEVICE)
            {
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

        private void MainPage_Unloaded(object sender, object args)
        {
            /* It's good practice to clean up after we're done */
            if (SpiADC != null)
            {
                SpiADC.Dispose();
            }

            if (ledPin != null)
            {
                ledPin.Dispose();
            }
        }


        #region ソケット通信

        StreamSocket serverSocket;

        StreamSocketListener listener;

        HostName localHost;

        string port = "5000";

        private DataWriter _writer;


        private async void InitServer()
        {
            localHost = NetworkInformation.GetHostNames().Where(q => q.Type == HostNameType.Ipv4).First();

            listener = new StreamSocketListener();

            listener.ConnectionReceived += (ss, ee) =>
            {
                serverSocket = ee.Socket;
                Debug.WriteLine("connected {0}", serverSocket.Information.RemoteAddress);

                ShowMessage(String.Format("connected {0}", serverSocket.Information.RemoteAddress));

                _writer = new DataWriter(serverSocket.OutputStream);
            };

            await listener.BindEndpointAsync(localHost, port);

            Debug.WriteLine("listen...");
            this.ShowMessage("listen...");
        }

        private async void SendData(String strData)
        {
            if (_writer != null)
            {
                _writer.WriteUInt32(_writer.MeasureString(strData));
                _writer.WriteString(strData);
                await _writer.StoreAsync();

                Debug.WriteLine("server send : {0}", strData);
            }
        }


        #endregion //ソケット通信

        private async void ShowMessage(String strMessage)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => { this.m_MessageText.Text = strMessage; });
        }


        #region メンバー変数

        enum AdcDevice { NONE, MCP3002, MCP3208, MCP3008 };

        /* Important! Change this to either AdcDevice.MCP3002, AdcDevice.MCP3208 or AdcDevice.MCP3008 depending on which ADC you chose     */
        private AdcDevice ADC_DEVICE = AdcDevice.MCP3008;

        private const int LED_PIN = 4; // Use pin 12 if you are using DragonBoard
        private GpioPin ledPin;

        private const string SPI_CONTROLLER_NAME = "SPI0";  /* Friendly name for Raspberry Pi 2 SPI controller          */
        private const Int32 SPI_CHIP_SELECT_LINE = 0;       /* Line 0 maps to physical pin number 24 on the Rpi2        */
        private SpiDevice SpiADC;

        private const byte MCP3002_CONFIG = 0x68; /* 01101000 channel configuration data for the MCP3002 */
        private const byte MCP3208_CONFIG = 0x06; /* 00000110 channel configuration data for the MCP3208 */
        private readonly byte[] MCP3008_CONFIG = { 0x01, 0x80 }; /* 00000001 10000000 channel configuration data for the MCP3008 */

        private Timer periodicTimer;
        private int adcValue;

        #endregion //メンバー変数
    }
}
