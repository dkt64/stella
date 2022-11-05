﻿using System;
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

#if WindowsCE
using Microsoft.WindowsCE.Forms;
#endif


/*
 * Autoformatowanie
 * Ctrl K
 * Ctrl D
 */

namespace APTerminal
{
    public partial class MainForm : Form
    {
        //
        // Jeżeli mamy serializować dane to tutaj jest info
        public static bool serializuj = false;
    
        //
        // Info o startcie aplikacji kasowane w Paint loga
        //

        bool start = true;
        bool aktywacja_ostatniego = false;

        //
        // Stałe do wątków
        //
        /*
        public const ThreadPriority THREAD_PRIORITY_CE_MAIN = ThreadPriority.AboveNormal;
        public const ThreadPriority THREAD_PRIORITY_PC_MAIN = ThreadPriority.AboveNormal;
        public const ThreadPriority THREAD_PRIORITY_CE_PLC = ThreadPriority.AboveNormal;
        public const ThreadPriority THREAD_PRIORITY_PC_PLC = ThreadPriority.AboveNormal;
        public const ThreadPriority THREAD_PRIORITY_CE_SERIAL = ThreadPriority.AboveNormal;
        public const ThreadPriority THREAD_PRIORITY_PC_SERIAL = ThreadPriority.AboveNormal;
        */

        public const ThreadPriority THREAD_PRIORITY_CE_MAIN = ThreadPriority.AboveNormal;
        public const ThreadPriority THREAD_PRIORITY_PC_MAIN = ThreadPriority.AboveNormal;
        public const ThreadPriority THREAD_PRIORITY_CE_PLC = ThreadPriority.AboveNormal;
        public const ThreadPriority THREAD_PRIORITY_PC_PLC = ThreadPriority.AboveNormal;
        public const ThreadPriority THREAD_PRIORITY_CE_SERIAL = ThreadPriority.Normal;
        public const ThreadPriority THREAD_PRIORITY_PC_SERIAL = ThreadPriority.AboveNormal;

        public const int THREAD_SLEEP_CE_PLC = 1;
        public const int THREAD_SLEEP_PC_PLC = 1;
        public const int THREAD_SLEEP_CE_SERIAL = 5;
        public const int THREAD_SLEEP_PC_SERIAL = 5;
        public const int THREAD_SLEEP_CE_MODBUS = 0;
        public const int THREAD_SLEEP_PC_MODBUS = 0;

        // Poziom dostępu UR
        static bool UR_ENABLED_AT_START = false;

        public static bool poziom_ur = false;
        public static bool poziom_ustawiacz = false;

        //const ushort ETPROC_KONIEC_PROCESU = 19;

        Thread PLCThread;
        public static StatusBar statusBar;

        public static bool work_ui_thread = false;
        static bool ui_thead_stopped_working = true;

        // Zmienne przechowujące dane aktualnie edytowanego parametru
        public static byte ActiveProgEdit = 0;
        public static string ActiveParamEdit;
        public static string ActiveToolParamEdit;

        // zmienna do badania pulsu od zezwolenia na programowanie
        //static bool i_zezwprog_pulse = false;

        public static bool zmiana_parametru_start = false;
        public static bool zmiana_parametru_start_2 = false;
        public static UInt16 parametr_nr = 0;
        public static bool plc_start = true;
        public static bool plc_stopped = false;
        public static bool aktywacja_start = false;
        public static string nazwa_przyrzadu_do_aktywacji = "";

        public static bool odswiez_program_start = false;

        // 
        // Pomiary czasów
        //
        public static int time_main_start, time_main;
        public static int time_aktywacja_start, time_aktywacja;
        public static int time_odswiez_program_start, time_odswiez_program;
        public static int time_zmiana_parametru_start, time_zmiana_parametru;
        public static int time_wyslanie_obrazu_wyjsc_start, time_wyslanie_obrazu_wyjsc;
        public static int time_plc_start, time_plc;
        public static int time_odczyt_obrazu_io_start, time_odczyt_obrazu_io;
        static bool reset_timers = false;

        public static int timeout_zmiana_parametru, timeout_odswiez_program, timeout_wyslanie_obrazu_wyjsc, timeout_odczyt_obrazu_io;

        public const int TIMEOUT_ZMIANA_PARAM = 300;
        public const int TIMEOUT_ZMIANA_PARAM_2 = 500;

        public const int TIMEOUT_WYSYLA_OBRAZ = 100;
        public const int TIMEOUT_WYSYLA_OBRAZ_2 = 200;
        public const int TIMEOUT_WYSYLA_OBRAZ_3 = 300;

        public const int TIMEOUT_ODCZYT_OBRAZ = 100;
        public const int TIMEOUT_ODCZYT_OBRAZ_2 = 200;
        public const int TIMEOUT_ODCZYT_OBRAZ_3 = 300;

        // Przeliczniki napięcia na wartości ADC
        public static float AOUT_Mul;
        public static float AIN_Mul;
        public static float AIN_Meas_Mul;


        // Przeliczniki wartosci napiecia na wielkosci fizyczne
        public static float MUL_Cisnienie = 0.0f;
        public static float MUL_CisnienieWydmuchu = 0.0f;
        public static float MUL_PozycjaCylindra = 0.0f;
        public static float MUL_Prad = 0.0f;
        public static float PRZEL_ENERGIA = 0.0f;

        //
        // Wymuszono zapis licznika przez PLC
        //
        //public static bool zapisz_licznik = false;
        //public static bool zapisz_licznik_max = false;

        // Rozkazy odswiezenia w ticku 10ms

        public static bool odswiez_liczniki = false;

        // Licznik timeout-ów do ustalenia czy mamy nawiązaną komunikację
        //static byte timeout_counter = 0;


#if WindowsCE
        InputPanel inputPanel;
#endif

        //
        // Obsluga zakłóceń
        //
        public static UInt32 err_kopia = 0, err = 0;

        //
        // Ilosc odpytan po zakonczeniu procesu
        // Odpytanie o program i wyniki
        //

        static int koniec_procesu_cnt = 0;

        //
        // Mruganie
        //

        static int blink_cnt = 0;

        bool blink_1;
        //bool blink_2;
        //bool blink_3;
        //bool blink_4;
        //bool blink_5;

        //
        // Nowy kodowany przyrzad
        //
        bool wykryto_nowy_przyrzad = false, brak_kodu = false;
        UInt16 kod_podlaczony = 0; //, kod_poprzedni = 0;

        //
        // Zmienne dla stsusbar
        //

        static bool status_aktywowano = false;
        static bool status_produkcja = false;

        //
        // Zmienna bool uzywana podczas ustawienia wyswietlanych parametrow
        //

        static bool hide_param = false;

        //
        // Odswiezenie plota - z watku PLC na watek timerreferesh
        //

        static bool odswiez_plota = false;

        //
        // Czas powstania ostatniego wykresu
        //

        static DateTime czas_wykres;

        //
        // Pierwszy tick - Paint od Loga
        //

        //
        // Opóźnienie aktywacji w ms
        //

        static int DELAY_AKTYWACJA = 3000;
        int delay_aktywacja = 0;

        //
        // Brak miejsca nadysku
        //

        static bool brak_miejsca_na_dysku = false;
        public const float ZAJETOSC_DYSKU_MAX = 99.0f;

        // 
        //  Właczenie odczytu wykresow
        //

        static bool wykresy = false;


        /* 
         * =========================================================================================================================================================
         * Nazwa:           MainForm 
         * 
         * Przeznaczenie:   Konstruktor klasy głównej formatki
         *                  
         * Parametry:       -
         * =========================================================================================================================================================
         */
        public MainForm()
        {
            InitializeComponent();
            // 
            // statusBar
            // 

            /*            
            statusBar = new System.Windows.Forms.StatusBar();
            statusBar.Location = new System.Drawing.Point(0, 431);
            statusBar.Name = "statusBar";
            statusBar.Size = new System.Drawing.Size(798, 20);
            statusBar.Text = "Start aplikacji";
            statusBar.Font = new System.Drawing.Font("Tahoma", 10F, System.Drawing.FontStyle.Regular);
            this.Controls.Add(statusBar);
            */

            //this.tabControlMain.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold);
        }

        /* 
         * =========================================================================================================================================================
         * Nazwa:           MainForm_Load 
         * 
         * Przeznaczenie:   Zdarzenie wywolywane po zaladowaniu formatki
         *                  
         * Parametry:       -
         * =========================================================================================================================================================
         */
        private void MainForm_Load(object sender, EventArgs e)
        {
            //Console.WriteLine("Start aplikacji...");

            Tools.ReadEnvironment();
            //statusBar.Text = "Inicjacja programu";

            if (!Tools.windowsCE)
            {
                ControlBox = true;
                FormBorderStyle = FormBorderStyle.FixedDialog;
            }

            Thread.CurrentThread.Name = "MainFormThread";

            if (Tools.windowsCE)
                Thread.CurrentThread.Priority = THREAD_PRIORITY_CE_MAIN;
            else
                Thread.CurrentThread.Priority = THREAD_PRIORITY_PC_MAIN;

            Tools.Init();
            Tools.Log("********************************************************************************");
            Tools.Log("Application init start ...");
            Tools.Log("********************************************************************************");
            Tools.Log("App name " + Tools.AppName);
            Tools.Log("App directory " + Tools.AppDirectory);
            Tools.Log("Operating system " + Tools.SystemName);
            Tools.Log("Using .NET Framework V" + Tools.NETVersion);
            //
            // Info o kulturze komputera
            //
            Tools.Log("Culture name = " + Tools.culture_name);


            pictureFlagaPL.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureFlagaPL.Image = Properties.Resources.flag_pl;
            pictureFlagaEN.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureFlagaEN.Image = Properties.Resources.flag_en;
            pictureFlagaDE.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureFlagaDE.Image = Properties.Resources.flag_de;
            pictureFlagaES.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureFlagaES.Image = Properties.Resources.flag_es;
            pictureFlagaHU.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureFlagaHU.Image = Properties.Resources.flag_hu;
            pictureLogoAPTerminal.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureLogoAPTerminal.Image = Properties.Resources.Logo_APTerminal;

            pictureBox_Apweld.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox_Apweld.Image = Properties.Resources.APWELD_LOGO;

            try
            {
                Bitmap logo_klienta = new Bitmap(Tools.AppDirectory + "\\logo.jpg");
                pictureBox_Klient.SizeMode = PictureBoxSizeMode.StretchImage;
                pictureBox_Klient.Image = Image.FromHbitmap(logo_klienta.GetHbitmap()); ;
            }
            catch { }
            
            Ploter.Init();
            Modbus.ModbusInit();
            Serial.Init();
            PLC.InitVars();
            UpdateModbusAddresses();
            Zarzadzanie.Init(this);
            TextsInit();

            if (Tools.ReadSetting("lang") != "")
                jezyk = Convert.ToInt32(Tools.ReadSetting("lang"));
            else
                jezyk = 0;

            PrzeladujTeksty();

            UpdateComList();

            if (Tools.ReadSetting("Autoconnect") == "True")
            {
                Autoconnect.CheckState = CheckState.Checked;
                Serial.Open(Tools.ReadSetting("COM"));
            }
            else
            {
                Autoconnect.CheckState = CheckState.Unchecked;
            }

            UpdateComList();

            if (Tools.windowsCE == true)
            {
                this.TopMost = true;
                //this.Size.Height = 480;
                //this.Size.Width = 800;
                //this.WindowState = FormWindowState.Maximized;
            }

            //
            // Tworzenie wątka PLC
            //
            PLCThread = new Thread(PLCThreadFunc);

            if (Tools.windowsCE)
                PLCThread.Priority = THREAD_PRIORITY_CE_PLC;
            else
                PLCThread.Priority = THREAD_PRIORITY_PC_PLC;

            PLCThread.Name = "PLCThread";
            work_ui_thread = true;
            PLCThread.Start();

            //
            // Wszystkie comba na 0
            //
            comboNrProgramu.SelectedIndex = 0;
            comboParametr.SelectedIndex = 0;
            comboParametrPrzyrzad.SelectedIndex = 0;

            //
            // Odczytanie długości obrazów i zalogowanie tego
            //
            Modbus.size_of_io = 94;         // (ushort)(System.Runtime.InteropServices.Marshal.SizeOf(Modbus.iobuf));
            Modbus.size_of_param = 60;      // (ushort)(System.Runtime.InteropServices.Marshal.SizeOf(Modbus.param_buf));
            //Tools.Log("Rozmiar obrazu IO = " + Modbus.size_of_io.ToString());
            //Tools.Log("Rozmiar parametrow = " + Modbus.size_of_param.ToString());

#if WindowsCE
            inputPanel = new InputPanel();
#endif

            //
            // Odczytanie przelicznikow z ustawien i zapisanie ich w zmiennych
            //

            if (Tools.ReadSetting("AOUT_Mul") == "")
                Tools.WriteSetting("AOUT_Mul", "1000.0");
            if (Tools.ReadSetting("AIN_Mul") == "")
                Tools.WriteSetting("AIN_Mul", "1000.0");
            if (Tools.ReadSetting("AIN_Meas_Mul") == "")
                Tools.WriteSetting("AIN_Meas_Mul", "1000.0");

            if (Tools.ReadSetting("MUL_Cisnienie") == "")
                Tools.WriteSetting("MUL_Cisnienie", "1.0");
            if (Tools.ReadSetting("MUL_CisnienieWydmuchu") == "")
                Tools.WriteSetting("MUL_CisnienieWydmuchu", "0.2");
            if (Tools.ReadSetting("MUL_PozycjaCylindra") == "")
                Tools.WriteSetting("MUL_PozycjaCylindra", "10.0");

            if (Tools.ReadSetting("MUL_Prad") == "")
                Tools.WriteSetting("MUL_Prad", "6.666");
            if (Tools.ReadSetting("PRZEL_ENERGIA") == "")
                Tools.WriteSetting("PRZEL_ENERGIA", "0.001");

            AOUT_Mul = Convert.ToSingle(Tools.ReadSetting("AOUT_Mul"), Tools.ang);
            AIN_Mul = Convert.ToSingle(Tools.ReadSetting("AIN_Mul"), Tools.ang);
            AIN_Meas_Mul = Convert.ToSingle(Tools.ReadSetting("AIN_Meas_Mul"), Tools.ang);

            MUL_Cisnienie = Convert.ToSingle(Tools.ReadSetting("MUL_Cisnienie"), Tools.ang);
            MUL_CisnienieWydmuchu = Convert.ToSingle(Tools.ReadSetting("MUL_CisnienieWydmuchu"), Tools.ang);
            MUL_PozycjaCylindra = Convert.ToSingle(Tools.ReadSetting("MUL_PozycjaCylindra"), Tools.ang);
            MUL_Prad = Convert.ToSingle(Tools.ReadSetting("MUL_Prad"), Tools.ang);
            PRZEL_ENERGIA = Convert.ToSingle(Tools.ReadSetting("PRZEL_ENERGIA"), Tools.ang);

            //Tools.Log("AOUT_Mul = " + AOUT_Mul.ToString());
            //Tools.Log("AIN_Mul = " + AIN_Mul.ToString());
            //Tools.Log("AIN_Meas_Mul = " + AIN_Meas_Mul.ToString());

            //
            // Wczytanie listy przyrzadów
            //
            OdswierzListePrzyrzadow();

            //
            // Aktywacja ostatnio używanego przyrządu ----- teraz z delayem
            //
            //AktywacjaOstatnioUzywanego();

            //
            // Wyswietlenie parametru
            //
            //WyswietlParametrZgrzewania();

            //
            // Status
            //
            //statusBar.Text = "Inicjacja programu zakończona";
            //Console.WriteLine("Start aplikacji zakończony.");

            //
            // Wyswieltenie listy parametrow w combo - domysle tylko podstawoweo parametry
            //

            WyswitlenieListy();

            //
            // TESTY
            // Ladowanie bitmap
            //
            //Zarzadzanie.LoadPictures("test", 0);

            pictureWizuTool.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureWizuProg.SizeMode = PictureBoxSizeMode.StretchImage;

            //
            // Włączenie timerów
            // Dopiero od teraz będzie cokolwiek wyświetlane
            //
            timer_100ms.Enabled = true;
            timer_10ms.Enabled = true;
            timer_1s.Enabled = true;
            timer_5s.Enabled = true;

            // 
            // Do pomiaru czasu inicjacji
            //
            Tools.time4 = Environment.TickCount - Tools.timer;
            Tools.Log("Init time (tick count) " + Tools.time4 + " ms");
            Tools.Log("********************************************************************************");
            Tools.Log("Application init end.");
            Tools.Log("********************************************************************************");

            //
            // Jezeli brak miejsca na dysku wyswietl komunikat
            //
            
            ObliczMiejsceNaDysku();

            if (brak_miejsca_na_dysku)
                BrakMiejscaInfo();

            timerPoziomUR.Enabled = UR_ENABLED_AT_START;
        }

        /* 
         * =========================================================================================================================================================
         * Nazwa:           MainForm_Closed 
         * 
         * Przeznaczenie:   Zdarzenie wywolywane po zamknieciu formatki
         *                  
         * Parametry:       -
         * =========================================================================================================================================================
         */
        private void MainForm_Closed(object sender, EventArgs e)
        {
            plc_start = false;
            MainThreadStop();
            Tools.WriteSetting("Autoconnect", Autoconnect.Checked.ToString());
            if (Serial.serial_port != null)
            {
                Tools.WriteSetting("COM", Serial.serial_port.PortName);
                if (Serial.connected)
                    Serial.Close();
            }
        }

        /* 
        * =========================================================================================================================================================
        * Nazwa:           PLC_AdditionalTasks
        * 
        * Przeznaczenie:   Tutaj wykonujemy dodatkowe dzialania w kooperacji z PLC. Funkcja wywolywana w wątku komunikacji (obrazy IO)
        *                  
        * Parametry:       -
        * =========================================================================================================================================================
        */
        static void PLC_AdditionalTasks()
        {
            //
            // Po odczytaniu obrazu kopiujemy komórki zawierające bity błędów do zmiennej
            //
            err = (UInt32)(PLC.err0.Word + (PLC.err1.Word << 16));

            //
            // Serializuj stan przyrządu jeżeli jest jakiś aktywny
            //
            if (serializuj && Zarzadzanie.AktywnyPrzyrzad != "" && Zarzadzanie.AktywacjaOK && plc_start && Modbus.CommunicationOK && !zmiana_parametru_start && !aktywacja_start)
            {
                Zarzadzanie.SerializujDoPlikuSMT();
                serializuj = false;
            }
            else
            {
                serializuj = false;
            }

            if ((PLC.program_stan.Word == ETPROC_KONIEC_PROCESU) && plc_start && Modbus.CommunicationOK && !zmiana_parametru_start && !aktywacja_start)
            {
                status_produkcja = true;

                do
                {
                    Modbus.ModbusRequest_OdczytProgramu(PLC.cykl_program_akt.Word);
                    if (Modbus.ModbusWaitAck_0x03(TIMEOUT_ZMIANA_PARAM))
                    {
                        Zarzadzanie.Transfer_ProgramDoPliku(Zarzadzanie.AktywnyPrzyrzad, Zarzadzanie.kod_aktywny, (byte)PLC.cykl_program_akt.Word);
                        koniec_procesu_cnt = 101;
                        ZapisWynikow();
                        serializuj = true;
                    }
                    else
                    {
                        koniec_procesu_cnt++;
                        Thread.Sleep(10);
                    }
                } while ((PLC.program_stan.Word == ETPROC_KONIEC_PROCESU) && koniec_procesu_cnt < 10 && plc_start);

                //
                // Odczyt danych do wykresów przebiegów prądu i napięcia
                //
                //
                if (wykresy)
                {
                    Modbus.ModbusRequest_OdczytWykresuWzor();
                    Modbus.ModbusWaitAck_0x03(TIMEOUT_ZMIANA_PARAM);
                    Thread.Sleep(1);
                    Modbus.ModbusRequest_OdczytWykresuPradu();
                    Modbus.ModbusWaitAck_0x03(TIMEOUT_ZMIANA_PARAM);
                    Thread.Sleep(1);
                    Modbus.ModbusRequest_OdczytWykresuNapiecie();
                    Modbus.ModbusWaitAck_0x03(TIMEOUT_ZMIANA_PARAM);
                    Thread.Sleep(1);
                    Modbus.ModbusRequest_OdczytWykresuWtopienie();
                    Modbus.ModbusWaitAck_0x03(TIMEOUT_ZMIANA_PARAM);
                    Thread.Sleep(1);
                    odswiez_plota = true;

                    czas_wykres = DateTime.Now;
                }

                
            }

            if (PLC.program_stan.Word < ETPROC_KONIEC_PROCESU)
                koniec_procesu_cnt = 0;
        }

        /* 
         * =========================================================================================================================================================
         * Nazwa:           PLCThread 
         * 
         * Przeznaczenie:   Watek do wykorzystania - petla glowna watku
         *                  
         * Parametry:       -
         * =========================================================================================================================================================
         */
        private static void PLCThreadFunc()
        {
            ui_thead_stopped_working = false;

            while (work_ui_thread == true)
            {
                time_main_start = Environment.TickCount;

                // ******************************************************************************
                // Zmiana parametru
                // ******************************************************************************
                //
                if (plc_start && Modbus.CommunicationOK && Zarzadzanie.AktywnyPrzyrzad != "")//!aktywacja_start)
                {
                    if (zmiana_parametru_start)
                    {
                        //
                        // Pierwsza próba
                        //
                        time_zmiana_parametru_start = Environment.TickCount;
                        Modbus.ModbusRequest_WyslanieProgramu(ActiveProgEdit);
                        if (Modbus.ModbusWaitAck_0x10(TIMEOUT_ZMIANA_PARAM))
                        {
                            Modbus.ModbusRequest_OdczytProgramu(ActiveProgEdit);
                            if (Modbus.ModbusWaitAck_0x03(TIMEOUT_ZMIANA_PARAM))
                            {
                                time_zmiana_parametru = Environment.TickCount - time_zmiana_parametru_start;
                            }
                            else
                            {
                                //
                                // Druga próba
                                //
                                Thread.Sleep(10);
                                time_zmiana_parametru_start = Environment.TickCount;
                                Modbus.ModbusRequest_WyslanieProgramu(ActiveProgEdit);
                                if (Modbus.ModbusWaitAck_0x10(TIMEOUT_ZMIANA_PARAM_2))
                                {
                                    Modbus.ModbusRequest_OdczytProgramu(ActiveProgEdit);
                                    Modbus.ModbusWaitAck_0x03(TIMEOUT_ZMIANA_PARAM_2);
                                    time_zmiana_parametru = Environment.TickCount - time_zmiana_parametru_start;
                                }
                            }
                        }

                        zmiana_parametru_start = false;
                        plc_stopped = false;
                        if (time_zmiana_parametru > TIMEOUT_ZMIANA_PARAM_2)
                        {
                            timeout_zmiana_parametru = time_zmiana_parametru;
                        }
                    }

                    if (zmiana_parametru_start_2)
                    {
                        //
                        // Pierwsza próba
                        //
                        time_zmiana_parametru_start = Environment.TickCount;
                        Modbus.ModbusRequest_WyslanieProgramu((byte)parametr_nr);
                        if (Modbus.ModbusWaitAck_0x10(TIMEOUT_ZMIANA_PARAM))
                        {
                            Modbus.ModbusRequest_OdczytProgramu(parametr_nr);
                            if (Modbus.ModbusWaitAck_0x03(TIMEOUT_ZMIANA_PARAM))
                            {
                                time_zmiana_parametru = Environment.TickCount - time_zmiana_parametru_start;
                            }
                            else
                            {
                                //
                                // Druga próba
                                //
                                Thread.Sleep(10);
                                time_zmiana_parametru_start = Environment.TickCount;
                                Modbus.ModbusRequest_WyslanieProgramu((byte)parametr_nr);
                                if (Modbus.ModbusWaitAck_0x10(TIMEOUT_ZMIANA_PARAM_2))
                                {
                                    Modbus.ModbusRequest_OdczytProgramu(parametr_nr);
                                    Modbus.ModbusWaitAck_0x03(TIMEOUT_ZMIANA_PARAM_2);
                                    time_zmiana_parametru = Environment.TickCount - time_zmiana_parametru_start;
                                }
                            }
                        }

                        zmiana_parametru_start_2 = false;
                        plc_stopped = false;
                        if (time_zmiana_parametru > TIMEOUT_ZMIANA_PARAM_2)
                        {
                            timeout_zmiana_parametru = time_zmiana_parametru;
                        }
                    }
                }

                // ******************************************************************************
                // Główny wątek komunikacji IO
                // ******************************************************************************
                //
                if (plc_start && Serial.connected)// && !zmiana_parametru_start && !aktywacja_start)
                {
                    // *************************
                    // Na początku obsługa rozkazu
                    // *************************
                    if (Modbus.rozkaz > 0)
                    {
                        PLC.rozkaz.Word = Modbus.rozkaz;
                        PLC.rozkaz_arg.Word = Modbus.rozkaz_arg;
                        Modbus.rozkaz = Modbus.rozkaz_arg = 0;
                        serializuj = true;
                    }

                    // *************************
                    // Wysyłamy obraz IO - Próba 1
                    // *************************
                    time_wyslanie_obrazu_wyjsc_start = Environment.TickCount;
                    Modbus.ModbusRequest_WyslanieObrazuWyjsc();
                    if (Modbus.ModbusWaitAck_0x10(TIMEOUT_WYSYLA_OBRAZ))
                    {
                        time_wyslanie_obrazu_wyjsc = Environment.TickCount - time_wyslanie_obrazu_wyjsc_start;

                        // *************************
                        // Jezeli odebrano potwierdzenie wyslanego obrazu to odczytujemy stan wejść
                        // *************************
                        time_odczyt_obrazu_io_start = Environment.TickCount;
                        Modbus.ModbusRequest_OdczytObrazuIO();
                        if (Modbus.ModbusWaitAck_0x03(TIMEOUT_ODCZYT_OBRAZ))
                        {
                            // *************************
                            // Jezeli odebrano obraz IO to OK - zrobimy PLC
                            // *************************
                            time_wyslanie_obrazu_wyjsc = Environment.TickCount - time_wyslanie_obrazu_wyjsc_start;

                            time_odczyt_obrazu_io = Environment.TickCount - time_odczyt_obrazu_io_start;

                            //
                            // Główne PLC
                            //
                            time_plc_start = Environment.TickCount;
                            plc_stopped = false;
                            PLC.PLC_Main();
                            PLC_AdditionalTasks();
                            time_plc = Environment.TickCount - time_plc_start;
                        }
                        else
                        {
                            // *************************
                            // Jezeli NIE odebrano potwierdzenie wyslanego obrazu to odczytujemy stan wejść JESZCZE RAZ
                            // *************************
                            time_odczyt_obrazu_io_start = Environment.TickCount;
                            Modbus.ModbusRequest_OdczytObrazuIO();
                            if (Modbus.ModbusWaitAck_0x03(TIMEOUT_ODCZYT_OBRAZ_2))
                            {
                                // *************************
                                // Jezeli odebrano obraz IO to OK - zrobimy PLC
                                // *************************
                                time_wyslanie_obrazu_wyjsc = Environment.TickCount - time_wyslanie_obrazu_wyjsc_start;

                                time_odczyt_obrazu_io = Environment.TickCount - time_odczyt_obrazu_io_start;

                                //
                                // Główne PLC
                                //
                                time_plc_start = Environment.TickCount;
                                plc_stopped = false;
                                PLC.PLC_Main();
                                PLC_AdditionalTasks();
                                time_plc = Environment.TickCount - time_plc_start;
                            }
                        }
                    }
                    else
                    {
                        // *************************
                        // Wysyłamy obraz IO - Próba 2
                        // *************************
                        time_wyslanie_obrazu_wyjsc_start = Environment.TickCount;
                        Modbus.ModbusRequest_WyslanieObrazuWyjsc();
                        if (Modbus.ModbusWaitAck_0x10(TIMEOUT_WYSYLA_OBRAZ_2))
                        {
                            time_wyslanie_obrazu_wyjsc = Environment.TickCount - time_wyslanie_obrazu_wyjsc_start;

                            // *************************
                            // Wysyłamy obraz IO
                            // *************************
                            time_odczyt_obrazu_io_start = Environment.TickCount;
                            Modbus.ModbusRequest_OdczytObrazuIO();
                            if (Modbus.ModbusWaitAck_0x03(TIMEOUT_ODCZYT_OBRAZ))
                            {
                                // *************************
                                // Jezeli odebrano obraz IO to OK - zrobimy PLC
                                // *************************
                                time_wyslanie_obrazu_wyjsc = Environment.TickCount - time_wyslanie_obrazu_wyjsc_start;

                                time_odczyt_obrazu_io = Environment.TickCount - time_odczyt_obrazu_io_start;

                                //
                                // Główne PLC
                                //
                                time_plc_start = Environment.TickCount;
                                plc_stopped = false;
                                PLC.PLC_Main();
                                PLC_AdditionalTasks();
                                time_plc = Environment.TickCount - time_plc_start;
                            }
                            else
                            {
                                // *************************
                                // Jezeli NIE odebrano potwierdzenie wyslanego obrazu to odczytujemy stan wejść JESZCZE RAZ
                                // *************************
                                time_odczyt_obrazu_io_start = Environment.TickCount;
                                Modbus.ModbusRequest_OdczytObrazuIO();
                                if (Modbus.ModbusWaitAck_0x03(TIMEOUT_ODCZYT_OBRAZ_2))
                                {
                                    // *************************
                                    // Jezeli odebrano obraz IO to OK - zrobimy PLC
                                    // *************************
                                    time_wyslanie_obrazu_wyjsc = Environment.TickCount - time_wyslanie_obrazu_wyjsc_start;

                                    time_odczyt_obrazu_io = Environment.TickCount - time_odczyt_obrazu_io_start;

                                    //
                                    // Główne PLC
                                    //
                                    time_plc_start = Environment.TickCount;
                                    plc_stopped = false;
                                    PLC.PLC_Main();
                                    PLC_AdditionalTasks();
                                    time_plc = Environment.TickCount - time_plc_start;
                                }
                            }
                        }
                    }
                }

                // ******************************************************************************
                // Aktywacja
                // ******************************************************************************
                //
                if (Modbus.CommunicationOK && aktywacja_start && nazwa_przyrzadu_do_aktywacji != "")
                {
                    time_aktywacja_start = Environment.TickCount;
                    Zarzadzanie.Aktywacja(nazwa_przyrzadu_do_aktywacji, 0);
                    aktywacja_start = false;
                    time_aktywacja = Environment.TickCount - time_aktywacja_start;
                    reset_timers = true;
                }

                // ******************************************************************************
                // Rozkaz
                // ******************************************************************************
                //
                if (PLC.rozkaz.Word > 0)
                {
                    PLC.rozkaz.Word = 0;
                    PLC.rozkaz_arg.Word = 0;
                    Modbus.rozkaz = Modbus.rozkaz_arg = 0;
                }

                // ******************************************************************************
                // Inne
                // ******************************************************************************
                //
                time_main = Environment.TickCount - time_main_start;

                if (plc_start == false)
                    plc_stopped = true;

                if (Tools.windowsCE)
                    Thread.Sleep(THREAD_SLEEP_CE_PLC);
                else
                    Thread.Sleep(THREAD_SLEEP_PC_PLC);

            }

            ui_thead_stopped_working = true;
        }

        /* 
         * =========================================================================================================================================================
         * Nazwa:           UIThreadStop 
         * 
         * Przeznaczenie:   Watek do wykorzystania - stop
         *                  
         * Parametry:       -
         * =========================================================================================================================================================
         */
        private void MainThreadStop()
        {
            try
            {
                work_ui_thread = false;

                if (Modbus.CommunicationOK & !UR_ENABLED_AT_START)
                {
                    while (!ui_thead_stopped_working)
                    {
                        Thread.Sleep(10);
                    }
                }

            }
            catch (Exception ex)
            {
                Tools.LogEx(ex, "MainThreadStop()");
            }
        }

        /* 
         * =========================================================================================================================================================
         * Nazwa:           timer_VeryFast_Tick 
         * 
         * Przeznaczenie:   Timer do odswieżania UI co 10 ms
         *                  
         * Parametry:       -
         * =========================================================================================================================================================
         */
        private void timer_VeryFast_Tick(object sender, EventArgs e)
        {
        }

        /* 
         * =========================================================================================================================================================
         * Nazwa:           timer_FastRefresh 
         * 
         * Przeznaczenie:   Timer do odswieżania UI co 100 ms
         *                  
         * Parametry:       -
         * =========================================================================================================================================================
         */
        private void timer_FastRefresh(object sender, EventArgs e)
        {
            try
            {
                //
                // Odliczanie aktywacji (opoznienie po starcie
                //

                if ((Environment.TickCount - delay_aktywacja > DELAY_AKTYWACJA) && aktywacja_ostatniego == false)
                {
                    AktywacjaOstatnioUzywanego();
                    aktywacja_ostatniego = true;
                }

                //
                // Mruganie 
                //

                if (blink_cnt++ > 4)
                    blink_cnt = 0;

                if (blink_cnt > 2)
                    blink_1 = true;
                else
                    blink_1 = false;

                //
                // Mruganie przycisków referujących
                //

                if ((PLC.status.Word & 0x01) > 0)
                {
                    if (blink_1)
                        buttonPomiarRefPradu.BackColor = System.Drawing.Color.MediumOrchid;
                    else
                        buttonPomiarRefPradu.BackColor = System.Drawing.Color.BlueViolet;
                }
                else
                    buttonPomiarRefPradu.BackColor = System.Drawing.Color.MediumOrchid;

                if ((PLC.status.Word & 0x02) > 0)
                {
                    if (blink_1)
                        buttonPomiarCylindra.BackColor = System.Drawing.Color.MediumOrchid;
                    else
                        buttonPomiarCylindra.BackColor = System.Drawing.Color.BlueViolet;
                }
                else
                    buttonPomiarCylindra.BackColor = System.Drawing.Color.MediumOrchid;

                if ((PLC.status.Word & 0x04) > 0)
                {
                    checkBox_IOTest.Checked = true;
                }
                else
                    checkBox_IOTest.Checked = false;

                //
                // Uprawanienie ustawiacza
                //

                poziom_ustawiacz = PLC.zezwprog.Bit;

                //
                // Wyswietlenie liczników operatora 
                //

                textLicznik.Text = PLC.licznik.Word.ToString().PadLeft(5, '0');
                textLicznikMax.Text = PLC.licznik_max.Word.ToString().PadLeft(5, '0');
                //if (comboParametrPrzyrzad.SelectedIndex != -1)
                //    textParametrPrzyrzad.Text = Modbus.io.sm[comboParametrPrzyrzad.SelectedIndex].ToString().PadLeft(5, '0');

                ActiveProgEdit = (byte)comboNrProgramu.SelectedIndex;

                if (hide_param == false && comboParametr.SelectedItem != null)
                {
                    if (comboParametr.Items.Count > 0)
                        ActiveParamEdit = comboParametr.SelectedItem.ToString();
                    else
                        ActiveParamEdit = "";
                }

                if (hide_param == false && comboParametrPrzyrzad.SelectedItem != null)
                {
                    if (comboParametrPrzyrzad.Items.Count > 0)
                        ActiveToolParamEdit = comboParametrPrzyrzad.SelectedItem.ToString();
                    else
                        ActiveToolParamEdit = "";
                }

                WyswietlParametrZgrzewania();
                WyswietlParametrPrzyrzadu();

                //
                // Jezeli resetujemy timery to trzeba wylaczyc na chwile watek plc
                //
                if (reset_timers)
                {
                    reset_timers = false;
                    plc_start = false;
                    while (!plc_stopped) { Thread.Sleep(100); }
                    ResetTimers();
                    plc_start = true;
                    label_time_zmiana_parametru.BackColor = label_time_wyslanie_obrazu_wyjsc.BackColor = label_time_odczyt_obrazu_io.BackColor = Color.DarkViolet;
                    label_time_odczyt_obrazu_io_max.Text = "0";
                    label_time_wyslanie_obrazu_wyjsc_max.Text = "0";
                    label_time_zmiana_parametru_max.Text = "0";
                }

                //
                // Kontrolki IO muszą być odświeżane szybciej
                //
                KontrolkiStanIO();

                // 
                // Informacje o komunikacji w oknie systemowym
                //
                label_counter_crc_errors.Text = Modbus.crc_err_counter.ToString();
                label_time_aktywacja.Text = time_aktywacja.ToString();
                label_time_main.Text = time_main.ToString();
                label_time_odczyt_obrazu_io.Text = time_odczyt_obrazu_io.ToString();
                label_time_plc.Text = time_plc.ToString();
                label_time_wyslanie_obrazu_wyjsc.Text = time_wyslanie_obrazu_wyjsc.ToString();
                label_time_zmiana_parametru.Text = time_zmiana_parametru.ToString();
                label_time_program.Text = time_odswiez_program.ToString();

                label_0x03_plot.Text = Modbus.licznik_ploter.ToString();
                label_0x03_io.Text = Modbus.licznik_io_0x03.ToString();
                label_0x03_prog.Text = Modbus.licznik_program.ToString();

                if (timeout_odczyt_obrazu_io > TIMEOUT_ODCZYT_OBRAZ_3)
                {
                    label_time_odczyt_obrazu_io.BackColor = Color.Red;
                    label_time_odczyt_obrazu_io_max.Text = timeout_odczyt_obrazu_io.ToString();
                }

                if (timeout_wyslanie_obrazu_wyjsc > TIMEOUT_WYSYLA_OBRAZ_3)
                {
                    label_time_wyslanie_obrazu_wyjsc.BackColor = Color.Red;
                    label_time_wyslanie_obrazu_wyjsc_max.Text = timeout_wyslanie_obrazu_wyjsc.ToString();
                }

                if (timeout_zmiana_parametru > TIMEOUT_ZMIANA_PARAM_2)
                {
                    label_time_zmiana_parametru.BackColor = Color.Red;
                    label_time_zmiana_parametru_max.Text = timeout_zmiana_parametru.ToString();
                }

                // 
                // Informacje o stanie procesu
                //

                labelAktualnyStanProgramu.Text = TxtCykl[PLC.program_stan.Word, jezyk];
                labelAktualnyStanProgramu2.Text = TxtCykl[PLC.program_stan.Word, jezyk];

                Czujniki();
                Wyswietlanie_ParametryCzujnikiStart((ushort)comboNrProgramu.SelectedIndex);
                Wyswietlanie_ParametryCzujnikiDK((ushort)comboNrProgramu.SelectedIndex);
                Wyswietlanie_ParametryCylindry((ushort)comboNrProgramu.SelectedIndex);
                Wyswietlanie_ParametryInne();

                labelAktualnyProgram.Text = PLC.cykl_program_akt.Word.ToString() + " / " + PLC.cykl_liczba_programow.Word.ToString();
                labelAktualnyProgram2.Text = TxtNrProgramu[jezyk] + ": " + PLC.cykl_program_akt.Word.ToString();

                if (((PLC.err1.Word & 0x02) == 0) && !(Zarzadzanie.usunieto_aktywny) && Modbus.CommunicationOK)
                {
                    Zarzadzanie.AktywacjaOK = true;
                    Zarzadzanie.AktywnyPrzyrzad = nazwa_przyrzadu_do_aktywacji;

                }
                else
                {
                    Zarzadzanie.AktywacjaOK = false;
                    Zarzadzanie.AktywnyPrzyrzad = "";

                }

                //
                // Kursor na wykresie
                //

                label_Wykresy_Prad.Text = String.Format("{0:0.0} kA", (((float)(Ploter.wykres_prad[Ploter.cursor / 10]) / AIN_Meas_Mul) * MUL_Prad));
                label_Wykresy_Napiecie.Text = String.Format("{0:0.0} V", (((float)(Ploter.wykres_napiecie[Ploter.cursor / 10]) / AIN_Meas_Mul)));
                label_Wykresy_Wtopienie.Text = String.Format("{0:0.0} mm", (((float)(Ploter.wykres_wtopienie[Ploter.cursor / 10]) / AIN_Mul) * MUL_PozycjaCylindra));
                label_Wykresy_Moc.Text = String.Format("{0:0.0} kVA", (((float)(Ploter.wykres_prad[Ploter.cursor / 10]) / AIN_Meas_Mul) * MUL_Prad) * (((float)(Ploter.wykres_napiecie[Ploter.cursor / 10]) / AIN_Meas_Mul)));

                if (czas_wykres != null)
                    label_Wykresy_Czas.Text = czas_wykres.Hour.ToString().PadLeft(2, '0') + ":" + czas_wykres.Minute.ToString().PadLeft(2, '0') + ":" + czas_wykres.Second.ToString().PadLeft(2, '0');

                label_Wykresy_NrProgramu.Text = PLC.ost_zgrzew_nr_programu.Word.ToString();

                //
                // Uśrednianie pomiarów referencyjnych
                //

                label_ref_prad_akt.Text = PLC.ref_cnt_prad.Word.ToString();
                label_ref_nakr_akt.Text = PLC.ref_cnt_nakr.Word.ToString();


                //
                // Aktywacja odczytu wykresow
                //

                wykresy = checkBox_Wykresy.Checked;

            }
            catch (Exception ex)
            {
                Tools.LogEx(ex, "timer_FastRefresh");
            }
        }

        /* 
         * =========================================================================================================================================================
         * Nazwa:           timer_SlowRefresh 
         * 
         * Przeznaczenie:   Timer 1 s do odswiezenia kontrolek których odświeżenie trwa więcej czasu
         *                  
         * Parametry:       -
         * =========================================================================================================================================================
         */
        private void timer_SlowRefresh(object sender, EventArgs e)
        {
            try
            {

                //
                // Odswiezenie plota
                //

                if (odswiez_plota)
                {
                    picPloter.Invalidate();
                    //MessageBox.Show("Odswiezanie plota");
                    odswiez_plota = false;
                }

                //
                // Bitmapki
                //

                try
                {
                    if (Zarzadzanie.AktywacjaOK)
                    {
                        pictureWizuProg.Show();
                        pictureWizuTool.Show();
                        pictureWizuTool.Image = Image.FromHbitmap(Zarzadzanie.pics[0].GetHbitmap());
                        pictureWizuProg.Image = Image.FromHbitmap(Zarzadzanie.pics[PLC.cykl_program_akt.Word + 1].GetHbitmap());
                    }
                }
                catch (Exception)
                {
                    // Gdyby nie bylo obrazka w tablicy to nie robimy nic
                    pictureWizuProg.Hide();
                    pictureWizuTool.Hide();
                }

                //
                // Info w status barze z innego wątku
                //
                if (status_aktywowano)
                {
                    status_aktywowano = false;
                    //statusBar.Text = "Aktywacja zakończona sukcesem";
                }
                if (status_produkcja)
                {
                    status_produkcja = false;
                    //statusBar.Text = "Produkcja";
                }


                //
                // Kodowanie
                // jeszcze nie skończone
                //
                //Kodowanie();

                //
                // Data i czas
                //

                labelData.Text = DateTime.Now.Year.ToString() + "-" + DateTime.Now.Month.ToString().PadLeft(2, '0') + "-" + DateTime.Now.Day.ToString().PadLeft(2, '0');
                labelCzas.Text = DateTime.Now.Hour.ToString().PadLeft(2, '0') + ":" + DateTime.Now.Minute.ToString().PadLeft(2, '0');

                //
                // Wyswietlenie obrazu IO w tablicy
                //

                if (Modbus.CommunicationOK)
                {
                    ushort j, k;
                    k = 0;
                    for (j = 0; j < 32; j++)
                        listObrazIO.Items[k++].SubItems[1].Text = Modbus.io.sm[j].ToString();
                    for (j = 0; j < 4; j++)
                        listObrazIO.Items[k++].SubItems[1].Text = Modbus.io.q[j].ToString();
                    for (j = 0; j < 4; j++)
                        listObrazIO.Items[k++].SubItems[1].Text = Modbus.io.aout[j].ToString();
                    for (j = 0; j < 6; j++)
                        listObrazIO.Items[k++].SubItems[1].Text = Modbus.io.i[j].ToString();
                    for (j = 0; j < 6; j++)
                        listObrazIO.Items[k++].SubItems[1].Text = Modbus.io.ain[j].ToString();
                }

                //
                // Kod przyrzadu
                //

                Zarzadzanie.kod_podlaczony = PLC.kod_podlaczony;

                label_kod_aktywny.Text = Zarzadzanie.kod_aktywny.ToString();
                label_kod_podlaczony.Text = Zarzadzanie.kod_podlaczony.ToString();

                //
                // Nazwa narzędzia w polach tekstowych
                //
                //string sama_nazwa = "";
                //if (Zarzadzanie.AktywnyPrzyrzad.LastIndexOf('.') != -1)
                //sama_nazwa = Zarzadzanie.AktywnyPrzyrzad.Substring(0, Zarzadzanie.AktywnyPrzyrzad.LastIndexOf('.'));
                labelToolName1.Text = Zarzadzanie.AktywnyPrzyrzad;
                labelToolName2.Text = Zarzadzanie.AktywnyPrzyrzad;
                labelToolName3.Text = Zarzadzanie.AktywnyPrzyrzad;
                labelToolName4.Text = TxtPrzyrzad[jezyk] + ": " + Zarzadzanie.AktywnyPrzyrzad;

                if (Modbus.CommunicationOK == false)
                    this.Text = "APTerminal (brak komunikacji!)";
                else
                    this.Text = "APTerminal (system gotowy)";

                //
                // Czy jest aktywny timer z poziomem ur?
                // Jezeli tak to mamy UR
                //

                if (timerPoziomUR.Enabled)
                    poziom_ur = true;
                else
                    poziom_ur = false;

                // ********************************************************
                // Włączenie kontrolek uzależnione od poziomu uprawnień
                // ********************************************************

                // ********************************************************
                // Poziom UR - przyciski w menu systemowym
                // Najwyższy - to musi być aktywne tylko dla UR
                // ********************************************************
                if (poziom_ur)
                {
                    Autoconnect.Enabled = true;
                    ButtonDisconnect.Enabled = true;
                    ButtonConnect.Enabled = true;
                    numericModbusSlaveID.Enabled = true;
                    this.MinimizeBox = true;
                    buttonMinimalizuj.Enabled = true;
                    buttonZamknij.Enabled = true;
                    checkBox_IOTest.Enabled = true;
                }
                else
                {
                    Autoconnect.Enabled = false;
                    ButtonDisconnect.Enabled = false;
                    ButtonConnect.Enabled = false;
                    numericModbusSlaveID.Enabled = false;
                    this.MinimizeBox = false;
                    buttonMinimalizuj.Enabled = false;
                    buttonZamknij.Enabled = false;
                    checkBox_IOTest.Enabled = false;
                }

                // ********************************************************
                // Jest komunikacja i poziom UR lub Ustawiacz
                // Tutaj można aktywoać przyrząd, zarządzać
                // Potwierdzić zakłócenie
                // ********************************************************
                if ((poziom_ustawiacz || poziom_ur) && Modbus.CommunicationOK)
                {
                    buttonToolActivate.Enabled = true;
                    buttonPotwierdzenieZaklocenia.Enabled = true;
                }
                else
                {
                    buttonToolActivate.Enabled = false;
                    buttonPotwierdzenieZaklocenia.Enabled = false;
                }

                if (poziom_ustawiacz || poziom_ur) 
                {
                    buttonToolNew.Enabled = true;
                    buttonToolNewCopy.Enabled = true;
                    buttonDeactivate.Enabled = true;
                    buttonDelete.Enabled = true;
                }
                else
                {
                    buttonToolNew.Enabled = false;
                    buttonToolNewCopy.Enabled = false;
                    buttonDeactivate.Enabled = false;
                    buttonDelete.Enabled = false;
                }

                // ********************************************************
                // Przyrząd jest aktywny a poziom UR lub Ustawiacz
                // Można zrobić wszystko co związane jest ustawieniami 
                // parametrami itp.
                // ********************************************************
                if ((poziom_ustawiacz || poziom_ur) && Modbus.CommunicationOK && Zarzadzanie.AktywacjaOK)
                {
                    buttonPozycjaWyjsciowa.Enabled = true;
                    button_PozycjaTol_Minus.Enabled = true;
                    button_PozycjaTol_Plus.Enabled = true;
                    button_WydmuchTol_Minus.Enabled = true;
                    button_WydmuchTol_Plus.Enabled = true;
                    buttonPomiarCylindra.Enabled = true;
                    buttonPomiarRefPradu.Enabled = true;
                    checkBox_Pomiary_Srednia.Enabled = true;
                    numeric_ref_nakr_zadane.Enabled = true;
                    numeric_ref_prad_zadane.Enabled = true;
                
                    plus10000.Enabled = true;
                    plus1000.Enabled = true;
                    plus100.Enabled = true;
                    plus10.Enabled = true;
                    plus1.Enabled = true;
                    minus10000.Enabled = true;
                    minus1000.Enabled = true;
                    minus100.Enabled = true;
                    minus10.Enabled = true;
                    minus1.Enabled = true;
                    checkBoxStartKonfig_01.Enabled = true;
                    checkBoxStartKonfig_02.Enabled = true;
                    checkBoxStartKonfig_03.Enabled = true;
                    checkBoxStartKonfig_04.Enabled = true;
                    checkBoxStartKonfig_05.Enabled = true;
                    checkBoxStartKonfig_06.Enabled = true;
                    checkBoxStartKonfig_07.Enabled = true;
                    checkBoxStartKonfig_08.Enabled = true;
                    checkBoxStartKonfig_09.Enabled = true;
                    checkBoxStartKonfig_10.Enabled = true;
                    checkBoxStartKonfig_11.Enabled = true;
                    checkBoxStartKonfig_12.Enabled = true;
                    checkBoxStartKonfig_13.Enabled = true;
                    checkBoxStartKonfig_14.Enabled = true;
                    radioButtonCzujnikStart_01_1.Enabled = true;
                    radioButtonCzujnikStart_02_1.Enabled = true;
                    radioButtonCzujnikStart_03_1.Enabled = true;
                    radioButtonCzujnikStart_04_1.Enabled = true;
                    radioButtonCzujnikStart_05_1.Enabled = true;
                    radioButtonCzujnikStart_06_1.Enabled = true;
                    radioButtonCzujnikStart_07_1.Enabled = true;
                    radioButtonCzujnikStart_08_1.Enabled = true;
                    radioButtonCzujnikStart_09_1.Enabled = true;
                    radioButtonCzujnikStart_10_1.Enabled = true;
                    radioButtonCzujnikStart_11_1.Enabled = true;
                    radioButtonCzujnikStart_12_1.Enabled = true;
                    radioButtonCzujnikStart_13_1.Enabled = true;
                    radioButtonCzujnikStart_14_1.Enabled = true;
                    radioButtonCzujnikStart_01_0.Enabled = true;
                    radioButtonCzujnikStart_02_0.Enabled = true;
                    radioButtonCzujnikStart_03_0.Enabled = true;
                    radioButtonCzujnikStart_04_0.Enabled = true;
                    radioButtonCzujnikStart_05_0.Enabled = true;
                    radioButtonCzujnikStart_06_0.Enabled = true;
                    radioButtonCzujnikStart_07_0.Enabled = true;
                    radioButtonCzujnikStart_08_0.Enabled = true;
                    radioButtonCzujnikStart_09_0.Enabled = true;
                    radioButtonCzujnikStart_10_0.Enabled = true;
                    radioButtonCzujnikStart_11_0.Enabled = true;
                    radioButtonCzujnikStart_12_0.Enabled = true;
                    radioButtonCzujnikStart_13_0.Enabled = true;
                    radioButtonCzujnikStart_14_0.Enabled = true;
                    checkBoxDKKonfig_001.Enabled = true;
                    checkBoxDKKonfig_02.Enabled = true;
                    checkBoxDKKonfig_03.Enabled = true;
                    checkBoxDKKonfig_04.Enabled = true;
                    checkBoxDKKonfig_05.Enabled = true;
                    checkBoxDKKonfig_06.Enabled = true;
                    checkBoxDKKonfig_07.Enabled = true;
                    checkBoxDKKonfig_08.Enabled = true;
                    checkBoxDKKonfig_09.Enabled = true;
                    checkBoxDKKonfig_10.Enabled = true;
                    checkBoxDKKonfig_11.Enabled = true;
                    checkBoxDKKonfig_12.Enabled = true;
                    checkBoxDKKonfig_13.Enabled = true;
                    checkBoxDKKonfig_14.Enabled = true;
                    radioButtonCzujnikDK_01_1.Enabled = true;
                    radioButtonCzujnikDK_02_1.Enabled = true;
                    radioButtonCzujnikDK_03_1.Enabled = true;
                    radioButtonCzujnikDK_04_1.Enabled = true;
                    radioButtonCzujnikDK_05_1.Enabled = true;
                    radioButtonCzujnikDK_06_1.Enabled = true;
                    radioButtonCzujnikDK_07_1.Enabled = true;
                    radioButtonCzujnikDK_08_1.Enabled = true;
                    radioButtonCzujnikDK_09_1.Enabled = true;
                    radioButtonCzujnikDK_10_1.Enabled = true;
                    radioButtonCzujnikDK_11_1.Enabled = true;
                    radioButtonCzujnikDK_12_1.Enabled = true;
                    radioButtonCzujnikDK_13_1.Enabled = true;
                    radioButtonCzujnikDK_14_1.Enabled = true;
                    radioButtonCzujnikDK_01_0.Enabled = true;
                    radioButtonCzujnikDK_02_0.Enabled = true;
                    radioButtonCzujnikDK_03_0.Enabled = true;
                    radioButtonCzujnikDK_04_0.Enabled = true;
                    radioButtonCzujnikDK_05_0.Enabled = true;
                    radioButtonCzujnikDK_06_0.Enabled = true;
                    radioButtonCzujnikDK_07_0.Enabled = true;
                    radioButtonCzujnikDK_08_0.Enabled = true;
                    radioButtonCzujnikDK_09_0.Enabled = true;
                    radioButtonCzujnikDK_10_0.Enabled = true;
                    radioButtonCzujnikDK_11_0.Enabled = true;
                    radioButtonCzujnikDK_12_0.Enabled = true;
                    radioButtonCzujnikDK_13_0.Enabled = true;
                    radioButtonCzujnikDK_14_0.Enabled = true;

                    checkBox_Cylinder_01.Enabled = true;
                    checkBox_Cylinder_01_DodSila.Enabled = true;
                    checkBox_Cylinder_02.Enabled = true;
                    checkBox_Cylinder_03.Enabled = true;
                    checkBox_Cylinder_04.Enabled = true;

                    checkBox_Zawor_11.Enabled = true;
                    checkBox_Zawor_12.Enabled = true;
                    checkBox_Zawor_21.Enabled = true;
                    checkBox_Zawor_22.Enabled = true;
                    checkBox_Zawor_31.Enabled = true;
                    checkBox_Zawor_32.Enabled = true;
                    checkBox_Zawor_41.Enabled = true;
                    checkBox_Zawor_42.Enabled = true;

                    checkBox_PulpitDwureczny.Enabled = true;
                    radioButton_Wyblokowanie_WDole.Enabled = true;
                    radioButton_Wyblokowanie_WGorze.Enabled = true;

                    parprzy_plus1.Enabled = true;
                    parprzy_plus10.Enabled = true;
                    parprzy_plus100.Enabled = true;
                    parprzy_plus1000.Enabled = true;
                    parprzy_plus10000.Enabled = true;
                    parprzy_minus1.Enabled = true;
                    parprzy_minus10.Enabled = true;
                    parprzy_minus100.Enabled = true;
                    parprzy_minus1000.Enabled = true;
                    parprzy_minus10000.Enabled = true;

                }
                else
                {
                    buttonPozycjaWyjsciowa.Enabled = false;
                    button_PozycjaTol_Minus.Enabled = false;
                    button_PozycjaTol_Plus.Enabled = false;
                    button_WydmuchTol_Minus.Enabled = false;
                    button_WydmuchTol_Plus.Enabled = false;
                    buttonPomiarCylindra.Enabled = false;
                    buttonPomiarRefPradu.Enabled = false;
                    checkBox_Pomiary_Srednia.Enabled = false;
                    numeric_ref_nakr_zadane.Enabled = false;
                    numeric_ref_prad_zadane.Enabled = false;

                    plus10000.Enabled = false;
                    plus1000.Enabled = false;
                    plus100.Enabled = false;
                    plus10.Enabled = false;
                    plus1.Enabled = false;
                    minus10000.Enabled = false;
                    minus1000.Enabled = false;
                    minus100.Enabled = false;
                    minus10.Enabled = false;
                    minus1.Enabled = false;
                    checkBoxStartKonfig_01.Enabled = false;
                    checkBoxStartKonfig_02.Enabled = false;
                    checkBoxStartKonfig_03.Enabled = false;
                    checkBoxStartKonfig_04.Enabled = false;
                    checkBoxStartKonfig_05.Enabled = false;
                    checkBoxStartKonfig_06.Enabled = false;
                    checkBoxStartKonfig_07.Enabled = false;
                    checkBoxStartKonfig_08.Enabled = false;
                    checkBoxStartKonfig_09.Enabled = false;
                    checkBoxStartKonfig_10.Enabled = false;
                    checkBoxStartKonfig_11.Enabled = false;
                    checkBoxStartKonfig_12.Enabled = false;
                    checkBoxStartKonfig_13.Enabled = false;
                    checkBoxStartKonfig_14.Enabled = false;
                    radioButtonCzujnikStart_01_1.Enabled = false;
                    radioButtonCzujnikStart_02_1.Enabled = false;
                    radioButtonCzujnikStart_03_1.Enabled = false;
                    radioButtonCzujnikStart_04_1.Enabled = false;
                    radioButtonCzujnikStart_05_1.Enabled = false;
                    radioButtonCzujnikStart_06_1.Enabled = false;
                    radioButtonCzujnikStart_07_1.Enabled = false;
                    radioButtonCzujnikStart_08_1.Enabled = false;
                    radioButtonCzujnikStart_09_1.Enabled = false;
                    radioButtonCzujnikStart_10_1.Enabled = false;
                    radioButtonCzujnikStart_11_1.Enabled = false;
                    radioButtonCzujnikStart_12_1.Enabled = false;
                    radioButtonCzujnikStart_13_1.Enabled = false;
                    radioButtonCzujnikStart_14_1.Enabled = false;
                    radioButtonCzujnikStart_01_0.Enabled = false;
                    radioButtonCzujnikStart_02_0.Enabled = false;
                    radioButtonCzujnikStart_03_0.Enabled = false;
                    radioButtonCzujnikStart_04_0.Enabled = false;
                    radioButtonCzujnikStart_05_0.Enabled = false;
                    radioButtonCzujnikStart_06_0.Enabled = false;
                    radioButtonCzujnikStart_07_0.Enabled = false;
                    radioButtonCzujnikStart_08_0.Enabled = false;
                    radioButtonCzujnikStart_09_0.Enabled = false;
                    radioButtonCzujnikStart_10_0.Enabled = false;
                    radioButtonCzujnikStart_11_0.Enabled = false;
                    radioButtonCzujnikStart_12_0.Enabled = false;
                    radioButtonCzujnikStart_13_0.Enabled = false;
                    radioButtonCzujnikStart_14_0.Enabled = false;
                    checkBoxDKKonfig_001.Enabled = false;
                    checkBoxDKKonfig_02.Enabled = false;
                    checkBoxDKKonfig_03.Enabled = false;
                    checkBoxDKKonfig_04.Enabled = false;
                    checkBoxDKKonfig_05.Enabled = false;
                    checkBoxDKKonfig_06.Enabled = false;
                    checkBoxDKKonfig_07.Enabled = false;
                    checkBoxDKKonfig_08.Enabled = false;
                    checkBoxDKKonfig_09.Enabled = false;
                    checkBoxDKKonfig_10.Enabled = false;
                    checkBoxDKKonfig_11.Enabled = false;
                    checkBoxDKKonfig_12.Enabled = false;
                    checkBoxDKKonfig_13.Enabled = false;
                    checkBoxDKKonfig_14.Enabled = false;
                    radioButtonCzujnikDK_01_1.Enabled = false;
                    radioButtonCzujnikDK_02_1.Enabled = false;
                    radioButtonCzujnikDK_03_1.Enabled = false;
                    radioButtonCzujnikDK_04_1.Enabled = false;
                    radioButtonCzujnikDK_05_1.Enabled = false;
                    radioButtonCzujnikDK_06_1.Enabled = false;
                    radioButtonCzujnikDK_07_1.Enabled = false;
                    radioButtonCzujnikDK_08_1.Enabled = false;
                    radioButtonCzujnikDK_09_1.Enabled = false;
                    radioButtonCzujnikDK_10_1.Enabled = false;
                    radioButtonCzujnikDK_11_1.Enabled = false;
                    radioButtonCzujnikDK_12_1.Enabled = false;
                    radioButtonCzujnikDK_13_1.Enabled = false;
                    radioButtonCzujnikDK_14_1.Enabled = false;
                    radioButtonCzujnikDK_01_0.Enabled = false;
                    radioButtonCzujnikDK_02_0.Enabled = false;
                    radioButtonCzujnikDK_03_0.Enabled = false;
                    radioButtonCzujnikDK_04_0.Enabled = false;
                    radioButtonCzujnikDK_05_0.Enabled = false;
                    radioButtonCzujnikDK_06_0.Enabled = false;
                    radioButtonCzujnikDK_07_0.Enabled = false;
                    radioButtonCzujnikDK_08_0.Enabled = false;
                    radioButtonCzujnikDK_09_0.Enabled = false;
                    radioButtonCzujnikDK_10_0.Enabled = false;
                    radioButtonCzujnikDK_11_0.Enabled = false;
                    radioButtonCzujnikDK_12_0.Enabled = false;
                    radioButtonCzujnikDK_13_0.Enabled = false;
                    radioButtonCzujnikDK_14_0.Enabled = false;

                    checkBox_Cylinder_01.Enabled = false;
                    checkBox_Cylinder_01_DodSila.Enabled = false;
                    checkBox_Cylinder_02.Enabled = false;
                    checkBox_Cylinder_03.Enabled = false;
                    checkBox_Cylinder_04.Enabled = false;

                    checkBox_Zawor_11.Enabled = false;
                    checkBox_Zawor_12.Enabled = false;
                    checkBox_Zawor_21.Enabled = false;
                    checkBox_Zawor_22.Enabled = false;
                    checkBox_Zawor_31.Enabled = false;
                    checkBox_Zawor_32.Enabled = false;
                    checkBox_Zawor_41.Enabled = false;
                    checkBox_Zawor_42.Enabled = false;

                    checkBox_PulpitDwureczny.Enabled = false;
                    radioButton_Wyblokowanie_WDole.Enabled = false;
                    radioButton_Wyblokowanie_WGorze.Enabled = false;

                    parprzy_plus1.Enabled = false;
                    parprzy_plus10.Enabled = false;
                    parprzy_plus100.Enabled = false;
                    parprzy_plus1000.Enabled = false;
                    parprzy_plus10000.Enabled = false;
                    parprzy_minus1.Enabled = false;
                    parprzy_minus10.Enabled = false;
                    parprzy_minus100.Enabled = false;
                    parprzy_minus1000.Enabled = false;
                    parprzy_minus10000.Enabled = false;

                }

                // ********************************************************
                // Poziom produkcji - to co może zrobić operator gdy
                // zgrzewa
                // ********************************************************
                if (Modbus.CommunicationOK && Zarzadzanie.AktywacjaOK)
                {
                    button_Licznik_Plus10000.Enabled = true;
                    button_Licznik_Plus1000.Enabled = true;
                    button_Licznik_Plus100.Enabled = true;
                    button_Licznik_Plus10.Enabled = true;
                    button_Licznik_Plus1.Enabled = true;
                    button_Licznik_Minus10000.Enabled = true;
                    button_Licznik_Minus1000.Enabled = true;
                    button_Licznik_Minus100.Enabled = true;
                    button_Licznik_Minus10.Enabled = true;
                    button_Licznik_Minus1.Enabled = true;
                    button_LicznikZeruj.Enabled = true;
                    button_LicznikMax_Plus10000.Enabled = true;
                    button_LicznikMax_Plus1000.Enabled = true;
                    button_LicznikMax_Plus100.Enabled = true;
                    button_LicznikMax_Plus10.Enabled = true;
                    button_LicznikMax_Plus1.Enabled = true;
                    button_LicznikMax_Minus10000.Enabled = true;
                    button_LicznikMax_Minus1000.Enabled = true;
                    button_LicznikMax_Minus100.Enabled = true;
                    button_LicznikMax_Minus10.Enabled = true;
                    button_LicznikMax_Minus1.Enabled = true;
                    button_LicznikMaxZeruj.Enabled = true;
                }
                else
                {
                    button_Licznik_Plus10000.Enabled = false;
                    button_Licznik_Plus1000.Enabled = false;
                    button_Licznik_Plus100.Enabled = false;
                    button_Licznik_Plus10.Enabled = false;
                    button_Licznik_Plus1.Enabled = false;
                    button_Licznik_Minus10000.Enabled = false;
                    button_Licznik_Minus1000.Enabled = false;
                    button_Licznik_Minus100.Enabled = false;
                    button_Licznik_Minus10.Enabled = false;
                    button_Licznik_Minus1.Enabled = false;
                    button_LicznikZeruj.Enabled = false;
                    button_LicznikMax_Plus10000.Enabled = false;
                    button_LicznikMax_Plus1000.Enabled = false;
                    button_LicznikMax_Plus100.Enabled = false;
                    button_LicznikMax_Plus10.Enabled = false;
                    button_LicznikMax_Plus1.Enabled = false;
                    button_LicznikMax_Minus10000.Enabled = false;
                    button_LicznikMax_Minus1000.Enabled = false;
                    button_LicznikMax_Minus100.Enabled = false;
                    button_LicznikMax_Minus10.Enabled = false;
                    button_LicznikMax_Minus1.Enabled = false;
                    button_LicznikMaxZeruj.Enabled = false;
                }

                // 
                // Info o aktualnym poziomie uprawnień
                //
                if (poziom_ur)
                    labelPoziomDostepu.Text = "Serwis";
                else if (PLC.zezwprog.Bit && !poziom_ur)
                    labelPoziomDostepu.Text = "Ustawiacz";
                else if (!PLC.zezwprog.Bit && !poziom_ur)
                    labelPoziomDostepu.Text = "Produkcja";

                //
                // Jezeli zmieniono pozycję okna to ustawiamy na 0,0
                //
                if (Tools.windowsCE)
                    if (this.Location.X != 0 || this.Location.Y != 0)
                        this.Location = new Point(0, 0);
                //
                // TickCount
                //
                label_TickCount.Text = Environment.TickCount.ToString();

                // 
                // Zakłócenia
                //
                //if (err_stary != err)
                //{

                err_kopia = err;

                zak.Items.Clear();
                for (int bit = 0; bit < 32; bit++)
                {
                    if ((err_kopia & (1 << bit)) > 0)
                    {
                        if (!zak.Items.Contains(zak_tab[bit]))
                            zak.Items.Add(zak_tab[bit]);

                    }
                }

                Tools.LogErr(err_kopia);

                //}

                // 
                // Brak komunikacji obslugiwany inaczej
                //
                if (!Modbus.CommunicationOK)
                {
                    if (!zak.Items.Contains("Brak komunikacji!"))
                        zak.Items.Add("Brak komunikacji!");
                }
                else
                {
                    if (zak.Items.Contains("Brak komunikacji!"))
                        zak.Items.Remove("Brak komunikacji!");
                }

                // 
                // Przekroczona zajetosc dysku w 80%
                //
                if (Tools.procent_zajetosci_dysku > (ZAJETOSC_DYSKU_MAX - 10.0f))
                {
                    if (!zak.Items.Contains("Ostrzeżenie: zajętość dysku przekroczyła " + (ZAJETOSC_DYSKU_MAX - 10.0f).ToString() + "%"))
                        zak.Items.Add("Ostrzeżenie: zajętość dysku przekroczyła " + (ZAJETOSC_DYSKU_MAX - 10.0f).ToString() + "%");
                }
                else
                {
                    if (zak.Items.Contains("Ostrzeżenie: zajętość dysku przekroczyła " + (ZAJETOSC_DYSKU_MAX - 10.0f).ToString() + "%"))
                        zak.Items.Remove("Ostrzeżenie: zajętość dysku przekroczyła " + (ZAJETOSC_DYSKU_MAX - 10.0f).ToString() + "%");
                }

                // 
                // Przekroczona zajetosc dysku w 90%
                //
                if (Tools.procent_zajetosci_dysku > ZAJETOSC_DYSKU_MAX)
                {
                    if (!zak.Items.Contains("Błąd: Zajętość dysku przekroczyła " + ZAJETOSC_DYSKU_MAX.ToString() + "%! Zarchiwizuj dane."))
                        zak.Items.Add("Błąd: Zajętość dysku przekroczyła " + ZAJETOSC_DYSKU_MAX.ToString() + "%! Zarchiwizuj dane.");
                }
                else
                {
                    if (zak.Items.Contains("Błąd: Zajętość dysku przekroczyła " + ZAJETOSC_DYSKU_MAX.ToString() + "%! Zarchiwizuj dane."))
                        zak.Items.Remove("Błąd: Zajętość dysku przekroczyła " + ZAJETOSC_DYSKU_MAX.ToString() + "%! Zarchiwizuj dane.");
                }


                err_kopia = err;
            }
            catch (Exception ex)
            {
                Tools.LogEx(ex, "timer_SlowRefresh");
            }
        }

        /* 
         * =========================================================================================================================================================
         * Nazwa:           timer_5S_Tick 
         * 
         * Przeznaczenie:   Timer 5 s do rzeczy robionych rzadko
         *                  
         * Parametry:       -
         * =========================================================================================================================================================
         */
        private void timer_5S_Tick(object sender, EventArgs e)
        {
            //
            // System->Info
            //

            label_NazwaMaszyny.Text =       Tools.ReadSetting("Name");
            label_NazwaSterownika.Text =    Tools.ReadSetting("ControllerName");
            label_WersjaAPTerminal.Text =   System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            label_WersjaFirmware.Text =     ((PLC.firmware_version.Word & 0xf000) >> 12).ToString() + "." +
                                            ((PLC.firmware_version.Word & 0x0f00) >> 8).ToString() + "." +
                                            ((PLC.firmware_version.Word & 0x00f0) >> 4).ToString() + "." +
                                            (PLC.firmware_version.Word & 0x000f).ToString();

            //
            // Rozpoznanie czy jest komunkacja
            //
            if ((Serial.connected && Modbus.frames > 2) || UR_ENABLED_AT_START)
                Modbus.CommunicationOK = true;
            else
                Modbus.CommunicationOK = false;

            Modbus.frames = 0;

            // 
            // Temperatura uC
            //
            label_Temperatura.Text = PLC.temperatura_uc.Word.ToString() + " \u00b0C";

            //
            // Miejsce na dysku
            // 

            progressBar_DyskInfo.Maximum = (int)(Tools.dysk_rozmiar / 1000000);
            progressBar_DyskInfo.Value = (int)((Tools.dysk_rozmiar - Tools.dysk_dostepne) / 1000000);
            labelDriveInfo_NazwaDysku.Text = Tools.nazwa_dysku.ToString();
            labelDriveInfo_ZajeteMiejsce.Text = (Tools.dysk_rozmiar / 1048576 - Tools.dysk_dostepne / 1048576).ToString() + " MB";
            labelDriveInfo_RozmiarDysku.Text = (Tools.dysk_rozmiar / 1048576).ToString() + " MB";
            labelDriveInfo_ZajeteProcent.Text = String.Format("{0:0.00}", Tools.procent_zajetosci_dysku);
            labelDriveInfo_ZajeteProcent.Text += " %";

        }

        /* 
         * =========================================================================================================================================================
         * Nazwa:           BrakMiejscaInfo
         * 
         * Przeznaczenie:   Wyswietlenie komunikatu o braku miejsca na dysku
         *                  
         * Parametry:       -
         * =========================================================================================================================================================
         */

        private static void BrakMiejscaInfo()
        {
            MessageBox.Show("Zajętość dysku przekroczyła " + ZAJETOSC_DYSKU_MAX.ToString() + "%! Zarchiwizuj dane.");
        }

        /* 
         * =========================================================================================================================================================
         * Nazwa:           BrakMiejscaInfo
         * 
         * Przeznaczenie:   Wyswietlenie komunikatu o braku miejsca na dysku
         *                  
         * Parametry:       -
         * =========================================================================================================================================================
         */

        private static void ObliczMiejsceNaDysku()
        {
            Tools.GetDiskInfo();

            Tools.procent_zajetosci_dysku = ((float)(((((float)Tools.dysk_rozmiar - (float)Tools.dysk_dostepne)) / (float)Tools.dysk_rozmiar) * 100.0));

            if (Tools.procent_zajetosci_dysku > ZAJETOSC_DYSKU_MAX)
                brak_miejsca_na_dysku = true;
            else
                brak_miejsca_na_dysku = false;
        }

        /* 
         * =========================================================================================================================================================
         * Nazwa:           UIThread 
         * 
         * Przeznaczenie:   Watek do wykorzystania - petla glowna watku
         *                  
         * Parametry:       -
         * =========================================================================================================================================================
         */

        private static void ResetTimers()
        {
            Modbus.crc_err_counter = 0;
            time_odczyt_obrazu_io_start = time_odczyt_obrazu_io = 0;
            time_wyslanie_obrazu_wyjsc_start = time_wyslanie_obrazu_wyjsc = 0;
            time_zmiana_parametru_start = time_zmiana_parametru = 0;
            timeout_odczyt_obrazu_io = timeout_wyslanie_obrazu_wyjsc = timeout_zmiana_parametru = 0;
        }

        /* 
        * =========================================================================================================================================================
        * Nazwa:           ...
        * 
        * Przeznaczenie:   Obsluga kodowania przyrządów 
        *                  
        * Parametry:       -
        * =========================================================================================================================================================
        */
        string SzukajKod(UInt16 kod)
        {
            string retval = "";

            string[] katalogi = Directory.GetDirectories(Tools.ToolsDirectory);

            string numer = kod.ToString();
            Char[] znaki = new Char[] { '.' };

            foreach (string tool_name in katalogi)
            {
                string[] splited = tool_name.Split(znaki);
                if (splited.Length > 1)
                {
                    if (splited[1].Length > 0)
                    {
                        if (splited[1] == numer)
                            retval = splited[0].Substring(splited[0].LastIndexOf('\\') + 1, splited[0].Length - splited[0].LastIndexOf('\\') - 1);
                    }
                }
            }

            return retval;
        }

        private void numeric_KodSzukanie_ValueChanged(object sender, EventArgs e)
        {
            numeric_KodSzukanie.BackColor = SystemColors.Window;
        }

        private void button_SzukajKodu_Click(object sender, EventArgs e)
        {
            if (SzukajKod((UInt16)numeric_KodSzukanie.Value) != "")
                numeric_KodSzukanie.BackColor = Color.LightGreen;
            else
                numeric_KodSzukanie.BackColor = Color.Red;

        }

        /* 
         * =========================================================================================================================================================
         * Nazwa:           Kodowanie 
         * 
         * Przeznaczenie:   Funkcja rozpoznająca nowe przyrządy
         *                  
         * Parametry:       -
         * =========================================================================================================================================================
         */
        void Kodowanie()
        {
            string nazwa = "";

            //
            // Jezeli blad: brak aktywnego przyrzadu to ladujemy
            //
            if ((err & 0x00000200) > 0 && Modbus.CommunicationOK && Serial.connected)
            {
                //
                // Jezeli kod == 0 to ladujemy ostatni przyrzad (przy starcie programu)
                //
                if (Zarzadzanie.kod_podlaczony == 0)
                {
                    AktywacjaOstatnioUzywanego();
                }
                else
                {
                    // 
                    // Jezeli nnumer <> 0 to trzeba szukac przyrzadu
                    //
                    if ((poziom_ustawiacz || poziom_ur) && Zarzadzanie.kod_podlaczony != 0 && (Zarzadzanie.kod_podlaczony != kod_podlaczony || Zarzadzanie.kod_aktywny != kod_podlaczony))// && wykryto_nowy_przyrzad == false && aktywacja_start == false)
                    {
                        Modbus.Rozkaz(6);
                        Tools.WriteSetting("Tool", "");

                        //kod_poprzedni = kod_podlaczony;
                        //wykryto_nowy_przyrzad = true;
                    }


                    // Szukanie bazy danych tylko gdy zmiana kodu (podlaczenie lub odlaczenie)
                    if (wykryto_nowy_przyrzad == true && Zarzadzanie.kod_podlaczony != kod_podlaczony)
                    {
                        brak_kodu = false;
                        //statusBar.Text = "Podłączono nowy przyrząd. Szukanie w bazie danych...";
                        nazwa = SzukajKod(Zarzadzanie.kod_podlaczony);
                        MessageBox.Show("kixx");
                    }

                    kod_podlaczony = Zarzadzanie.kod_podlaczony;

                    //
                    // Jezeli podlaczono nowy to akcja
                    //
                    if (wykryto_nowy_przyrzad == true && nazwa != "" && Zarzadzanie.kod_podlaczony != 0)
                    {
                        wykryto_nowy_przyrzad = false;
                        Zarzadzanie.kod_aktywny = Zarzadzanie.kod_podlaczony;
                        //statusBar.Text = ("Wykryto przyrząd " + nazwa + " o kodzie: " + Zarzadzanie.kod_podlaczony.ToString() + ". Następuje jego aktywacja...");
                        brak_kodu = false;
                    }

                    if (wykryto_nowy_przyrzad == true && nazwa == "" && Zarzadzanie.kod_podlaczony != 0 && brak_kodu == false)
                    {

                        wykryto_nowy_przyrzad = false;
                        Zarzadzanie.AktywacjaOK = false;
                        Zarzadzanie.AktywnyPrzyrzad = "";
                        Zarzadzanie.kod_aktywny = 0;
                        brak_kodu = true;
                        //statusBar.Text = ("Wykryto nowy przyrząd o kodzie: " + Zarzadzanie.kod_podlaczony.ToString() + ". Nie znaleziono odpowiedniego w bazie danych! Należy utworzyć nowy z kodem nr " + Zarzadzanie.kod_podlaczony.ToString() + ".");
                    }

                    if (wykryto_nowy_przyrzad == true && Zarzadzanie.kod_podlaczony == 0)
                    {
                        wykryto_nowy_przyrzad = false;
                        Zarzadzanie.AktywacjaOK = false;
                        Zarzadzanie.AktywnyPrzyrzad = "";
                        //statusBar.Text = ("Odłączono przyrząd " + kod_poprzedni.ToString() + ". Wybierz odpowiedni z listy.");
                        Zarzadzanie.kod_aktywny = 0;
                        brak_kodu = false;
                    }

                }
            }

        }

        /* 
         * =========================================================================================================================================================
         * Nazwa:           AktywacjaOstatnioUzywanego
         * 
         * Przeznaczenie:   Aktywacja ostatniou używanego przyrzadu. Funkcja wywolywana na początku programu.
         *                  
         * Parametry:       -
         * =========================================================================================================================================================
         */
        void AktywacjaOstatnioUzywanego()
        {
            nazwa_przyrzadu_do_aktywacji = Tools.ReadSetting("Tool");

            if ((nazwa_przyrzadu_do_aktywacji != "") && (aktywacja_start == false))
            {
                if (nazwa_przyrzadu_do_aktywacji != "" && Directory.Exists(Tools.AppDirectory + "\\tools\\" + nazwa_przyrzadu_do_aktywacji + ".0"))
                {
                    aktywacja_start = true;
                }
            }
        }

        /* 
         * =========================================================================================================================================================
         * Nazwa:           buttonToolActivate_Click
         * 
         * Przeznaczenie:   Zdarzenie - Aktywacja wybranego z listy przyrzadu. 
         *                  
         * Parametry:       -
         * =========================================================================================================================================================
         */
        private void buttonToolActivate_Click(object sender, EventArgs e)
        {
            //
            // Jezeli brak miejsca na dysku wyswietl komunikat
            //
            if (brak_miejsca_na_dysku)
                BrakMiejscaInfo();

            if (aktywacja_start == false)
            {
                if (listaPrzyrzadow.SelectedIndex != -1 && Modbus.CommunicationOK && Serial.connected)
                {
                    string nazwa = listaPrzyrzadow.SelectedItem.ToString();

                    Form_AktywacjaPrzyrzadu nowy = new Form_AktywacjaPrzyrzadu(nazwa);

                    nowy.ShowDialog();

                    if (nowy.DialogResult == DialogResult.OK)
                    {
                        nazwa_przyrzadu_do_aktywacji = nazwa;
                        aktywacja_start = true;
                    }

                    //if (MessageBox.Show("Czy chcesz napewo aktywować przyrząd: " + nazwa + " ?", "Aktywacja", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) == DialogResult.Yes)
                    //{
                    //    nazwa_przyrzadu_do_aktywacji = nazwa;
                    //    aktywacja_start = true;
                    //}

                }
            }
            else
            {
                MessageBox.Show("Trwa aktywacja");
            }
        }

        private void buttonToolNewCopy_Click(object sender, EventArgs e)
        {
            string nazwa = "";
            int kod = 0;

            //
            // Jezeli brak miejsca na dysku wyswietl komunikat
            //
            if (brak_miejsca_na_dysku)
                BrakMiejscaInfo();

            Form_PodajNazwePrzyrzadu nowy = new Form_PodajNazwePrzyrzadu();

            nowy.ShowDialog();

            if (nowy.textNazwa.Text != "")
            {
                Cursor.Current = Cursors.WaitCursor;

                nazwa = nowy.textNazwa.Text;
                kod = (int)nowy.numeric_Kod.Value;

                nazwa = nazwa.Replace('.', '_');  // Kropka przed nazwa bedzie oznaczala numer przyrzadu
                nazwa = nazwa.Replace(' ', '_');
                nazwa = nazwa.Replace('\\', '_');
                nazwa = nazwa.Replace('/', '_');
                nazwa = nazwa.Replace(':', '_');
                nazwa = nazwa.Replace('*', '_');
                nazwa = nazwa.Replace('?', '_');
                nazwa = nazwa.Replace('"', '_');
                nazwa = nazwa.Replace('<', '_');
                nazwa = nazwa.Replace('>', '_');
                nazwa = nazwa.Replace('|', '_');

                if (Directory.Exists(Tools.ToolsDirectory + "\\" + nazwa))
                    MessageBox.Show("Przyrząd taki już istnieje! Wybierz inną nazwę lub kod", "Tworzenie nowego przyrządu");
                else
                {
                    Zarzadzanie.NowyPrzyrzadKopia(nazwa, kod);
                    OdswierzListePrzyrzadow();
                }

                //statusBar.Text = "Utworzono nowy przyrząd '" + nazwa + "'";

                Cursor.Current = Cursors.Default;
            }

        }

        /* 
         * =========================================================================================================================================================
         * Nazwa:           UpdateComList 
         * 
         * Przeznaczenie:   Odczytanie z pliku konfiguracyjnego i wpisanieni do zmiennych adresów Modbus
         *                  
         * Parametry:       -
         * =========================================================================================================================================================
         */
        private void UpdateModbusAddresses()
        {
            string temp_slave_id, temp_master_id;

            temp_slave_id = Tools.ReadSetting("ModbusSlaveID");
            temp_master_id = Tools.ReadSetting("ModbusMasterID");

            if (temp_slave_id == "")
            {
                temp_slave_id = "2";
                Tools.WriteSetting("ModbusSlaveID", temp_slave_id);
            }
            if (temp_master_id == "")
            {
                temp_master_id = "1";
                Tools.WriteSetting("ModbusMasterID", temp_master_id);
            }

            Modbus.slave_id = Convert.ToByte(temp_slave_id);
            Modbus.master_id = Convert.ToByte(temp_master_id);

            //numericModbusMasterID.Value = Modbus.master_id;
            numericModbusSlaveID.Value = Modbus.slave_id;
        }

        /* 
         * =========================================================================================================================================================
         * Nazwa:           buttonSendReq_Click 
         * 
         * Przeznaczenie:   Reczne wywolanie z kontrolki button odczytu obrazu IO. Wykorzystywane przy testach.
         *                  
         * Parametry:       -
         * =========================================================================================================================================================
         */
        private void buttonSendReq_Click(object sender, EventArgs e)
        {
            Modbus.ModbusRequest_OdczytObrazuIO();
        }

        /* 
         * =========================================================================================================================================================
         * Nazwa:           UpdateComList 
         * 
         * Przeznaczenie:   Odzczytanie dostepnych na kompie portow COM i utworzenie listy w kontrolce COMBO
         *                  
         * Parametry:       -
         * =========================================================================================================================================================
         */
        private void UpdateComList()
        {
            try
            {
                //Tools.Log("UpdateComList()");
                Serial.CreateAllAvailablePortsList();
                Serial.CreateFreeToOpenPortsList();
                com_list.Items.Clear();
                if (Serial.FreeToOpenPortsList != null)
                    foreach (string str in Serial.FreeToOpenPortsList)
                        com_list.Items.Add(str);

                com_list.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Tools.LogEx(ex, "UpdateComList()");
            }
        }

        /* 
         * =========================================================================================================================================================
         * Nazwa:           Update_Click 
         * 
         * Przeznaczenie:   Wywolanie funkcji odswiezajacej liste com'ów z przycisku
         *                  
         * Parametry:       -
         * =========================================================================================================================================================
         */
        private void Update_Click(object sender, EventArgs e)
        {
            UpdateComList();
        }

        /* 
         * =========================================================================================================================================================
         * Nazwa:           ButtonConnect_Click 
         * 
         * Przeznaczenie:   Połączyć przez COM wybrany na liście
         *                  
         * Parametry:       -
         * =========================================================================================================================================================
         */
        private void ButtonConnect_Click(object sender, EventArgs e)
        {
            if (Serial.Open(com_list.SelectedItem.ToString()))
            {
                Tools.WriteSetting("COM", Serial.serial_port.PortName);
            }

            UpdateComList();
        }

        /* 
         * =========================================================================================================================================================
         * Nazwa:           ButtonDisconnect_Click 
         * 
         * Przeznaczenie:   Rozłączyć COMa aktualnie otwartego przez naszą aplikację
         *                  
         * Parametry:       -
         * =========================================================================================================================================================
         */
        private void ButtonDisconnect_Click(object sender, EventArgs e)
        {
            Serial.Close();
            UpdateComList();
        }

        /* 
         * =========================================================================================================================================================
         * Nazwa:           checkBox1_CheckStateChanged 
         * 
         * Przeznaczenie:   Zdarzenie od zaznaczenia/odznaczenia na kontrolce o automatycznym połączeniu po włączeniu aplikacji. Zapis w pliku ustawien.
         *                  
         * Parametry:       -
         * =========================================================================================================================================================
         */
        private void checkBox1_CheckStateChanged(object sender, EventArgs e)
        {
            Tools.WriteSetting("Autoconnect", Autoconnect.Checked.ToString());
        }

        /* 
         * =========================================================================================================================================================
         * Nazwa:           numericModbusMasterID_ValueChanged 
         * 
         * Przeznaczenie:   Zdarzenie od zmiany adresu mastera MOdbus na kontrolce. Zapis w pliku ustawien.
         *                  
         * Parametry:       -
         * =========================================================================================================================================================
         */
        private void numericModbusMasterID_ValueChanged(object sender, EventArgs e)
        {
            //Tools.WriteSetting("ModbusMasterID", numericModbusMasterID.Value.ToString());
            UpdateModbusAddresses();
        }

        /* 
         * =========================================================================================================================================================
         * Nazwa:           numericModbusSlaveID_ValueChanged 
         * 
         * Przeznaczenie:   Zdarzenie od zmiany adresu slave'a MOdbus na kontrolce. Zapis w pliku ustawien.
         *                  
         * Parametry:       -
         * =========================================================================================================================================================
         */
        private void numericModbusSlaveID_ValueChanged(object sender, EventArgs e)
        {
            Tools.WriteSetting("ModbusSlaveID", numericModbusSlaveID.Value.ToString());
            UpdateModbusAddresses();
        }

        /* 
         * =========================================================================================================================================================
         * Nazwa:           tabControlMain_SelectedIndexChanged
         * 
         * Przeznaczenie:   Zdarzenie wywolywane przyzmianie tab'a
         *                  
         * Parametry:       -
         * =========================================================================================================================================================
         */
        private void tabControlMain_SelectedIndexChanged(object sender, EventArgs e)
        {
        }


        /* 
         * =========================================================================================================================================================
         * Nazwa:           WyswietlParametrMaszyny
         * 
         * Przeznaczenie:   Wyswietlanie parametru w kontrolce TEXT do edycji parametru
         *                  
         * Parametry:       -
         * =========================================================================================================================================================
         */
        private void WyswietlParametrMaszyny()
        {
        }

        /* 
         * =========================================================================================================================================================
         * Nazwa:           AktywacjaNowoPodlaczonego
         * 
         * Przeznaczenie:   Aktywacja nowo podłączonego przyrządu
         *                  
         * Parametry:       -
         * =========================================================================================================================================================
         */
        void AktywacjaNowoPodlaczonego()
        {

        }

        /* 
         * =========================================================================================================================================================
         * Nazwa:           buttonDelete_Click
         * 
         * Przeznaczenie:   Zdarzenie - Usuniecie wybranego z listy przyrzadu. 
         *                  
         * Parametry:       -
         * =========================================================================================================================================================
         */
        private void buttonDelete_Click(object sender, EventArgs e)
        {
            if (listaPrzyrzadow.SelectedIndex != -1)
            {
                string nazwa = listaPrzyrzadow.SelectedItem.ToString();

                if (MessageBox.Show("Czy chcesz napewo usunąć przyrząd: " + nazwa + " ?", "Usuwanie", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) == DialogResult.Yes)
                {
                    //statusBar.Text = "Usunięto przyrząd z listy: " + nazwa;
                    Zarzadzanie.Usun(nazwa);
                    OdswierzListePrzyrzadow();
                }
            }
        }

        /* 
         * =========================================================================================================================================================
         * Nazwa:           ZmianaParametru
         * 
         * Przeznaczenie:   Funkcja, która wywolywana jest w innym miejscu. Zmiana aktualnie edytowanego parametru
         *                  
         * Parametry:       Sposob zmiany parametru - może być wcisniety enter, może tylko jeden z plusów
         * =========================================================================================================================================================
         */

        ushort ChangeParam(ushort input, short change_val, ushort max_val)
        {
            int _input, _change_val, _max_val, ret_val;

            _input = (int)input;
            _change_val = (int)change_val;
            _max_val = (int)max_val;
            ret_val = 0;

            if (_input + _change_val > _max_val)
                ret_val = max_val;
            else if (_input + _change_val < 0)
                ret_val = 0;
            else
                ret_val = (_input + _change_val);

            return (ushort)ret_val;
        }

        /*
        byte ChangeParam(ushort input, short change_val, byte max_val)
        {
            byte retval = 0;
            if ((short)input + change_val > (short)max_val)
                retval = max_val;
            else if ((short)input + change_val < 0)
                retval = 0;
            else
                retval = (byte)(input + change_val);
            return retval;
        }
        */

        private void ZmianaParametruZgrzewania(short val)
        {
            if (Zarzadzanie.AktywnyPrzyrzad != "")
            {
                ushort parametr = 0, parametr_new = 0;

                while (zmiana_parametru_start)
                {
                    Thread.Sleep(1);
                }

                try
                {
                    if (ActiveParamEdit == TxtParametryNazwa[PARAM_NAME_WeldCurrent, jezyk])
                    {
                        parametr = (ushort)Zarzadzanie.par.prog[ActiveProgEdit].WeldCurrent;
                        parametr_new = ChangeParam(parametr, val, 999);
                        Zarzadzanie.par.prog[ActiveProgEdit].WeldCurrent = parametr_new;
                    }
                    else if (ActiveParamEdit == TxtParametryNazwa[PARAM_NAME_WeldTime, jezyk])
                    {
                        parametr = (ushort)Zarzadzanie.par.prog[ActiveProgEdit].WeldTime;
                        parametr_new = Zarzadzanie.par.prog[ActiveProgEdit].WeldTime = (byte)ChangeParam(parametr, val, 99);
                    }

                    else if (ActiveParamEdit == TxtParametryNazwa[PARAM_NAME_Impulses, jezyk])
                    {
                        parametr = (ushort)Zarzadzanie.par.prog[ActiveProgEdit].Impulses;
                        parametr_new = Zarzadzanie.par.prog[ActiveProgEdit].Impulses = (byte)ChangeParam(parametr, val, 10);
                    }
                    else if (ActiveParamEdit == TxtParametryNazwa[PARAM_NAME_ImpulsesPause, jezyk])
                    {
                        parametr = (ushort)Zarzadzanie.par.prog[ActiveProgEdit].ImpulsesPause;
                        parametr_new = Zarzadzanie.par.prog[ActiveProgEdit].ImpulsesPause = (byte)ChangeParam(parametr, val, 99);
                    }
                    else if (ActiveParamEdit == TxtParametryNazwa[PARAM_NAME_PrePressTime, jezyk])
                    {
                        parametr = (ushort)Zarzadzanie.par.prog[ActiveProgEdit].PrePressTime;
                        parametr_new = Zarzadzanie.par.prog[ActiveProgEdit].PrePressTime = (byte)ChangeParam(parametr, val, 99);
                    }

                    else if (ActiveParamEdit == TxtParametryNazwa[PARAM_NAME_PreWeldCurrent, jezyk])
                    {
                        parametr = (ushort)Zarzadzanie.par.prog[ActiveProgEdit].PreWeldCurrent;
                        parametr_new = ChangeParam(parametr, val, 999);
                        Zarzadzanie.par.prog[ActiveProgEdit].PreWeldCurrent = parametr_new;
                    }

                    else if (ActiveParamEdit == TxtParametryNazwa[PARAM_NAME_PreWeldTime, jezyk])
                    {
                        parametr = (ushort)Zarzadzanie.par.prog[ActiveProgEdit].PreWeldTime;
                        parametr_new = Zarzadzanie.par.prog[ActiveProgEdit].PreWeldTime = (byte)ChangeParam(parametr, val, 99);
                    }
                    else if (ActiveParamEdit == TxtParametryNazwa[PARAM_NAME_PreWeldPause, jezyk])
                    {
                        parametr = (ushort)Zarzadzanie.par.prog[ActiveProgEdit].PreWeldPause;
                        parametr_new = Zarzadzanie.par.prog[ActiveProgEdit].PreWeldPause = (byte)ChangeParam(parametr, val, 99);
                    }
                    else if (ActiveParamEdit == TxtParametryNazwa[PARAM_NAME_PostWeldPause, jezyk])
                    {
                        parametr = (ushort)Zarzadzanie.par.prog[ActiveProgEdit].PostWeldPause;
                        parametr_new = Zarzadzanie.par.prog[ActiveProgEdit].PostWeldPause = (byte)ChangeParam(parametr, val, 99);
                    }
                    else if (ActiveParamEdit == TxtParametryNazwa[PARAM_NAME_PostWeldCurrent, jezyk])
                    {
                        parametr = (ushort)Zarzadzanie.par.prog[ActiveProgEdit].PostWeldCurrent;
                        parametr_new = Zarzadzanie.par.prog[ActiveProgEdit].PostWeldCurrent = ChangeParam(parametr, val, 999);
                    }
                    else if (ActiveParamEdit == TxtParametryNazwa[PARAM_NAME_PostWeldTime, jezyk])
                    {
                        parametr = (ushort)Zarzadzanie.par.prog[ActiveProgEdit].PostWeldTime;
                        parametr_new = Zarzadzanie.par.prog[ActiveProgEdit].PostWeldTime = (byte)ChangeParam(parametr, val, 99);
                    }
                    else if (ActiveParamEdit == TxtParametryNazwa[PARAM_NAME_PostPressTime, jezyk])
                    {
                        parametr = (ushort)Zarzadzanie.par.prog[ActiveProgEdit].PostPressTime;
                        parametr_new = Zarzadzanie.par.prog[ActiveProgEdit].PostPressTime = (byte)ChangeParam(parametr, val, 99);
                    }
                    else if (ActiveParamEdit == TxtParametryNazwa[PARAM_NAME_StepperPercent, jezyk])
                    {
                        parametr = (ushort)Zarzadzanie.par.prog[ActiveProgEdit].StepperPercent;
                        parametr_new = Zarzadzanie.par.prog[ActiveProgEdit].StepperPercent = (byte)ChangeParam(parametr, val, 200);
                    }
                    else if (ActiveParamEdit == TxtParametryNazwa[PARAM_NAME_StepperCounter, jezyk])
                    {
                        parametr = (ushort)Zarzadzanie.par.prog[ActiveProgEdit].StepperCounter;
                        parametr_new = Zarzadzanie.par.prog[ActiveProgEdit].StepperCounter = ChangeParam(parametr, val, 65535);
                    }

                    else if (ActiveParamEdit == TxtParametryNazwa[PARAM_NAME_PressureSet, jezyk])
                    {
                        parametr = (ushort)Zarzadzanie.par.prog[ActiveProgEdit].PressureSet;
                        parametr_new = Zarzadzanie.par.prog[ActiveProgEdit].PressureSet = ChangeParam(parametr, val, 10000);
                    }
                    else if (ActiveParamEdit == TxtParametryNazwa[PARAM_NAME_PressureSwitch, jezyk])
                    {
                        parametr = (ushort)Zarzadzanie.par.prog[ActiveProgEdit].PressureSwitch;
                        parametr_new = Zarzadzanie.par.prog[ActiveProgEdit].PressureSwitch = ChangeParam(parametr, val, 10000);
                    }

                    else if (ActiveParamEdit == TxtParametryNazwa[PARAM_NAME_CylinderPositionDown, jezyk])
                    {
                        parametr = (ushort)Zarzadzanie.par.prog[ActiveProgEdit].CylinderPositionDown;
                        parametr_new = Zarzadzanie.par.prog[ActiveProgEdit].CylinderPositionDown = ChangeParam(parametr, val, 10000);
                    }
                    else if (ActiveParamEdit == TxtParametryNazwa[PARAM_NAME_CylinderPositionDownTolerance, jezyk])
                    {
                        parametr = (ushort)Zarzadzanie.par.prog[ActiveProgEdit].CylinderPositionDownTolerance;
                        parametr_new = Zarzadzanie.par.prog[ActiveProgEdit].CylinderPositionDownTolerance = ChangeParam(parametr, val, 10000);
                    }
                    else if (ActiveParamEdit == TxtParametryNazwa[PARAM_NAME_ExhaustPressure, jezyk])
                    {
                        parametr = (ushort)Zarzadzanie.par.prog[ActiveProgEdit].ExhaustPressure;
                        parametr_new = Zarzadzanie.par.prog[ActiveProgEdit].ExhaustPressure = ChangeParam(parametr, val, 10000);
                    }
                    else if (ActiveParamEdit == TxtParametryNazwa[PARAM_NAME_ExhaustPressureTolerance, jezyk])
                    {
                        parametr = (ushort)Zarzadzanie.par.prog[ActiveProgEdit].ExhaustPressureTolerance;
                        parametr_new = Zarzadzanie.par.prog[ActiveProgEdit].ExhaustPressureTolerance = ChangeParam(parametr, val, 10000);
                    }

                    else if (ActiveParamEdit == TxtParametryNazwa[PARAM_NAME_IrefTolerance, jezyk])
                    {
                        parametr = (ushort)Zarzadzanie.par.prog[ActiveProgEdit].IrefTolerance;
                        parametr_new = Zarzadzanie.par.prog[ActiveProgEdit].IrefTolerance = (byte)ChangeParam(parametr, val, 50);
                    }
                    else if (ActiveParamEdit == TxtParametryNazwa[PARAM_NAME_UrefTolerance, jezyk])
                    {
                        parametr = (ushort)Zarzadzanie.par.prog[ActiveProgEdit].UrefTolerance;
                        parametr_new = Zarzadzanie.par.prog[ActiveProgEdit].UrefTolerance = (byte)ChangeParam(parametr, val, 50);
                    }
                    else if (ActiveParamEdit == TxtParametryNazwa[PARAM_NAME_ErefTolerance, jezyk])
                    {
                        parametr = (ushort)Zarzadzanie.par.prog[ActiveProgEdit].ErefTolerance;
                        parametr_new = Zarzadzanie.par.prog[ActiveProgEdit].ErefTolerance = (byte)ChangeParam(parametr, val, 50);
                    }

                    else if (ActiveParamEdit == TxtParametryNazwa[PARAM_NAME_Injection, jezyk])
                    {
                        parametr = (ushort)Zarzadzanie.par.prog[ActiveProgEdit].Injection;
                        parametr_new = Zarzadzanie.par.prog[ActiveProgEdit].Injection = ChangeParam(parametr, val, 10000);
                    }

                    else if (ActiveParamEdit == TxtParametryNazwa[PARAM_NAME_Iref, jezyk])
                    {
                        parametr = (ushort)Zarzadzanie.par.prog[ActiveProgEdit].Iref;
                        parametr_new = Zarzadzanie.par.prog[ActiveProgEdit].Iref = ChangeParam(parametr, val, 10000);
                    }
                    else if (ActiveParamEdit == TxtParametryNazwa[PARAM_NAME_Uref, jezyk])
                    {
                        parametr = (ushort)Zarzadzanie.par.prog[ActiveProgEdit].Uref;
                        parametr_new = Zarzadzanie.par.prog[ActiveProgEdit].Uref = ChangeParam(parametr, val, 10000);
                    }
                    else if (ActiveParamEdit == TxtParametryNazwa[PARAM_NAME_Eref, jezyk])
                    {
                        parametr = (ushort)Zarzadzanie.par.prog[ActiveProgEdit].Eref;
                        parametr_new = Zarzadzanie.par.prog[ActiveProgEdit].Eref = ChangeParam(parametr, val, 50000);
                    }
                    
                }
                catch (Exception ex)
                {
                    Tools.LogEx(ex, "Transfer_ParametrDoObrazu()");
                }

                while (zmiana_parametru_start)
                {
                    Thread.Sleep(1);
                }

                zmiana_parametru_start = true;

                while (zmiana_parametru_start)
                {
                    Thread.Sleep(1);
                }

                Zarzadzanie.Transfer_ProgramDoPliku(Zarzadzanie.AktywnyPrzyrzad, Zarzadzanie.kod_aktywny, ActiveProgEdit);

                Tools.LogParam("Zmiana parametru: Program " + ActiveProgEdit.ToString() + ", " +
                                ActiveParamEdit + ", Stara wartosc " + parametr.ToString() + ", Nowa wartosc " + parametr_new.ToString());

                serializuj = true;
                //statusBar.Text = "Zmiana parametru " + ActiveParamEdit + " w programie nr " + ActiveProgEdit;
            }
            else
            {
                MessageBox.Show("Żaden przyrząd nie jest aktywny !", "Błąd zmiany parametru");
            }
        }

        /* 
         * =========================================================================================================================================================
         * Nazwa:           WyswietlParametr
         * 
         * Przeznaczenie:   Wyswietlanie parametru w kontrolce TEXT do edycji parametru
         *                  
         * Parametry:       -
         * =========================================================================================================================================================
         */
        private void WyswietlParametrZgrzewania()
        {
            try
            {
                if (ActiveParamEdit != "")
                {
                    //textParametr.Text = Zarzadzanie.Transfer_ParametrZObrazu(ActiveProgEdit, ActiveParamEdit).PadLeft(textParametr.MaxLength, '0');

                    if (ActiveParamEdit == TxtParametryNazwa[PARAM_NAME_PrePressTime, jezyk])
                    {
                        textParametr.Text = Zarzadzanie.par.prog[ActiveProgEdit].PrePressTime.ToString().PadLeft(5, '0');
                        labelJedn.Text = "per";
                        labelOpis.Text = "Czas Docisku Wstępnego (VHZ) \n\nCzas przytrzymania cylindra w dolnym położeniu przed rozpoczęciem zgrzewania. Wartość z przedziału 0 - 99. Jednostka - periody (20ms). \nOpóźnienie czasowe pomiędzy osiągnięciem dolnej pozycji cylindra a rozpoczęciem zgrzewania.";
                    }
                    else if (ActiveParamEdit == TxtParametryNazwa[PARAM_NAME_WeldCurrent, jezyk])
                    {
                        textParametr.Text = Zarzadzanie.par.prog[ActiveProgEdit].WeldCurrent.ToString().PadLeft(5, '0');
                        labelJedn.Text = "jedn.";
                        labelOpis.Text = "Prąd Zgrzewania (Strom) \n\nZasadniczy prąd zgrzewania. \nWartość z przedziału 0 - 999. Jednostka - promile całkowitej mocy zgrzewarki. \nPrąd wynikowy uzależniony jest od mocy tansformatora.";
                    }
                    else if (ActiveParamEdit == TxtParametryNazwa[PARAM_NAME_WeldTime, jezyk])
                    {
                        textParametr.Text = Zarzadzanie.par.prog[ActiveProgEdit].WeldTime.ToString().PadLeft(5, '0');
                        labelJedn.Text = "per";
                        labelOpis.Text = "Czas Zgrzewania (SZ) \n\nZasadniczy czas zgrzewania. \nWartość z przedział 0 - 99. Jednostka - periody (20ms). \nMaksymalny czas 2 sekundy.";
                    }
                    else if (ActiveParamEdit == TxtParametryNazwa[PARAM_NAME_PostPressTime, jezyk])
                    {
                        textParametr.Text = Zarzadzanie.par.prog[ActiveProgEdit].PostPressTime.ToString().PadLeft(5, '0');
                        labelJedn.Text = "per";
                        labelOpis.Text = "Czas Docisku Końcowego (NHZ) \n\nCzas przytrzymania cylindra w dolnym położeniu po zakończeniu zgrzewania. \nWartość z przedziału 0 - 99. Jednostka - periody (20ms). \nOpóźnienie czasowe pomiędzy końcem zgrzewania a ruchem powrotnym cylindra.";
                    }
                    else if (ActiveParamEdit == TxtParametryNazwa[PARAM_NAME_Impulses, jezyk])
                    {
                        textParametr.Text = Zarzadzanie.par.prog[ActiveProgEdit].Impulses.ToString().PadLeft(5, '0');
                        labelJedn.Text = "";
                        labelOpis.Text = "Impulsy Ilość (Impulse) \n\nIlość impulsów zgrzewania. \nWartość z przedziału 0 - 10. Określa liczbę powtórzeń całego cyklu (kilka zgrzewów w tym samym miejscu). \nWykorzystywane w zgrzewaniu punktowym.";
                    }
                    else if (ActiveParamEdit == TxtParametryNazwa[PARAM_NAME_ImpulsesPause, jezyk])
                    {
                        textParametr.Text = Zarzadzanie.par.prog[ActiveProgEdit].ImpulsesPause.ToString().PadLeft(5, '0');
                        labelJedn.Text = "per";
                        labelOpis.Text = "Impulsy Pauza (IZ) \n\nOdstęp czasu pomiędzy impulsami. \nWartość z przedziału 0 - 99. Jednostka - periody (20ms). \nOkreśla odstęp czasu pomiędzy kolejnymi impulsami.";
                    }
                    else if (ActiveParamEdit == TxtParametryNazwa[PARAM_NAME_PreWeldCurrent, jezyk])
                    {
                        textParametr.Text = Zarzadzanie.par.prog[ActiveProgEdit].PreWeldCurrent.ToString().PadLeft(5, '0');
                        labelJedn.Text = "%";
                        labelOpis.Text = "Prad Podgrzewania Wstępnego \n\nWartość prądu podgrzewania. \nWartość z przedziału 0 - 999. Jednostka - promile całkowitej mocy zgrzewarki. \nPrąd wynikowy uzależniony jest od mocy tansformatora.";
                    }
                    else if (ActiveParamEdit == TxtParametryNazwa[PARAM_NAME_PreWeldTime, jezyk])
                    {
                        textParametr.Text = Zarzadzanie.par.prog[ActiveProgEdit].PreWeldTime.ToString().PadLeft(5, '0');
                        labelJedn.Text = "per";
                        labelOpis.Text = "Czas Podgrzewania Wstępnego \n\nCzas trwania prądu podgrzewającego. \nWartość z przedział 0 - 99. Jednostka - periody (20ms). Maksymalny czas 2 sekundy.";
                    }
                    else if (ActiveParamEdit == TxtParametryNazwa[PARAM_NAME_PreWeldPause, jezyk])
                    {
                        textParametr.Text = Zarzadzanie.par.prog[ActiveProgEdit].PreWeldPause.ToString().PadLeft(5, '0');
                        labelJedn.Text = "per";
                        labelOpis.Text = "Pauza Podgrzewania Wstępnego \n\nOdstęp czasu pomiędzy impulsem podgrzewania wstępnego a impulsami zgrzewania zasadniczego. \nWartość z przedział 0 - 99. Jednostka - periody (20ms). \nMaksymalny czas 2 sekundy.";
                    }
                    else if (ActiveParamEdit == TxtParametryNazwa[PARAM_NAME_PostWeldPause, jezyk])
                    {
                        textParametr.Text = Zarzadzanie.par.prog[ActiveProgEdit].PostWeldPause.ToString().PadLeft(5, '0');
                        labelJedn.Text = "per";
                        labelOpis.Text = "Pauza Podgrzewania Końcowego \n\nOdstęp czasu pomiędzy impulsem zgrzewającym a dodatkowym impulsem dogrzewającym. \nWartość z przedział 0 - 99. Jednostka - periody (20ms). Maksymalny czas 2 sekundy.";
                    }
                    else if (ActiveParamEdit == TxtParametryNazwa[PARAM_NAME_PostWeldCurrent, jezyk])
                    {
                        textParametr.Text = Zarzadzanie.par.prog[ActiveProgEdit].PostWeldCurrent.ToString().PadLeft(5, '0');
                        labelJedn.Text = "%";
                        labelOpis.Text = "Prąd Podgrzewania Końcowego \n\nWartość prądu podgrzewania końcowego. \nWartość z przedziału 0 - 999. Jednostka - promile całkowitej mocy zgrzewarki. \nPrąd wynikowy uzależniony jest od mocy tansformatora.";
                    }
                    else if (ActiveParamEdit == TxtParametryNazwa[PARAM_NAME_PostWeldTime, jezyk])
                    {
                        textParametr.Text = Zarzadzanie.par.prog[ActiveProgEdit].PostWeldTime.ToString().PadLeft(5, '0');
                        labelJedn.Text = "per";
                        labelOpis.Text = "Czas Podgrzewania Końcowego \n\nCzas trwania prądu podgrzewania końcowego. \nWartość z przedziału 0 - 99. Jednostka - periody (20ms). \nMaksymalny czas 2 sekundy.";
                    }
                    else if (ActiveParamEdit == TxtParametryNazwa[PARAM_NAME_StepperPercent, jezyk])
                    {
                        textParametr.Text = Zarzadzanie.par.prog[ActiveProgEdit].StepperPercent.ToString().PadLeft(5, '0');
                        labelJedn.Text = "%";
                        labelOpis.Text = "Stepper Procent \n\nProcent prądu dodany do wartości prądu zasadniczego przy maksymalnej wartości licznika steppera. \nWartość z przedziału 0 - 99. Jednostka - procenty.";
                    }
                    else if (ActiveParamEdit == TxtParametryNazwa[PARAM_NAME_StepperCounter, jezyk])
                    {
                        textParametr.Text = Zarzadzanie.par.prog[ActiveProgEdit].StepperCounter.ToString().PadLeft(5, '0');
                        labelJedn.Text = "zgrz";
                        labelOpis.Text = "Stepper Licznik \n\nLicznik steppera. \nWartość z przedziału 0 - 65535. Jednostka - liczba zgrzewów. \nMaksymalna wartość tego licznika określa również konieczność regeneracji elektrod.";
                    }

                    else if (ActiveParamEdit == TxtParametryNazwa[PARAM_NAME_PressureSet, jezyk])
                    {
                        textParametr.Text = Zarzadzanie.par.prog[ActiveProgEdit].PressureSet.ToString().PadLeft(5, '0');
                        labelJedn.Text = "mbar";
                        labelOpis.Text = "Ciśnienie zadane \n\nUstawione ciśnienie na zawór proporcjonalny. \nWartość w (miliwoltach) z przedziału 0 - 10000 (10000 = 10 V). Często zawory proporcjonalne do regulacji ciśnienia posiadają bezpośredni przelicznik V -> bar, więc 10V odpowiada 10 bar.";
                    }
                    else if (ActiveParamEdit == TxtParametryNazwa[PARAM_NAME_PressureSwitch, jezyk])
                    {
                        textParametr.Text = Zarzadzanie.par.prog[ActiveProgEdit].PressureSwitch.ToString().PadLeft(5, '0');
                        labelJedn.Text = "mbar";
                        labelOpis.Text = "Kontrola ciśnienia \n\nWartość przy której sygnalizowane jest osiągnięcie ciśnienia. \nWartość w (miliwoltach) milibarach z przedziału 0 - 10000 (10000 = 10 V = 10 bar). Często zawory proporcjonalne do regulacji ciśnienia posiadają bezpośredni przelicznik V -> bar, więc np. 10V odpowiada 10 bar.";
                    }
                    else if (ActiveParamEdit == TxtParametryNazwa[PARAM_NAME_CylinderPositionDown, jezyk])
                    {
                        textParametr.Text = Zarzadzanie.par.prog[ActiveProgEdit].CylinderPositionDown.ToString().PadLeft(5, '0');
                        labelJedn.Text = "mV";
                        labelOpis.Text = "Pozycja Cylindra Dół \n\nKontrola pozycji cylindra w dolnym położeniu. \nWartość w (miliwoltach) z przedziału 0 - 10000 (10000 = 10 V). Często przetworniki pozycji posiadają bezpośredni przelicznik V -> mm, więc np. 10V odpowiada 100 mm.";
                    }
                    else if (ActiveParamEdit == TxtParametryNazwa[PARAM_NAME_CylinderPositionDownTolerance, jezyk])
                    {
                        textParametr.Text = Zarzadzanie.par.prog[ActiveProgEdit].CylinderPositionDownTolerance.ToString().PadLeft(5, '0');
                        labelJedn.Text = "mV";
                        labelOpis.Text = "Pozycja Cylindra Dół Tolerancja \n\nToleracja pozycji cylindra w dolnym położeniu.\nWartość w (miliwoltach) z przedziału 0 - 10000 (10000 = 10 V). Często przetworniki pozycji posiadają bezpośredni przelicznik V -> mm, więc np. 10V odpowiada 100 mm.";
                    }
                    else if (ActiveParamEdit == TxtParametryNazwa[PARAM_NAME_ExhaustPressure, jezyk])
                    {
                        textParametr.Text = Zarzadzanie.par.prog[ActiveProgEdit].ExhaustPressure.ToString().PadLeft(5, '0');
                        labelJedn.Text = "mV";
                        labelOpis.Text = "Kontrola Przez Wydmuch \n\nKontrola położenia nakrętki przez pomiar ciśnienia wydmuchu na elektrodzie. \nWartość w (miliwoltach) milibarach z przedziału 0 - 10000 (10000 = 10 V = 10 bar). Często zawory proporcjonalne do regulacji ciśnienia posiadają bezpośredni przelicznik V -> bar, więc np. 10V odpowiada 10 bar.";
                    }
                    else if (ActiveParamEdit == TxtParametryNazwa[PARAM_NAME_ExhaustPressureTolerance, jezyk])
                    {
                        textParametr.Text = Zarzadzanie.par.prog[ActiveProgEdit].ExhaustPressureTolerance.ToString().PadLeft(5, '0');
                        labelJedn.Text = "mV";
                        labelOpis.Text = "Kontrola Przez Wydmuch Tolerancja \n\nTolerancja przy pomiarze ciśnienia wydmuchu - Kontrola położenia nakrętki przez pomiar ciśnienia wydmuchu na elektrodzie. \nWartość w (miliwoltach) milibarach z przedziału 0 - 10000 (10000 = 10 V = 10 bar). Często zawory proporcjonalne do regulacji ciśnienia posiadają bezpośredni przelicznik V -> bar, więc np. 10V odpowiada 10 bar.";
                    }
                    else if (ActiveParamEdit == TxtParametryNazwa[PARAM_NAME_IrefTolerance, jezyk])
                    {
                        textParametr.Text = Zarzadzanie.par.prog[ActiveProgEdit].IrefTolerance.ToString().PadLeft(5, '0');
                        labelJedn.Text = "%";
                        labelOpis.Text = "I ref Tolerancja (Tolerancja kontroli) \n\nTolerancja prądu zgrzewania. \nWartość z przedziału 0 - 99.  Jednostka - procent.";
                    }
                    else if (ActiveParamEdit == TxtParametryNazwa[PARAM_NAME_UrefTolerance, jezyk])
                    {
                        textParametr.Text = Zarzadzanie.par.prog[ActiveProgEdit].UrefTolerance.ToString().PadLeft(5, '0');
                        labelJedn.Text = "%";
                        labelOpis.Text = "UrefTolerancja - tolerancja napięcia zgrzewania. \nWartość z przedziału 0 - 99.  Jednostka - procent.";
                    }
                    else if (ActiveParamEdit == TxtParametryNazwa[PARAM_NAME_ErefTolerance, jezyk])
                    {
                        textParametr.Text = Zarzadzanie.par.prog[ActiveProgEdit].ErefTolerance.ToString().PadLeft(5, '0');
                        labelJedn.Text = "%";
                        labelOpis.Text = "ErefTolerancja - tolerancja energii zgrzewania. Wartość z przedziału 0 - 99.  Jednostka - procent.";
                    }
                    else if (ActiveParamEdit == TxtParametryNazwa[PARAM_NAME_Injection, jezyk])
                    {
                        textParametr.Text = Zarzadzanie.par.prog[ActiveProgEdit].Injection.ToString().PadLeft(5, '0');
                        labelJedn.Text = "mV";
                        labelOpis.Text = "Wtopienie - minimalne wtopienie jakie musi zajść podczas zgrzewania. \nWartość w (miliwoltach) z przedziału 0 - 10000 (10000 = 10 V).";
                    }
                    else if (ActiveParamEdit == TxtParametryNazwa[PARAM_NAME_Iref, jezyk])
                    {
                        textParametr.Text = Zarzadzanie.par.prog[ActiveProgEdit].Iref.ToString().PadLeft(5, '0');
                        labelJedn.Text = "mV";
                        labelOpis.Text = "I ref - prąd referencyjny \n\nWartość z przedziału 0 - 10000.  Jednostka - miliwolt (wartosc z wejscia analogowego).";
                    }
                    else if (ActiveParamEdit == TxtParametryNazwa[PARAM_NAME_Uref, jezyk])
                    {
                        textParametr.Text = Zarzadzanie.par.prog[ActiveProgEdit].Uref.ToString().PadLeft(5, '0');
                        labelJedn.Text = "mV";
                        labelOpis.Text = "U ref - napięcie referencyjne \n\nWartość z przedziału 0 - 10000.  Jednostka - miliwolt (wartosc z wejscia analogowego).";
                    }
                    else if (ActiveParamEdit == TxtParametryNazwa[PARAM_NAME_Eref, jezyk])
                    {
                        textParametr.Text = Zarzadzanie.par.prog[ActiveProgEdit].Eref.ToString().PadLeft(5, '0');
                        labelJedn.Text = "mV";
                        labelOpis.Text = "E ref - energia referencyjna \n\nWartość z przedziału 0 - 10000.  Jednostka - miliwolt (wartosc z wejscia analogowego).";
                    }
                }
            }
            catch (Exception ex)
            {
                Tools.LogEx(ex, "WyswietlParametrZgrzewania()");
            }
        }

        /* 
         * =========================================================================================================================================================
         * Nazwa:           WyswietlParametrPrzyrzadu
         * 
         * Przeznaczenie:   Wyswietlanie parametru w kontrolce TEXT do edycji parametru
         *                  
         * Parametry:       -
         * =========================================================================================================================================================
         */
        private void WyswietlParametrPrzyrzadu()
        {
            //string val_string = "";

            try
            {
                /*
                    Licznik
                    Licznik Max
                    Licznik Stepper Akt
                    Licznik Stepper Ostrzeżenie
                    Licznik Stepper Max
                    Sluza
                    Liczba Programow W Cyklu
                    RES 01
                    RES 02
                 */

                textParametrPrzyrzad.Text = Modbus.io.sm[comboParametrPrzyrzad.SelectedIndex + Modbus.PRZESUNIECIE_SM].ToString().PadLeft(textParametr.MaxLength, '0');

                if (ActiveToolParamEdit != "")
                {
                    if (ActiveToolParamEdit == TxtToolParametryNazwa[TOOLPARAM_NAME_Licznik, jezyk])
                    {
                        labelJednPrzyrzad.Text = "szt";
                        labelOpisPrzyrzad.Text = "Licznik użytkownika. Wyświetlany normalnie w oknie Produkcja.";
                    }
                    else if (ActiveToolParamEdit == TxtToolParametryNazwa[TOOLPARAM_NAME_LicznikMax, jezyk])
                    {
                        labelJednPrzyrzad.Text = "szt";
                        labelOpisPrzyrzad.Text = "Maksymalna wartość licznika. Wyświetlany normalnie w oknie Produkcja.";
                    }
                    else if (ActiveToolParamEdit == TxtToolParametryNazwa[TOOLPARAM_NAME_StepperLicznik, jezyk])
                    {
                        labelJednPrzyrzad.Text = "zgrz";
                        labelOpisPrzyrzad.Text = "Aktualna wartość licznika steppera - liczba zgrzewów wykonana na danym przyrządzie.";
                    }
                    else if (ActiveToolParamEdit == TxtToolParametryNazwa[TOOLPARAM_NAME_StepperOstrzezenie, jezyk])
                    {
                        labelJednPrzyrzad.Text = "zgrz";
                        labelOpisPrzyrzad.Text = "Wartość licznika, przy której zapali się ostrzeżenie wymiany elektrody (nie zatrzyma maszyny).";
                    }
                    else if (ActiveToolParamEdit == TxtToolParametryNazwa[TOOLPARAM_NAME_StepperMax, jezyk])
                    {
                        labelJednPrzyrzad.Text = "zgrz";
                        labelOpisPrzyrzad.Text = "Wartość licznika, przy której zapali się lampka wymiany elektrody (atrzymanie maszyny).";
                    }
                    else if (ActiveToolParamEdit == TxtToolParametryNazwa[TOOLPARAM_NAME_Sluza, jezyk])
                    {
                        labelJednPrzyrzad.Text = "ms";
                        labelOpisPrzyrzad.Text = "Konfiguracja śluzy. Typ śluzy definiowany jest poprzez czas. Czas równy zero mówi - brak śluzy. Czas z przedziału 1...999 ms mówi - śluza optyczna. Czas z przedziału 1000...65535 ms mówi - śluza KLT. Czas ten będzie czasem otwarcia śluzy i sygnałem liczącym na Hydrę.";
                    }
                    else if (ActiveToolParamEdit == TxtToolParametryNazwa[TOOLPARAM_NAME_LiczbaProgramow, jezyk])
                    {
                        labelJednPrzyrzad.Text = "";
                        labelOpisPrzyrzad.Text = "Całkowita liczba programów wykorzystanych w tym przyrządzie.";
                    }
                }

            }
            catch (Exception ex)
            {
                Tools.LogEx(ex, "WyswietlParametrPrzyrzadu()");
            }

            //textParametrPrzyrzad.Text = val_string.PadLeft(textParametrPrzyrzad.MaxLength, '0');
        }

        /* 
         * =========================================================================================================================================================
         * Nazwa:           MainForm_Closing
         * 
         * Przeznaczenie:   Zamknięcie aplikacji przez krzyżyk. Zezwolenie jeżeli jest zezwolenie programowania. Pytanie czy na pewno.
         *                  
         * Parametry:       -
         * =========================================================================================================================================================
         */
        private void MainForm_Closing(object sender, CancelEventArgs e)
        {
            if (!poziom_ur)
            {
                MessageBox.Show("Brak zezwolenia na zamknięcie programu (brak uprawnień UR).");
                e.Cancel = true;
            }
            else
            {
                DialogResult dr = MessageBox.Show("Czy na pewno chcesz zamknąć program?", "Zamykanie programu", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);

                if (dr == DialogResult.No)
                    e.Cancel = true;
            }
        }

        /* 
         * =========================================================================================================================================================
         * Nazwa:           button_ResetTimers_Click
         * 
         * Przeznaczenie:   Kasowanie timerów na tabie System
         *                  
         * Parametry:       -
         * =========================================================================================================================================================
         */
        private void button_ResetTimers_Click(object sender, EventArgs e)
        {
            reset_timers = true;
        }


        /* 
        * =========================================================================================================================================================
        * Nazwa:           ...
        * 
        * Przeznaczenie:   Obsluga minimalizacji, przywracania i zamykania aplikacji 
        *                  
        * Parametry:       -
        * =========================================================================================================================================================
        */
        private void MainForm_GotFocus(object sender, EventArgs e)
        {
            if (Tools.windowsCE)
            {
                this.Size = new Size(800, 480);
                this.TopMost = true;
            }
        }

        private void buttonMinimalizuj_Click(object sender, EventArgs e)
        {
            if (Tools.windowsCE)
            {
                this.Size = new Size(200, 75);
                this.TopMost = false;
            }
        }

        private void buttonZamknij_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /* 
         * =========================================================================================================================================================
         * Nazwa:           OdswierzListePrzyrzadow
         * 
         * Przeznaczenie:   Funkcja odswiezająca zawartość listy przyrzadów wywolywana w innych miejscach.
         *                  
         * Parametry:       -
         * =========================================================================================================================================================
         */
        private void OdswierzListePrzyrzadow()
        {
            string[] katalogi;
            string nazwa, sama_nazwa = "", sam_kod = "";

            try
            {
                if (Directory.Exists(Tools.AppDirectory + "\\tools"))
                {
                    katalogi = Directory.GetDirectories(Tools.AppDirectory + "\\tools");

                    listaPrzyrzadow.Items.Clear();

                    for (int i = 0; i < katalogi.Length; i++)
                    {
                        nazwa = katalogi[i].Substring(katalogi[i].LastIndexOf('\\') + 1);

                        if (nazwa.LastIndexOf('.') != -1)
                        {
                            sama_nazwa = nazwa.Substring(0, nazwa.LastIndexOf('.'));
                            sam_kod = nazwa.Substring(nazwa.LastIndexOf('.') + 1, nazwa.Length - nazwa.LastIndexOf('.') - 1);
                        }
                        else
                        {
                            sama_nazwa = nazwa;
                            sam_kod = "0";
                        }

                        if (sam_kod == "0")
                            listaPrzyrzadow.Items.Add(sama_nazwa);
                        else
                            listaPrzyrzadow.Items.Add(sama_nazwa + "." + sam_kod);
                    }

                    brak_kodu = false;
                }
            }
            catch (Exception ex)
            {
                Tools.LogEx(ex, "OdswierzListePrzyrzadow");
            }
        }

        /* 
         * =========================================================================================================================================================
         * Nazwa:           buttonToolNew_Click
         * 
         * Przeznaczenie:   Zdarzenie od przycisku Tworzenie nowego przyrzadu
         *                  
         * Parametry:       -
         * =========================================================================================================================================================
         */
        private void buttonToolNew_Click(object sender, EventArgs e)
        {
            string nazwa = "";
            int kod = 0;

            //
            // Jezeli brak miejsca na dysku wyswietl komunikat
            //
            if (brak_miejsca_na_dysku)
                BrakMiejscaInfo();

            Form_PodajNazwePrzyrzadu nowy = new Form_PodajNazwePrzyrzadu();

            nowy.ShowDialog();

            if (nowy.textNazwa.Text != "")
            {
                Cursor.Current = Cursors.WaitCursor;

                nazwa = nowy.textNazwa.Text;
                kod = (int)nowy.numeric_Kod.Value;

                nazwa = nazwa.Replace('.', '_');  // Kropka przed nazwa bedzie oznaczala numer przyrzadu
                nazwa = nazwa.Replace(' ', '_');
                nazwa = nazwa.Replace('\\', '_');
                nazwa = nazwa.Replace('/', '_');
                nazwa = nazwa.Replace(':', '_');
                nazwa = nazwa.Replace('*', '_');
                nazwa = nazwa.Replace('?', '_');
                nazwa = nazwa.Replace('"', '_');
                nazwa = nazwa.Replace('<', '_');
                nazwa = nazwa.Replace('>', '_');
                nazwa = nazwa.Replace('|', '_');

                if (Directory.Exists(Tools.ToolsDirectory + "\\" + nazwa))
                    MessageBox.Show("Przyrząd taki już istnieje! Wybierz inną nazwę lub kod", "Tworzenie nowego przyrządu");
                else
                {
                    Zarzadzanie.NowyPrzyrzad(nazwa, kod);
                    OdswierzListePrzyrzadow();
                }

                //statusBar.Text = "Utworzono nowy przyrząd '" + nazwa + "'";

                Cursor.Current = Cursors.Default;
            }
        }


        /* 
        * =========================================================================================================================================================
        * Nazwa:           ...
        * 
        * Przeznaczenie:   Konfiguracje czujników przez COMBO
        *                  
        * Parametry:       -
        * =========================================================================================================================================================
        */

        void WyslijProgram()
        {
            if (Modbus.CommunicationOK && Zarzadzanie.AktywacjaOK && (poziom_ur || poziom_ustawiacz))
            {
                while (zmiana_parametru_start)
                {
                    Thread.Sleep(1);
                }

                zmiana_parametru_start = true;

                while (zmiana_parametru_start)
                {
                    Thread.Sleep(1);
                }

                Zarzadzanie.Transfer_ProgramDoPliku(Zarzadzanie.AktywnyPrzyrzad, Zarzadzanie.kod_aktywny, ActiveProgEdit);

                Tools.LogParam("Zmiana parametrów w programie " + ActiveProgEdit.ToString());
            }
        }

        /* 
        * =========================================================================================================================================================
        * Nazwa:           WyslijProgram
        * 
        * Przeznaczenie:   Request wysyłania programu z określonym numerem
        *                  
        * Parametry:       nr - numer programu
        * =========================================================================================================================================================
        */

        void WyslijProgram(UInt16 nr)
        {
            if (Modbus.CommunicationOK && Zarzadzanie.AktywacjaOK && (poziom_ur || poziom_ustawiacz))
            {
                while (zmiana_parametru_start_2)
                {
                    Thread.Sleep(1);
                }

                zmiana_parametru_start_2 = true;

                while (zmiana_parametru_start_2)
                {
                    Thread.Sleep(1);
                }

                Zarzadzanie.Transfer_ProgramDoPliku(Zarzadzanie.AktywnyPrzyrzad, Zarzadzanie.kod_aktywny, (byte)nr);

                Tools.LogParam("Zmiana parametrów w programie " + nr.ToString());
            }
        }


        /* 
        * =========================================================================================================================================================
        * Nazwa:           pictureLogoAPTerminal_Paint
        * 
        * Przeznaczenie:   Zdarzenia od rysowania loga. Wiemy że aplikacja się pojawiła na ekranie.
        *                  
        * Parametry:       -
        * =========================================================================================================================================================
        */

        private void pictureLogoAPTerminal_Paint(object sender, PaintEventArgs e)
        {
            if (start)
            {
                start = false;

                delay_aktywacja = Environment.TickCount;
            }

        }

        /* 
         * =========================================================================================================================================================
         * Nazwa:           buttonDeactivate_Click
         * 
         * Przeznaczenie:   Zdarzenie - Deaktywacja przyrzadu. Wyslanie rozkazu do sterownika i wymazanie wpisu w ustawieniach
         * =========================================================================================================================================================
         */
        private void buttonDeactivate_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Czy chcesz napewo deaktywować przyrząd?", "Deaktywacja", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) == DialogResult.Yes)
            {
                Zarzadzanie.Deaktywacja();
            }
        }

    }
}
