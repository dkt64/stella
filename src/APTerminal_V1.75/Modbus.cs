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

    public static class Modbus
    {
        public const UInt16 SM_CYLINDER_GORA_TOL = 15;

        const ushort MAX_MODBUSPACK = 512;
        const ushort MAX_MODBUSMESSAGE = 512;
        const byte PARSED_EMPTY = 0;
        const byte PARSED_ID = 1;
        const byte PARSED_FUNC = 2;
        const byte PARSED_ADDRESS = 3;
        const byte PARSED_QUANTITY = 4;
        const byte PARSED_BYTECOUNT = 5;
        const byte PARSED_DATA = 6;
        const byte PARSED_FULL = 10;

        public const byte WYKRES_TAB_SIZE = 100;

        public static byte slave_id = 2;
        public static byte master_id = 1;

        public static byte status; 			// status odebranej ramki
        public static ushort len; 				// aktualna ilość bajtów w tablicy buf
        static byte func;				// odebrana funkcja
        static ushort start_address;		// adres spod którego chcemy odczytac dane
        static ushort quantity;			// liczba rejestrów do odczytania
        static byte byte_count;			// liczba rejestrów do odczytania
        public static byte[] buf; 	            // dane
        static ushort crc_received;		// odebrany crc
        static ushort crc_computed;		// obliczony crc
        public static ushort crc_received_kopia;		// odebrany crc
        public static ushort crc_computed_kopia;		// obliczony crc
        static bool address_lo;		    // merker
        static bool quantity_lo;		// merker
        static bool crc_lo;			    // merker
        static byte[] message;	        // cała wiadomość do obliczenia crc
        static ushort lenm;				// index dla całej wiadomości
        public static ushort crc_err_counter;
        public static ushort crc_ok_counter;

        static byte req_id;				    // odebrany id urządzenia
        // static byte req_func;               // funkcja którą wysłamy
        static ushort req_start_address;		// adres spod którego chcemy odczytac dane
        static ushort req_quantity;			// liczba rejestrów do odczytania
        //static byte req_byte_count;			// liczba rejestrów do zapisu
        static byte[] req_buf; 	            // dane
        static ushort req_crc_computed;		// obliczony crc
        static byte[] req_message;	        // cała wiadomość do wysłania
        static ushort req_lenm;				// index dla całej wiadomości

        public static int txCount = 0;
        public static int rxCount = 0;
        public static int rxCount_last = 0;

        public static long respTimeMin = (long)ushort.MaxValue;
        public static long respTimeMax = 0;
        public static bool txCountZero = true;
        public static bool rxCountZero = true;
        public static bool respTimeMaxZero = true;
        public static bool respTimeMinZero = true;
        public static bool crcCountZero = true;

        public static int counter_tx_param = 0;
        public static int counter_rx_param = 0;
        public static int counter_rx_param_total = 0;

        //unsafe public static IOImageBuf iobuf;
        public static IOImage io;
        public static IOImage io_def;
        //unsafe public static ParametryStruct param_buf;

        public static bool got_answer_0x03 = false;
        public static bool got_answer_0x10 = false;
        public static int lostframes_0x03 = 0;
        public static int lostframes_0x10 = 0;

        public static ushort size_of_io;
        public static ushort size_of_param;

        public static bool CommunicationOK = false;

        public static bool odswierz_program = false;

        static byte last_prog_nr_sent;

        //
        // wykonaj rozkaz znaczy wyslij rozkaz do sterownika
        //
        public const byte PRZESUNIECIE_SM = 4;
        public static ushort rozkaz = 0;
        public static ushort rozkaz_arg = 0;

        //
        // Informacje o wysłanych requestach
        //

        static bool reqmem_ploter_wzor = false;
        static bool reqmem_ploter_prad = false;
        static bool reqmem_ploter_napiecie = false;
        static bool reqmem_ploter_wtopienie = false;

        static bool reqmem_odczyt_io = false;
        static bool reqmem_odczyt_programu = false;

        public static int licznik_ploter = 0;
        public static int licznik_io_0x03 = 0;
        public static int licznik_io_0x10 = 0;
        public static int licznik_program = 0;

        //
        // Licznik odebranych ramek do rozpoznania komunikacji
        //

        public static int frames = 0;

        /* 
         * =========================================================================================================================================================
         * Nazwa:           ModbusInit
         * 
         * Przeznaczenie:   Inicjacja tablic i wywolanie funkcji inicjujacych
         *                 
         * Parametry:       -
         * =========================================================================================================================================================
         */
        public static void ModbusInit()
        {
            buf = new byte[MAX_MODBUSPACK];
            message = new byte[MAX_MODBUSMESSAGE];
            req_buf = new byte[MAX_MODBUSPACK];
            req_message = new byte[MAX_MODBUSMESSAGE];
            ModbusInit_ReadPack();
            ModbusInit_ReqPack();
            io.Init();
            io_def.Init();

            io_def.sm[Modbus.SM_CYLINDER_GORA_TOL] = 100;   // 1mm tolerancji na górną pozycję
        }

        /* 
         * =========================================================================================================================================================
         * Nazwa:           ReqMemClear
         * 
         * Przeznaczenie:   Inicjacja zmiennych reqmem - konieczna przed kazdym requestem
         *                 
         * Parametry:       -
         * =========================================================================================================================================================
         */
        public static void ReqMemClear()
        {
            reqmem_odczyt_io = false;
            reqmem_odczyt_programu = false;
            reqmem_ploter_napiecie = false;
            reqmem_ploter_prad = false;
            reqmem_ploter_wtopienie = false;
            reqmem_ploter_wzor = false;
        }

        /* 
         * =========================================================================================================================================================
         * Nazwa:           ModbusInit_ReadPack
         * 
         * Przeznaczenie:   Inicjacja zmiennych w pakiecie uzywanym do odbierania
         *                 
         * Parametry:       -
         * =========================================================================================================================================================
         */
        public static void ModbusInit_ReadPack()
        {
            address_lo = false;
            byte_count = 0;
            crc_lo = false;
            crc_ok_counter = 0;
            func = 0;
            len = 0;
            lenm = 0;
            quantity = 0;
            quantity_lo = false;
            start_address = 0;
            status = PARSED_EMPTY;
            if (Serial.serial_port != null)
                Serial.serial_port.DiscardInBuffer();
        }

        /* 
         * =========================================================================================================================================================
         * Nazwa:           ModbusInit_ReqPack
         * 
         * Przeznaczenie:   Inicjacja zmiennych w pakiecie uzywanym do nadawania
         *                 
         * Parametry:       -
         * =========================================================================================================================================================
         */
        public static void ModbusInit_ReqPack()
        {
            req_id = 0;
            //req_byte_count = 0;
            req_crc_computed = 0;
            //req_func = 0;
            req_lenm = 0;
            req_quantity = 0;
            req_start_address = 0;

            address_lo = false;
            byte_count = 0;
            crc_lo = false;
            crc_ok_counter = 0;
            func = 0;
            len = 0;
            lenm = 0;
            quantity = 0;
            quantity_lo = false;
            start_address = 0;
            status = PARSED_EMPTY;
            ReqMemClear();
            if (Serial.serial_port != null)
                Serial.serial_port.DiscardInBuffer();
        }

        /* 
         * =========================================================================================================================================================
         * Nazwa:           ModbusOK
         * 
         * Przeznaczenie:   Porownania CRC odebranego z CRC obliczonym
         *                 
         * Parametry:       bool - true jezeli CRC sie zgadza
         * =========================================================================================================================================================
         */
        public static bool ModbusOK()
        {
            if (crc_received == crc_computed)
                return true;
            else
                return false;
        }

        /* 
         * =========================================================================================================================================================
         * Nazwa:           ModbusCRC_Read
         * 
         * Przeznaczenie:   Obliczenie CRC w pakiecie odbiorczym i wpisanie wartosci do crc_computed
         *                 
         * Parametry:       -
         * =========================================================================================================================================================
         */
        public static void ModbusCRC_Read()
        {
            ushort CRCFull = 0xFFFF;
            byte CRCLSB;
            int i, j;

            for (i = 0; i < lenm; i++)
            {
                CRCFull = (ushort)(CRCFull ^ message[i]);

                for (j = 0; j < 8; j++)
                {
                    CRCLSB = (byte)(CRCFull & 0x0001);
                    CRCFull = (ushort)((CRCFull >> 1) & 0x7FFF);

                    if (CRCLSB == 1)
                        CRCFull = (ushort)(CRCFull ^ 0xA001);
                }
            }

            crc_computed = CRCFull;
        }

        /* 
         * =========================================================================================================================================================
         * Nazwa:           ModbusCRC_Req
         * 
         * Przeznaczenie:   Obliczenie CRC w pakiecie nadawczym i wpisanie wartosci do req_crc_computed
         *                 
         * Parametry:       -
         * =========================================================================================================================================================
         */
        public static void ModbusCRC_Req()
        {
            ushort CRCFull = 0xFFFF;
            byte CRCLSB;
            int i, j;

            for (i = 0; i < req_lenm; i++)
            {
                CRCFull = (ushort)(CRCFull ^ req_message[i]);

                for (j = 0; j < 8; j++)
                {
                    CRCLSB = (byte)(CRCFull & 0x0001);
                    CRCFull = (ushort)((CRCFull >> 1) & 0x7FFF);

                    if (CRCLSB == 1)
                        CRCFull = (ushort)(CRCFull ^ 0xA001);
                }
            }

            req_crc_computed = CRCFull;
        }

        /* 
         * =========================================================================================================================================================
         * Odczyty wykresów
         * =========================================================================================================================================================
         */
        public static void ModbusRequest_OdczytWykresuWzor()
        {
            if (Serial.connected)
            {
                ModbusInit_ReqPack();

                req_message[req_lenm++] = req_id = slave_id;
                req_message[req_lenm++] = 0x03;

                req_start_address = (ushort)(0x5000);
                req_message[req_lenm++] = (byte)((req_start_address & 0xff00) >> 8);
                req_message[req_lenm++] = (byte)(req_start_address & 0x00ff);

                req_quantity = (ushort)(Modbus.WYKRES_TAB_SIZE);
                req_message[req_lenm++] = (byte)((req_quantity & 0xff00) >> 8);
                req_message[req_lenm++] = (byte)(req_quantity & 0x00ff);

                ModbusCRC_Req();

                req_message[req_lenm++] = (byte)(req_crc_computed & 0x00ff);
                req_message[req_lenm++] = (byte)((req_crc_computed & 0xff00) >> 8);

                Serial.SendBytes(req_message, req_lenm);

                reqmem_ploter_wzor = true;
            }

        }

        public static void ModbusRequest_OdczytWykresuPradu()
        {
            if (Serial.connected)
            {
                ModbusInit_ReqPack();

                req_message[req_lenm++] = req_id = slave_id;
                req_message[req_lenm++] = 0x03;

                req_start_address = (ushort)(0x5100);
                req_message[req_lenm++] = (byte)((req_start_address & 0xff00) >> 8);
                req_message[req_lenm++] = (byte)(req_start_address & 0x00ff);

                req_quantity = (ushort)(Modbus.WYKRES_TAB_SIZE);
                req_message[req_lenm++] = (byte)((req_quantity & 0xff00) >> 8);
                req_message[req_lenm++] = (byte)(req_quantity & 0x00ff);

                ModbusCRC_Req();

                req_message[req_lenm++] = (byte)(req_crc_computed & 0x00ff);
                req_message[req_lenm++] = (byte)((req_crc_computed & 0xff00) >> 8);

                Serial.SendBytes(req_message, req_lenm);

                reqmem_ploter_prad = true;
            }

        }

        public static void ModbusRequest_OdczytWykresuNapiecie()
        {
            if (Serial.connected)
            {
                ModbusInit_ReqPack();

                req_message[req_lenm++] = req_id = slave_id;
                req_message[req_lenm++] = 0x03;

                req_start_address = (ushort)(0x5200);
                req_message[req_lenm++] = (byte)((req_start_address & 0xff00) >> 8);
                req_message[req_lenm++] = (byte)(req_start_address & 0x00ff);

                req_quantity = (ushort)(Modbus.WYKRES_TAB_SIZE);
                req_message[req_lenm++] = (byte)((req_quantity & 0xff00) >> 8);
                req_message[req_lenm++] = (byte)(req_quantity & 0x00ff);

                ModbusCRC_Req();

                req_message[req_lenm++] = (byte)(req_crc_computed & 0x00ff);
                req_message[req_lenm++] = (byte)((req_crc_computed & 0xff00) >> 8);

                Serial.SendBytes(req_message, req_lenm);

                reqmem_ploter_napiecie = true;
            }

        }

        public static void ModbusRequest_OdczytWykresuWtopienie()
        {
            if (Serial.connected)
            {
                ModbusInit_ReqPack();

                req_message[req_lenm++] = req_id = slave_id;
                req_message[req_lenm++] = 0x03;

                req_start_address = (ushort)(0x5300);
                req_message[req_lenm++] = (byte)((req_start_address & 0xff00) >> 8);
                req_message[req_lenm++] = (byte)(req_start_address & 0x00ff);

                req_quantity = (ushort)(Modbus.WYKRES_TAB_SIZE);
                req_message[req_lenm++] = (byte)((req_quantity & 0xff00) >> 8);
                req_message[req_lenm++] = (byte)(req_quantity & 0x00ff);

                ModbusCRC_Req();

                req_message[req_lenm++] = (byte)(req_crc_computed & 0x00ff);
                req_message[req_lenm++] = (byte)((req_crc_computed & 0xff00) >> 8);

                Serial.SendBytes(req_message, req_lenm);

                reqmem_ploter_wtopienie = true;
            }

        }

        /* 
         * =========================================================================================================================================================
         * Nazwa:           ModbusRequest_OdczytProgramu
         * 
         * Przeznaczenie:   Funkcja wysylajaca pelna ramke MODBUS do sterownika. Rozkaz 0x03, adres 0x1xxx - odczyt programu zgrzewania
         *                 
         * Parametry:       -
         * =========================================================================================================================================================
         */
        public static void ModbusRequest_OdczytProgramu(ushort prog_nr)
        {
            if (Serial.connected)
            {
                ModbusInit_ReqPack();

                req_message[req_lenm++] = req_id = slave_id;
                req_message[req_lenm++] = 0x03;

                req_start_address = (ushort)(0x1000 + 0x100 * prog_nr);
                req_message[req_lenm++] = (byte)((req_start_address & 0xff00) >> 8);
                req_message[req_lenm++] = (byte)(req_start_address & 0x00ff);

                req_quantity = (ushort)(Modbus.size_of_param / 2);
                req_message[req_lenm++] = (byte)((req_quantity & 0xff00) >> 8);
                req_message[req_lenm++] = (byte)(req_quantity & 0x00ff);

                ModbusCRC_Req();

                req_message[req_lenm++] = (byte)(req_crc_computed & 0x00ff);
                req_message[req_lenm++] = (byte)((req_crc_computed & 0xff00) >> 8);

                Serial.SendBytes(req_message, req_lenm);

                last_prog_nr_sent = (byte)prog_nr;

                reqmem_odczyt_programu = true;
                //Tools.Log("SEND 0x03 " + req_start_address.ToString() + " req_lenm = " + req_lenm.ToString());
            }

        }

        /* 
         * =========================================================================================================================================================
         * Nazwa:           ModbusRequest_OdczytObrazuIO
         * 
         * Przeznaczenie:   Funkcja wysylajaca pelna ramke MODBUS do sterownika. Rozkaz 0x03, adres 0x0 - odczyt obrazu IO
         *                 
         * Parametry:       -
         * =========================================================================================================================================================
         */
        public static void ModbusRequest_OdczytObrazuIO()
        {
            if (Serial.connected)
            {
                ModbusInit_ReqPack();

                req_message[req_lenm++] = req_id = slave_id;
                req_message[req_lenm++] = 0x03;

                req_start_address = 0x0;
                req_message[req_lenm++] = (byte)((req_start_address & 0xff00) >> 8);
                req_message[req_lenm++] = (byte)(req_start_address & 0x00ff);

                req_quantity = (ushort)(Modbus.size_of_io / 2);
                req_message[req_lenm++] = (byte)((req_quantity & 0xff00) >> 8);
                req_message[req_lenm++] = (byte)(req_quantity & 0x00ff);

                ModbusCRC_Req();

                req_message[req_lenm++] = (byte)(req_crc_computed & 0x00ff);
                req_message[req_lenm++] = (byte)((req_crc_computed & 0xff00) >> 8);

                Serial.SendBytes(req_message, req_lenm);

                reqmem_odczyt_io = true;
            }

        }

        /* 
         * =========================================================================================================================================================
         * Nazwa:           ModbusRequest_WyslanieObrazuWyjsc
         * 
         * Przeznaczenie:   Funkcja wysylajaca pelna ramke MODBUS do sterownika. Rozkaz 0x10, adres 0x0 - zapis do obrazu Wyjsc
         *                 
         * Parametry:       -
         * =========================================================================================================================================================
         */
        public static void ModbusRequest_WyslanieObrazuWyjsc()
        {
            if (Serial.connected)
            {
                ModbusInit_ReqPack();

                req_message[req_lenm++] = slave_id;     // ID
                req_message[req_lenm++] = 0x10;         // Funkcja 0x10

                // Start address
                req_message[req_lenm++] = 0;
                req_message[req_lenm++] = 0;

                // Quantity
                req_message[req_lenm++] = 0;
                req_message[req_lenm++] = 4;

                // Bytecount
                req_message[req_lenm++] = 8;

                for (int i = 0; i < 4; i++)
                {
                    req_message[req_lenm++] = (byte)(io.sm[i] & 0x00ff);
                    req_message[req_lenm++] = (byte)((io.sm[i] & 0xff00) >> 8);
                }

                //for (int i = 0; i < 4; i++)
                //    req_message[req_lenm++] = io.q[i];

                /*
                for (int i = 0; i < 4; i++)
                {
                    req_message[req_lenm++] = (byte)(io.aout[i] & 0x00ff);
                    req_message[req_lenm++] = (byte)((io.aout[i] & 0xff00) >> 8);
                }

                for (int i = 0; i < 8; i++)
                {
                    req_message[req_lenm++] = (byte)(io.sm[i] & 0x00ff);
                    req_message[req_lenm++] = (byte)((io.sm[i] & 0xff00) >> 8);
                }
                */

                ModbusCRC_Req();

                req_message[req_lenm++] = (byte)(req_crc_computed & 0x00ff);
                req_message[req_lenm++] = (byte)((req_crc_computed & 0xff00) >> 8);

                Serial.SendBytes(req_message, req_lenm);
            }
        }

        public static void ModbusRequest_WyslanieObrazuWyjsc_Full()
        {
            if (Serial.connected)
            {
                ModbusInit_ReqPack();

                req_message[req_lenm++] = slave_id;     // ID
                req_message[req_lenm++] = 0x10;         // Funkcja 0x10

                // Start address
                req_message[req_lenm++] = 0;
                req_message[req_lenm++] = 0;

                // Quantity
                req_message[req_lenm++] = 0;
                req_message[req_lenm++] = 4;

                // Bytecount
                req_message[req_lenm++] = 32;

                for (int i = 0; i < 16; i++)
                {
                    req_message[req_lenm++] = (byte)(io.sm[i] & 0x00ff);
                    req_message[req_lenm++] = (byte)((io.sm[i] & 0xff00) >> 8);
                }

                //for (int i = 0; i < 4; i++)
                //    req_message[req_lenm++] = io.q[i];

                /*
                for (int i = 0; i < 4; i++)
                {
                    req_message[req_lenm++] = (byte)(io.aout[i] & 0x00ff);
                    req_message[req_lenm++] = (byte)((io.aout[i] & 0xff00) >> 8);
                }

                for (int i = 0; i < 8; i++)
                {
                    req_message[req_lenm++] = (byte)(io.sm[i] & 0x00ff);
                    req_message[req_lenm++] = (byte)((io.sm[i] & 0xff00) >> 8);
                }
                */

                ModbusCRC_Req();

                req_message[req_lenm++] = (byte)(req_crc_computed & 0x00ff);
                req_message[req_lenm++] = (byte)((req_crc_computed & 0xff00) >> 8);

                Serial.SendBytes(req_message, req_lenm);
            }
        }

        /* 
         * =========================================================================================================================================================
         * Nazwa:           ModbusRequest_WyslanieProgramu
         * 
         * Przeznaczenie:   Funkcja wysylajaca pelna ramke MODBUS do sterownika. Rozkaz 0x10, adres 0x1000...0x1f00 - zapis parametrow zgrzewania
         *                 
         * Parametry:       Numer programu zgrzewania
         * =========================================================================================================================================================
         */
        public static void ModbusRequest_WyslanieProgramu(byte nr_prog)
        {
            if (Serial.connected)
            {

                ModbusInit_ReqPack();

                req_message[req_lenm++] = slave_id;     // ID
                req_message[req_lenm++] = 0x10;         // Funkcja 0x10

                // Start address
                req_start_address = (ushort)(0x1000 + (0x100 * nr_prog));
                req_message[req_lenm++] = (byte)((req_start_address & 0xff00) >> 8);
                req_message[req_lenm++] = (byte)(req_start_address & 0x00ff);

                // Quantity
                req_quantity = (ushort)(Modbus.size_of_param / 2);
                //req_quantity = (ushort)(System.Runtime.InteropServices.Marshal.SizeOf(Zarzadzanie.par.prog[0]) / 2);
                req_message[req_lenm++] = (byte)((req_quantity & 0xff00) >> 8);
                req_message[req_lenm++] = (byte)(req_quantity & 0x00ff);

                // Bytecount
                //req_quantity = (ushort)(System.Runtime.InteropServices.Marshal.SizeOf(Zarzadzanie.par.prog[0]));
                req_message[req_lenm++] = (byte)Modbus.size_of_param;

                //Tools.Log("Ilosc bajtow do wyslania = " + (byte)System.Runtime.InteropServices.Marshal.SizeOf(Zarzadzanie.par.prog[0]));

                //Tools.Log("Wartosc req_lenm przed skompletowaniem danych = " + req_lenm);

                // Parametry
                req_message[req_lenm++] = (byte)(Zarzadzanie.par.prog[nr_prog].PrePressTime);
                req_message[req_lenm++] = (byte)(Zarzadzanie.par.prog[nr_prog].PreWeldCurrent & 0x00ff);
                req_message[req_lenm++] = (byte)((Zarzadzanie.par.prog[nr_prog].PreWeldCurrent & 0xff00) >> 8);

                req_message[req_lenm++] = (byte)(Zarzadzanie.par.prog[nr_prog].PreWeldTime);
                req_message[req_lenm++] = (byte)(Zarzadzanie.par.prog[nr_prog].PreWeldPause);

                req_message[req_lenm++] = (byte)(Zarzadzanie.par.prog[nr_prog].WeldCurrent & 0x00ff);
                req_message[req_lenm++] = (byte)((Zarzadzanie.par.prog[nr_prog].WeldCurrent & 0xff00) >> 8);

                req_message[req_lenm++] = (byte)(Zarzadzanie.par.prog[nr_prog].WeldTime);

                req_message[req_lenm++] = (byte)(Zarzadzanie.par.prog[nr_prog].PostWeldPause);
                req_message[req_lenm++] = (byte)(Zarzadzanie.par.prog[nr_prog].PostWeldCurrent & 0x00ff);     // 10
                req_message[req_lenm++] = (byte)((Zarzadzanie.par.prog[nr_prog].PostWeldCurrent & 0xff00) >> 8);

                req_message[req_lenm++] = (byte)(Zarzadzanie.par.prog[nr_prog].PostWeldTime);

                req_message[req_lenm++] = (byte)(Zarzadzanie.par.prog[nr_prog].PostPressTime);

                req_message[req_lenm++] = (byte)(Zarzadzanie.par.prog[nr_prog].Impulses);
                req_message[req_lenm++] = (byte)(Zarzadzanie.par.prog[nr_prog].ImpulsesPause);

                req_message[req_lenm++] = (byte)(Zarzadzanie.par.prog[nr_prog].StepperPercent);
                req_message[req_lenm++] = (byte)(Zarzadzanie.par.prog[nr_prog].StepperCounter & 0x00ff);
                req_message[req_lenm++] = (byte)((Zarzadzanie.par.prog[nr_prog].StepperCounter & 0xff00) >> 8);

                req_message[req_lenm++] = (byte)(Zarzadzanie.par.prog[nr_prog].CylinderNumber);

                req_message[req_lenm++] = (byte)(Zarzadzanie.par.prog[nr_prog].CylinderPositionDown & 0x00ff);            // 20
                req_message[req_lenm++] = (byte)((Zarzadzanie.par.prog[nr_prog].CylinderPositionDown & 0xff00) >> 8);

                req_message[req_lenm++] = (byte)(Zarzadzanie.par.prog[nr_prog].CylinderPositionDownTolerance & 0x00ff);
                req_message[req_lenm++] = (byte)((Zarzadzanie.par.prog[nr_prog].CylinderPositionDownTolerance & 0xff00) >> 8);
                req_message[req_lenm++] = (byte)(Zarzadzanie.par.prog[nr_prog].Injection & 0x00ff);
                req_message[req_lenm++] = (byte)((Zarzadzanie.par.prog[nr_prog].Injection & 0xff00) >> 8);

                req_message[req_lenm++] = (byte)(Zarzadzanie.par.prog[nr_prog].PressureSet & 0x00ff);
                req_message[req_lenm++] = (byte)((Zarzadzanie.par.prog[nr_prog].PressureSet & 0xff00) >> 8);
                req_message[req_lenm++] = (byte)(Zarzadzanie.par.prog[nr_prog].PressureSwitch & 0x00ff);
                req_message[req_lenm++] = (byte)((Zarzadzanie.par.prog[nr_prog].PressureSwitch & 0xff00) >> 8);

                req_message[req_lenm++] = (byte)(Zarzadzanie.par.prog[nr_prog].ExhaustPressure & 0x00ff);
                req_message[req_lenm++] = (byte)((Zarzadzanie.par.prog[nr_prog].ExhaustPressure & 0xff00) >> 8);
                req_message[req_lenm++] = (byte)(Zarzadzanie.par.prog[nr_prog].ExhaustPressureTolerance & 0x00ff);
                req_message[req_lenm++] = (byte)((Zarzadzanie.par.prog[nr_prog].ExhaustPressureTolerance & 0xff00) >> 8);

                req_message[req_lenm++] = (byte)(Zarzadzanie.par.prog[nr_prog].Iref & 0x00ff);
                req_message[req_lenm++] = (byte)((Zarzadzanie.par.prog[nr_prog].Iref & 0xff00) >> 8);

                req_message[req_lenm++] = (byte)(Zarzadzanie.par.prog[nr_prog].IrefTolerance);
                req_message[req_lenm++] = (byte)(Zarzadzanie.par.prog[nr_prog].Iakt & 0x00ff);
                req_message[req_lenm++] = (byte)((Zarzadzanie.par.prog[nr_prog].Iakt & 0xff00) >> 8);

                req_message[req_lenm++] = (byte)(Zarzadzanie.par.prog[nr_prog].Uref & 0x00ff);
                req_message[req_lenm++] = (byte)((Zarzadzanie.par.prog[nr_prog].Uref & 0xff00) >> 8);
                req_message[req_lenm++] = (byte)(Zarzadzanie.par.prog[nr_prog].UrefTolerance);
                req_message[req_lenm++] = (byte)(Zarzadzanie.par.prog[nr_prog].Uakt & 0x00ff);
                req_message[req_lenm++] = (byte)((Zarzadzanie.par.prog[nr_prog].Uakt & 0xff00) >> 8);

                req_message[req_lenm++] = (byte)(Zarzadzanie.par.prog[nr_prog].Eref & 0x00ff);
                req_message[req_lenm++] = (byte)((Zarzadzanie.par.prog[nr_prog].Eref & 0xff00) >> 8);

                req_message[req_lenm++] = (byte)(Zarzadzanie.par.prog[nr_prog].ErefTolerance);
                req_message[req_lenm++] = (byte)(Zarzadzanie.par.prog[nr_prog].Eakt & 0x00ff);
                req_message[req_lenm++] = (byte)((Zarzadzanie.par.prog[nr_prog].Eakt & 0xff00) >> 8);

                req_message[req_lenm++] = (byte)(Zarzadzanie.par.prog[nr_prog].SensorsUpPositionConfig & 0x00ff);
                req_message[req_lenm++] = (byte)((Zarzadzanie.par.prog[nr_prog].SensorsUpPositionConfig & 0xff00) >> 8);
                req_message[req_lenm++] = (byte)(Zarzadzanie.par.prog[nr_prog].SensorsUpPositionSignals & 0x00ff);
                req_message[req_lenm++] = (byte)((Zarzadzanie.par.prog[nr_prog].SensorsUpPositionSignals & 0xff00) >> 8);
                req_message[req_lenm++] = (byte)(Zarzadzanie.par.prog[nr_prog].SensorsDownPositionConfig & 0x00ff);
                req_message[req_lenm++] = (byte)((Zarzadzanie.par.prog[nr_prog].SensorsDownPositionConfig & 0xff00) >> 8);
                req_message[req_lenm++] = (byte)(Zarzadzanie.par.prog[nr_prog].SensorsDownPositionSignals & 0x00ff);
                req_message[req_lenm++] = (byte)((Zarzadzanie.par.prog[nr_prog].SensorsDownPositionSignals & 0xff00) >> 8);

                req_message[req_lenm++] = (byte)(Zarzadzanie.par.prog[nr_prog].Valves & 0x00ff);
                req_message[req_lenm++] = (byte)((Zarzadzanie.par.prog[nr_prog].Valves & 0xff00) >> 8);

                req_message[req_lenm++] = (byte)(Zarzadzanie.par.prog[nr_prog].ProgramConfiguration & 0x00ff);
                req_message[req_lenm++] = (byte)((Zarzadzanie.par.prog[nr_prog].ProgramConfiguration & 0xff00) >> 8);

                ModbusCRC_Req();

                req_message[req_lenm++] = (byte)(req_crc_computed & 0x00ff);
                req_message[req_lenm++] = (byte)((req_crc_computed & 0xff00) >> 8);

                //Tools.Log("Wartosc req_lenm po korekcie wyslaniem = " + req_lenm);

                Serial.SendBytes(req_message, req_lenm);
                //Tools.Log("SEND 0x10 " + req_start_address.ToString() + " req_lenm = " + req_lenm.ToString());

                last_prog_nr_sent = nr_prog;
            }

        }

        /* 
         * =========================================================================================================================================================
         * Nazwa:           ModbusInput(byte newbyte)
         * 
         * Przeznaczenie:   Obsluga pakietu MODBUS. Wywolywana zawsze gdy pojawi sie nowy bajt w buforze RS-a
         *                 
         * Parametry:       Nowy bajt odczytany z bufora RS-a
         * =========================================================================================================================================================
         */
        public static void ModbusInput(byte newbyte)
        {
            switch (status)
            {
                case PARSED_DATA:
                    if (crc_lo == false)
                    {
                        crc_received = (ushort)(newbyte);
                        crc_lo = true;
                    }
                    else
                    {
                        crc_received |= (ushort)(newbyte << 8);
                        status = PARSED_FULL;
                    }
                    break;
                //*****************************************************************************

                case PARSED_BYTECOUNT:
                    switch (func)
                    {
                        case 0x03:
                            if (len < byte_count && len < MAX_MODBUSPACK)
                            {
                                message[lenm++] = newbyte;
                                buf[len++] = newbyte;
                            }
                            if (len >= byte_count || len >= MAX_MODBUSMESSAGE)
                            {
                                status = PARSED_DATA;
                            }
                            break;
                        default:
                            break;
                    }
                    break;
                //*****************************************************************************

                case PARSED_QUANTITY:
                    switch (func)
                    {
                        case 0x10:
                            message[lenm++] = newbyte;
                            byte_count = newbyte;
                            status = PARSED_BYTECOUNT;
                            break;
                        default:
                            break;
                    }
                    break;
                //*****************************************************************************

                case PARSED_ADDRESS:
                    switch (func)
                    {
                        case 0x10:
                            if (quantity_lo == false)
                            {
                                message[lenm++] = newbyte;
                                quantity = (ushort)(newbyte << 8);
                                quantity_lo = true;
                            }
                            else
                            {
                                message[lenm++] = newbyte;
                                quantity |= (ushort)(newbyte);
                                status = PARSED_DATA;
                            }
                            break;

                        case 0x03:
                            message[lenm++] = newbyte;
                            byte_count = (byte)newbyte;
                            status = PARSED_BYTECOUNT;
                            break;

                        default:
                            break;
                    }
                    break;
                //*****************************************************************************

                case PARSED_FUNC:
                    if (address_lo == false)
                    {
                        message[lenm++] = newbyte;
                        start_address = (ushort)(newbyte << 8);
                        address_lo = true;
                    }
                    else
                    {
                        message[lenm++] = newbyte;
                        start_address |= (ushort)(newbyte);
                        status = PARSED_ADDRESS;
                    }
                    break;
                //*****************************************************************************

                case PARSED_ID:
                    if (newbyte == 0x10)
                    {
                        message[lenm++] = newbyte;
                        func = newbyte;
                        status = PARSED_FUNC;
                    }
                    else if (newbyte == 0x03)
                    {
                        message[lenm++] = newbyte;
                        func = newbyte;
                        status = PARSED_ADDRESS;
                    }
                    else
                    {
                        status = PARSED_EMPTY;
                    }
                    break;
                //*****************************************************************************

                case PARSED_EMPTY:
                    if (newbyte == slave_id)
                    {
                        message[lenm++] = newbyte;
                        status = PARSED_ID;
                    }
                    break;
                //*****************************************************************************

                case PARSED_FULL:
                    len = 0;
                    lenm = 0;
                    status = PARSED_EMPTY;
                    break;

                default:

                    len = 0;
                    lenm = 0;
                    status = PARSED_EMPTY;
                    break;
            }

            if (len > MAX_MODBUSPACK || lenm > MAX_MODBUSMESSAGE)
            {
                len = 0;
                lenm = 0;
                status = PARSED_EMPTY;
            }
        }

        /* 
         * =========================================================================================================================================================
         * Nazwa:           ModbusService
         * 
         * Przeznaczenie:   Funkcja wywyolywana w osobnym watku do odczytu i reakcji na odebrane ramki MODBUS
         *                 
         * Parametry:       -
         * =========================================================================================================================================================
         */
        /*
        public static void ModbusService()
        {
            byte data;

            // Sprawdzamy czy jest cos w buforze
            if (Serial.serial_glowa != Serial.serial_ogon)
            {
                // Odczytaujemy bajt do zmiennej
                data = Serial.serial_bufor[Serial.serial_ogon++];

                if (Serial.serial_ogon >= Serial.SERIAL_BUFOR_SIZE)
                    Serial.serial_ogon = 0;

                // Obsluga wejscia danych modbus
                ModbusInput((byte)data);

                // Jezeli odebrano wszystko obsluga rozkazu modbus
                if (status == PARSED_FULL)
                {
                    // Obliczenie CRC
                    ModbusCRC_Read();

                    // Wyswietlenie wyników CRC
                    crc_computed_kopia = crc_computed;
                    crc_received_kopia = crc_received;

                    // Jezeli CRC sie zgadza to mozemy przygotowac odpowiedź
                    if (ModbusOK() == true)
                    {
                        crc_ok_counter++;
                        ModbusReadResponse();
                    }
                    else
                    {
                        // Jezeli suma kontrolna sie zgadza to zwikszamy licznik bledych odbiorow
                        crc_err_counter++;
                    }

                    // Inicjacja packietu modbus
                    status = PARSED_EMPTY;

                }

                if (status == PARSED_EMPTY)
                {
                    ModbusInit_ReadPack();
                }
            }
        }
        */

        /* 
         * =========================================================================================================================================================
         * Nazwa:           DataReceviedHandler
         * 
         * Przeznaczenie:   Funkcja odczytująca dane po serial porcie
         *                 
         * Parametry:       -
         * =========================================================================================================================================================
         */
        public static void DataReceviedHandler(object sender,
                        SerialDataReceivedEventArgs e)
        {
            //Thread.CurrentThread.Priority = ThreadPriority.Highest;

            int new_bytes, i, readed;

            SerialPort sp = (SerialPort)sender;
            new_bytes = sp.BytesToRead;

            if (new_bytes > 0)
            {
                readed = sp.Read(Serial.serial_small_bufor, 0, new_bytes);
                for (i = 0; i < readed; i++)
                {
                    Serial.serial_bufor[Serial.serial_glowa] = Serial.serial_small_bufor[i];
                    Serial.serial_glowa++;
                    if (Serial.serial_glowa >= Serial.SERIAL_BUFOR_SIZE)
                        Serial.serial_glowa = 0;
                }
            }
        }

        /* 
         * =========================================================================================================================================================
         * Nazwa:           Read
         * 
         * Przeznaczenie:   Funkcja odczytująca ramkę MOdbus z bufora Serial
         *                 
         * Parametry:       -
         * =========================================================================================================================================================
         */
        public static bool Read(byte code)
        {
            byte data;
            bool retval = false;
            //int new_bytes, i, readed;
            //byte counter = 0;


            /*
             * 
             * przenioslem do zdarzenia serial data recevied
             * 
            new_bytes = Serial.serial_port.BytesToRead;
            if ((new_bytes > 0) && Serial.connected)
            {
                readed = Serial.serial_port.Read(Serial.serial_small_bufor, 0, new_bytes);
                for (i = 0; i < readed; i++)
                {
                    Serial.serial_bufor[Serial.serial_glowa] = Serial.serial_small_bufor[i];
                    Serial.serial_glowa++;
                    if (Serial.serial_glowa >= Serial.SERIAL_BUFOR_SIZE)
                        Serial.serial_glowa = 0;
                }
            }
             */

            // Sprawdzamy czy jest cos w buforze
            if (Serial.serial_glowa != Serial.serial_ogon)
            {
                //while (Serial.serial_glowa != Serial.serial_ogon && (counter++ < 16))
                //{

                // Odczytaujemy bajt do zmiennej
                data = Serial.serial_bufor[Serial.serial_ogon++];

                if (Serial.serial_ogon >= Serial.SERIAL_BUFOR_SIZE)
                    Serial.serial_ogon = 0;

                // Obsluga wejscia danych modbus
                ModbusInput((byte)data);

                // Jezeli odebrano wszystko obsluga rozkazu modbus
                if (status == PARSED_FULL)
                {
                    frames++;

                    //Thread.Sleep(0);

                    //Serial.serial_port.DiscardInBuffer();
                    //Serial.serial_port.DiscardOutBuffer();

                    // Obliczenie CRC
                    ModbusCRC_Read();

                    // Wyswietlenie wyników CRC
                    crc_computed_kopia = crc_computed;
                    crc_received_kopia = crc_received;

                    // Jezeli CRC sie zgadza to mozemy przygotowac odpowiedź
                    if (ModbusOK() == true)
                    {
                        crc_ok_counter++;
                        ModbusReadResponse();

                        if (code == func)
                            retval = true;
                    }
                    else
                    {
                        // Jezeli suma kontrolna sie zgadza to zwikszamy licznik bledych odbiorow
                        crc_err_counter++;
                    }

                    // Inicjacja packietu modbus
                    status = PARSED_EMPTY;

                }

                if (status == PARSED_EMPTY)
                {
                    ModbusInit_ReadPack();
                }
                //}
            }

            if (Tools.windowsCE)
                Thread.Sleep(MainForm.THREAD_SLEEP_CE_MODBUS);
            else
                Thread.Sleep(MainForm.THREAD_SLEEP_PC_MODBUS);


            return retval;
        }
        /* 
         * =========================================================================================================================================================
         * Nazwa:           ModbusReadResponse
         * 
         * Przeznaczenie:   Funkcja w ktyrej nastepuje reakcja na odebranie ramki MODBUS
         *                 
         * Parametry:       -
         * =========================================================================================================================================================
         */
        public static void ModbusReadResponse()
        {

            if (func == 0x10)
            {
                licznik_io_0x10++;
            }
            else if (func == 0x03 && reqmem_odczyt_io && len == Modbus.size_of_io)
            {
                licznik_io_0x03++;
                reqmem_odczyt_io = false;
                io.CopyImageBufToImage();
            }
            else if (func == 0x03 && reqmem_odczyt_programu && len == Modbus.size_of_param)
            {
                licznik_program++;
                reqmem_odczyt_programu = false;
                Zarzadzanie.par.CopyModbusBufToParam(last_prog_nr_sent);
            }
            else if (func == 0x03 && reqmem_ploter_wzor == true)// && len != Modbus.size_of_param && len != Modbus.size_of_io)
            {
                reqmem_ploter_wzor = false;
                licznik_ploter++;
                Ploter.liczba_probek_wzor = byte_count;
                Ploter.KopiujDaneModbusWzor();
            }
            else if (func == 0x03 && reqmem_ploter_prad == true)// && len != Modbus.size_of_param && len != Modbus.size_of_io)
            {
                reqmem_ploter_prad = false;
                licznik_ploter++;
                Ploter.liczba_probek_prad = byte_count;
                Ploter.KopiujDaneModbusPrad();
            }
            else if (func == 0x03 && reqmem_ploter_napiecie == true)// && len != Modbus.size_of_param && len != Modbus.size_of_io)
            {
                reqmem_ploter_napiecie = false;
                licznik_ploter++;
                Ploter.liczba_probek_napiecie = byte_count;
                Ploter.KopiujDaneModbusNapiecie();
            }
            else if (func == 0x03 && reqmem_ploter_wtopienie == true)// && len != Modbus.size_of_param && len != Modbus.size_of_io)
            {
                reqmem_ploter_wtopienie = false;
                licznik_ploter++;
                Ploter.liczba_probek_wtopienie = byte_count;
                Ploter.KopiujDaneModbusWtopienie();
            }
        }

        /* 
         * =========================================================================================================================================================
         * Nazwa:           ModbusWaitAnswer0x03
         * 
         * Przeznaczenie:   Funkcja czekająca na potwierdzenie odbioru ramki 0x03
         *                 
         * Parametry:       -
         * =========================================================================================================================================================
         */
        static public bool ModbusWaitAck_0x03(int timeout)
        {
            bool retval = false;

            if (Serial.connected)
            {
                int time_start = Environment.TickCount;
                while (Environment.TickCount - time_start < timeout)
                {
                    //Thread.Sleep(0);

                    if (Modbus.Read(0x03))
                    {
                        retval = true;
                        break;
                    }
                }
            }
            else
                retval = true;

            return retval;
        }

        /* 
         * =========================================================================================================================================================
         * Nazwa:           ModbusWaitAck_0x10
         * 
         * Przeznaczenie:   Funkcja czekająca na potwierdzenie odbioru ramki 0x03
         *                 
         * Parametry:       -
         * =========================================================================================================================================================
         */
        static public bool ModbusWaitAck_0x10(int timeout)
        {
            bool retval = false;

            if (Serial.connected)
            {
                int time_start = Environment.TickCount;

                while (Environment.TickCount - time_start < timeout)
                {
                    //Thread.Sleep(0);

                    if (Modbus.Read(0x10))
                    {
                        retval = true;
                        break;
                    }
                }
            }
            else
                retval = true;

            return retval;

        }

        /* 
 * =========================================================================================================================================================
 * Nazwa:           Rozkaz(ushort code, ushort arg1)
 * 
 * Przeznaczenie:   Obsluga rozkazów
 *                  
 * Parametry:       -
 * =========================================================================================================================================================
 */
        /*
         * 	code = (byte) ((rozkaz & 0xff00) >> 8);
        	address = (byte) (rozkaz & 0xff);
         */
        public static void Rozkaz(ushort code, ushort arg1)
        {
            //if (Zarzadzanie.AktywacjaOK)
            //{

            rozkaz = code;
            rozkaz_arg = arg1;
            //}
            //else
            //{
            //    MessageBox.Show("Żaden przyrząd nie jest aktywny", "Błąd zmiany parametru");
            //}
        }

        public static void Rozkaz(ushort code)
        {
            //if (Zarzadzanie.AktywacjaOK)
            //{
            rozkaz = code;
            //}
            //else
            //{
            //    MessageBox.Show("Żaden przyrząd nie jest aktywny", "Błąd zmiany parametru");
            //}
        }
    }
}


