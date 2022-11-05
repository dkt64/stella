using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO;
using System.IO.Ports;
using System.Threading;

namespace APTerminal
{
    static class Serial
    {
        // -------------------------------------------------------------------------------------------------------------------------------------------
        // Zmienne
        // -------------------------------------------------------------------------------------------------------------------------------------------

        static int baudrate;
        const int baudWindowsPC = 115200;
        const int baudWindowsCE = 115200;

        public const int SERIAL_BUFOR_SIZE = 1024;

        volatile public static byte[] serial_bufor;
        volatile public static byte[] serial_small_bufor;
        volatile public static int serial_ogon, serial_glowa, serial_bufor_przepelnienie;
        public static bool serial_thead_stopped_working;

        public static bool connected = false;
        private static bool work = false;
        public static Thread comThread;

        public static string[] AllAvailablePortsList;
        public static string[] FreeToOpenPortsList;

        public static SerialPort serial_port;

        /* 
         * =========================================================================================================================================================
         * Nazwa:           Init 
         * 
         * Przeznaczenie:   Inicjacja tablic i zmiennych.
         *                  
         * Parametry:      
         * =========================================================================================================================================================
         */
        public static void Init()
        {
            serial_bufor_przepelnienie = serial_ogon = serial_glowa = 0;
            serial_bufor = new byte[SERIAL_BUFOR_SIZE];
            serial_thead_stopped_working = true;

            if (Tools.windowsCE)
                baudrate = baudWindowsCE;
            else
                baudrate = baudWindowsPC;
        }

        /* 
         * =========================================================================================================================================================
         * Nazwa:           CreateAllAvailablePortsList 
         * 
         * Przeznaczenie:   Utworzenie w tablicy AllAvailablePortsList listy dostepnych portow COM na komputerze
         *                  
         * Parametry:      
         * =========================================================================================================================================================
         */
        public static void CreateAllAvailablePortsList()
        {
            //Tools.Log("CreateAllAvailablePortsList()");
            try
            {
                AllAvailablePortsList = SerialPort.GetPortNames();
                Tools.Log("COM Ports Available:");
                foreach (string str in AllAvailablePortsList)
                {
                    Tools.Log(str);
                }
            }
            catch (Exception ex)
            {
                Tools.LogEx(ex, "CreateAllAvailablePortsList()");
            }
        }

        /* 
         * =========================================================================================================================================================
         * Nazwa:           CreateFreeToOpenPortsList 
         * 
         * Przeznaczenie:   Utworzenie w tablicy FreeToOpenPortsList listy dostepnych NIEOTWARTYCH portow COM na komputerze
         *                  
         * Parametry:      
         * =========================================================================================================================================================
         */
        public static void CreateFreeToOpenPortsList()
        {
            int ind;
            SerialPort port;

            //Tools.Log("CreateFreeToOpenPortsList()");

            port = new SerialPort();
            FreeToOpenPortsList = AllAvailablePortsList;

            //Tools.Log("CreateFreeToOpenPortsList() 2");

            try
            {
                ind = 0;
                foreach (string str in AllAvailablePortsList)
                {
                    //Tools.Log("CreateFreeToOpenPortsList() 3");

                    try
                    {
                        port.PortName = str;
                        port.Open();
                        port.Close();
                    }
                    catch (Exception)
                    {
                        // Tools.Log("CreateFreeToOpenPortsList() 4");

                        FreeToOpenPortsList[ind] += "*";

                        if (serial_port != null)
                            if (str == serial_port.PortName)
                                FreeToOpenPortsList[ind] += "*";
                    }
                    ind++;
                }
            }
            catch (Exception ex)
            {
                Tools.LogEx(ex, "CreateFreeToOpenPortsList()");
            }
        }

        /* 
         * =========================================================================================================================================================
         * Nazwa:           Open 
         * 
         * Przeznaczenie:   Połączenie z portem COM i zapisanie tego w ustawieniach
         *                  
         * Parametry:       string portname - nazwa portu szeregowego
         * =========================================================================================================================================================
         */
        public static bool Open(string portname)
        {
            bool retval = false;

            if (portname != null)
            {
                if (portname != "")
                {
                    connected = false;

                    Tools.Log("Open()");

                    Close();

                    try
                    {
                        if (portname.IndexOf('*') != -1)
                        {
                            MessageBox.Show("Port niedostepny!");
                        }
                        else
                        {
                            serial_port = new SerialPort(portname, baudrate, Parity.None, 8, StopBits.One);

                            if (!serial_port.IsOpen)
                            {

                                //serial_port.DtrEnable = true;
                                //serial_port.WriteTimeout = 100;
                                //serial_port.ReadTimeout = 100;
                                //serial_port.NewLine = "\n";

                                //serial_port.DataReceived += new SerialDataReceivedEventHandler(Modbus.DataReceviedHandler);

                                serial_port.Open();
                                retval = true;
                                connected = true;
                                Tools.Log("Comm.Open() Pomyslnie otwarto port komunikacyjny " + serial_port.PortName);

                                serial_small_bufor = new byte[serial_port.ReadBufferSize];

                                StartSerialThread();

                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Tools.LogEx(ex, "Comm.Open(string portname)");
                    }
                }
            }
            return retval;
        }

        /* 
         * =========================================================================================================================================================
         * Nazwa:           Close 
         * 
         * Przeznaczenie:   Zamkniecie aktualnie otwartego portu COM
         *                  
         * Parametry:       -
         * =========================================================================================================================================================
         */
        public static void Close()
        {
            //Tools.Log("Close()");

            try
            {
                connected = false;
                work = false;

                while (!serial_thead_stopped_working)
                {
                    Thread.Sleep(10);
                }

                if (serial_port != null && serial_port.IsOpen)
                {
                    connected = false;
                    serial_port.Close();
                }
            }
            catch (Exception ex)
            {
                Tools.LogEx(ex, "Comm.Close()");
            }
        }

        /* 
         * =========================================================================================================================================================
         * Nazwa:           StartSerialThread 
         * 
         * Przeznaczenie:   Funkcja startujaca watek odbioru danych przez RS-a
         *                  
         * Parametry:       -
         * =========================================================================================================================================================
         */
        public static void StartSerialThread()
        {
            work = true;
            comThread = new Thread(SerialThread);

            if (Tools.windowsCE)
                comThread.Priority = MainForm.THREAD_PRIORITY_CE_SERIAL;
            else
                comThread.Priority = MainForm.THREAD_PRIORITY_PC_SERIAL;

            comThread.Name = "SerialThread";
            comThread.Start();
        }

        /* 
         * =========================================================================================================================================================
         * Nazwa:           SerialThread 
         * 
         * Przeznaczenie:   Watek odbioru danych przez RS-a. Tutaj rowniez wywolywana jest funkcja MOdbusService do obslugi odbioru danych MODBUS
         *                  
         * Parametry:       -
         * =========================================================================================================================================================
         */
        private static void SerialThread()
        {
            int new_bytes, i, readed;

            while (work)
            {
                serial_thead_stopped_working = false;

                // Czekamy aż nastąpi połączenie ze sterownikiem
                while (!connected)
                {
                    Thread.Sleep(100);
                };

                // Czyścimy cały bufor odbiorczy na początku
                if (connected)
                    serial_port.DiscardInBuffer();

                while (connected && serial_port != null && serial_port.PortName != null && serial_port.PortName != "")
                {
                    try
                    {

                        new_bytes = serial_port.BytesToRead;
                        if ((new_bytes > 0) && connected && work)
                        {
                            readed = serial_port.Read(serial_small_bufor, 0, new_bytes);
                            for (i = 0; i < readed; i++)
                            {
                                serial_bufor[serial_glowa] = serial_small_bufor[i];
                                serial_glowa++;
                                if (serial_glowa >= SERIAL_BUFOR_SIZE)
                                    serial_glowa = 0;
                            }
                            //serial_port.DiscardInBuffer();
                        }

                        //Modbus.ModbusService();
                        if (Tools.windowsCE)
                            Thread.Sleep(MainForm.THREAD_SLEEP_CE_SERIAL);
                        else
                            Thread.Sleep(MainForm.THREAD_SLEEP_PC_SERIAL);

                        // ------------------------------------------------------------------------------------------------

                    }
                    catch (TimeoutException)
                    {
                        // Tego nie przechwytujemy
                    }
                    catch (IOException ex)
                    {
                        Tools.LogEx(ex, "IOException -> void SerialThread(). Sprawdź czy sterownik połączony jest z panelem.");
                    }
                    catch (ThreadAbortException ex)
                    {
                        Tools.LogEx(ex, "SerialThread()");
                    }
                    catch (IndexOutOfRangeException ex)
                    {
                        serial_port.DiscardInBuffer();
                        Tools.LogEx(ex, "void SerialThread()");
                    }
                    catch (Exception ex)
                    {
                        Tools.LogEx(ex, "SerialThread(). Wyjątek nie obsługiwany.");
                    }
                }
            }

            serial_thead_stopped_working = true;
        }

        /* 
         * =========================================================================================================================================================
         * Nazwa:           SendBytes 
         * 
         * Przeznaczenie:   Prosta funkcja ktora wysyla przez RS-a tablice
         *                  
         * Parametry:       byte[] data, ushort ilosc
         * =========================================================================================================================================================
         */
        static public void SendBytes(byte[] data, ushort ilosc)
        {
            if (serial_port != null)
            {
                if (serial_port.IsOpen && connected)
                {
                    serial_port.Write(data, 0, ilosc);
                }
            }
        }
    }
}

