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
     * Nazwa:           Struktura obrazu IOImage     
     *
     * Przeznaczenie:   Struktura uzywana jako obraz IO. Bezposreniio w niej deklarowane sa tablice do ktorej kopiowana jest wartosc IOIMageBuf podczas transmisji
     *                  oraz z niej przesylane sa dane podczas wysylania wyjsc do sterownika
     *                 
     * Parametry:       -
     * =========================================================================================================================================================
     */
    public struct IOImage
    {
        public ushort[] sm;
        public byte[] q;
        public ushort[] aout;
        public byte[] i;
        public ushort[] ain;

        public void Init()
        {
            sm = new ushort[32];
            q = new byte[4];
            aout = new ushort[4];
            i = new byte[6];
            ain = new ushort[6];
        }

        /* 
         * =========================================================================================================================================================
         * Nazwa:           CopyImageBufToImage     
         *
         * Przeznaczenie:   Funkcja wykorzystywana przy odczycie obrazu przez RS-a. Kopiowane sa dane z bufora Modbus bajt po bajcie do struktury ImageBuf
         *                  Nastepnie z ImageBuf (dostep przez wskaznik do bajtow) do IOImage (dostep normalny)
         *                 
         * Parametry:       -
         * =========================================================================================================================================================
         */
        public void CopyImageBufToImage()
        {
            ushort l, j = 0;

            for (l = 0; l < 32; l++)
            {
                sm[l] = (ushort)(Modbus.buf[j] + (Modbus.buf[j+1] << 8));
                j += 2;
            }

            for (l = 0; l < 4; l++)
                q[l] = Modbus.buf[j++];

            for (l = 0; l < 4; l++)
            {
                aout[l] = (ushort)(Modbus.buf[j] + (Modbus.buf[j + 1] << 8));
                j += 2;
            }

            for (l = 0; l < 6; l++)
                i[l] = Modbus.buf[j++];

            for (l = 0; l < 6; l++)
            {
                ain[l] = (ushort)(Modbus.buf[j] + (Modbus.buf[j + 1] << 8));
                j += 2;
            }

        }
    }

}

// ********************************************************************
// WEJSCIA I WYJŚCIA - OPIS
// ********************************************************************

/*
 * I00 [X4.01] [image.i[0] & 0x01] - Synchro
 * I01 [X4.02] [image.i[0] & 0x02] - START
 * I02 [X4.03] [image.i[0] & 0x04] - DK
 * I03 [X4.04] [image.i[0] & 0x08] - Wyblokowanie - zezwolenie
 * I04 [X4.05] [image.i[0] & 0x10] - RESET BLEDU ZGRZEWANIA
 * I05 [X4.06] [image.i[0] & 0x20] - NOTAUS (Info)
 * I06 [X4.07] [image.i[0] & 0x40] - WODA
 * I07 [X4.08] [image.i[0] & 0x80] - TEMP. TRAFA
 * I10 [X4.09] [image.i[1] & 0x01] - TEMP. TYRYSTORA
 * I11 [X4.10] [image.i[1] & 0x02] - Z PRĄDEM
 * I12 [X4.11] [image.i[1] & 0x04] - ZEZWOLENIE PROGRAMOWANIA (KLUCZYK)
 * I13 [X4.12] [image.i[1] & 0x08] - KASUJ LICZNIK 1
 * I14 [X4.13] [image.i[1] & 0x10] - KASUJ LICZNIKI STEPERÓW (Wymiana elektrody)
 * I15 [X4.14] [image.i[1] & 0x20] - CYLINDER U GÓRY
 * I16 [X4.15] [image.i[1] & 0x40] - BLOKADA Z HYDRY
 * I17 [X4.16] [image.i[1] & 0x80] - CZUJNIK ŚLUZY
 *
 * I20 [X5.01] - CZUJNIK 1
 * I21 [X5.02] - CZUJNIK 2
 * I22 [X5.03] - CZUJNIK 3
 * I23 [X5.04] - .
 * I24 [X5.05] - .
 * I25 [X5.06] - .
 * I26 [X5.07] -
 * I27 [X5.08] -
 * I30 [X5.09] -
 * I31 [X5.10] -
 * I32 [X5.11] - .
 * I33 [X5.12] - .
 * I34 [X5.13] - .
 * I35 [X5.14] - CZUJNIK 14

 * I36 [X6.01] - NR PRZYRZ BIT 0 / NR PROGRA BIT 0
 * I37 [X6.02] - NR PRZYRZ BIT 1 / NR PROGRA BIT 1
 * I40 [X6.03] - NR PRZYRZ BIT 2 / NR PROGRA BIT 2
 * I41 [X6.04] - NR PRZYRZ BIT 3 / NR PROGRA BIT 3
 * I42 [X6.05] - NR PRZYRZ BIT 4
 * I43 [X6.06] - NR PRZYRZ BIT 5
 * I44 [X6.07] - NR PRZYRZ BIT 6
 * I45 [X6.08] - NR PRZYRZ BIT 7
 * I46 [X6.09] - NR PRZYRZ BIT 8
 * I47 [X6.10] - NR PRZYRZ BIT 9
 *
 */

/*
 * Q00 [X8.01] - Impuls
 * Q01 [X8.02] - MV1
 * Q02 [X8.03] - MV2 - zwiększona siła
 * Q03 [X8.04] - Wyblokowanie - żądanie
 * Q04 [X8.05] - VZ
 * Q05 [X8.06] - FK - Koniec zgrzewu
 * Q06 [X8.07] - Licznik - Koniec cyklu
 * Q07 [X8.08] - BLAD ZGRZEWANIA
 * Q10 [X8.09] - GOTOWOSC (STATYCZNA PRZY STARCIE (WODA ITP.) - LAMPKA ZIELONA)
 * Q11 [X8.10] - LICZNIK użyutkownika osiągnięty (LAMPKA ŻÓŁTA) 
 * Q12 [X8.11] - LICZNIK STEPPERA ostrzeżenie (LAMPKA żółta)
 * Q13 [X8.12] - LICZNIK STEPPERA max (Lampka czerwona)

 * Q14 [X9.01] - ZAWOR 1-1
 * Q15 [X9.02] - ZAWOR 1-2
 * Q16 [X9.03] - ZAWOR 2-1
 * Q17 [X9.04] - ZAWOR 2-2
 * Q20 [X9.05] - ZAWOR 3-1
 * Q21 [X9.06] - ZAWOR 3-2
 * Q22 [X9.07] - ZAWOR 4-1
 * Q23 [X9.08] - ZAWOR 4-2
 * Q24 [X9.09] - Cylinder dodatkowy 1
 * Q25 [X9.10] - Cylinder dodatkowy 2
 * Q26 [X9.11] - Cylinder dodatkowy 3
 * Q27 [X9.12] - Hardware C1 OK
 */


