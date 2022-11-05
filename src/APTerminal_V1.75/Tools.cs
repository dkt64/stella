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
using System.Diagnostics;
using System.Globalization;

namespace APTerminal
{
    static class Tools
    {
        const int MAX_LINES_FOR_SETTINGS = 20;
        public static string SystemName;
        public static string SystemNameForLogs;
        public static string NETVersion;
        public static string AppDirectory, ToolsDirectory;
        public static string AppName;
        public static string filenameSettings, filenameMachineState;
        public static bool windowsCE;
        public static CultureInfo ang;
        public static string culture_name;

        public static Int64 timer, time4;
        static string last_exception;
        static UInt32 err_stary;

        /* 
         * =========================================================================================================================================================
         * Nazwa:           Odczyt zajętości dysku
         * =========================================================================================================================================================
         */

        public static ulong dysk_dostepne;
        public static ulong dysk_rozmiar;
        public static ulong dysk_wolne;
        public static float procent_zajetosci_dysku;
        public static string nazwa_dysku;

#if WindowsCE
        [DllImport("coredll.dll", SetLastError = true, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetDiskFreeSpaceEx(string lpDirectoryName,
          out ulong lpFreeBytesAvailable,
          out ulong lpTotalNumberOfBytes,
          out ulong lpTotalNumberOfFreeBytes);

#else
        [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetDiskFreeSpaceEx(string lpDirectoryName,
          out ulong lpFreeBytesAvailable,
          out ulong lpTotalNumberOfBytes,
          out ulong lpTotalNumberOfFreeBytes);
#endif
        public static void GetDiskInfo()
        {
            if (windowsCE)
                nazwa_dysku = @"\StorageCard";
            else
                nazwa_dysku = Path.GetPathRoot(AppDirectory + @"\" + AppName);

            GetDiskFreeSpaceEx(nazwa_dysku, out dysk_dostepne, out dysk_rozmiar, out dysk_wolne);
        }

        /* 
         * =========================================================================================================================================================
         * Nazwa:           Init 
         * 
         * Przeznaczenie:   Inicjacja. Jezeli nie ma katalogu LOG to tworzymy go
         *                  
         * Parametry:      
         * =========================================================================================================================================================
         */
        public static void Init()
        {
            if (!Directory.Exists(AppDirectory + "\\log"))
                Directory.CreateDirectory(AppDirectory + "\\log");
        }

        /* 
         * =========================================================================================================================================================
         * Nazwa:           ReadEnvironment 
         * 
         * Przeznaczenie:   Funkcja wywolywana podczas startu programu. Odczyt zmiennych srodowiskowych i ustawienie sciezek dostepu do plikow
         *                  
         * Parametry:       
         * =========================================================================================================================================================
         */
        public static void ReadEnvironment()
        {
            string AppDirectoryString;

            SystemName = Environment.OSVersion.ToString();
            if (SystemName.IndexOf("CE") != -1)
            {
                windowsCE = true;
                SystemNameForLogs = "CE";
            }
            else
            {
                windowsCE = false;
                SystemNameForLogs = "PC";
            }


            NETVersion = Environment.Version.ToString();
            AppName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;

            AppDirectoryString = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
            Uri AppDirectoryUri = new Uri(AppDirectoryString);
            AppDirectory = AppDirectoryUri.LocalPath.ToString();

            ToolsDirectory = AppDirectory + @"\tools";

            filenameSettings = AppDirectory + "\\" + AppName + "_settings_" + SystemNameForLogs + ".html";
            filenameMachineState = AppDirectory + "\\" + "Machinestate.xml";


            ang = new CultureInfo("en-US");

            culture_name = CultureInfo.CurrentCulture.Name;

            //Console.WriteLine("SystemNameForLogs = " + SystemNameForLogs);
            //Console.WriteLine("filenameSettings = " + filenameSettings);
            //Console.WriteLine("filenameMachineState = " + filenameMachineState);

        }

        /* 
         * =========================================================================================================================================================
         * Nazwa:           MinMax 
         * 
         * Przeznaczenie:   Zmienia wartość zmiennej na maksymalna jak jest wieksza lub minimalna jak jest mniejsza
         *                  
         * Parametry:      
         * =========================================================================================================================================================
         */
        public static void Max(ref ushort wartosc, ushort max)
        {
            if (wartosc > max)
                wartosc = max;
        }

        /* 
         * =========================================================================================================================================================
         * Nazwa:           WriteSetting 
         * 
         * Przeznaczenie:   Zapisanie ustawienia w pliku html z ustawieniami. Jezeli nie ma takiego ustawienia w pliku tworzony jest nowy wpis
         *                  
         * Parametry:       Nazwa parametru i wartosc
         * =========================================================================================================================================================
         */
        public static void WriteSetting(string param, string val)
        {
            string[] text;
            string line;
            int line_nr;
            bool found = false;

            if (param != null && val != null)
            {
                if (param != "")// && val != "")
                {
                    //Log("WriteSetting(" + param + "," + val + ")");

                    text = new string[MAX_LINES_FOR_SETTINGS];

                    for (line_nr = 0; line_nr < MAX_LINES_FOR_SETTINGS; line_nr++)
                        text[line_nr] = "";

                    try
                    {
                        if (!File.Exists(filenameSettings))
                        {
                            using (StreamWriter sw = new StreamWriter(filenameSettings, true))
                            {
                                sw.WriteLine("<head> <title>" + AppName + " settings file</title> <meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\"/> </head> <body><font face=\"verdana\" size=\"1\">");
                            }
                        }

                        // Najpierw odczytujemy plik ustawien do tablicy
                        // ---------------------------------------------
                        line_nr = 0;
                        using (StreamReader sr = new StreamReader(filenameSettings))
                            while ((line = sr.ReadLine()) != null && line_nr < MAX_LINES_FOR_SETTINGS)
                                text[line_nr++] = line;

                        // Przeszukujemy tablice stringow w poszukiwaniu naszego ustawienia i zmieniamy go
                        // ----------------------------------------------------------------
                        for (line_nr = 0; line_nr < MAX_LINES_FOR_SETTINGS; line_nr++)
                        {
                            if (text[line_nr].IndexOf("<p>" + param + "=") != -1)
                            {
                                //Log("Found " + text[line_nr]);
                                text[line_nr] = "<p>" + param + "=" + val + "</p>";
                                found = true;
                                //Log("Found " + text[line_nr]);
                                break;
                            }
                        }

                        // Jezeli nie znaleziono lancucha wpisujemy nowy
                        // ----------------------------------------------------------------
                        if (found == false)
                        {
                            for (line_nr = 0; line_nr < MAX_LINES_FOR_SETTINGS; line_nr++)
                            {
                                if (text[line_nr] == "")
                                {
                                    text[line_nr] = "<p>" + param + "=" + val + "</p>";
                                    break;
                                }
                            }
                        }

                        // Zapisujemy tablice stringow do pliku
                        // ----------------------------------------------------------------

                        text[0] = "<head><title>" + AppName + " settings file</title> <meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\"/> </head><body><font face=\"verdana\" size=\"1\">";
                        line_nr = 0;
                        using (StreamWriter sw = new StreamWriter(filenameSettings, false))
                        {
                            while (text[line_nr] != "" && line_nr < MAX_LINES_FOR_SETTINGS)
                            {
                                sw.WriteLine(text[line_nr]);
                                line_nr++;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        LogEx(ex, "SaveSettings()");
                    }
                }
            }

            Log("Zapis ustawienia " + param + " = " + val);
            //MainForm.statusBar.Text = "Zapis ustawienia " + param + " = " + val;

        }

        /* 
         * =========================================================================================================================================================
         * Nazwa:           WriteSetting 
         * 
         * Przeznaczenie:   Odczytanie ustawienia z pliku html z ustawieniami. Jezeli nie ma takiego ustawienia zwracamy ""
         *                  
         * Parametry:       Nazwa parametru. Zwraca stringa z wartoscia
         * =========================================================================================================================================================
         */
        public static string ReadSetting(string param)
        {
            string retval = "";
            string line;
            int i1, i2;

            if (param != null)
            {
                if (param != "")
                {
                    //Log("ReadSetting()");

                    if (File.Exists(filenameSettings))
                    {
                        //Log("Reading parameter " + param);

                        using (StreamReader sr = new StreamReader(filenameSettings))
                        {
                            while ((line = sr.ReadLine()) != null)
                            {
                                if (line.IndexOf("<p>" + param + "=") != -1)
                                {
                                    i1 = line.IndexOf("=") + 1;
                                    i2 = line.IndexOf("</p>");
                                    retval = line.Substring(i1, i2 - i1);
                                    //Log("Odczytalem " + param + " = " + retval);
                                    //Log("Founded.");
                                }
                            }
                        }
                    }
                    else
                    {
                        Log("Settings file missing!");
                    }
                }
            }
            return retval;
        }

        /* 
         * =========================================================================================================================================================
         * Nazwa:           WriteData 
         * 
         * Przeznaczenie:   Zapisanie zmiannych produkcyjnych w pliku
         *                  
         * Parametry:       Nazwa parametru i wartosc
         * =========================================================================================================================================================
         */
        public static void WriteData(string param, string val)
        {
            string[] text;
            string line;
            int line_nr;
            bool found = false;

            if (param != null && val != null)
            {
                if (param != "" && val != "")
                {

                    //Log("WriteSetting(" + param + "," + val + ")");

                    if (!Directory.Exists(AppDirectory + "\\tools\\" + Zarzadzanie.AktywnyPrzyrzad))
                        Directory.CreateDirectory(AppDirectory + "\\tools\\" + Zarzadzanie.AktywnyPrzyrzad);

                    string filename = "";

                    filename = AppDirectory + "\\tools\\" + Zarzadzanie.AktywnyPrzyrzad + "\\" + Zarzadzanie.AktywnyPrzyrzad + "_data.html";

                    text = new string[MAX_LINES_FOR_SETTINGS];

                    for (line_nr = 0; line_nr < MAX_LINES_FOR_SETTINGS; line_nr++)
                        text[line_nr] = "";

                    try
                    {
                        if (!File.Exists(filename))
                        {
                            using (StreamWriter sw = new StreamWriter(filename, true))
                            {
                                sw.WriteLine("<head><title>" + AppName + " settings file</title> <meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\"/> </head><body><font face=\"verdana\" size=\"1\">");
                            }
                        }

                        // Najpierw odczytujemy plik ustawien do tablicy
                        // ---------------------------------------------
                        line_nr = 0;
                        using (StreamReader sr = new StreamReader(filename))
                            while ((line = sr.ReadLine()) != null && line_nr < MAX_LINES_FOR_SETTINGS)
                                text[line_nr++] = line;

                        // Przeszukujemy tablice stringow w poszukiwaniu naszego ustawienia i zmieniamy go
                        // ----------------------------------------------------------------
                        for (line_nr = 0; line_nr < MAX_LINES_FOR_SETTINGS; line_nr++)
                        {
                            if (text[line_nr].IndexOf("<p>" + param + "=") != -1)
                            {
                                //Log("Found " + text[line_nr]);
                                text[line_nr] = "<p>" + param + "=" + val + "</p>";
                                found = true;
                                //Log("Found " + text[line_nr]);
                                break;
                            }
                        }

                        // Jezeli nie znaleziono lancucha wpisujemy nowy
                        // ----------------------------------------------------------------
                        if (found == false)
                        {
                            for (line_nr = 0; line_nr < MAX_LINES_FOR_SETTINGS; line_nr++)
                            {
                                if (text[line_nr] == "")
                                {
                                    text[line_nr] = "<p>" + param + "=" + val + "</p>";
                                    break;
                                }
                            }
                        }

                        // Zapisujemy tablice stringow do pliku
                        // ----------------------------------------------------------------

                        text[0] = "<head><title>" + AppName + " " + Zarzadzanie.AktywnyPrzyrzad + " data file</title> <meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\"/> </head><body><font face=\"verdana\" size=\"1\">";
                        line_nr = 0;
                        using (StreamWriter sw = new StreamWriter(filename, false))
                        {
                            while (text[line_nr] != "" && line_nr < MAX_LINES_FOR_SETTINGS)
                            {
                                sw.WriteLine(text[line_nr]);
                                line_nr++;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        LogEx(ex, "SaveSettings()");
                    }
                }
            }

            Log("Zapis ustawiwnia " + param + " = " + val);
            //MainForm.statusBar.Text = "Zapis ustawienia " + param + " = " + val;

        }

        /* 
         * =========================================================================================================================================================
         * Nazwa:           WriteSetting 
         * 
         * Przeznaczenie:   Odczytanie ustawienia z pliku html z ustawieniami. Jezeli nie ma takiego ustawienia zwracamy ""
         *                  
         * Parametry:       Nazwa parametru. Zwraca stringa z wartoscia
         * =========================================================================================================================================================
         */
        public static string ReadData(string param)
        {
            string retval = "";
            string line;
            int i1, i2;

            if (param != null)
            {
                if (param != "")
                {
                    //Log("ReadSetting()");

                    string filename = "";

                    filename = AppDirectory + "\\tools\\" + Zarzadzanie.AktywnyPrzyrzad + "\\" + Zarzadzanie.AktywnyPrzyrzad + "_data.html";

                    if (File.Exists(filenameSettings))
                    {
                        //Log("Reading parameter " + param);

                        using (StreamReader sr = new StreamReader(filenameSettings))
                        {
                            while ((line = sr.ReadLine()) != null)
                            {
                                if (line.IndexOf("<p>" + param + "=") != -1)
                                {
                                    i1 = line.IndexOf("=") + 1;
                                    i2 = line.IndexOf("</p>");
                                    retval = line.Substring(i1, i2 - i1);
                                    //Log("Founded.");
                                }
                            }
                        }
                    }
                    else
                    {
                        Log("Settings file missing!");
                    }
                }
            }
            return retval;
        }
        /* 
         * =========================================================================================================================================================
         * Nazwa:           Log(string str) 
         * 
         * Przeznaczenie:   Dopisanie stringa do pliku logowania. Nazwa uzalezniona jest od daty (dzien), oraz watek z ktorego wywolana jest funkcja
         *                  
         * Parametry:       string - logowany ciag znakow
         * =========================================================================================================================================================
         */
        public static void Log(string str)
        {
            string data = DateTime.Now.Year.ToString() + "_" + DateTime.Now.Month.ToString().PadLeft(2, '0') + "_" + DateTime.Now.Day.ToString().PadLeft(2, '0');
            string czas = '[' + DateTime.Now.Year.ToString() + "_" + DateTime.Now.Month.ToString().PadLeft(2, '0') + "_" + DateTime.Now.Day.ToString().PadLeft(2, '0') + " " + DateTime.Now.Hour.ToString().PadLeft(2, '0') + ":" + DateTime.Now.Minute.ToString().PadLeft(2, '0') + ":" + DateTime.Now.Second.ToString().PadLeft(2, '0') + "] ";
            bool header = false;

            if (str != null)
            {
                try
                {
                    string filename = "";

                    filename = AppDirectory + "\\log\\" + AppName + "_log_" + Thread.CurrentThread.Name + "_" + data + ".html";

                    if (!File.Exists(filename))
                        header = true;
                    else
                        header = false;

                    using (StreamWriter sw = new StreamWriter(filename, true))
                    {
                        if (header)
                            //sw.WriteLine("<head> <title>" + AppName + " " + Thread.CurrentThread.Name + " log file</title> <meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\"/> <link rel=\"stylesheet\" type=\"text/css\" href=\"css/style.css\" /> </head> <body>");
                            sw.WriteLine("<head><title>" + AppName + " " + Thread.CurrentThread.Name + " log file</title> <meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\"/> </head><body><font face=\"verdana\" size=\"1\">");

                        sw.WriteLine("<p>" + czas + str + "</p>");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Exception in file logging. " + ex.Message);
                }
            }
        }

        /* 
         * =========================================================================================================================================================
         * Nazwa:           LogEx(Exception logex, string str)
         * 
         * Przeznaczenie:   Dopisanie stringa z tekstem wyjatku do pliku logowania, oraz komentarza. 
         *                  Nazwa uzalezniona jest od daty (dzien), oraz watek z ktorego wywolana jest funkcja
         *                  
         * Parametry:       Exception - wyjatek, oraz string - logowany ciag znakow
         * =========================================================================================================================================================
         */
        public static void LogEx(Exception logex, string str)
        {
            string data = DateTime.Now.Year.ToString() + "_" + DateTime.Now.Month.ToString().PadLeft(2, '0') + "_" + DateTime.Now.Day.ToString().PadLeft(2, '0');
            string czas = '[' + DateTime.Now.Year.ToString() + "_" + DateTime.Now.Month.ToString().PadLeft(2, '0') + "_" + DateTime.Now.Day.ToString().PadLeft(2, '0') + " " + DateTime.Now.Hour.ToString().PadLeft(2, '0') + ":" + DateTime.Now.Minute.ToString().PadLeft(2, '0') + ":" + DateTime.Now.Second.ToString().PadLeft(2, '0') + "] ";
            bool header = false;

            try
            {
                string filename = "";

                filename = AppDirectory + "\\log\\" + AppName + "_logex_" + Thread.CurrentThread.Name + "_" + data + ".html";

                if (!File.Exists(filename))
                    header = true;
                else
                    header = false;

                using (StreamWriter sw = new StreamWriter(filename, true))
                {
                    if (header)
                        sw.WriteLine("<head><title>" + AppName + " exceptions log file</title> <meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\"/> </head><body><font face=\"verdana\" size=\"1\">");

                    sw.WriteLine("<p>" + czas + "EXCEPTION in " + str + ": " + logex.Message + logex.StackTrace + "</p>");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception in file logging. " + ex.Message);
            }

            if (logex.Message != last_exception)
                MessageBox.Show(logex.Message, str);

            last_exception = logex.Message;
        }

        /* 
         * =========================================================================================================================================================
         * Nazwa:           LogParam
         * 
         * Przeznaczenie:   Logowanie w logu przyrzadu - katalog glebiej w tools\nazwa\log\...html
         *                  Nazwa uzalezniona jest od daty (dzien), oraz watek z ktorego wywolana jest funkcja
         *                  
         * Parametry:       string - logowany ciag znakow
         * =========================================================================================================================================================
         */
        public static void LogParam(string str)
        {
            string data = DateTime.Now.Year.ToString() + "_" + DateTime.Now.Month.ToString().PadLeft(2, '0') + "_" + DateTime.Now.Day.ToString().PadLeft(2, '0');
            string czas = '[' + DateTime.Now.Year.ToString() + "_" + DateTime.Now.Month.ToString().PadLeft(2, '0') + "_" + DateTime.Now.Day.ToString().PadLeft(2, '0') + " " + DateTime.Now.Hour.ToString().PadLeft(2, '0') + ":" + DateTime.Now.Minute.ToString().PadLeft(2, '0') + ":" + DateTime.Now.Second.ToString().PadLeft(2, '0') + "] ";
            bool header = false;
            string logdir;

            if (str != null)
            {
                try
                {
                    if (Zarzadzanie.AktywacjaOK)
                        logdir = AppDirectory + "\\tools\\" + Zarzadzanie.AktywnyPrzyrzad + '.' + Zarzadzanie.kod_aktywny.ToString() + "\\log";
                    else
                        logdir = AppDirectory + "\\log\\";

                    if (!Directory.Exists(logdir))
                        Directory.CreateDirectory(logdir);

                    string filename = "";

                    if (Zarzadzanie.AktywacjaOK)
                    filename = logdir + "\\" + 
                        Zarzadzanie.AktywnyPrzyrzad + "_log_" + data + ".html";
                    else
                        filename = logdir + AppName + "_logparam_" + Thread.CurrentThread.Name + "_" + data + ".html";

                    if (!File.Exists(filename))
                        header = true;
                    else
                        header = false;

                    using (StreamWriter sw = new StreamWriter(filename, true))
                    {
                        if (header)
                            sw.WriteLine("<head><title>" + AppName + " " + Zarzadzanie.AktywnyPrzyrzad + " log file</title>  <meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\"/> </head><body><font face=\"verdana\" size=\"1\">");

                        sw.WriteLine("<p>" + czas + str + "</p>");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Exception in file logging. " + ex.Message);
                }
            }
        }

        /* 
         * =========================================================================================================================================================
         * Nazwa:           LogujZaklocenia
         * 
         * Przeznaczenie:   Logowanie w logu przyrzadu - katalog glebiej w tools\nazwa\log\...html
         *                  Logowanie zakłóceń na podstawie bitów z wartości wejściowej err
         *                  
         * Parametry:       err - bity zakłóceń
         * =========================================================================================================================================================
         */
        public static void LogErr(UInt32 err)
        {
            UInt32 err_akt;

            err_akt = err;

            string logdir = AppDirectory + "\\tools\\" + Zarzadzanie.AktywnyPrzyrzad + '.' + Zarzadzanie.kod_aktywny.ToString() + "\\log";

            if (err != 0 && err != err_stary)
            {

                string str = "";
                string data = DateTime.Now.Year.ToString() + "_" + DateTime.Now.Month.ToString().PadLeft(2, '0') + "_" + DateTime.Now.Day.ToString().PadLeft(2, '0');
                string czas = '[' + DateTime.Now.Year.ToString() + "_" + DateTime.Now.Month.ToString().PadLeft(2, '0') + "_" + DateTime.Now.Day.ToString().PadLeft(2, '0') + " " + DateTime.Now.Hour.ToString().PadLeft(2, '0') + ":" + DateTime.Now.Minute.ToString().PadLeft(2, '0') + ":" + DateTime.Now.Second.ToString().PadLeft(2, '0') + "] ";
                bool header = false;

                try
                {
                    string filename = "";

                    if (Zarzadzanie.AktywnyPrzyrzad != "")
                    {
                        if (!Directory.Exists(logdir))
                            Directory.CreateDirectory(logdir);

                        filename = logdir + "\\" + 
                            Zarzadzanie.AktywnyPrzyrzad + "_logerr_" + data + ".html";
                    }
                    else
                    {
                        filename = AppDirectory + "\\log\\" + AppName + "_logerr_" + Thread.CurrentThread.Name + "_" + data + ".html";
                    }

                    if (!File.Exists(filename))
                        header = true;
                    else
                        header = false;

                    using (StreamWriter sw = new StreamWriter(filename, true))
                    {
                        if (header)
                            sw.WriteLine("<head><title>" + AppName + " " + Zarzadzanie.AktywnyPrzyrzad + " tool errors log file</title> <meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\"/> </head><body><font face=\"verdana\" size=\"1\">");

                        sw.WriteLine("<p>" + czas + "ERR = " + err.ToString() + "</p>");

                        //
                        // Rozapoznanie wszystkich bitów błędów
                        //
                        UInt32 bit_and = 1;

                        for (int bit = 1; bit < 32; bit++)
                        {
                            if (((err_akt & bit_and) > 0) && ((err_stary & bit_and) == 0))
                            {
                                str = MainForm.zak_tab[bit - 1];
                                sw.WriteLine("<p>" + czas + str + "</p>");
                            }

                            bit_and = (uint)(1 << bit);
                        }

                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Exception in file logging. " + ex.Message);
                }


            }

            err_stary = err_akt;

        }

    }
}
