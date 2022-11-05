using System;

using System.Collections.Generic;
using System.Text;

namespace APTerminal
{

    public struct plcvar
    {
        byte type;
        byte bajt;
        byte bit;

        const byte TYPE_NONE = 0;
        const byte TYPE_INPUT_BYTE = 1;
        const byte TYPE_INPUT_BIT = 2;
        const byte TYPE_SYSTEM_MERKER_WORD = 3;
        const byte TYPE_SYSTEM_MERKER_BIT = 4;
        const byte TYPE_OUTPUT_BYTE = 5;
        const byte TYPE_OUTPUT_BIT = 6;

        public bool Bit
        {
            get
            {
                bool retval = false;
                switch (type)
                {
                    case TYPE_INPUT_BIT:
                        retval = (Modbus.io.i[bajt] & (bit << 1)) > 0 ? true : false;
                        break;
                    case TYPE_SYSTEM_MERKER_BIT:
                        retval = (Modbus.io.sm[bajt] & (bit << 1)) > 0 ? true : false;
                        break;
                    case TYPE_OUTPUT_BIT:
                        retval = (Modbus.io.q[bajt] & (bit << 1)) > 0 ? true : false;
                        break;
                }
                return retval;
            }

            set
            {
                switch (type)
                {
                    case TYPE_SYSTEM_MERKER_BIT:
                        if (value)
                            Modbus.io.sm[bajt] |= (ushort)(1 << bit);
                        else
                            Modbus.io.sm[bajt] &= (ushort)(~((ushort)(1 << bit)));
                        break;
                    case TYPE_OUTPUT_BIT:
                        if (value)
                            Modbus.io.q[bajt] |= (byte)(1 << bit);
                        else
                            Modbus.io.q[bajt] &= (byte)(~((byte)(1 << bit)));
                        break;
                }

            }
        }

        public byte Byte
        {
            get
            {
                byte retval = 0;
                switch (type)
                {
                    case TYPE_INPUT_BYTE:
                        retval = Modbus.io.i[bajt];
                        break;
                    case TYPE_OUTPUT_BYTE:
                        retval = Modbus.io.q[bajt];
                        break;
                }
                return retval;
            }

            set
            {
                switch (type)
                {
                    case TYPE_OUTPUT_BYTE:
                        Modbus.io.q[bajt] = value;
                        break;
                }

            }
        }

        public ushort Word
        {
            get
            {
                ushort retval = 0;
                switch (type)
                {
                    case TYPE_SYSTEM_MERKER_WORD:
                        //Tools.Log(bajt.ToString());
                        retval = Modbus.io.sm[bajt];
                        break;
                }
                return retval;
            }

            set
            {
                switch (type)
                {
                    case TYPE_SYSTEM_MERKER_WORD:
                        Modbus.io.sm[bajt] = value;
                        break;
                }

            }
        }

        public string Name
        {
            set
            {
                Char[] znaki = new Char[] { '.' };

                string tempstr = "";
                string[] tabstr;

                if (value.IndexOf("IB") == 0)
                {
                    type = TYPE_INPUT_BYTE;
                    tempstr = value.Substring("IB".Length, value.Length - "IB".Length);
                    bajt = Convert.ToByte(tempstr);
                }
                else if (value.IndexOf("I") == 0)
                {
                    type = TYPE_INPUT_BIT;
                    tempstr = value.Substring("I".Length, value.Length - "I".Length);
                    tabstr = tempstr.Split(znaki);
                    bajt = Convert.ToByte(tabstr[0]);
                    bit = Convert.ToByte(tabstr[1]);
                }
                else if (value.IndexOf("SMW") == 0)
                {
                    type = TYPE_SYSTEM_MERKER_WORD;
                    tempstr = value.Substring("SMW".Length, value.Length - "SMW".Length);
                    bajt = Convert.ToByte(tempstr);
                }
                else if (value.IndexOf("SM") == 0)
                {
                    type = TYPE_SYSTEM_MERKER_BIT;
                    tempstr = value.Substring("SM".Length, value.Length - "SM".Length);
                    tabstr = tempstr.Split(znaki);
                    bajt = Convert.ToByte(tabstr[0]);
                    bit = Convert.ToByte(tabstr[1]);
                }
                else if (value.IndexOf("QB") == 0)
                {
                    type = TYPE_OUTPUT_BYTE;
                    tempstr = value.Substring("QB".Length, value.Length - "QB".Length);
                    bajt = Convert.ToByte(tempstr);
                }
                else if (value.IndexOf("Q") == 0)
                {
                    type = TYPE_OUTPUT_BIT;
                    tempstr = value.Substring("Q".Length, value.Length - "Q".Length);
                    tabstr = tempstr.Split(znaki);
                    bajt = Convert.ToByte(tabstr[0]);
                    bit = Convert.ToByte(tabstr[1]);
                }
            }
        }
    }

    /* 
     * =========================================================================================================================================================
     * Nazwa:           Klasa PLC
     * 
     * Przeznaczenie:   
     *                 
     * Parametry:       -
     * =========================================================================================================================================================
     */
    public static class PLC
    {
        /* 
        * =========================================================================================================================================================
        * Nazwa:           Zmienne PLC
        * 
        * Przeznaczenie:   
        *                 
        * Parametry:       -
        * =========================================================================================================================================================
        */

        /*
        //
        // ZMIENNE MODYFIKOWALNE W OBRAZIE SM
        //

        #define SM_ROZKAZ					0
        #define SM_ROZKAZ_ARG				1
        #define SM_ROZKAZ_RES1				2
        #define SM_ROZKAZ_RES2				3

        #define SM_LICZNIK					4	//przyrząd
        #define SM_LICZNIK_MAX				5	//przyrząd
        #define SM_LICZNIK_STEPPER			6	//przyrząd
        #define SM_LICZNIK_STEPPER_OST		7	//przyrząd
        #define SM_LICZNIK_STEPPER_MAX		8	//przyrząd
        #define SM_SLUZA					9	//przyrząd
        #define	SM_CYKL_LICZBA_PROG			10	//przyrząd
        #define SM_CYLINDER_GORA			11	//przyrząd
        #define	SM_STATUS					12	//przyrząd
        #define SM_REF_CNT_PRAD				13	//przyrząd
        #define	SM_REF_CNT_NAKR				14	//przyrząd
        #define	SM_POZ_WYJ					15	//przyrząd

        //
        // ZMIENNE TYLKO DO ODCZYTU - MODYFIKOWANE PRZEZ ROZKAZ
        //

        #define SM_CYKL_PROGRAM_AKT			16
        #define SM_OSTZGRZEW_NR_PROGRAMU	17
        #define SM_OSTZGRZEW_PRAD			18
        #define SM_OSTZGRZEW_POMIAR_PRAD	19
        #define SM_OSTZGRZEW_POMIAR_NAP		20
        #define SM_OSTZGRZEW_POMIAR_ENER	21
        #define SM_OSTZGRZEW_POMIAR_WTOP	22
        #define SM_PROGRAM_STAN				23

        #define SM_TEMPERATURA_UC			24
        #define SM_LICZNIK_PRZERWAN_SYNC	25
        #define SM_INDEX					26
        #define SM_KAT						27
        #define SM_WTOPIENIE_POCZATKOWE		28
        #define SM_VERSION					29

        #define SM_ERR0						30
        #define SM_ERR1						31
        */

        //
        // ZMIENNE BITOWE
        //
        public static plcvar zezwprog;
        const string NAME_ZEZWPROG = "I1.2";

        public static plcvar hardware_ok;
        const string NAME_HARDWARE_OK = "Q2.7";

        //
        // ZMIENNE ROZKAZU
        //
        public static plcvar rozkaz;
        const string NAME_SM_ROZKAZ = "SMW0";

        public static plcvar rozkaz_arg;
        const string NAME_SM_ROZKAZ_ARG = "SMW1";

        // 
        // ZMIENNE PRZYRZADU
        //

        public static plcvar licznik;
        const string NAME_SM_LICZNIK = "SMW4";

        public static plcvar licznik_max;
        const string NAME_SM_LICZNIK_MAX = "SMW5";

        public static plcvar licznik_stepper;
        const string NAME_SM_LICZNIK_STEPPER = "SMW6";

        public static plcvar licznik_stepper_ost;
        const string NAME_SM_LICZNIK_STEPPER_OST = "SMW7";

        public static plcvar licznik_stepper_max;
        const string NAME_SM_LICZNIK_STEPPER_MAX = "SMW8";

        public static plcvar sluza;
        const string NAME_SM_SLUZA = "SMW9";

        public static plcvar cykl_liczba_programow;
        const string NAME_SM_CYKL_LICZBA_PROG = "SMW10";

        //
        // ZMIENNE MEASZYNOWE - TYLKO DO OODCZYTU
        //

        public static plcvar cykl_program_akt;
        const string NAME_SM_CYKL_PROGRAM_AKT = "SMW16";

        public static plcvar ost_zgrzew_nr_programu;
        const string NAME_SM_OSTZGRZEW_NR_PROGRAMU = "SMW17";

        public static plcvar ost_zgrzew_prad;
        const string NAME_SM_OSTZGRZEW_PRAD = "SMW18";

        public static plcvar ost_zgrzew_pomiar_prad;
        const string NAME_SM_OSTZGRZEW_POMIAR_PRAD = "SMW19";

        public static plcvar ost_zgrzew_pomiar_nap;
        const string NAME_SM_OSTZGRZEW_POMIAR_NAP = "SMW20";

        public static plcvar ost_zgrzew_pomiar_energii;
        const string NAME_SM_OSTZGRZEW_POMIAR_ENER = "SMW21";

        public static plcvar ost_zgrzew_pomiar_wtopienia;
        const string NAME_SM_OSTZGRZEW_POMIAR_WTOP = "SMW22";

        public static plcvar program_stan;
        const string NAME_SM_PROGRAM_STAN = "SMW23";

        public static plcvar temperatura_uc;
        const string NAME_SM_TEMPERATURA_UC = "SMW24";

        public static plcvar err0;
        const string NAME_SM_ERR0 = "SMW30";

        public static plcvar err1;
        const string NAME_SM_ERR1 = "SMW31";

        public static plcvar cylinder_gora;
        const string NAME_SM_CYLINDER_GORA = "SMW11";

        public static plcvar status;
        const string NAME_SM_STATUS = "SMW12";

        //
        // pozostale zmienne plc
        //
        public static bool init = true;

        public static plcvar kod_lo;
        const string NAME_IB_KOD_HI = "IB4";
        public static plcvar kod_hi;
        const string NAME_IB_KOD_LO = "IB3";

        // 
        // Kod przyrzadu
        //
        public static UInt16 kod_podlaczony = 0, kod_odczyt = 0, kod_odczyt_stary = 0, kod_cnt = 0;

        // 
        // Liczniki referenecji
        //

        public static plcvar ref_cnt_prad;
        const string NAME_SM_REF_CNT_PRAD = "SMW13";
        public static plcvar ref_cnt_nakr;
        const string NAME_SM_REF_CNT_NAKR = "SMW14";
        
        //
        // Tolerancja górnej pozycji cylindra
        //
        public static plcvar cylinder_gora_tol;
        const string NAME_SM_CYLINDER_GORA_TOL = "SMW15";

        //
        // Wersja firmware
        //

        public static plcvar firmware_version;
        const string NAME_SM_VERSION = "SMW29";

        /* 
         * =========================================================================================================================================================
         * Nazwa:           InitVars
         * 
         * Przeznaczenie:   Inicjacja zmiennych PLC
         *                 
         * Parametry:       -
         * =========================================================================================================================================================
         */
        public static void InitVars()
        {
            zezwprog.Name = NAME_ZEZWPROG;
            hardware_ok.Name = NAME_HARDWARE_OK;

            rozkaz.Name = NAME_SM_ROZKAZ;
            rozkaz_arg.Name = NAME_SM_ROZKAZ_ARG;

            licznik.Name = NAME_SM_LICZNIK;
            licznik_max.Name = NAME_SM_LICZNIK_MAX;
            licznik_stepper.Name = NAME_SM_LICZNIK_STEPPER;
            licznik_stepper_ost.Name = NAME_SM_LICZNIK_STEPPER_OST;
            licznik_stepper_max.Name = NAME_SM_LICZNIK_STEPPER_MAX;

            sluza.Name = NAME_SM_SLUZA;
            cykl_liczba_programow.Name = NAME_SM_CYKL_LICZBA_PROG;

            cykl_program_akt.Name = NAME_SM_CYKL_PROGRAM_AKT;
            ost_zgrzew_nr_programu.Name = NAME_SM_OSTZGRZEW_NR_PROGRAMU;
            ost_zgrzew_prad.Name = NAME_SM_OSTZGRZEW_PRAD;
            ost_zgrzew_pomiar_prad.Name = NAME_SM_OSTZGRZEW_POMIAR_PRAD;
            ost_zgrzew_pomiar_nap.Name = NAME_SM_OSTZGRZEW_POMIAR_NAP;
            ost_zgrzew_pomiar_energii.Name = NAME_SM_OSTZGRZEW_POMIAR_ENER;
            ost_zgrzew_pomiar_wtopienia.Name = NAME_SM_OSTZGRZEW_POMIAR_WTOP;
            program_stan.Name = NAME_SM_PROGRAM_STAN;
            err0.Name = NAME_SM_ERR0;
            err1.Name = NAME_SM_ERR1;
            temperatura_uc.Name = NAME_SM_TEMPERATURA_UC;
            cylinder_gora.Name = NAME_SM_CYLINDER_GORA;
            cylinder_gora_tol.Name = NAME_SM_CYLINDER_GORA_TOL;
            status.Name = NAME_SM_STATUS;

            kod_hi.Name = NAME_IB_KOD_HI;
            kod_lo.Name = NAME_IB_KOD_LO;

            ref_cnt_nakr.Name = NAME_SM_REF_CNT_NAKR;
            ref_cnt_prad.Name = NAME_SM_REF_CNT_PRAD;

            firmware_version.Name = NAME_SM_VERSION;
        }


        /* 
         * =========================================================================================================================================================
         * Nazwa:           PLC_Main
         * 
         * Przeznaczenie:   Główna funkcja programu PLC
         *                 
         * Parametry:       -
         * =========================================================================================================================================================
         */
        public static void PLC_Main()
        {
            kod_odczyt = (UInt16)(((kod_lo.Byte & 0xc0) >> 6) + (kod_hi.Byte << 2));
            
            if (kod_odczyt == kod_odczyt_stary)
            {
                if (kod_cnt > 5)
                {
                    kod_podlaczony = kod_odczyt;
                }
                else
                    kod_cnt++;
            }
            else
                kod_cnt = 0;

            kod_odczyt_stary = kod_odczyt;
        }
    }
}

