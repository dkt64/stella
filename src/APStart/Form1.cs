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

namespace APStart
{
    public partial class Form1 : Form
    {
        string path = "";

        const int MAX_LINES_FOR_SETTINGS = 20;
        public static string SystemName;
        public static string SystemNameForLogs;
        public static string NETVersion;
        public static string AppDirectory;
        public static string AppName;
        public static string filenameSettings;
        public static bool windowsCE;
        public static CultureInfo ang;
        public static string culture_name;
        public static Int64 timer, time4;

        const int TIMEOUT = 5;

        int time = TIMEOUT;

        /* 
         * =========================================================================================================================================================
         * Nazwa:           Form1 
         * 
         * Przeznaczenie:   
         *                  
         * Parametry:      
         * =========================================================================================================================================================
         */
        public Form1()
        {
            InitializeComponent();
            labelTimer.Text = "Start in " + time.ToString() + " seconds";
            ReadEnvironment();
            path = ReadSetting("APTerminalPath");
            labelPath.Text = path;

            timerStart.Enabled = true;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void buttonChange_Click(object sender, EventArgs e)
        {
            timerStart.Enabled = false;
            buttonStartStop.Text = "Start";

            FileDialog dialog = new OpenFileDialog();
            dialog.Filter = "exe files (*.exe)|*.exe";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                path = dialog.FileName;
                labelPath.Text = path;
                WriteSetting("APTerminalPath", path);
            }
        }

        private void buttonStartStop_Click(object sender, EventArgs e)
        {
            if (timerStart.Enabled)
            {
                timerStart.Enabled = false;
                buttonStartStop.Text = "Start";
            }
            else
            {
                time = TIMEOUT;
                timerStart.Enabled = true;
                buttonStartStop.Text = "Stop";
            }
        }

        private void timerStart_Tick(object sender, EventArgs e)
        {
            if (time >= 1)
            {
                time--;
                labelTimer.Text = "Start in " + time.ToString() + " seconds";
            }

            if (time == 0 && File.Exists(path))
            {
                labelTimer.Text = "Starting APTerminal ...";
                timerStart.Enabled = false;
                //buttonCancel.Enabled = false;
                buttonChange.Enabled = false;
                buttonStartStop.Enabled = false;
                labelTimer.Text = "Starting APTerminal ...";
                timerClose.Enabled = true;
                System.Diagnostics.Process.Start(path, "");
            }

            if (time == 0 && !File.Exists(path))
            {
                timerStart.Enabled = false;
                buttonStartStop.Text = "Start";
                MessageBox.Show(path + " doesn't exists");
            }
        }

        private void timerClose_Tick(object sender, EventArgs e)
        {
            this.Close();
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

            filenameSettings = AppDirectory + "\\" + AppName + "_settings_" + SystemNameForLogs + ".html";

            ang = new CultureInfo("en-US");

            culture_name = CultureInfo.CurrentCulture.Name;
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
                if (param != "" && val != "")
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
                                sw.WriteLine("<head><title>" + AppName + " settings file</title></head><body><font face=\"verdana\" size=\"1\">");
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

                        text[0] = "<head><title>" + AppName + " settings file</title></head><body><font face=\"verdana\" size=\"1\">";
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
                        MessageBox.Show(ex.ToString(), "SaveSettings()");
                    }
                }
            }

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
                        MessageBox.Show("Settings file missing!");
                    }
                }
            }
            return retval;
        }
    }
}
