using System;
using System.Reactive;
using System.Windows;
using System.ComponentModel;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Runtime.InteropServices;
using JuliusSweetland.OptiKey.Contracts;
using JuliusSweetland.OptiKey.Static;
using Newtonsoft.Json;

namespace MirandaOptikey
{
    public class MirandaOptikey : IPointService, IDisposable
    {
        #region Fields

        private event EventHandler<Timestamped<Point>> pointEvent;

        private BackgroundWorker pollWorker;
        private UdpClient udpClient;
        private IPEndPoint remoteEndPoint;
        private Point latestPoint = new Point(0, 0);
        private readonly object pointLock = new object(); // Lock for thread safety

        #endregion

        #region Ctor

        public MirandaOptikey()
        {
            // Initialize UDP client
            int port = 9999;
            udpClient = new UdpClient(port);
            remoteEndPoint = new IPEndPoint(IPAddress.Any, port);

            pollWorker = new BackgroundWorker();
            pollWorker.DoWork += pollPosition;
            pollWorker.WorkerSupportsCancellation = true;

            // Start receiving UDP messages in a background thread
            new Thread(ReceiveUdpMessages).Start();
        }

        public void Dispose()
        {
            pollWorker.CancelAsync();
            pollWorker.Dispose();
            udpClient.Close();
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;
        }

        #endregion

        #region Events

        public event EventHandler<Exception> Error;

        public event EventHandler<Timestamped<Point>> Point
        {
            add
            {
                if (pointEvent == null)
                {
                    // Start polling for tracking positions from UDP data
                    pollWorker.RunWorkerAsync();
                }

                pointEvent += value;
            }
            remove
            {
                pointEvent -= value;

                if (pointEvent == null)
                {
                    pollWorker.CancelAsync();
                }
            }
        }

        #endregion

        #region Private methods        

        private void pollPosition(object sender, DoWorkEventArgs e)
        {
            while (!pollWorker.CancellationPending)
            {
                Point pointToEmit;
                lock (pointLock)
                {
                    pointToEmit = latestPoint;  // Always get the latest value
                }

                var timeStamp = Time.HighResolutionUtcNow.ToUniversalTime();

                // Emit a point event with the latest UDP coordinates
                pointEvent?.Invoke(this, new Timestamped<Point>(pointToEmit, timeStamp));

                // Sleep thread to avoid hot loop
                int delay = 30; // ms
                Thread.Sleep(delay);
            }
        }

        private void ReceiveUdpMessages()
        {
            while (true)
            {
                try
                {
                    byte[] receiveBytes = udpClient.Receive(ref remoteEndPoint);
                    string receivedData = Encoding.ASCII.GetString(receiveBytes);

                    var message = JsonConvert.DeserializeObject<Message>(receivedData);

                    if (message != null)
                    {
                        lock (pointLock)
                        {
                            latestPoint.X = message.X;
                            latestPoint.Y = message.Y;
                        }
                    }
                }
                catch (Exception ex)
                {
                    PublishError(this, ex);
                }
            }
        }

        #endregion

        #region Publish Error

        private void PublishError(object sender, Exception ex)
        {
            Error?.Invoke(sender, ex);
        }

        #endregion

        #region Nested Types

        // Define a class to map the incoming JSON structure
        public class Message
        {
            public double X { get; set; }
            public double Y { get; set; }
            public string Timestamp { get; set; }
        }

        #endregion
    }
}
