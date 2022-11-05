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
using APTerminal.Properties;

#if WindowsCE
using Microsoft.WindowsCE.Forms;
#endif

/*
 * ------------------------------------------------------------------------------------
 * Przestrzeń nazw programu
 * ------------------------------------------------------------------------------------
 */

namespace APTerminal
{
   /*
    * ------------------------------------------------------------------------------------
    * Klasa formatki
    * ------------------------------------------------------------------------------------
    */
    public partial class Form_Main : Form
    {
        /*
         * ------------------------------------------------------------------------------------
         * Zmienne
         * ------------------------------------------------------------------------------------
         */

        bool io_get = true;

        DateTime dataczas;

        bool windowsCE;

        string OstatnioUzywanyPrzyrzad = "";
        string OstatnioUzywanyCom = "";

        string[] comPorts;
        SerialPort port;
        bool connected = false;
        Thread comThread;
        bool parametry_zaladowane = false;

        bool sending = false;

        int I00 = 0;
        int I01 = 0;
        int I02 = 0;
        int O00 = 0;

        /*
         * ------------------------------------------------------------------------------------
         * Konstrukctor klasy formatki
         * ------------------------------------------------------------------------------------
         */
        public Form_Main()
        {
            InitializeComponent();
        }

        /*
         * ------------------------------------------------------------------------------------
         * Metoda wywoływana przy załadowaniu formatki
         * Tutaj wykonujemy wszelkie inicjacje
         * ------------------------------------------------------------------------------------
         */
        private void Form_Main_Load(object sender, EventArgs e)
        {
            // Sprawdzamy z jakim systemem mamy do czynienia
            // Jezeli Windows CE to true w innym przypadku false
            // -------------------------------------------------
            Console.WriteLine("System: " + Environment.OSVersion);
            if (Environment.OSVersion.ToString().IndexOf("Windows CE") > -1)
                windowsCE = true;
            else windowsCE = false;

            dataczas = new DateTime();

            System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
            timer.Interval = 1000;
            timer.Tick += new EventHandler(MyTick);
            timer.Enabled = true;

            // Okno na stałą wielkość - przy edycji w Visualu może być inne bo łatwiej ustawia się kontrolki
            // ---------------------------------------------------------------------------------------------
            if (windowsCE)
                this.Size = new Size(320, 234);

            // Teraz główna inicjacja
            // ----------------------

            LogApp("Start aplikacji");

            // 1. Ładowanie ustawień
            LadujUstawienia();
            Console.WriteLine("Ostatnio uzywany przyrzad: " + OstatnioUzywanyPrzyrzad);
            Console.WriteLine("Ostatnio uzywany COM: " + OstatnioUzywanyCom);

            // 2. Połączenie
            GetAvailableComs();
            ConnectToController();
            if (connected)
            {
                LogApp("Polaczono ze sterownikiem");

                // 3. Jeżeli połączono wątek dla rs-a
                comThread = new Thread(SerialThread);
                comThread.Start();

                // 4. Ładowanie parametrów i wysyłanie do sterownika
                LadujParametry(OstatnioUzywanyPrzyrzad);
                WyslijWszystkieParametry();

                // 5. Wyswietlenie listy przyrządów
                ListaPrzyrzadow();

                // 6. Zaspis ustawien
                ZapiszUstawienia();

                // 7. Odświeżenie wejść wyjść poprzez wysłanie zapytania
                IOGet();

                parametry_zaladowane = true;
            }
            else
            {
                LogApp("Nie polaczono sie ze sterownikiem!");
                
            }
        }

        /*
         * ------------------------------------------------------------------------------------
         * Logowanie zdarzen w pliku
         * ------------------------------------------------------------------------------------
         */
        void LogApp(string str)
        {
            string data = DateTime.Now.Year.ToString() + "_" + DateTime.Now.Month.ToString().PadLeft(2, '0');;
            string czas = DateTime.Now.Day.ToString().PadLeft(2, '0') +"_" + DateTime.Now.Hour.ToString().PadLeft(2, '0') +":" + DateTime.Now.Minute.ToString().PadLeft(2, '0') +": ";

            try
            {
                string filename = "";

                if (windowsCE)
                    filename = @"\norflash\log\APTerminal.log_" + data + ".html";
                else
                    filename = @"log\APTerminal.log_" + data + ".html";

                using (StreamWriter sw = new StreamWriter(filename, true))
                {
                    sw.WriteLine("<p>" + czas + str + "</p>");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception -> void ZapiszUstawienia(). " + ex.Message);
            }
        }

        /*
         * ------------------------------------------------------------------------------------
         * Gdy nasze okno uzyska focus ustawiamy je w pozycji 0,0
         * Na wypadek gdy w Windowsie CE ktoś manipulował oknem
         * ------------------------------------------------------------------------------------
         */
        private void Form_Main_GotFocus(object sender, EventArgs e)
        {
            if (windowsCE)
            {
                this.Location = new Point(0, 0);
            }
        }

        private void Form_Main_LostFocus(object sender, EventArgs e)
        {
            if (windowsCE)
            {
                this.Location = new Point(0, 0);
            }
        }

        /*
         * ------------------------------------------------------------------------------------
         * Przy próbie zmiany rozmiaru okna wpisujemy stałą wartość
         * Aby użytkownik nie mógł zmieniać rozmiaru
         * ------------------------------------------------------------------------------------
         */
        private void Form_Main_Resize(object sender, EventArgs e)
        {
            if (windowsCE)
                this.Size = new Size(320, 234);
        }

        /*
         * ------------------------------------------------------------------------------------
         * Zdarzenie zamykania formatki
         * ------------------------------------------------------------------------------------
         */
        private void Form_Main_Closing(object sender, CancelEventArgs e)
        {
            if ((I00 & 32) == 0 && connected && windowsCE)
            {
                MessageBox.Show("Brak zezwolenia na wyłączenie programu (ustaw kluczyk w odpowiednią pozycję).");
                e.Cancel = true;
            }
            else
            {
                DialogResult dr = MessageBox.Show("Czy na pewno chcesz zamknąć program?", "Zamykanie programu", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);

                if (dr == DialogResult.Yes)
                {
                    connected = false;
                    if (comThread != null)
                        comThread.Join(2000);
                    CloseAllPorts();
                }
                else
                    e.Cancel = true;
            }
        }

        /*
         * ------------------------------------------------------------------------------------
         * Ładowanie ustawień z pliku config
         * ------------------------------------------------------------------------------------
         */
        void LadujUstawienia()
        {
            string str="";

            try
            {
                string filename = "";

                if (windowsCE)
                    filename = @"\norflash\APTerminal.config.html";
                else
                    filename = "APTerminal.config.html";

                using (StreamReader sr = new StreamReader(filename))
                {
                    String line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        str = line;
                        //Console.WriteLine(line);
                        string[] splitedline = line.Split('=');
                        switch (splitedline[0])
                        {
                            case "<p>Ostatnio uzywany przyrzad":
                                OstatnioUzywanyPrzyrzad = splitedline[1];
                                break;

                            case "<p>Ostatnio uzywany COM":
                                OstatnioUzywanyCom = splitedline[1];
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception -> void LadujUstawienia(). " + ex.Message + "Parametr = " + str);
            }
        }

        /*
         * ------------------------------------------------------------------------------------
         * Zapisywanie ustawień
         * ------------------------------------------------------------------------------------
         */
        void ZapiszUstawienia()
        {
            try
            {
                string filename="";

                if (windowsCE)
                    filename = @"\norflash\APTerminal.config.html";
                else
                    filename = "APTerminal.config.html";

                    using (StreamWriter sw = new StreamWriter(filename, false))
                    {
                        sw.WriteLine("<p>- APTerminal Ustawienia -</p>");
                        sw.WriteLine("<p>Ostatnio uzywany przyrzad=" + OstatnioUzywanyPrzyrzad);
                        sw.WriteLine("<p>Ostatnio uzywany COM=" + OstatnioUzywanyCom);
                    }            
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception -> void ZapiszUstawienia(). " + ex.Message);
            }
        }

        /*
         * ------------------------------------------------------------------------------------
         * Ładowanie parametrów z pliku
         * ------------------------------------------------------------------------------------
         */
        void LadujParametry(string tool)
        {
            string filename;

            if (windowsCE)
                filename = @"\norflash\tools\" + tool + @".par.html";
            else
                filename = @"tools\" + tool + @".par.html";

            try
            {
                if (filename != "")
                {
                    using (StreamReader sr = new StreamReader(filename))
                    {
                        String line;
                        while ((line = sr.ReadLine()) != null)
                        {
                            string[] splitedline = line.Split('=');
                            switch (splitedline[0])
                            {
                                case "<p>Prad zgrzewania":
                                    ParametrFormatuj(Prad, splitedline[1]);
                                    break;

                                case "<p>Czas zgrzewania":
                                    ParametrFormatuj(Czas, splitedline[1]);
                                    break;

                                case "<p>Czas docisku wstepnego":
                                    ParametrFormatuj(CzWs, splitedline[1]);
                                    break;

                                case "<p>Czas docisku koncowego":
                                    ParametrFormatuj(CzKo, splitedline[1]);
                                    break;
                                case "<p>Stepper licznik":
                                    ParametrFormatuj(StLi, splitedline[1]);
                                    break;

                                case "<p>Stepper procent":
                                    ParametrFormatuj(StPr, splitedline[1]);
                                    break;

                                case "<p>Impulsy liczba":
                                    ParametrFormatuj(ImLi, splitedline[1]);
                                    break;

                                case "<p>Impulsy pauza":
                                    ParametrFormatuj(ImPa, splitedline[1]);
                                    break;

                                case "<p>Licznik":
                                    ParametrFormatuj(Licz, splitedline[1]);
                                    break;

                                case "<p>Licznik pojemnik":
                                    ParametrFormatuj(LiPo, splitedline[1]);
                                    break;

                                case "<p>Licznik zmiana":
                                    ParametrFormatuj(LiZm, splitedline[1]);
                                    break;

                                case "<p>Licznik elektroda":
                                    ParametrFormatuj(LiEl, splitedline[1]);
                                    break;
                            }
                        }
                    }
                   
                }
                LogApp("Zaladowano przyrzad " + tool);
                OstatnioUzywanyPrzyrzad = Przyrzad_Aktywny.Text = tool;                
            }

            catch (Exception ex)
            {
                MessageBox.Show("Exception -> void LadujParametry(string tool). " + ex.Message + "Parametr = " + filename);
            }
        }

        /*
         * ------------------------------------------------------------------------------------
         * Zapisywanie parametrów
         * ------------------------------------------------------------------------------------
         */
        void ZapiszParametry(string name)
        {
            string filename;

            if (windowsCE)
                filename = @"\norflash\tools\" + name + @".par.html";
            else
                filename = @"tools\" + name + @".par.html";

            try
            {
                if (filename != "")
                {
                    using (StreamWriter sw = new StreamWriter(filename, false))
                    {
                        sw.WriteLine("<p>- PARAMETRY ZGRZEWANIA -</p>");
                        sw.WriteLine("<p>Przyrzad: " + name + "</p>");
                        sw.WriteLine("<p></p>");
                        sw.WriteLine("<p>Prad zgrzewania=" + Prad.Text);
                        sw.WriteLine("<p>Czas zgrzewania=" + Czas.Text);
                        sw.WriteLine("<p>Czas docisku wstepnego=" + CzWs.Text);
                        sw.WriteLine("<p>Czas docisku koncowego=" + CzKo.Text);
                        sw.WriteLine("<p>Stepper licznik=" + StLi.Text);
                        sw.WriteLine("<p>Stepper procent=" + StPr.Text);
                        sw.WriteLine("<p>Impulsy liczba=" + ImLi.Text);
                        sw.WriteLine("<p>Impulsy pauza=" + ImPa.Text);
                        sw.WriteLine("<p>Licznik=" + Licz.Text);
                        sw.WriteLine("<p>Licznik pojemnik=" + LiPo.Text);
                        sw.WriteLine("<p>Licznik zmiana=" + LiPo.Text);
                        sw.WriteLine("<p>Licznik elektroda=" + LiPo.Text);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception -> void ZapiszParametry(). " + ex.Message + "Parametr = " + filename);
            }
        }

        /*
         * ------------------------------------------------------------------------------------
         * Zapisywanie parametrów
         * ------------------------------------------------------------------------------------
         */
        void ZapiszParametry_Nowe(string name)
        {
            string filename;

            if (windowsCE)
                filename = @"\norflash\tools\" + name + @".par.html";
            else
                filename = @"tools\" + name + @".par.html";

            try
            {
                if (filename != "")
                {
                    using (StreamWriter sw = new StreamWriter(filename, false))
                    {
                        sw.WriteLine("<p>- PARAMETRY ZGRZEWANIA -</p>");
                        sw.WriteLine("<p>Przyrzad: " + name + "</p>");
                        sw.WriteLine("<p></p>");
                        sw.WriteLine("<p>Prad zgrzewania=0");
                        sw.WriteLine("<p>Czas zgrzewania=0");
                        sw.WriteLine("<p>Czas docisku wstepnego=5");
                        sw.WriteLine("<p>Czas docisku koncowego=5");
                        sw.WriteLine("<p>Stepper licznik=0");
                        sw.WriteLine("<p>Stepper procent=0");
                        sw.WriteLine("<p>Impulsy liczba=1");
                        sw.WriteLine("<p>Impulsy pauza=0");
                        sw.WriteLine("<p>Licznik=0");
                        sw.WriteLine("<p>Licznik pojemnik=0");
                        sw.WriteLine("<p>Licznik zmiana=0");
                        sw.WriteLine("<p>Licznik elektroda=0");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception -> void ZapiszParametry(). " + ex.Message + "Parametr = " + filename);
            }
        }

        /*
         * ------------------------------------------------------------------------------------
         * Ładowanie listy przyrządów na podstawie zawartości katalogu 'tools'
         * ------------------------------------------------------------------------------------
         */
        void ListaPrzyrzadow()
        {
            string[] lista;
            string[] tempstr;

            try
            {
                if (windowsCE)
                    lista = Directory.GetFiles(@"\norflash\tools", "*.par.html");
                else
                    lista = Directory.GetFiles("tools", "*.par.html");

                Przyrzady_Lista.Items.Clear();

                foreach (string przyrzad in lista)
                {
                    tempstr = przyrzad.Split(new Char [] {'\\' , '.'});

                    if (windowsCE)
                        Przyrzady_Lista.Items.Add(tempstr[3]);
                    else
                        Przyrzady_Lista.Items.Add(tempstr[1]);
                }
                
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception -> void ListaPrzyrzadow(). " + ex.Message);
            }
        }

        /*
         * ------------------------------------------------------------------------------------
         * Zdarzenie ładowania przyrządu
         * ------------------------------------------------------------------------------------
         */
        private void Przyrzady_Laduj_Click(object sender, EventArgs e)
        {
            if ((I00 & 32) == 0 && parametry_zaladowane && windowsCE)
            {
                MessageBox.Show("Brak zezwolenia na ladowanie innych parametrow (ustaw kluczyk w odpowiednią pozycję).");
            }
            else if (Przyrzady_Lista.SelectedIndex > -1 && connected)
            {
                LadujParametry(Przyrzady_Lista.SelectedItem.ToString());
                WyslijWszystkieParametry();
                ZapiszUstawienia();
            }
        }

        /*
        * ------------------------------------------------------------------------------------
        * Tworzymy nowy przyrząd (plik)
        * ------------------------------------------------------------------------------------
        */

        private void Przyrzady_Nowy_Click(object sender, EventArgs e)
        {
            string nazwa;


            if ((I00 & 32) == 0 && parametry_zaladowane && windowsCE)
            {
                MessageBox.Show("Brak zezwolenia na zmianę parametrów (ustaw kluczyk w odpowiednią pozycję).");
            }
            else
            {
                Form_Nowy nowy = new Form_Nowy(windowsCE);

                this.TopMost = false;
                if (nowy.ShowDialog() == DialogResult.OK)
                {
                    nazwa = nowy.NazwaPrzyrzadu.Text;

                    nazwa = nazwa.Replace('.', '_');
                    nazwa = nazwa.Replace('\\', '_');
                    nazwa = nazwa.Replace('/', '_');
                    nazwa = nazwa.Replace(':', '_');
                    nazwa = nazwa.Replace('*', '_');
                    nazwa = nazwa.Replace('?', '_');
                    nazwa = nazwa.Replace('"', '_');
                    nazwa = nazwa.Replace('<', '_');
                    nazwa = nazwa.Replace('>', '_');
                    nazwa = nazwa.Replace('|', '_');

                    ZapiszParametry_Nowe(nazwa);
                    ListaPrzyrzadow();
                }
                this.TopMost = true;
            }
        }

        /*
        * ------------------------------------------------------------------------------------
        * Usuń przyrząd (plik) z listy
        * ------------------------------------------------------------------------------------
        */
        private void Przyrzady_Usun_Click(object sender, EventArgs e)
        {
            string filename="";

            if ((I00 & 32) == 0 && parametry_zaladowane && windowsCE)
            {
                MessageBox.Show("Brak zezwolenia na zmianę parametrów (ustaw kluczyk w odpowiednią pozycję).");
            }
            else if (Przyrzady_Lista.SelectedIndex > -1)
            {
                DialogResult dr = MessageBox.Show("Czy na pewno chcesz usunąć wybrany z listy przyrząd?", "Usuń", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);

                if (dr == DialogResult.Yes)
                {
                    try
                    {
                        if (windowsCE)
                        {
                            filename = @"\norflash\tools\" + Przyrzady_Lista.SelectedItem.ToString() + ".par.html";
                            File.Delete(filename);
                        }
                        else
                        {
                            filename = @"tools\" + Przyrzady_Lista.SelectedItem.ToString() + ".par.html";
                            File.Delete(filename);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Exception -> Przyrzady_Usun_Click(). " + ex.Message + "Parametr = " + filename);
                    }

                    ListaPrzyrzadow();
                }
            }
        }

        void MyTick(Object myObject, EventArgs myEventArgs)
        {
            dataczas = DateTime.Now;
            string str;

            str = dataczas.Month.ToString() + "-" + dataczas.Day.ToString().PadLeft(2, '0') + " " +
                    dataczas.Hour.ToString().PadLeft(2, '0') + ":" + dataczas.Minute.ToString().PadLeft(2, '0');// +":" + dataczas.Second.ToString().PadLeft(2, '0');

            if (connected)
            {
                this.Text = "APWELD   " + OstatnioUzywanyPrzyrzad + "   " + str;
            }
            else
            {
                this.Text = "APWELD - RESET SYSTEM!";
            }

        }
/*
 * ------------------------------------------------------------------------------------
 * Obsługa portu szeregowego
 * ------------------------------------------------------------------------------------
 */

        /*
         * ------------------------------------------------------------------------------------
         * Odczytanie dostępnych portów na komputerze
         * ------------------------------------------------------------------------------------
         */
        void GetAvailableComs()
        {
            try
            {
                comPorts = SerialPort.GetPortNames();
                Console.WriteLine("Dostępne porty COM:");
                foreach (string str in comPorts)
                {
                    Console.WriteLine(str);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception -> void GetAvailableComs()" + ex.Message);
            }

        }

        /*
         * ------------------------------------------------------------------------------------
         * Połączenie ze sterownikiem
         * ------------------------------------------------------------------------------------
         */
        void ConnectToController()
        {
            bool founded = false;

            port = new SerialPort();
            port.BaudRate = 115200;
            port.DtrEnable = true;
            port.WriteTimeout = 1000;
            port.ReadTimeout = 2000;
            port.NewLine = "\n";

            foreach (string portname in comPorts)
            {
                if (portname.IndexOf(OstatnioUzywanyCom) > -1)
                {
                    founded = true;
                }
            }

            if (!founded)
            {
                foreach (string name in comPorts)
                {
                    port.PortName = name;
                    if (!port.IsOpen)
                    {
                        try
                        {
                            port.Open();
                            Console.WriteLine(port.PortName + " otwarty");

                            port.WriteLine("\nTu Panel CE\n");

                            try
                            {
                                while (!connected)
                                {
                                    string line = port.ReadLine();
                                    Console.WriteLine(line);
                                    if (line.IndexOf("Tu sterowniczek") > -1)
                                    {
                                        founded = true;
                                        connected = true;
                                        OstatnioUzywanyCom = port.PortName;
                                        Console.WriteLine("Komunikacja nawiazana.");
                                    }
                                }
                            }
                            catch (TimeoutException)
                            {
                            }
                        }
                        catch (Exception ex)
                        {
                            connected = false;
                            MessageBox.Show("Exception -> void ConnectToController(). Nie znaleziono sterownika zgrzewania. Sprawdź czy jest prawidłowo podłączony. " + ex.Message);
                        }
                    }
                }
            }
            else if (OstatnioUzywanyCom != "NO")
            {
                port.PortName = OstatnioUzywanyCom;
                if (!port.IsOpen)
                {
                    try
                    {
                        port.Open();
                        Console.WriteLine(port.PortName + " otwarty");

                        port.WriteLine("Tu Panel CE");
                        try
                        {
                            while (!connected)
                            {
                                string line = port.ReadLine();
                                Console.WriteLine(line);
                                if (line.IndexOf("Tu sterowniczek") > -1)
                                {
                                    connected = true;
                                    Console.WriteLine("Komunikacja nawiazana.");
                                }
                            }
                        }
                        catch (TimeoutException ex)
                        {
                            connected = false;
                            MessageBox.Show("Exception -> void ConnectToController(). Nie znaleziono sterownika zgrzewania. Sprawdź czy jest prawidłowo podłączony. " + ex.Message);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Exception -> void ConnectToController(). Nie znaleziono sterownika zgrzewania. Sprawdź czy jest prawidłowo podłączony. " + ex.Message);
                    }
                }
            }

            // Zmieniamy timeouty na szybsze
            port.WriteTimeout = 500;
            port.ReadTimeout = 500;
        }

        /*
         * ------------------------------------------------------------------------------------
         * Zamyka wszystkie porty COM
         * ------------------------------------------------------------------------------------
         */
        void CloseAllPorts()
        {
            try
            {
                if (port.IsOpen)
                {
                    port.Close();
                    Console.WriteLine(port.PortName + " zamkniety");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception -> void CloseAllPorts(). " + ex.Message);
            }

            foreach (string portname in comPorts)
            {
                if (port.IsOpen)
                {
                    port.PortName = portname;
                    try
                    {
                        port.Close();
                        Console.WriteLine(port.PortName + " zamkniety");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Exception -> void CloseAllPorts(). " + ex.Message);
                    }
                }
            }
        }

        /*
         * ------------------------------------------------------------------------------------
         * Wątek odbioru danych przez rs-a
         * ------------------------------------------------------------------------------------
         */
        void SerialThread()
        {
            string line="";
            string[] param;

            // Czekamy aż nastąpi połączenie ze sterownikiem
            while (!connected) { };

            // Czyścimy cały bufor odbiorczy na początku
            port.DiscardInBuffer();

            while (connected)
            {
                try
                {
                    line = port.ReadLine();
                    Console.WriteLine(line);

                    param = line.Split(new Char[] { '=', ':' });

                    if (param[0] == "COM")
                    {
                        //Console.WriteLine("Odebrano: " + param[1] + param[2]);
                        OdbiorParametrowDelegate odbiorParametrow = new OdbiorParametrowDelegate(OdbiorParametrow);
                        this.Invoke(odbiorParametrow, param[1], param[2]);
                    }
                    else if (param[0] == "LOG")
                    {

                    }

                    if (io_get == true)
                    {
                        IOGet();
                        io_get = false;
                    }

                    Thread.Sleep(1);
                }
                catch (TimeoutException)
                {
                    // Tego nie przechwytujemy
                }
                catch (IOException ex)
                {
                    MessageBox.Show("IOException -> void SerialThread(). Sprawdź czy sterownik połączony jest z panelem. " + ex.Message);
                    connected = false;
                    break;
                }
                catch (ThreadAbortException ex)
                {
                    connected = false;
                    Console.WriteLine("ThreadAbortException -> void SerialThread(). " + ex.Message);
                }
                catch (IndexOutOfRangeException ex)
                {
                    port.DiscardInBuffer();
                    Console.WriteLine("IndexOutOfRangeException -> void SerialThread(). Problemy z komunikacją (nie poprawny format danych). " + ex.Message + "Parametr = " + line);
                }
                catch (Exception ex)
                {
                    connected = false;
                    MessageBox.Show("Exception -> void SerialThread(). Wyjątek nie obsługiwany. Wyłącz i włącz maszynę!" + ex.Message);
                }
            }
        }

        /*
         * ------------------------------------------------------------------------------------
         * Wysyła łańcuch przez rs-a
         * ------------------------------------------------------------------------------------
         */
        void Send(string str)
        {
            try
            {
                // Jezeli wywolywane przez inny wątek to musimy poczekac na zmienną
                while (sending) { };
                sending = true;
                port.WriteLine(str + "\n");
                sending = false;
            }
            catch (TimeoutException)
            {
                // Nie przechwytujemy
                sending = false;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception -> void Send(string str). " + ex.Message + "Parametr = " + str);
                sending = false;
            }
        }

        /*
         * ------------------------------------------------------------------------------------
         * Wysyła wszystkie parametry - robimy to przy ładowaniu przyrządu (parametrów)
         * ------------------------------------------------------------------------------------
         */
        void WyslijWszystkieParametry()
        {
            ParametrWyslij(Prad, 0);
            ParametrWyslij(Czas, 0);
            ParametrWyslij(CzWs, 0);
            ParametrWyslij(CzKo, 0);
            ParametrWyslij(StLi, 0);
            ParametrWyslij(StPr, 0);
            ParametrWyslij(ImLi, 0);
            ParametrWyslij(ImPa, 0);
            ParametrWyslij(Licz, 0);
            ParametrWyslij(LiPo, 0);
            ParametrWyslij(LiZm, 0);
            ParametrWyslij(LiEl, 0);

            PrSt.Text = "";
        }

        /*
         * ------------------------------------------------------------------------------------
         * Wysyła wartość parametru przez rs-a
         * ------------------------------------------------------------------------------------
         */
        void ParametrWyslijWartosc(TextBox tbox, int x)
        {
            try
            {
                Send("COM:" + tbox.Name + "=" + x.ToString());
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception -> void ParametrWyslij(TextBox tbox, int x). " + ex.Message + "Parametr = " + tbox.Text + " ma byc " + x.ToString());
            }

        }

        /*
         * ------------------------------------------------------------------------------------
         * Wysyła parametr przez rs-a z jednoczesną dekrementacją lub inkrementacją parametru
         * ------------------------------------------------------------------------------------
         */
        void ParametrWyslij(TextBox tbox, int x)
        {
            int val;
            string line="";

            if ((I00 & 32) == 0 && parametry_zaladowane && TabControl.SelectedIndex == 2 && windowsCE)
            {
                MessageBox.Show("Brak zezwolenia na zmianę parametrów (ustaw kluczyk w odpowiednią pozycję).");
            }
            else 
            {
                try
                {
                    val = int.Parse(tbox.Text);
                    val = val + x;
                    if (val >= 0)
                    {
                        //Console.WriteLine("Wysylam do sterownnika: " + "COM:" + tbox.Name + "=" + val.ToString());
                        line = "COM:" + tbox.Name + "=" + val.ToString();
                        Send(line);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception -> void ParametrWyslij(TextBox tbox, int x). " + ex.Message + "Parametr = " + tbox.Text + ";" + line);
                }
            }
        }

        /*
         * ------------------------------------------------------------------------------------
         * Wpisuje wartość do kontrolki TextBox i formatuje ją
         * Funkjca używana po odbiorze danych przez rs-a
         * ------------------------------------------------------------------------------------
         */
        void ParametrFormatuj(TextBox tbox, string str)
        {
            int val;
            string str2="";

            try
            {
                val = int.Parse(str);
                str2 = val.ToString().PadLeft(tbox.MaxLength, '0');
                tbox.Text = str2;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception -> void ParametrFormatuj(TextBox tbox, string str). " + ex.Message + "Parametr = " + tbox.Text + ";" + str + ";" + str2);
            }
        }

        /*
         * ------------------------------------------------------------------------------------
         * IOGet - Wysłanie rozkazów, które dadzą w odpowiedzi stan wejść/wyjść
         * ------------------------------------------------------------------------------------
         */

        void IOGet()
        {
            Send("COM:I00?");
            Send("COM:I01?");
            Send("COM:I02?");
            Send("COM:O00?");
        }

        /*
         * ------------------------------------------------------------------------------------
         * Lampka - ustawienie bitmapy w zależności od wartości w bajcie IO
         * ------------------------------------------------------------------------------------
         */

        void SetImage(PictureBox pbox, int val)
        {
            if (val > 0)
            {
                if (windowsCE)
                    pbox.Image = Resources.OnCE;
                else
                    pbox.Image = Resources.On;
            }
            else
            {
                if (windowsCE)
                    pbox.Image = Resources.OffCE;
                else
                    pbox.Image = Resources.Off;
            }

        }

        /*
         * ------------------------------------------------------------------------------------
         * Odbiór stanu we/wy
         * Ustawienie zmiennych + zobrazowanie na formatce
         * ------------------------------------------------------------------------------------
         */


        void IOUpdate(string io, string val)
        {
            try
            {
                int io_val = int.Parse(val);

                switch (io)
                {
                    case "I00":
                        I00 = io_val;
                        SetImage(I_00_1, io_val & 1);
                        SetImage(I_00_2, io_val & 2);
                        SetImage(I_00_4, io_val & 4);
                        SetImage(I_00_8, io_val & 8);
                        SetImage(I_00_16, io_val & 16);
                        SetImage(I_00_32, io_val & 32);
                        SetImage(I_00_64, io_val & 64);
                        SetImage(I_00_128, io_val & 128);
                        break;

                    case "I01":
                        I01 = io_val;
                        SetImage(I_01_1, io_val & 1);
                        SetImage(I_01_2, io_val & 2);
                        break;

                    case "I02":
                        I02 = io_val;
                        SetImage(I_02_1, io_val & 1);
                        SetImage(I_02_2, io_val & 2);
                        SetImage(I_02_4, io_val & 4);
                        break;

                    case "O00":
                        O00 = io_val;
                        SetImage(O_00_1, io_val & 1);
                        SetImage(O_00_2, io_val & 2);
                        SetImage(O_00_4, io_val & 4);
                        SetImage(O_00_8, io_val & 8);
                        SetImage(O_00_16, io_val & 16);
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception -> void IOUpdate(string io, string val). " + ex.Message + "io=" + io + " val=" + val);
                io_get = true;
            }

            if ((I00 & 32) == 0 && connected && windowsCE)
            {
                MinimizeBox = false;
            }
            else
                MinimizeBox = true;

        }

        /*
         * ------------------------------------------------------------------------------------
         * Delegata, którą wywołuje wątek odbioru danych przez rs-a
         * ------------------------------------------------------------------------------------
         */

        delegate void OdbiorParametrowDelegate(string par, string val);

        void OdbiorParametrow(string par, string val)
        {
            switch (par)
            {
                case "Prad":
                    ParametrFormatuj(Prad, val);
                    ZapiszParametry(Przyrzad_Aktywny.Text);
                    LogApp("Zmiana parametru " + Prad.Name + "=" + Prad.Text);
                    break;
                case "Czas":
                    ParametrFormatuj(Czas, val);
                    ZapiszParametry(Przyrzad_Aktywny.Text);
                    LogApp("Zmiana parametru " + Czas.Name + "=" + Czas.Text);
                    break;
                case "CzWs":
                    ParametrFormatuj(CzWs, val);
                    ZapiszParametry(Przyrzad_Aktywny.Text);
                    LogApp("Zmiana parametru " + CzWs.Name + "=" + CzWs.Text);
                    break;
                case "CzKo":
                    ParametrFormatuj(CzKo, val);
                    ZapiszParametry(Przyrzad_Aktywny.Text);
                    LogApp("Zmiana parametru " + CzKo.Name + "=" + CzKo.Text);
                    break;
                case "StLi":
                    ParametrFormatuj(StLi, val);
                    ZapiszParametry(Przyrzad_Aktywny.Text);
                    LogApp("Zmiana parametru " + StLi.Name + "=" + StLi.Text);
                    break;
                case "StPr":
                    ParametrFormatuj(StPr, val);
                    ZapiszParametry(Przyrzad_Aktywny.Text);
                    LogApp("Zmiana parametru " + StPr.Name + "=" + StPr.Text);
                    break;
                case "ImLi":
                    ParametrFormatuj(ImLi, val);
                    ZapiszParametry(Przyrzad_Aktywny.Text);
                    LogApp("Zmiana parametru " + ImLi.Name + "=" + ImLi.Text);
                    break;
                case "ImPa":
                    ParametrFormatuj(ImPa, val);
                    ZapiszParametry(Przyrzad_Aktywny.Text);
                    LogApp("Zmiana parametru " + ImPa.Name + "=" + ImPa.Text);
                    break;
                case "Licz":
                    ParametrFormatuj(Licz, val);
                    Statystyki();
                    ZapiszParametry(Przyrzad_Aktywny.Text);
                    break;
                case "LiPo":
                    ParametrFormatuj(LiPo, val);
                    Statystyki();
                    ZapiszParametry(Przyrzad_Aktywny.Text);
                    break;
                case "LiZm":
                    ParametrFormatuj(LiZm, val);
                    Statystyki();
                    ZapiszParametry(Przyrzad_Aktywny.Text);
                    break;
                case "LiEl":
                    ParametrFormatuj(LiEl, val);
                    Statystyki();
                    ZapiszParametry(Przyrzad_Aktywny.Text);
                    LogApp("Zmiana parametru " + LiEl.Name + "=" + LiEl.Text);
                    break;
                case "I00":
                    IOUpdate("I00", val);
                    break;
                case "I01":
                    IOUpdate("I01", val);
                    break;
                case "I02":
                    IOUpdate("I02", val);
                    break;
                case "O00":
                    IOUpdate("O00", val);
                    break;
                case "PrSt":
                    ParametrFormatuj(PrSt, val);
                    break;
            }
            
        }

        /*
         * ------------------------------------------------------------------------------------
         * Krótkie zdarzenia do edycji pól na formatce
         * PLUSY I MINUSY
         * ------------------------------------------------------------------------------------
         */

        // ------------------------------------------------------------------------------------
        // Licz
        private void Licznik_Plus1(object sender, EventArgs e)
        {
            //Console.WriteLine("Wcisnieto plus 1");
            ParametrWyslij(Licz, 1);
        }

        private void Licznik_Plus10(object sender, EventArgs e)
        {
            ParametrWyslij(Licz, 10);
        }

        private void Licznik_Plus100(object sender, EventArgs e)
        {
            ParametrWyslij(Licz, 100);
        }

        private void Licznik_Plus1000(object sender, EventArgs e)
        {
            ParametrWyslij(Licz, 1000);
        }

        private void Licznik_Minus1(object sender, EventArgs e)
        {
            ParametrWyslij(Licz, -1);
        }

        private void Licznik_Minus10(object sender, EventArgs e)
        {
            ParametrWyslij(Licz, -10);
        }

        private void Licznik_Minus100(object sender, EventArgs e)
        {
            ParametrWyslij(Licz, -100);
        }

        private void Licznik_Minus1000(object sender, EventArgs e)
        {
            ParametrWyslij(Licz, -1000);
        }

        // ------------------------------------------------------------------------------------
        // LiPo
        private void LicznikPojemnik_Plus1(object sender, EventArgs e)
        {
            ParametrWyslij(LiPo, 1);
        }

        private void LicznikPojemnik_Plus10(object sender, EventArgs e)
        {
            ParametrWyslij(LiPo, 10);
        }

        private void LicznikPojemnik_Plus100(object sender, EventArgs e)
        {
            ParametrWyslij(LiPo, 100);
        }

        private void LicznikPojemnik_Plus1000(object sender, EventArgs e)
        {
            ParametrWyslij(LiPo, 1000);
        }

        private void LicznikPojemnik_Minus1(object sender, EventArgs e)
        {
            ParametrWyslij(LiPo, -1);
        }

        private void LicznikPojemnik_Minus10(object sender, EventArgs e)
        {
            ParametrWyslij(LiPo, -10);
        }

        private void LicznikPojemnik_Minus100(object sender, EventArgs e)
        {
            ParametrWyslij(LiPo, -100);
        }

        private void LicznikPojemnik_Minus1000(object sender, EventArgs e)
        {
            ParametrWyslij(LiPo, -1000);
        }

        // ------------------------------------------------------------------------------------
        // LiZm
        private void LicznikZmiana_Plus1(object sender, EventArgs e)
        {
            ParametrWyslij(LiZm, 1);
        }

        private void LicznikZmiana_Plus10(object sender, EventArgs e)
        {
            ParametrWyslij(LiZm, 10);
        }

        private void LicznikZmiana_Plus100(object sender, EventArgs e)
        {
            ParametrWyslij(LiZm, 100);
        }

        private void LicznikZmiana_Plus1000(object sender, EventArgs e)
        {
            ParametrWyslij(LiZm, 1000);
        }

        private void LicznikZmiana_Minus1(object sender, EventArgs e)
        {
            ParametrWyslij(LiZm, -1);
        }

        private void LicznikZmiana_Minus10(object sender, EventArgs e)
        {
            ParametrWyslij(LiZm, -10);
        }

        private void LicznikZmiana_Minus100(object sender, EventArgs e)
        {
            ParametrWyslij(LiZm, -100);
        }

        private void LicznikZmiana_Minus1000(object sender, EventArgs e)
        {
            ParametrWyslij(LiZm, -1000);
        }

        // ------------------------------------------------------------------------------------
        // LiEl
        private void LicznikElektroda_Plus1(object sender, EventArgs e)
        {
            ParametrWyslij(LiEl, 1);
        }

        private void LicznikElektroda_Plus10(object sender, EventArgs e)
        {
            ParametrWyslij(LiEl, 10);
        }

        private void LicznikElektroda_Plus100(object sender, EventArgs e)
        {
            ParametrWyslij(LiEl, 100);
        }

        private void LicznikElektroda_Plus1000(object sender, EventArgs e)
        {
            ParametrWyslij(LiEl, 1000);
        }

        private void LicznikElektroda_Minus1(object sender, EventArgs e)
        {
            ParametrWyslij(LiEl, -1);
        }

        private void LicznikElektroda_Minus10(object sender, EventArgs e)
        {
            ParametrWyslij(LiEl, -10);
        }

        private void LicznikElektroda_Minus100(object sender, EventArgs e)
        {
            ParametrWyslij(LiEl, -100);
        }

        private void LicznikElektroda_Minus1000(object sender, EventArgs e)
        {
            ParametrWyslij(LiEl, -1000);
        }

        // ------------------------------------------------------------------------------------
        // Prad
        private void Parametr_Prad_Plus1_Click(object sender, EventArgs e)
        {
            ParametrWyslij(Prad, 1);
        }

        private void Parametr_Prad_Plus10_Click(object sender, EventArgs e)
        {
            ParametrWyslij(Prad, 10);
        }

        private void Parametr_Prad_Plus100_Click(object sender, EventArgs e)
        {
            ParametrWyslij(Prad, 100);
        }

        private void Parametr_Prad_Minus1_Click(object sender, EventArgs e)
        {
            ParametrWyslij(Prad, -1);
        }

        private void Parametr_Prad_Minus10_Click(object sender, EventArgs e)
        {
            ParametrWyslij(Prad, -10);
        }

        private void Parametr_Prad_Minus100_Click(object sender, EventArgs e)
        {
            ParametrWyslij(Prad, -100);
        }

        // ------------------------------------------------------------------------------------
        // Czas
        private void Parametr_CzasZgrz_Plus1_Click(object sender, EventArgs e)
        {
            ParametrWyslij(Czas, 1);
        }

        private void Parametr_CzasZgrz_Plus10_Click(object sender, EventArgs e)
        {
            ParametrWyslij(Czas, 10);
        }

        private void Parametr_CzasZgrz_Minus1_Click(object sender, EventArgs e)
        {
            ParametrWyslij(Czas, -1);
        }

        private void Parametr_CzasZgrz_Minus10_Click(object sender, EventArgs e)
        {
            ParametrWyslij(Czas, -10);
        }

        // ------------------------------------------------------------------------------------
        // CzWs
        private void Parametr_CzasDocWst_Plus1_Click(object sender, EventArgs e)
        {
            ParametrWyslij(CzWs, 1);
        }

        private void Parametr_CzasDocWst_Plus10_Click(object sender, EventArgs e)
        {
            ParametrWyslij(CzWs, 10);
        }

        private void Parametr_CzasDocWst_Minus1_Click(object sender, EventArgs e)
        {
            ParametrWyslij(CzWs, -1);
        }

        private void Parametr_CzasDocWst_Minus10_Click(object sender, EventArgs e)
        {
            ParametrWyslij(CzWs, -10);
        }

        // ------------------------------------------------------------------------------------
        // CzKo
        private void Parametr_CzasDocKon_Plus1_Click(object sender, EventArgs e)
        {
            ParametrWyslij(CzKo, 1);
        }

        private void Parametr_CzasDocKon_Plus10_Click(object sender, EventArgs e)
        {
            ParametrWyslij(CzKo, 10);
        }

        private void Parametr_CzasDocKon_Minus1_Click(object sender, EventArgs e)
        {
            ParametrWyslij(CzKo, -1);
        }

        private void Parametr_CzasDocKon_Minus10_Click(object sender, EventArgs e)
        {
            ParametrWyslij(CzKo, -10);
        }

        // ------------------------------------------------------------------------------------
        // StLi
        private void Parametr_StepperLicznik_Plus1_Click(object sender, EventArgs e)
        {
            ParametrWyslij(StLi, 1);
        }

        private void Parametr_StepperLicznik_Plus10_Click(object sender, EventArgs e)
        {
            ParametrWyslij(StLi, 10);
        }

        private void Parametr_StepperLicznik_Plus100_Click(object sender, EventArgs e)
        {
            ParametrWyslij(StLi, 100);
        }

        private void Parametr_StepperLicznik_Plus1000_Click(object sender, EventArgs e)
        {
            ParametrWyslij(StLi, 1000);
        }

        private void Parametr_StepperLicznik_Minus1_Click(object sender, EventArgs e)
        {
            ParametrWyslij(StLi, -1);
        }

        private void Parametr_StepperLicznik_Minus10_Click(object sender, EventArgs e)
        {
            ParametrWyslij(StLi, -10);
        }

        private void Parametr_StepperLicznik_Minus100_Click(object sender, EventArgs e)
        {
            ParametrWyslij(StLi, -100);
        }

        private void Parametr_StepperLicznik_Minus1000_Click(object sender, EventArgs e)
        {
            ParametrWyslij(StLi, -1000);
        }

        // ------------------------------------------------------------------------------------
        // StPr
        private void Parametr_StepperProcent_Plus1_Click(object sender, EventArgs e)
        {
            ParametrWyslij(StPr, 1);
        }

        private void Parametr_StepperProcent_Plus10_Click(object sender, EventArgs e)
        {
            ParametrWyslij(StPr, 10);
        }

        private void Parametr_StepperProcent_Plus100_Click(object sender, EventArgs e)
        {
            ParametrWyslij(StPr, 100);
        }

        private void Parametr_StepperProcent_Minus1_Click(object sender, EventArgs e)
        {
            ParametrWyslij(StPr, -1);
        }

        private void Parametr_StepperProcent_Minus10_Click(object sender, EventArgs e)
        {
            ParametrWyslij(StPr, -10);
        }

        private void Parametr_StepperProcent_Minus100_Click(object sender, EventArgs e)
        {
            ParametrWyslij(StPr, -100);
        }

        // ------------------------------------------------------------------------------------
        // ImLi
        private void Parametr_ImpulsyLiczba_Plus1_Click(object sender, EventArgs e)
        {
            ParametrWyslij(ImLi, 1);
        }

        private void Parametr_ImpulsyLiczba_Minus1_Click(object sender, EventArgs e)
        {
            ParametrWyslij(ImLi, -1);
        }

        // ------------------------------------------------------------------------------------
        // ImPa
        private void Parametr_ImpulsyPauza_Plus1_Click(object sender, EventArgs e)
        {
            ParametrWyslij(ImPa, 1);
        }

        private void Parametr_ImpulsyPauza_Plus10_Click(object sender, EventArgs e)
        {
            ParametrWyslij(ImPa, 10);
        }

        private void Parametr_ImpulsyPauza_Minus1_Click(object sender, EventArgs e)
        {
            ParametrWyslij(ImPa, -1);
        }

        private void Parametr_ImpulsyPauza_Minus10_Click(object sender, EventArgs e)
        {
            ParametrWyslij(ImPa, -10);
        }

        // ------------------------------------------------------------------------------------
        // Zerowanie liczników
        private void Licz_Zeruj_Click(object sender, EventArgs e)
        {
            ParametrWyslijWartosc(Licz, 0);
        }

        private void LiPo_Zeruj_Click(object sender, EventArgs e)
        {
            ParametrWyslijWartosc(LiPo, 0);
        }

        private void LiZm_Zeruj_Click(object sender, EventArgs e)
        {
            ParametrWyslijWartosc(LiZm, 0);
        }

        private void LiEl_Zeruj_Click(object sender, EventArgs e)
        {
            ParametrWyslijWartosc(LiEl, 0);
        }

        /*
         * ------------------------------------------------------------------------------------
         * Tutaj odświeżamy dane statystyczne (np.  z liczników)
         * ------------------------------------------------------------------------------------
         */
        void Statystyki()
        {
            try
            {
                if (int.Parse(LiPo.Text) > 0)
                {
                    PozostaloDoPojemnika_Value.Text = (int.Parse(LiPo.Text) - int.Parse(Licz.Text) / int.Parse(LiPo.Text)).ToString();
                    ZostaloPojemnikow_Value.Text = ((int.Parse(LiZm.Text) - int.Parse(Licz.Text)) / int.Parse(LiPo.Text)).ToString();
                }
                else
                {
                    PozostaloDoPojemnika_Value.Text = "0";
                    ZostaloPojemnikow_Value.Text = "0";
                }

                if (int.Parse(LiZm.Text) > 0)
                {
                    PozostaloNaZmianie_Value.Text = (int.Parse(LiZm.Text) - int.Parse(Licz.Text)).ToString();
                }
                else
                {
                    PozostaloNaZmianie_Value.Text = "0";
                }

                if (int.Parse(LiZm.Text) > 0)
                {
                    PozostaloDoWymianyElektrody_Value.Text = (int.Parse(LiEl.Text) - int.Parse(Licz.Text)).ToString();
                }
                else
                {
                    PozostaloDoWymianyElektrody_Value.Text = "0";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception -> void Statystyki(). " + ex.Message);
            }
        }

    }

}
