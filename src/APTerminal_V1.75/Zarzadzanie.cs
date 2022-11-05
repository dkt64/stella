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
using System.Xml.Serialization;

namespace APTerminal
{
    static class Zarzadzanie
    {
        public static Bitmap[] pics;

        public static Parametry par;
        public static string AktywnyPrzyrzad = ""; // bez kodu
        public static bool AktywacjaOK = false;
        public static string filenameToolState = ""; // bez kodu

        public static bool usunieto_aktywny = false;

        public static UInt16 kod_aktywny = 0;
        public static UInt16 kod_podlaczony = 0;

        /* 
         * =========================================================================================================================================================
         * Nazwa:           Init 
         * 
         * Przeznaczenie:   Inicjacja struktury z parametrami. Wywolanie funkcji Init w strukturze
         *                  
         * Parametry:       Obiekt formatki ktora wywoluje ta funkcje
         * =========================================================================================================================================================
         */
        public static void Init(Form _form)
        {
            par.Init();
        }

        /* 
         * =========================================================================================================================================================
         * Nazwa:           SerializujDoPlikuSMT
         * 
         * Przeznaczenie:   
         *                  
         * Parametry:       
         * =========================================================================================================================================================
         */
        public static void SerializujDoPlikuSMT()
        {
            StreamWriter sw = null;

            if (filenameToolState != "" && AktywacjaOK)
            {
                try
                {
                    XmlSerializer xmlsrl = new XmlSerializer(typeof(ushort[]));
                    sw = new StreamWriter(filenameToolState);
                    xmlsrl.Serialize(sw, Modbus.io.sm);
                }
                catch (Exception ex)
                {
                    Tools.LogEx(ex, "SerializujDoPlikuSMT()");
                }
                finally
                {
                    if (sw != null) sw.Close();
                }
            }
        }

        public static void SerializujDoPlikuSMTKopia(string filename)
        {
            StreamWriter sw = null;

            try
            {
                XmlSerializer xmlsrl = new XmlSerializer(typeof(ushort[]));
                sw = new StreamWriter(filename);
                xmlsrl.Serialize(sw, Modbus.io.sm);
            }
            catch (Exception ex)
            {
                Tools.LogEx(ex, "SerializujDoPlikuSMTKopia(string filename)");
            }
            finally
            {
                if (sw != null) sw.Close();
            }
        }

        public static void SerializujDoPlikuSMTDefault(string filename)
        {
            StreamWriter sw = null;

            try
            {
                XmlSerializer xmlsrl = new XmlSerializer(typeof(ushort[]));
                sw = new StreamWriter(filename);
                xmlsrl.Serialize(sw, Modbus.io_def.sm);
            }
            catch (Exception ex)
            {
                Tools.LogEx(ex, "SerializujDoPlikuSMTDefault(string filename)");
            }
            finally
            {
                if (sw != null) sw.Close();
            }
        }
        /* 
         * =========================================================================================================================================================
         * Nazwa:           SerializujZPlikuSMT
         * 
         * Przeznaczenie:   
         *                  
         * Parametry:      
         * =========================================================================================================================================================
         */
        public static void SerializujZPlikuSMT()
        {
            StreamReader sr = null;

            if (File.Exists(filenameToolState))
            {
                try
                {
                    XmlSerializer xmlsrl = new XmlSerializer(typeof(ushort[]));
                    sr = new StreamReader(filenameToolState);
                    Modbus.io.sm = (ushort[])xmlsrl.Deserialize(sr);
                }
                catch (Exception ex)
                {
                    Tools.LogEx(ex, "SerializujZPlikuSMT");
                }
                finally
                {
                    if (sr != null) sr.Close();

                }
            }
        }

        /* 
         * =========================================================================================================================================================
         * Nazwa:           SerializujDoPlikuSMM
         * 
         * Przeznaczenie:   
         *                  
         * Parametry:       
         * =========================================================================================================================================================
         */
        public static void SerializujDoPlikuSMM()
        {
            StreamWriter sw = null;

            try
            {
                XmlSerializer xmlsrl = new XmlSerializer(typeof(ushort[]));
                sw = new StreamWriter(Tools.filenameMachineState);
                xmlsrl.Serialize(sw, Modbus.io.sm);
            }
            catch (Exception ex)
            {
                Tools.LogEx(ex, "SerializujDoPlikuSMM");
            }
            finally
            {
                if (sw != null) sw.Close();
            }
        }

        /* 
         * =========================================================================================================================================================
         * Nazwa:           SerializujZPlikuSMM
         * 
         * Przeznaczenie:   
         *                  
         * Parametry:      
         * =========================================================================================================================================================
         */
        public static void SerializujZPlikuSMM()
        {
            StreamReader sr = null;

            if (File.Exists(Tools.filenameMachineState))
            {
                try
                {
                    XmlSerializer xmlsrl = new XmlSerializer(typeof(ushort[]));
                    sr = new StreamReader(Tools.filenameMachineState);
                    Modbus.io.sm = (ushort[])xmlsrl.Deserialize(sr);
                }
                catch (Exception ex)
                {
                    Tools.LogEx(ex, "SerializujZPlikuSMM");
                }
                finally
                {
                    if (sr != null) sr.Close();
                }
            }
        }

        /* 
         * =========================================================================================================================================================
         * Nazwa:           Transfer_ParametrZObrazu
         * 
         * Przeznaczenie:   Przetworzenie wpisanej wartosci w polu tekstowym i wpisanie do tablicy z parametrami
         *                  
         * Parametry:       Numer programu, parametr, wartosc
         * =========================================================================================================================================================
         */
        public static string Transfer_ParametrZObrazu(int prog_nr, string param)
        {
            string retval = "";

            try
            {
                switch (param)
                {
                    // --------------------------------
                    case "WeldCurrent":
                        retval = par.prog[prog_nr].WeldCurrent.ToString().PadLeft(5, '0');
                        break;

                    case "WeldTime":
                        retval = par.prog[prog_nr].WeldTime.ToString().PadLeft(5, '0');
                        break;

                    // --------------------------------
                    case "Impulses":
                        retval = par.prog[prog_nr].Impulses.ToString().PadLeft(5, '0');
                        break;

                    case "ImpulsesPause":
                        retval = par.prog[prog_nr].ImpulsesPause.ToString().PadLeft(5, '0');
                        break;

                    // --------------------------------
                    case "PrePressTime":
                        retval = par.prog[prog_nr].PrePressTime.ToString().PadLeft(5, '0');
                        break;

                    case "PreWeldCurrent":
                        retval = par.prog[prog_nr].PreWeldCurrent.ToString().PadLeft(5, '0');
                        break;

                    case "PreWeldTime":
                        retval = par.prog[prog_nr].PreWeldTime.ToString().PadLeft(5, '0');
                        break;

                    case "PreWeldPause":
                        retval = par.prog[prog_nr].PreWeldPause.ToString().PadLeft(5, '0');
                        break;

                    // --------------------------------
                    case "PostWeldPause":
                        retval = par.prog[prog_nr].PostWeldPause.ToString().PadLeft(5, '0');
                        break;

                    case "PostWeldCurrent":
                        retval = par.prog[prog_nr].PostWeldCurrent.ToString().PadLeft(5, '0');
                        break;

                    case "PostWeldTime":
                        retval = par.prog[prog_nr].PostWeldTime.ToString().PadLeft(5, '0');
                        break;

                    case "PostPressTime":
                        retval = par.prog[prog_nr].PostPressTime.ToString().PadLeft(5, '0');
                        break;

                    // --------------------------------
                    case "StepperPercent":
                        retval = par.prog[prog_nr].StepperPercent.ToString().PadLeft(5, '0');
                        break;

                    case "StepperCounter":
                        retval = par.prog[prog_nr].StepperCounter.ToString().PadLeft(5, '0');
                        break;

                    // --------------------------------
                    case "SensorsUpPositionConfig":
                        retval = par.prog[prog_nr].SensorsUpPositionConfig.ToString().PadLeft(5, '0');
                        break;

                    case "SensorsUpPositionSignals":
                        retval = par.prog[prog_nr].SensorsUpPositionSignals.ToString().PadLeft(5, '0');
                        break;

                    case "SensorsDownPositionConfig":
                        retval = par.prog[prog_nr].SensorsDownPositionConfig.ToString().PadLeft(5, '0');
                        break;

                    case "SensorsDownPositionSignals":
                        retval = par.prog[prog_nr].SensorsDownPositionSignals.ToString().PadLeft(5, '0');
                        break;

                    case "Valves":
                        retval = par.prog[prog_nr].Valves.ToString().PadLeft(5, '0');
                        break;

                    case "ProgramConfiguration":
                        retval = par.prog[prog_nr].ProgramConfiguration.ToString().PadLeft(5, '0');
                        break;

                    // --------------------------------
                    case "CylinderNumber":
                        retval = par.prog[prog_nr].CylinderNumber.ToString().PadLeft(5, '0');
                        break;

                    case "CylinderPositionDown":
                        retval = par.prog[prog_nr].CylinderPositionDown.ToString().PadLeft(5, '0');
                        break;

                    case "CylinderPositionDownTolerance":
                        retval = par.prog[prog_nr].CylinderPositionDownTolerance.ToString().PadLeft(5, '0');
                        break;

                    case "NutPenetration":
                        retval = par.prog[prog_nr].Injection.ToString().PadLeft(5, '0');
                        break;

                    case "PressureSet":
                        retval = par.prog[prog_nr].PressureSet.ToString().PadLeft(5, '0');
                        break;

                    case "PressureSwitch":
                        retval = par.prog[prog_nr].PressureSwitch.ToString().PadLeft(5, '0');
                        break;

                    case "ExhaustPressure":
                        retval = par.prog[prog_nr].ExhaustPressure.ToString().PadLeft(5, '0');
                        break;

                    case "ExhaustPressureTolerance":
                        retval = par.prog[prog_nr].ExhaustPressureTolerance.ToString().PadLeft(5, '0');
                        break;

                    // --------------------------------
                    case "Iref":
                        retval = par.prog[prog_nr].Iref.ToString().PadLeft(5, '0');
                        break;

                    case "IrefTolerance":
                        retval = par.prog[prog_nr].IrefTolerance.ToString().PadLeft(5, '0');
                        break;

                    case "Iakt":
                        retval = par.prog[prog_nr].Iakt.ToString().PadLeft(5, '0');
                        break;

                    // --------------------------------
                    case "Uref":
                        retval = par.prog[prog_nr].Uref.ToString().PadLeft(5, '0');
                        break;

                    case "UrefTolerance":
                        retval = par.prog[prog_nr].UrefTolerance.ToString().PadLeft(5, '0');
                        break;

                    case "Uakt":
                        retval = par.prog[prog_nr].Uakt.ToString().PadLeft(5, '0');
                        break;

                    case "Eref":
                        retval = par.prog[prog_nr].Eref.ToString().PadLeft(5, '0');
                        break;

                    case "ErefTolerance":
                        retval = par.prog[prog_nr].ErefTolerance.ToString().PadLeft(5, '0');
                        break;

                    case "Eakt":
                        retval = par.prog[prog_nr].Eakt.ToString().PadLeft(5, '0');
                        break;

                    // --------------------------------
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                Tools.LogEx(ex, "Transfer_ParametrZObrazu()");
            }

            return retval;
        }

        /* 
        * =========================================================================================================================================================
        * Nazwa:           NowyPrzyrzad
        * 
        * Przeznaczenie:   Utworzenie katalogu z przyrzadem i dodatkowymi podkatalogami.
        *                  
        * Parametry:       Nazwa przyrzadu
        * =========================================================================================================================================================
        */

        public static void NowyPrzyrzad(string toolname, int kod)
        {
            string tooldir = "";

            try
            {
                // Utworzenie katalogow
                if (!Directory.Exists(Tools.AppDirectory + "\\tools"))
                {
                    Directory.CreateDirectory(Tools.AppDirectory + "\\tools");
                }

                if (Directory.Exists(Tools.AppDirectory + "\\tools\\" + toolname + '.' + kod.ToString()))
                {
                    MessageBox.Show("Nazwa już istnieje");
                    return;
                }

                tooldir = Tools.AppDirectory + "\\tools\\" + toolname + '.' + kod.ToString();

                Directory.CreateDirectory(tooldir);
                Directory.CreateDirectory(tooldir + "\\prog");
                Directory.CreateDirectory(tooldir + "\\log");

                // Utworzenie plikow z parametrami
                for (byte i = 0; i < 16; i++)
                    Transfer_ProgramDoPlikuDefault(toolname, kod, i);

                SerializujDoPlikuSMTDefault(tooldir + "\\" + toolname + "_toolstate.xml");

                Tools.Log("Utworzono nowy przyrzad w katalogu " + tooldir);
            }
            catch (Exception ex)
            {
                Tools.LogEx(ex, "NowyPrzyrzad(string nazwa)");
            }
        }

        /* 
        * =========================================================================================================================================================
        * Nazwa:           NowyPrzyrzadKopia
        * 
        * Przeznaczenie:   Utworzenie katalogu z przyrzadem i dodatkowymi podkatalogami - kopia aktywnego
        *                  
        * Parametry:       Nazwa przyrzadu
        * =========================================================================================================================================================
        */

        public static void NowyPrzyrzadKopia(string toolname, int kod)
        {
            string tooldir = "";

            try
            {
                // Utworzenie katalogow
                if (!Directory.Exists(Tools.AppDirectory + "\\tools"))
                {
                    Directory.CreateDirectory(Tools.AppDirectory + "\\tools");
                }

                if (Directory.Exists(Tools.AppDirectory + "\\tools\\" + toolname + '.' + kod.ToString()))
                {
                    MessageBox.Show("Nazwa już istnieje");
                    return;
                }

                tooldir = Tools.AppDirectory + "\\tools\\" + toolname + '.' + kod.ToString();

                Directory.CreateDirectory(tooldir);
                Directory.CreateDirectory(tooldir + "\\prog");
                Directory.CreateDirectory(tooldir + "\\log");

                // Utworzenie plikow z parametrami
                for (byte i = 0; i < 16; i++)
                    Transfer_ProgramDoPliku(toolname, kod, i);

                SerializujDoPlikuSMTKopia(tooldir + "\\" + toolname + "_toolstate.xml");

                Tools.Log("Utworzono nowy przyrzad (kopia) w katalogu " + tooldir);
            }
            catch (Exception ex)
            {
                Tools.LogEx(ex, "NowyPrzyrzadKopia(string nazwa)");
            }
        }

        /* 
         * =========================================================================================================================================================
         * Nazwa:           Aktywacja
         * 
         * Przeznaczenie:   Zdefiniowanie nazwy narzedzia do aktywacji i wystartowanie jej.
         *                  
         * Parametry:       Nazwa przyrzadu
         * =========================================================================================================================================================
         */
        public static void Aktywacja(string toolname, int kod)
        {
            string dirname = Tools.AppDirectory + "\\tools\\" + toolname + "." + kod.ToString();

            if (Directory.Exists(dirname))
            {
                try
                {
                    //
                    // ***************************************
                    // Wysłanie 16-tu programów do sterownika
                    // ***************************************
                    //
                    for (byte i = 0; i < 16; i++)
                    {
                        Transfer_ProgramZPliku(toolname, kod, i);

                        // Pierwsza próba wysłania programu
                        Modbus.ModbusRequest_WyslanieProgramu(i);
                        if (!Modbus.ModbusWaitAck_0x10(MainForm.TIMEOUT_ZMIANA_PARAM))
                        {
                            // Druga próba wysłania programu
                            Thread.Sleep(20);
                            Modbus.ModbusRequest_WyslanieProgramu(i);
                            if (!Modbus.ModbusWaitAck_0x10(MainForm.TIMEOUT_ZMIANA_PARAM_2))
                            {
                                // TRzecia próba wysłania programu
                                Thread.Sleep(20);
                                Modbus.ModbusRequest_WyslanieProgramu(i);
                                Modbus.ModbusWaitAck_0x10(MainForm.TIMEOUT_ZMIANA_PARAM_2);
                            }
                        }

                        // PIerwsza próba odczytu programu
                        Modbus.ModbusRequest_OdczytProgramu(i);
                        if (!Modbus.ModbusWaitAck_0x03(MainForm.TIMEOUT_ZMIANA_PARAM))
                        {
                            // Druga próba odczytu programu
                            Thread.Sleep(20);
                            Modbus.ModbusRequest_OdczytProgramu(i);
                            Modbus.ModbusWaitAck_0x03(MainForm.TIMEOUT_ZMIANA_PARAM_2);
                        }

                        Thread.Sleep(5);
                    }

                    //
                    // ***************************************
                    // Odczyt obrazu IO z pliku XML
                    // ***************************************
                    //
                    filenameToolState = dirname + "\\" + toolname + "_toolstate.xml";
                    SerializujZPlikuSMT();

                    //
                    // ***************************************
                    // Wysłanie obrazu IO do sterownika
                    // ***************************************
                    //

                    // Pierwsza próba wysłania obrazu
                    Modbus.ModbusRequest_WyslanieObrazuWyjsc_Full();
                    if (!Modbus.ModbusWaitAck_0x10(MainForm.TIMEOUT_WYSYLA_OBRAZ))
                    {
                        // Druga próba
                        Thread.Sleep(20);
                        Modbus.ModbusRequest_WyslanieObrazuWyjsc_Full();
                        Modbus.ModbusWaitAck_0x10(MainForm.TIMEOUT_WYSYLA_OBRAZ);
                    }

                    //
                    // ***************************************
                    // Rozkaz 4-ty do sterownika o aktywacji
                    // ***************************************
                    //
                    Modbus.Rozkaz(4);

                    Zarzadzanie.usunieto_aktywny = false;

                    kod_aktywny = (ushort)kod;

                    Tools.WriteSetting("Tool", toolname);

                    //
                    // Zaladowanie bitmapek do tablicy
                    //
                    LoadPictures(toolname, kod);

                    Tools.Log("Aktywacja przyrządu " + toolname);
                }
                catch (Exception ex)
                {
                    Tools.LogEx(ex, "Aktywacja()");
                }
            }
        }

        /* 
         * =========================================================================================================================================================
         * Nazwa:           Usun
         * 
         * Przeznaczenie:   Usuniecie katalogu przyrzadu wraz z podkatalogami
         *                  
         * Parametry:       Nazwa przyrzadu
         * =========================================================================================================================================================
         */
        public static void Usun(string toolname)
        {
            string dirname = "";

            if (toolname.Contains("."))
                dirname = Tools.AppDirectory + "\\tools\\" + toolname;
            else
                dirname = Tools.AppDirectory + "\\tools\\" + toolname + ".0";

            if (Directory.Exists(dirname))
            {
                if (toolname == AktywnyPrzyrzad)
                {
                    AktywnyPrzyrzad = "";
                    AktywacjaOK = false;

                    Modbus.Rozkaz(6);
                    Tools.WriteSetting("Tool", "");

                    usunieto_aktywny = true;
                }

                try
                {
                    Directory.Delete(dirname, true);
                }
                catch (Exception)
                {
                }

            }
        }

        /* 
         * =========================================================================================================================================================
         * Nazwa:           Deaktywaacja
         * 
         * Przeznaczenie:   Deaktywacja przyrzadu. Wyslanie rozkazu do sterownika i wymazanie wpisu w ustawieniach
         * =========================================================================================================================================================
         */
        public static void Deaktywacja()
        {
            AktywnyPrzyrzad = "";
            AktywacjaOK = false;

            Modbus.Rozkaz(6);
            Tools.WriteSetting("Tool", "");

            usunieto_aktywny = true;
        }

        /* 
         * =========================================================================================================================================================
         * Nazwa:           Transfer_ProgramZPliku
         * 
         * Przeznaczenie:   Odczytanie programu (parametrow) z pliku - zapisuje wartosci w tablicy
         *                  
         * Parametry:       Nazwa przyrzadu, nume programu
         * =========================================================================================================================================================
         */
        public static void Transfer_ProgramZPliku(string toolname, int kod, byte nr)
        {
            string filename = Tools.AppDirectory + "\\tools\\" + toolname + '.' + kod.ToString() + "\\prog\\" + toolname + "_prog" + nr.ToString().PadLeft(2, '0') + "_param.html";

            if (toolname != null)
            {
                if (toolname != "")
                {
                    try
                    {
                        if (File.Exists(filename))
                        {
                            using (StreamReader sr = new StreamReader(filename))
                            {
                                string line;
                                Char[] znaki = new Char[] { '<', '>', '_', '=' };

                                while ((line = sr.ReadLine()) != null)
                                {
                                    string[] lines = line.Split(znaki);

                                    if (lines[3] == "PrePressTime")
                                        par.prog[nr].PrePressTime = Convert.ToByte(lines[4]);
                                    if (lines[3] == "PreWeldCurrent")
                                        par.prog[nr].PreWeldCurrent = Convert.ToUInt16(lines[4]);
                                    if (lines[3] == "PreWeldTime")
                                        par.prog[nr].PreWeldTime = Convert.ToByte(lines[4]);
                                    if (lines[3] == "PreWeldPause")
                                        par.prog[nr].PreWeldPause = Convert.ToByte(lines[4]);
                                    if (lines[3] == "WeldCurrent")
                                        par.prog[nr].WeldCurrent = Convert.ToUInt16(lines[4]);
                                    if (lines[3] == "WeldTime")
                                        par.prog[nr].WeldTime = Convert.ToByte(lines[4]);
                                    if (lines[3] == "PostWeldPause")
                                        par.prog[nr].PostWeldPause = Convert.ToByte(lines[4]);
                                    if (lines[3] == "PostWeldCurrent")
                                        par.prog[nr].PostWeldCurrent = Convert.ToUInt16(lines[4]);
                                    if (lines[3] == "PostWeldTime")
                                        par.prog[nr].PostWeldTime = Convert.ToByte(lines[4]);
                                    if (lines[3] == "PostPressTime")
                                        par.prog[nr].PostPressTime = Convert.ToByte(lines[4]);

                                    if (lines[3] == "Impulses")
                                        par.prog[nr].Impulses = Convert.ToByte(lines[4]);
                                    if (lines[3] == "ImpulsesPause")
                                        par.prog[nr].ImpulsesPause = Convert.ToByte(lines[4]);

                                    if (lines[3] == "StepperPercent")
                                        par.prog[nr].StepperPercent = Convert.ToByte(lines[4]);
                                    if (lines[3] == "StepperCounter")
                                        par.prog[nr].StepperCounter = Convert.ToUInt16(lines[4]);

                                    if (lines[3] == "CylinderNumber")
                                        par.prog[nr].CylinderNumber = Convert.ToByte(lines[4]);
                                    if (lines[3] == "CylinderPositionDown")
                                        par.prog[nr].CylinderPositionDown = Convert.ToUInt16(lines[4]);
                                    if (lines[3] == "CylinderPositionDownTolerance")
                                        par.prog[nr].CylinderPositionDownTolerance = Convert.ToUInt16(lines[4]);
                                    if (lines[3] == "NutPenetration")
                                        par.prog[nr].Injection = Convert.ToUInt16(lines[4]);

                                    if (lines[3] == "PressureSet")
                                        par.prog[nr].PressureSet = Convert.ToUInt16(lines[4]);
                                    if (lines[3] == "PressureSwitch")
                                        par.prog[nr].PressureSwitch = Convert.ToUInt16(lines[4]);

                                    if (lines[3] == "ExhaustPressure")
                                        par.prog[nr].ExhaustPressure = Convert.ToUInt16(lines[4]);
                                    if (lines[3] == "ExhaustPressureTolerance")
                                        par.prog[nr].ExhaustPressureTolerance = Convert.ToUInt16(lines[4]);

                                    if (lines[3] == "Iref")
                                        par.prog[nr].Iref = Convert.ToUInt16(lines[4]);
                                    if (lines[3] == "IrefTolerance")
                                        par.prog[nr].IrefTolerance = Convert.ToByte(lines[4]);
                                    if (lines[3] == "Iakt")
                                        par.prog[nr].Iakt = Convert.ToUInt16(lines[4]);

                                    if (lines[3] == "Uref")
                                        par.prog[nr].Uref = Convert.ToUInt16(lines[4]);
                                    if (lines[3] == "UrefTolerance")
                                        par.prog[nr].UrefTolerance = Convert.ToByte(lines[4]);
                                    if (lines[3] == "Uakt")
                                        par.prog[nr].Uakt = Convert.ToUInt16(lines[4]);

                                    if (lines[3] == "Eref")
                                        par.prog[nr].Eref = Convert.ToUInt16(lines[4]);
                                    if (lines[3] == "ErefTolerance")
                                        par.prog[nr].ErefTolerance = Convert.ToByte(lines[4]);
                                    if (lines[3] == "Eakt")
                                        par.prog[nr].Eakt = Convert.ToUInt16(lines[4]);

                                    if (lines[3] == "SensorsUpPositionConfig")
                                        par.prog[nr].SensorsUpPositionConfig = Convert.ToUInt16(lines[4]);
                                    if (lines[3] == "SensorsUpPositionSignals")
                                        par.prog[nr].SensorsUpPositionSignals = Convert.ToUInt16(lines[4]);
                                    if (lines[3] == "SensorsDownPositionConfig")
                                        par.prog[nr].SensorsDownPositionConfig = Convert.ToUInt16(lines[4]);
                                    if (lines[3] == "SensorsDownPositionSignals")
                                        par.prog[nr].SensorsDownPositionSignals = Convert.ToUInt16(lines[4]);
                                    if (lines[3] == "Valves")
                                        par.prog[nr].Valves = Convert.ToUInt16(lines[4]);
                                    if (lines[3] == "ProgramConfiguration")
                                        par.prog[nr].ProgramConfiguration = Convert.ToUInt16(lines[4]);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Tools.LogEx(ex, "Transfer_ProgramZPliku()");
                    }
                }
            }

        }

        /* 
         * =========================================================================================================================================================
         * Nazwa:           Transfer_ProgramDoPliku
         * 
         * Przeznaczenie:   Zapisanie programu (parametrow) do pliku - pobiera dane z tablicy i zapisuje w katalogu z narzedziem
         *                  
         * Parametry:       Nazwa przyrzadu, nume programu
         * =========================================================================================================================================================
         */
        public static void Transfer_ProgramDoPliku(string toolname, int kod, byte nr)
        {
            string filename = Tools.AppDirectory + "\\tools\\" + toolname + '.' + kod.ToString() + "\\prog\\" + toolname + "_prog" + nr.ToString().PadLeft(2, '0') + "_param.html";

            if (toolname != null)
            {
                if (toolname != "")
                {
                    try
                    {
                        using (StreamWriter sw = new StreamWriter(filename, false))
                        {
                            sw.WriteLine("<head><title>" + toolname + " welding parameters file - program nr " + nr.ToString().PadLeft(2, '0') + " </title> <meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\"/> </head><body><font face=\"verdana\" size=\"1\">");

                            sw.WriteLine("<p>Prog" + nr.ToString().PadLeft(2, '0') + "_PrePressTime=" + Zarzadzanie.par.prog[nr].PrePressTime.ToString() + "</p>");
                            sw.WriteLine("<p>Prog" + nr.ToString().PadLeft(2, '0') + "_PreWeldCurrent=" + Zarzadzanie.par.prog[nr].PreWeldCurrent.ToString() + "</p>");
                            sw.WriteLine("<p>Prog" + nr.ToString().PadLeft(2, '0') + "_PreWeldTime=" + Zarzadzanie.par.prog[nr].PreWeldTime.ToString() + "</p>");
                            sw.WriteLine("<p>Prog" + nr.ToString().PadLeft(2, '0') + "_PreWeldPause=" + Zarzadzanie.par.prog[nr].PreWeldPause.ToString() + "</p>");
                            sw.WriteLine("<p>Prog" + nr.ToString().PadLeft(2, '0') + "_WeldCurrent=" + Zarzadzanie.par.prog[nr].WeldCurrent.ToString() + "</p>");
                            sw.WriteLine("<p>Prog" + nr.ToString().PadLeft(2, '0') + "_WeldTime=" + Zarzadzanie.par.prog[nr].WeldTime.ToString() + "</p>");
                            sw.WriteLine("<p>Prog" + nr.ToString().PadLeft(2, '0') + "_PostWeldPause=" + Zarzadzanie.par.prog[nr].PostWeldPause.ToString() + "</p>");
                            sw.WriteLine("<p>Prog" + nr.ToString().PadLeft(2, '0') + "_PostWeldCurrent=" + Zarzadzanie.par.prog[nr].PostWeldCurrent.ToString() + "</p>");
                            sw.WriteLine("<p>Prog" + nr.ToString().PadLeft(2, '0') + "_PostWeldTime=" + Zarzadzanie.par.prog[nr].PostWeldTime.ToString() + "</p>");
                            sw.WriteLine("<p>Prog" + nr.ToString().PadLeft(2, '0') + "_PostPressTime=" + Zarzadzanie.par.prog[nr].PostPressTime.ToString() + "</p>");
                            sw.WriteLine("<p>Prog" + nr.ToString().PadLeft(2, '0') + "_Impulses=" + Zarzadzanie.par.prog[nr].Impulses.ToString() + "</p>");
                            sw.WriteLine("<p>Prog" + nr.ToString().PadLeft(2, '0') + "_ImpulsesPause=" + Zarzadzanie.par.prog[nr].ImpulsesPause.ToString() + "</p>");
                            sw.WriteLine("<p>Prog" + nr.ToString().PadLeft(2, '0') + "_StepperPercent=" + Zarzadzanie.par.prog[nr].StepperPercent.ToString() + "</p>");
                            sw.WriteLine("<p>Prog" + nr.ToString().PadLeft(2, '0') + "_StepperCounter=" + Zarzadzanie.par.prog[nr].StepperCounter.ToString() + "</p>");
                            sw.WriteLine("<p>Prog" + nr.ToString().PadLeft(2, '0') + "_CylinderNumber=" + Zarzadzanie.par.prog[nr].CylinderNumber.ToString() + "</p>");
                            sw.WriteLine("<p>Prog" + nr.ToString().PadLeft(2, '0') + "_CylinderPositionDown=" + Zarzadzanie.par.prog[nr].CylinderPositionDown.ToString() + "</p>");
                            sw.WriteLine("<p>Prog" + nr.ToString().PadLeft(2, '0') + "_CylinderPositionDownTolerance=" + Zarzadzanie.par.prog[nr].CylinderPositionDownTolerance.ToString() + "</p>");
                            sw.WriteLine("<p>Prog" + nr.ToString().PadLeft(2, '0') + "_NutPenetration=" + Zarzadzanie.par.prog[nr].Injection.ToString() + "</p>");

                            sw.WriteLine("<p>Prog" + nr.ToString().PadLeft(2, '0') + "_PressureSet=" + Zarzadzanie.par.prog[nr].PressureSet.ToString() + "</p>");
                            sw.WriteLine("<p>Prog" + nr.ToString().PadLeft(2, '0') + "_PressureSwitch=" + Zarzadzanie.par.prog[nr].PressureSwitch.ToString() + "</p>");
                            sw.WriteLine("<p>Prog" + nr.ToString().PadLeft(2, '0') + "_ExhaustPressure=" + Zarzadzanie.par.prog[nr].ExhaustPressure.ToString() + "</p>");
                            sw.WriteLine("<p>Prog" + nr.ToString().PadLeft(2, '0') + "_ExhaustPressureTolerance=" + Zarzadzanie.par.prog[nr].ExhaustPressureTolerance.ToString() + "</p>");
                            sw.WriteLine("<p>Prog" + nr.ToString().PadLeft(2, '0') + "_Iref=" + Zarzadzanie.par.prog[nr].Iref.ToString() + "</p>");
                            sw.WriteLine("<p>Prog" + nr.ToString().PadLeft(2, '0') + "_IrefTolerance=" + Zarzadzanie.par.prog[nr].IrefTolerance.ToString() + "</p>");
                            sw.WriteLine("<p>Prog" + nr.ToString().PadLeft(2, '0') + "_Iakt=" + Zarzadzanie.par.prog[nr].Iakt.ToString() + "</p>");
                            sw.WriteLine("<p>Prog" + nr.ToString().PadLeft(2, '0') + "_Uref=" + Zarzadzanie.par.prog[nr].Uref.ToString() + "</p>");
                            sw.WriteLine("<p>Prog" + nr.ToString().PadLeft(2, '0') + "_UrefTolerance=" + Zarzadzanie.par.prog[nr].UrefTolerance.ToString() + "</p>");
                            sw.WriteLine("<p>Prog" + nr.ToString().PadLeft(2, '0') + "_Uakt=" + Zarzadzanie.par.prog[nr].Uakt.ToString() + "</p>");
                            sw.WriteLine("<p>Prog" + nr.ToString().PadLeft(2, '0') + "_Eref=" + Zarzadzanie.par.prog[nr].Eref.ToString() + "</p>");
                            sw.WriteLine("<p>Prog" + nr.ToString().PadLeft(2, '0') + "_ErefTolerance=" + Zarzadzanie.par.prog[nr].ErefTolerance.ToString() + "</p>");
                            sw.WriteLine("<p>Prog" + nr.ToString().PadLeft(2, '0') + "_Eakt=" + Zarzadzanie.par.prog[nr].Eakt.ToString() + "</p>");

                            sw.WriteLine("<p>Prog" + nr.ToString().PadLeft(2, '0') + "_SensorsUpPositionConfig=" + Zarzadzanie.par.prog[nr].SensorsUpPositionConfig.ToString() + "</p>");
                            sw.WriteLine("<p>Prog" + nr.ToString().PadLeft(2, '0') + "_SensorsUpPositionSignals=" + Zarzadzanie.par.prog[nr].SensorsUpPositionSignals.ToString() + "</p>");
                            sw.WriteLine("<p>Prog" + nr.ToString().PadLeft(2, '0') + "_SensorsDownPositionConfig=" + Zarzadzanie.par.prog[nr].SensorsDownPositionConfig.ToString() + "</p>");
                            sw.WriteLine("<p>Prog" + nr.ToString().PadLeft(2, '0') + "_SensorsDownPositionSignals=" + Zarzadzanie.par.prog[nr].SensorsDownPositionSignals.ToString() + "</p>");
                            sw.WriteLine("<p>Prog" + nr.ToString().PadLeft(2, '0') + "_Valves=" + Zarzadzanie.par.prog[nr].Valves.ToString() + "</p>");
                            sw.WriteLine("<p>Prog" + nr.ToString().PadLeft(2, '0') + "_ProgramConfiguration=" + Zarzadzanie.par.prog[nr].ProgramConfiguration.ToString() + "</p>");
                        }
                    }
                    catch (Exception ex)
                    {
                        Tools.LogEx(ex, "Transfer_ProgramDoPliku() write. ");
                    }
                }
            }
        }

        /* 
         * =========================================================================================================================================================
         * Nazwa:           Transfer_ProgramDoPlikuDefault
         * 
         * Przeznaczenie:   Zapisanie programu (parametrow) do pliku - nie pobiera z tablicy parametrów
         *                  Wykorzystywana przy definiowaniu nowego przyrzadu
         *                  
         * Parametry:       Nazwa przyrzadu, nume programu
         * =========================================================================================================================================================
         */
        public static void Transfer_ProgramDoPlikuDefault(string toolname, int kod, byte nr)
        {
            string filename = Tools.AppDirectory + "\\tools\\" + toolname + '.' + kod.ToString() + "\\prog\\" + toolname + "_prog" + nr.ToString().PadLeft(2, '0') + "_param.html";

            if (toolname != null)
            {
                if (toolname != "")
                {
                    try
                    {
                        using (StreamWriter sw = new StreamWriter(filename, true))
                        {
                            sw.WriteLine("<head><title>" + toolname + " welding parameters file - program nr " + nr.ToString().PadLeft(2, '0') + " </title> <meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\"/> </head><body><font face=\"verdana\" size=\"1\">");
                            sw.WriteLine("<p>Prog" + nr.ToString().PadLeft(2, '0') + "_PrePressTime=20</p>");
                            sw.WriteLine("<p>Prog" + nr.ToString().PadLeft(2, '0') + "_PreWeldCurrent=0</p>");
                            sw.WriteLine("<p>Prog" + nr.ToString().PadLeft(2, '0') + "_PreWeldTime=0</p>");
                            sw.WriteLine("<p>Prog" + nr.ToString().PadLeft(2, '0') + "_PreWeldPause=0</p>");
                            sw.WriteLine("<p>Prog" + nr.ToString().PadLeft(2, '0') + "_WeldCurrent=0</p>");
                            sw.WriteLine("<p>Prog" + nr.ToString().PadLeft(2, '0') + "_WeldTime=0</p>");
                            sw.WriteLine("<p>Prog" + nr.ToString().PadLeft(2, '0') + "_PostWeldPause=0</p>");
                            sw.WriteLine("<p>Prog" + nr.ToString().PadLeft(2, '0') + "_PostWeldCurrent=0</p>");
                            sw.WriteLine("<p>Prog" + nr.ToString().PadLeft(2, '0') + "_PostWeldTime=0</p>");
                            sw.WriteLine("<p>Prog" + nr.ToString().PadLeft(2, '0') + "_PostPressTime=10</p>");
                            sw.WriteLine("<p>Prog" + nr.ToString().PadLeft(2, '0') + "_Impulses=1</p>");
                            sw.WriteLine("<p>Prog" + nr.ToString().PadLeft(2, '0') + "_ImpulsesPause=0</p>");
                            sw.WriteLine("<p>Prog" + nr.ToString().PadLeft(2, '0') + "_StepperPercent=0</p>");
                            sw.WriteLine("<p>Prog" + nr.ToString().PadLeft(2, '0') + "_StepperCounter=0</p>");
                            sw.WriteLine("<p>Prog" + nr.ToString().PadLeft(2, '0') + "_CylinderNumber=1</p>");
                            sw.WriteLine("<p>Prog" + nr.ToString().PadLeft(2, '0') + "_CylinderPositionDown=0</p>");
                            sw.WriteLine("<p>Prog" + nr.ToString().PadLeft(2, '0') + "_CylinderPositionDownTolerance=10</p>");

                            sw.WriteLine("<p>Prog" + nr.ToString().PadLeft(2, '0') + "_NutPenetration=0</p>");

                            sw.WriteLine("<p>Prog" + nr.ToString().PadLeft(2, '0') + "_PressureSet=0</p>");
                            sw.WriteLine("<p>Prog" + nr.ToString().PadLeft(2, '0') + "_PressureSwitch=0</p>");
                            sw.WriteLine("<p>Prog" + nr.ToString().PadLeft(2, '0') + "_ExhaustPressure=0</p>");
                            sw.WriteLine("<p>Prog" + nr.ToString().PadLeft(2, '0') + "_ExhaustPressureTolerance=50</p>");

                            sw.WriteLine("<p>Prog" + nr.ToString().PadLeft(2, '0') + "_Iref=0</p>");
                            sw.WriteLine("<p>Prog" + nr.ToString().PadLeft(2, '0') + "_IrefTolerance=10</p>");
                            sw.WriteLine("<p>Prog" + nr.ToString().PadLeft(2, '0') + "_Iakt=0</p>");
                            sw.WriteLine("<p>Prog" + nr.ToString().PadLeft(2, '0') + "_Uref=0</p>");
                            sw.WriteLine("<p>Prog" + nr.ToString().PadLeft(2, '0') + "_UrefTolerance=10</p>");
                            sw.WriteLine("<p>Prog" + nr.ToString().PadLeft(2, '0') + "_Uakt=0</p>");
                            sw.WriteLine("<p>Prog" + nr.ToString().PadLeft(2, '0') + "_Eref=0</p>");
                            sw.WriteLine("<p>Prog" + nr.ToString().PadLeft(2, '0') + "_ErefTolerance=20</p>");
                            sw.WriteLine("<p>Prog" + nr.ToString().PadLeft(2, '0') + "_Eakt=0</p>");

                            sw.WriteLine("<p>Prog" + nr.ToString().PadLeft(2, '0') + "_SensorsUpPositionConfig=0</p>");
                            sw.WriteLine("<p>Prog" + nr.ToString().PadLeft(2, '0') + "_SensorsUpPositionSignals=0</p>");
                            sw.WriteLine("<p>Prog" + nr.ToString().PadLeft(2, '0') + "_SensorsDownPositionConfig=0</p>");
                            sw.WriteLine("<p>Prog" + nr.ToString().PadLeft(2, '0') + "_SensorsDownPositionSignals=0</p>");
                            sw.WriteLine("<p>Prog" + nr.ToString().PadLeft(2, '0') + "_Valves=0</p>");
                            sw.WriteLine("<p>Prog" + nr.ToString().PadLeft(2, '0') + "_ProgramConfiguration=2</p>");
                        }
                    }
                    catch (Exception ex)
                    {
                        Tools.LogEx(ex, "Transfer_ProgramDoPlikuDefault() write. ");
                    }
                }
            }
        }

        /* 
         * =========================================================================================================================================================
         * Nazwa:           LoadPictures
         * 
         * Przeznaczenie:   Wczytanie bitmap dla danego przyrzadu
         *                  
         * Parametry:       Nazwa narzedzie
         * =========================================================================================================================================================
         */

        public static void LoadPictures(string toolname, int kod)
        {
            //
            // nazwa katalogu z lokalizacja plikow
            //
            string dirname = Tools.AppDirectory + "\\tools\\" + toolname + "." + kod.ToString() + "\\prog\\images\\";

            // 
            // Tworzenie bitmapy z pliku
            //

            pics = new Bitmap[16];

            try
            {
                pics[0] = new Bitmap(dirname + "\\tool.jpg");
                pics[1] = new Bitmap(dirname + "\\prog00.jpg");
                pics[2] = new Bitmap(dirname + "\\prog01.jpg");
                pics[3] = new Bitmap(dirname + "\\prog02.jpg");
                pics[4] = new Bitmap(dirname + "\\prog03.jpg");
                pics[5] = new Bitmap(dirname + "\\prog04.jpg");
                pics[6] = new Bitmap(dirname + "\\prog05.jpg");
                pics[7] = new Bitmap(dirname + "\\prog06.jpg");
                pics[8] = new Bitmap(dirname + "\\prog07.jpg");
                pics[9] = new Bitmap(dirname + "\\prog08.jpg");
                pics[10] = new Bitmap(dirname + "\\prog09.jpg");
                pics[11] = new Bitmap(dirname + "\\prog10.jpg");
                pics[12] = new Bitmap(dirname + "\\prog11.jpg");
                pics[13] = new Bitmap(dirname + "\\prog12.jpg");
                pics[14] = new Bitmap(dirname + "\\prog13.jpg");
                pics[15] = new Bitmap(dirname + "\\prog14.jpg");
                pics[16] = new Bitmap(dirname + "\\prog15.jpg");
            }
            catch (Exception)
            {
                // Jak by nie bylo plikow to nic nie robimy
            }

            // 
            // Wyswietlenie
            //

        }

    }
}
