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

    /* 
     * =========================================================================================================================================================
     * Nazwa:           struct ParametryStruct
     * 
     * Przeznaczenie:   Struktura z parametrami zgrzewania. Z niej tworzymy tablice programow zgrzewania
     *                 
     * Parametry:       -
     * =========================================================================================================================================================
     */

    [StructLayout(LayoutKind.Sequential)]
    public struct ParametryStruct
    {
        public byte PrePressTime;               // 0

        public ushort PreWeldCurrent;        // 1
        public byte PreWeldTime;          // 3
        public byte PreWeldPause;         // 4

        public ushort WeldCurrent;                   // 5
        public byte WeldTime;                     // 7

        public byte PostWeldPause;         // 8   
        public ushort PostWeldCurrent;        // 9
        public byte PostWeldTime;          // 11

        public byte PostPressTime;               // 12

        public byte Impulses;                       // 13
        public byte ImpulsesPause;                       // 14

        public byte StepperPercent;                     // 15
        public ushort StepperCounter;                   // 16

        public byte CylinderNumber;                      // 18
        public ushort CylinderPositionDown;               // 19
        public ushort CylinderPositionDownTolerance;     // 21

        public ushort Injection;      // 23

        public ushort PressureSet;                  // 25
        public ushort PressureSwitch;              // 27

        public ushort ExhaustPressure;             // 29
        public ushort ExhaustPressureTolerance;          // 31

        public ushort Iref;                             // 33
        public byte IrefTolerance;                     // 35
        public ushort Iakt;                             // 36

        public ushort Uref;                             // 38
        public byte UrefTolerance;                     // 40
        public ushort Uakt;                             // 41

        public ushort Eref;                             // 43
        public byte ErefTolerance;                     // 45
        public ushort Eakt;                             // 46

        public ushort SensorsUpPositionConfig;              // 48
        public ushort SensorsUpPositionSignals;             // 50
        public ushort SensorsDownPositionConfig;                 // 52
        public ushort SensorsDownPositionSignals;                // 54
        public ushort Valves;                          // 56
        public ushort ProgramConfiguration;                           // 58
    }

    /*
     * NumerCylindra
     * bit0 - Cylinder główny					Q0.1
     * bit1 - Cylinder główny zwiększona siła	Q0.2
     * bit2 - Cylinder dodatkowy 1				Q2.4
     * bit3	- Cylinder dodatkowy 2				Q2.5
     * bit4 - Cylinder dodatkowy 3				Q2.6
     */
    /*
     * Konfig:
     * -    bit 0:
     *      1 -> przejscie do kolejnego programu bez puszczenia pulpitu 2-recznego
     *      0 -> przejscie do kolejnego programu z koniecznością puszczenia pulpitu 2-recznego
     */

    /* 
     * =========================================================================================================================================================
     * Nazwa:           struct MachineConfig
     * 
     * Przeznaczenie:   Struktura z parametrami maszyny. Tutaj sa stale do obliczen.
     *                 
     * Parametry:       -
     * =========================================================================================================================================================
     */
    /*
    public struct MachineConfig
    {
        ushort PozycjaCylindraGora;
        ushort Moc;		// moc trafa
        ushort Vpri;		// napiecie na uzwojeniu pierwotym
        ushort Vsec;		// napiecie na uzwojeniu wtornym
        ushort Rogowski;	// wartosc mV na 1kA (standardowo 150mV/1kA)
    }
    */

    /* 
     * =========================================================================================================================================================
     * Nazwa:           struct Parametry
     * 
     * Przeznaczenie:   Struktura z parametrami zgrzewania. Funkcje do obslugi tablic parametrow i sama tablica parametrow
     *                 
     * Parametry:       -
     * =========================================================================================================================================================
     */
    public struct Parametry
    {
        public ParametryStruct[] prog;

        /* 
         * =========================================================================================================================================================
         * Nazwa:           Init
         * 
         * Przeznaczenie:   Utworzenie tablicy z parametrami
         *                 
         * Parametry:       -
         * =========================================================================================================================================================
         */
        public void Init()
        {
            prog = new ParametryStruct[16];
        }

        // -------------------------------------------------------------------------------------------------------------------------------------------
        // Przepisanie danych z ramki Modbus do bufora z parametrami (za pomocą wskaznika)
        // Przepisanie z bufora do normalnej zmiennej
        // -------------------------------------------------------------------------------------------------------------------------------------------

        /* 
         * =========================================================================================================================================================
         * Nazwa:           CopyModbusBufToParam UNSAFE
         * 
         * Przeznaczenie:   Funkcja uzywana do kopiowania tablic 
         *                 
         * Parametry:       -
         * =========================================================================================================================================================
         */
        public void CopyModbusBufToParam(byte nr_prog)
        {

            prog[nr_prog].PrePressTime = Modbus.buf[0];
            prog[nr_prog].PreWeldCurrent = (ushort)(Modbus.buf[1] + (Modbus.buf[2] << 8));
            prog[nr_prog].PreWeldTime = Modbus.buf[3];
            prog[nr_prog].PreWeldPause = Modbus.buf[4];

            prog[nr_prog].WeldCurrent = (ushort)(Modbus.buf[5] + (Modbus.buf[6] << 8));
            prog[nr_prog].WeldTime = Modbus.buf[7];

            prog[nr_prog].PostWeldPause = Modbus.buf[8];
            prog[nr_prog].PostWeldCurrent = (ushort)(Modbus.buf[9] + (Modbus.buf[10] << 8));
            prog[nr_prog].PostWeldTime = Modbus.buf[11];

            prog[nr_prog].PostPressTime = Modbus.buf[12];

            prog[nr_prog].Impulses = Modbus.buf[13];
            prog[nr_prog].ImpulsesPause = Modbus.buf[14];
            prog[nr_prog].StepperPercent = Modbus.buf[15];
            prog[nr_prog].StepperCounter = (ushort)(Modbus.buf[16] + (Modbus.buf[17] << 8));

            prog[nr_prog].CylinderNumber = Modbus.buf[18];
            prog[nr_prog].CylinderPositionDown = (ushort)(Modbus.buf[19] + (Modbus.buf[20] << 8));
            prog[nr_prog].CylinderPositionDownTolerance = (ushort)(Modbus.buf[21] + (Modbus.buf[22] << 8));
            prog[nr_prog].Injection = (ushort)(Modbus.buf[23] + (Modbus.buf[24] << 8));
            prog[nr_prog].PressureSet = (ushort)(Modbus.buf[25] + (Modbus.buf[26] << 8));
            prog[nr_prog].PressureSwitch = (ushort)(Modbus.buf[27] + (Modbus.buf[28] << 8));
            prog[nr_prog].ExhaustPressure = (ushort)(Modbus.buf[29] + (Modbus.buf[30] << 8));
            prog[nr_prog].ExhaustPressureTolerance = (ushort)(Modbus.buf[31] + (Modbus.buf[32] << 8));


            prog[nr_prog].Iref = (ushort)(Modbus.buf[33] + (Modbus.buf[34] << 8));
            prog[nr_prog].IrefTolerance = Modbus.buf[35];
            prog[nr_prog].Iakt = (ushort)(Modbus.buf[36] + (Modbus.buf[37] << 8));

            prog[nr_prog].Uref = (ushort)(Modbus.buf[38] + (Modbus.buf[39] << 8));
            prog[nr_prog].UrefTolerance = Modbus.buf[40];
            prog[nr_prog].Uakt = (ushort)(Modbus.buf[41] + (Modbus.buf[42] << 8));

            prog[nr_prog].Eref = (ushort)(Modbus.buf[43] + (Modbus.buf[44] << 8));
            prog[nr_prog].ErefTolerance = Modbus.buf[45];
            prog[nr_prog].Eakt = (ushort)(Modbus.buf[46] + (Modbus.buf[47] << 8));

            prog[nr_prog].SensorsUpPositionConfig = (ushort)(Modbus.buf[48] + (Modbus.buf[49] << 8));
            prog[nr_prog].SensorsUpPositionSignals = (ushort)(Modbus.buf[50] + (Modbus.buf[51] << 8));
            prog[nr_prog].SensorsDownPositionConfig = (ushort)(Modbus.buf[52] + (Modbus.buf[53] << 8));
            prog[nr_prog].SensorsDownPositionSignals = (ushort)(Modbus.buf[54] + (Modbus.buf[55] << 8));
            prog[nr_prog].Valves = (ushort)(Modbus.buf[56] + (Modbus.buf[57] << 8));
            prog[nr_prog].ProgramConfiguration = (ushort)(Modbus.buf[58] + (Modbus.buf[59] << 8));


        }

    }
}
