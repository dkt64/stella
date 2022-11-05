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
using System.Drawing.Drawing2D;


#if WindowsCE
using Microsoft.WindowsCE.Forms;
#endif


namespace APTerminal
{
    static class Ploter
    {
        static Graphics g;
        static Pen pen;

        public static int cursor = 0;

        //
        // Stałe
        //
        const int X0 = 30;
        const int Y0 = 1020;

        const int X_SCALE = 4;
        const int Y_SCALE = 10;

        const int X_PODZIALKI = 40;
        const int Y_PODZIALKI = 100;

        const int X_PODZIALKI_TXT = X_PODZIALKI * 5;
        const int Y_PODZIALKI_TXT = Y_PODZIALKI;

        const int X_AREA = 2000;
        const int Y_AREA = 1000;


        //
        // Tablice
        //

        public static ushort[] wykres_wzor;
        public static byte liczba_probek_wzor = 0;
        public static ushort[] wykres_prad;
        public static byte liczba_probek_prad = 0;
        public static ushort[] wykres_napiecie;
        public static byte liczba_probek_napiecie = 0;
        public static ushort[] wykres_wtopienie;
        public static byte liczba_probek_wtopienie = 0;

        //
        // Inicjacja
        //

        public static void Init()
        {
            wykres_wzor = new ushort[Modbus.WYKRES_TAB_SIZE];
            wykres_prad = new ushort[Modbus.WYKRES_TAB_SIZE];
            wykres_napiecie = new ushort[Modbus.WYKRES_TAB_SIZE];
            wykres_wtopienie = new ushort[Modbus.WYKRES_TAB_SIZE];
        }

        //
        // Line - rysowanie linii
        // 
        static void Line(Pen pen, int x0, int y0, int x1, int y1)
        {
            g.DrawLine(pen, X0 + x0, Y0 - y0, X0 + x1, Y0 - y1);
        }

        //
        // Pisanie tekstu okreslonym z góry fontem
        //
        static void Text(string txt, int x0, int y0)
        {
            Font drawFont = new System.Drawing.Font("Arial", 10, FontStyle.Regular);
            SolidBrush drawBrush = new System.Drawing.SolidBrush(System.Drawing.Color.White);
            g.DrawString(txt, drawFont, drawBrush, X0 + x0, Y0 - y0);
        }

        //
        // Wyrysowanie osi
        //

        static void Osie()
        {
            pen = new Pen(Color.DarkGray, 1.0f);

            // Podziałka x
            for (int i = 0; i < X_AREA; i += X_PODZIALKI)
                Line(pen, i, -3, i, 3);

            for (int i = 0; i < X_AREA; i += X_PODZIALKI_TXT)
                Text((i/X_SCALE).ToString(), i-11, -4);

            // Podziałka Y
            for (int i = 0; i < Y_AREA; i += Y_PODZIALKI)
                Line(pen, -3, i, 3, i);

            for (int i = Y_PODZIALKI_TXT; i < Y_AREA; i += Y_PODZIALKI_TXT)
            {
                Text((i / Y_SCALE).ToString(), -25, i + 7);
                Text((i / Y_SCALE / 10).ToString(), 10, i + 7);
            }

            //
            // Kursaor
            //

            Line(pen, cursor * X_SCALE, 0, cursor * X_SCALE, (int)Y_AREA - 10);

            // 
            // Na koncu osie
            //
            Line(pen, 0, 0, 0, (int)Y_AREA);
            Line(pen, 0, 0, (int)X_AREA, 0);
            Line(pen, 0, (int)Y_AREA, -5, (int)Y_AREA - 5);
            Line(pen, 0, (int)Y_AREA, +5, (int)Y_AREA - 5);
            Line(pen, (int)X_AREA, 0, (int)X_AREA - 5, 0 + 5);
            Line(pen, (int)X_AREA, 0, (int)X_AREA - 5, 0 - 5);

            //
            // Jednostki
            //
            Text("kA", -30, Y_AREA + 7);
            Text("V, mm", 15, Y_AREA + 7);
            Text("ms", X_AREA - 11, -4);
        }

        //
        // Refresh - odrysowanie wszystkiego
        //
        public static void Refresh(PaintEventArgs e)
        {
            g = e.Graphics;
            Osie();
            RysujWykres();
        }

        public static void Cursor(int x)
        {
            cursor = (x - X0) / X_SCALE;
        }

        public static void KopiujDaneModbusWzor()
        {
            int j;

            for (int i = 0; i < Modbus.WYKRES_TAB_SIZE; i++)
            {
                wykres_wzor[i] = 0;
            }

            j = 0;
            for (int i = 0; i < liczba_probek_wzor / 2; i++)
            {
                wykres_wzor[i] = (ushort)(Modbus.buf[j] + (Modbus.buf[j + 1] << 8));
                j += 2;
            }
        }

        public static void KopiujDaneModbusPrad()
        {
            int j;

            for (int i = 0; i < Modbus.WYKRES_TAB_SIZE; i++)
            {
                wykres_prad[i] = 0;
            }

            j = 0;
            for (int i = 0; i < liczba_probek_prad / 2; i++)
            {
                wykres_prad[i] = (ushort)(Modbus.buf[j] + (Modbus.buf[j + 1] << 8));
                j += 2;
            }
        }

        public static void KopiujDaneModbusNapiecie()
        {
            int j;

            for (int i = 0; i < Modbus.WYKRES_TAB_SIZE; i++)
            {
                wykres_napiecie[i] = 0;
            }

            j = 0;
            for (int i = 0; i < liczba_probek_napiecie / 2; i++)
            {
                wykres_napiecie[i] = (ushort)(Modbus.buf[j] + (Modbus.buf[j + 1] << 8));
                j += 2;
            }
        }

        public static void KopiujDaneModbusWtopienie()
        {
            int j;

            for (int i = 0; i < Modbus.WYKRES_TAB_SIZE; i++)
            {
                wykres_wtopienie[i] = 0;
            }

            j = 0;
            for (int i = 0; i < liczba_probek_wtopienie / 2; i++)
            {
                wykres_wtopienie[i] = (ushort)(Modbus.buf[j] + (Modbus.buf[j + 1] << 8));
                j += 2;
            }
        }

        public static void RysujWykres()
        {
            pen = new Pen(Color.White, 2.0f);

            for (int i = 0; i < liczba_probek_wzor / 2; i++)
            {
                Line(pen, i * X_PODZIALKI, wykres_wzor[i], (i + 1) * X_PODZIALKI, wykres_wzor[i]);
                Line(pen, (i + 1) * X_PODZIALKI, wykres_wzor[i], (i + 1) * X_PODZIALKI, wykres_wzor[i+1]);
            }

            pen = new Pen(Color.Red, 2.0f);

            for (int i = 0; i < liczba_probek_prad / 2; i++)
            {
                Line(pen, i * X_PODZIALKI, (int)((float)(wykres_prad[i] * MainForm.MUL_Prad / 100)), (i + 1) * X_PODZIALKI, (int)((float)(wykres_prad[i] * MainForm.MUL_Prad / 100)));
                Line(pen, (i + 1) * X_PODZIALKI, (int)((float)(wykres_prad[i] * MainForm.MUL_Prad / 100)), (i + 1) * X_PODZIALKI, (int)((float)(wykres_prad[i + 1] * MainForm.MUL_Prad / 100)));
            }

            pen = new Pen(Color.Yellow, 2.0f);

            for (int i = 0; i < liczba_probek_napiecie / 2; i++)
            {
                Line(pen, i * X_PODZIALKI, (int)((float)(wykres_napiecie[i] * 0.1F)), (i + 1) * X_PODZIALKI, (int)((float)(wykres_napiecie[i] * 0.1F)));
                Line(pen, (i + 1) * X_PODZIALKI, (int)((float)(wykres_napiecie[i] * 0.1F)), (i + 1) * X_PODZIALKI, (int)((float)(wykres_napiecie[i + 1] * 0.1F)));
            }

            pen = new Pen(Color.LightGreen, 2.0f);

            for (int i = 0; i < liczba_probek_wtopienie / 2; i++)
            {
                Line(pen, i * X_PODZIALKI, (int)((float)(wykres_wtopienie[i] * MainForm.MUL_PozycjaCylindra / 10)), (i + 1) * X_PODZIALKI, (int)((float)(wykres_wtopienie[i] * MainForm.MUL_PozycjaCylindra / 10)));
                Line(pen, (i + 1) * X_PODZIALKI, (int)((float)(wykres_wtopienie[i] * MainForm.MUL_PozycjaCylindra / 10)), (i + 1) * X_PODZIALKI, (int)((float)(wykres_wtopienie[i + 1] * MainForm.MUL_PozycjaCylindra / 10)));
            }

            
        }
    }
}


