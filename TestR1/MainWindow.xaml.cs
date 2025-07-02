using BLEDeviceAPI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using UHFAPP;
using UHFAPP.utils;
using static UHFAPP.UHFAPI;

namespace TestR1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private UHFAPI uhf = null;
        bool isPz = false;
        string strStart = "Start";
        string strStart2 = "开始";
        string strStop = "Stop";
        string strStop2 = "停止";
        bool isRuning = false;
        public static bool isVisible = false;
        int beginTime = 0;
        int workTime = 0;
        int total = 0;

        private UHFAPP.UHFAPI.OnDataReceived onDataReceived = DataReceived;

        static bool isUIFast = false;
        private ObservableCollection<EpcInfo> epcList = new ObservableCollection<EpcInfo>();
        public ObservableCollection<EpcInfo> EpcList => epcList;

        private int totalTag = 0;
        public int TotalTag
        {
            get => totalTag;
            set
            {
                totalTag = value;
                lblTotal.Text = totalTag.ToString(); // pastikan `lblTotal` adalah TextBlock atau Label
            }
        }


        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            uhf = UHFAPI.getInstance();
        }

        private static void DataReceived(IntPtr pdata, short len)
        {
            short contentLen;
            short index = 0;
            byte type;

            byte[] cellData = new byte[len];
            Marshal.Copy(pdata, cellData, 0, len);
            // cellData= Utils.CopyArray(pdata,0,len);

            byte[] pcontent;
            // printf("OnReceivedData:");
            for (int i = 0; i < len; i++)
            {
                // Console.WriteLine("%02X", cellData[i]);
            }
            byte[] temp = null;
            string str;
            //  printf("\n");
            while (index < len)
            {
                type = cellData[index++];
                if ((cellData[index] & 0x80) == 0x80)
                {
                    contentLen = (short)(((cellData[index] & 0x7F) << 7) | (cellData[index + 1] & 0x7F));
                    index += 2;
                }
                else
                {
                    contentLen = cellData[index++];
                }

                pcontent = Utils.CopyArray(cellData, index, contentLen);
                index += contentLen;
            }
        }

        //Connect/Disconnect USB

        private bool isUsbOpen = false;

        private void btnUsbToggle_Click(object sender, RoutedEventArgs e)
        {
            if (!isUsbOpen)
            {
                // Coba buka USB
                bool result = uhf.OpenUsb();
                if (result)
                {
                    UHFAPI.setOnDataReceived(onDataReceived);

                    isUsbOpen = true;
                    btnUsbToggle.Content = "Disconnect";
                }
                else
                {
                    MessageBox.Show("Failed to connect USB.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                // Tutup koneksi
                uhf.CloseUsb();
                    isUsbOpen = false;
                    btnUsbToggle.Content = "Connect";
            }
        }

        //setting

        //Buzzer
        private void btnGetBuzzer_Click(object sender, RoutedEventArgs e)
        {
            byte[] mode = new byte[10];

            if (!uhf.UHFGetBuzzer(mode))
            {
                txtBuzzerStatus.Text = "Failure!";
                return;
            }

            if (mode[0] == 0)
            {
                rbEnableBuzzer.IsChecked = false;
                rbDisableBuzzer.IsChecked = true;
            }
            else if (mode[0] == 1)
            {
                rbDisableBuzzer.IsChecked = false;
                rbEnableBuzzer.IsChecked = true;
            }

            txtBuzzerStatus.Text = "Success!";
        }

        private void btnSetBuzzer_Click(object sender, RoutedEventArgs e)
        {
            byte mode = 0;

            if (rbEnableBuzzer.IsChecked == true)
            {
                mode = 1;
            }
            else if (rbDisableBuzzer.IsChecked == true)
            {
                mode = 0;
            }
            else
            {
                txtBuzzerStatus.Text = "Failure!";
                return;
            }

            if (!uhf.UHFSetBuzzer(mode))
            {
                txtBuzzerStatus.Text = "Failure!";
                return;
            }

            txtBuzzerStatus.Text = "Success!";
        }




        //read, stop and time
        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            UHFAPI.setOnDataReceived(onDataReceived);

            if (btnStart.Content == "Stop")
            {
                StopEPC(true);

            }
            else
            {
                if (uhf.StartInventory())
                {
                    btnStart.Content = "Stop";
                    isVisible = true;
                    StartTimer();
                    StartReceiveThread();
                }
                else
                {
                    MessageBox.Show("Inventory failure!", "Notifikasi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void StartReceiveThread()
        {
            Console.WriteLine("StartReceiveThread dipanggil");
            if (isRuning) return;

            isRuning = true;

            Thread receiveThread = new Thread(() =>
            {
                try
                {
                    ReadEPC();
                }
                catch (Exception ex)
                {
                    Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show("Terjadi kesalahan saat membaca tag: " + ex.Message);
                    });
                }
            });

            receiveThread.IsBackground = true; // agar thread berhenti saat aplikasi ditutup
            receiveThread.Start();
        }


        private void ReadEPC()
        {
            Console.WriteLine("ReadEPC begin.");
            try
            {
                while (isRuning)
                {
                    if (!isVisible)
                    {
                        Console.WriteLine("Skip: isVisible = false");
                        Thread.Sleep(10);
                        continue;
                    }

                    if (uhf == null)
                    {
                        Console.WriteLine("uhf is null!");
                        break;
                    }

                    UHFTAGInfo info = uhf.ReadTagFromBuffer();

                    if (info != null)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            UpdateTagUI(info.Epc, info.Tid, info.Rssi, "1", info.Ant, info.User);
                        });
                    }
                    else
                    {
                        Thread.Sleep(10);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error di ReadEPC: " + ex.Message);
            }
            Console.WriteLine("ReadEPC end.");
        }


        private void StopEPC(bool isStop)
        {
            StopTimer();
            bool result = uhf.StopInventory();
            workTime = 0;
            if (!result)
            {
                //MessageBox.Show(!IsChineseSimple() ? "Stop fail" : "停止失败");
            }

            btnStart.Content = "Start";
            Thread.Sleep(100);
        }

        private void UpdateTagUI(string epc, string tid, string rssi, string countStr, string antStr, string user)
        {
            int count = int.TryParse(countStr, out var c) ? c : 1;
            int ant = int.TryParse(antStr, out var a) ? a : 0;

            var existing = epcList.FirstOrDefault(t => t.Epc == epc && t.Tid == tid);
            if (existing != null)
            {
                existing.AddAntennaInfoByAnt(ant, rssi);
                existing.User = user;
                existing.Rssi = rssi;
            }
            else
            {
                var epcBytes = UHFAPP.DataConvert.HexStringToByteArray(epc);
                var tidBytes = UHFAPP.DataConvert.HexStringToByteArray(tid);
                var item = new EpcInfo(epc, tid, count, epcBytes, tidBytes, ant, rssi, user)
                {
                    No = epcList.Count + 1
                };
                epcList.Insert(0, item);

                TotalTag++;
            }
        }

        DispatcherTimer timer = new DispatcherTimer();
        DateTime startTime;

        private void StartTimer()
        {
            startTime = DateTime.Now;
            timer.Interval = TimeSpan.FromMilliseconds(100);
            timer.Tick += (s, e) =>
            {
                var elapsed = DateTime.Now - startTime;
                lblTime.Text = $"{elapsed.TotalMilliseconds:F0} ms";
            };
            timer.Start();
        }

        private void StopTimer()
        {
            timer.Stop();
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            epcList.Clear();
            TotalTag = 0;
        }


        //Temp

        private void btnTemp_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Untuk debugging, pastikan kita tahu status OpenUsb
                Console.WriteLine("Cek suhu: OpenUsb sudah dipanggil sebelumnya");

                string temp = uhf.GetTemperature();
                Console.WriteLine("GetTemperature() result: " + (string.IsNullOrEmpty(temp) ? "NULL/EMPTY" : temp));

                if (!string.IsNullOrEmpty(temp))
                {
                    lblTemp.Text = temp + "°C";
                    MessageBox.Show("Suhu saat ini: " + temp + "°C", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    lblTemp.Text = "Err";
                    MessageBox.Show("Gagal membaca suhu dari reader. Coba lagi.", "Gagal", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                lblTemp.Text = "Err";
                MessageBox.Show("Exception saat ambil suhu: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


    }
}
