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

#if WindowsCE
using Microsoft.WindowsCE.Forms;
#endif

namespace APTerminal
{
    public partial class MainForm
    {

        const string ERR_MESS_00 = "Brak synchronizacji z siecią zasilającą";
        const string ERR_MESS_01 = "Przerwa w obwodzie bezpieczeństwa";
        const string ERR_MESS_02 = "Brak przepływu wody chłodzącej";
        const string ERR_MESS_03 = "Przekroczona temperatura transformatora zgrzewalniczego";
        const string ERR_MESS_04 = "Przekroczona temperatura tyrystorów";
        const string ERR_MESS_05 = "Przerwano proces zgrzewania";
        const string ERR_MESS_06 = "Kontrola dynamiczna czujników (brak zmiany stanu czujnika)";
        const string ERR_MESS_07 = "Nie osiąnięto zadanej energii zgrzewu (U*I*t)";
        const string ERR_MESS_08 = "Przekroczono zadaną energię zgrzewu (U*I*t)";
        const string ERR_MESS_09 = "Prąd zgrzewania za mały";
        const string ERR_MESS_10 = "Prąd zgrzewania za duży";
        const string ERR_MESS_11 = "Napięcie zgrzewania za małe";
        const string ERR_MESS_12 = "Napięcie zgrzewania za duże";
        const string ERR_MESS_13 = "Zwarcie tyrystorów";
        const string ERR_MESS_14 = "Brak napięcia wtórnego transformatora";
        const string ERR_MESS_15 = "Brak przepływu prądu";
        const string ERR_MESS_16 = "Za małe wtopienie";
        const string ERR_MESS_17 = "Żaden przyrząd nie jest aktywny";
        const string ERR_MESS_18 = "Śluza optyczna przerwana więcej niż 1 raz";
        const string ERR_MESS_19 = "Przekroczona granica regulacji kąta zapłonu (999 jednostek)";
        const string ERR_MESS_20 = "";
        const string ERR_MESS_21 = "";
        const string ERR_MESS_22 = "";
        const string ERR_MESS_23 = "";

        // Tutaj tylko informacje - nie przerywa cyklu tylko dziala przy starcie
        const string ERR_MESS_24 = "Brak zezwolenia z Hydry";
        const string ERR_MESS_25 = "Błąd śluzy - brak sygnału";
        const string ERR_MESS_26 = "Błąd śluzy optycznej - nie przełożono detalu";
        const string ERR_MESS_27 = "";
        const string ERR_MESS_28 = "";
        const string ERR_MESS_29 = "";
        const string ERR_MESS_30 = "";
        const string ERR_MESS_31 = "";

        public static string[] zak_tab = { ERR_MESS_00, ERR_MESS_01, ERR_MESS_02, ERR_MESS_03, ERR_MESS_04, ERR_MESS_05, ERR_MESS_06, ERR_MESS_07, ERR_MESS_08, 
                                    ERR_MESS_09, ERR_MESS_10, ERR_MESS_11, ERR_MESS_12, ERR_MESS_13, ERR_MESS_14, ERR_MESS_15, ERR_MESS_16, ERR_MESS_17, 
                                    ERR_MESS_18, ERR_MESS_19, ERR_MESS_20, ERR_MESS_21, ERR_MESS_22, ERR_MESS_23, ERR_MESS_24, ERR_MESS_25, ERR_MESS_26,
                                    ERR_MESS_27, ERR_MESS_28, ERR_MESS_29, ERR_MESS_30, ERR_MESS_31};

        /* 
         * =========================================================================================================================================================
         * Nazwa:           CylinderUGory
         * 
         * Przeznaczenie:   Sprawdzenie czy cylinder jest u góry
         *                  
         * Parametry:       -
         * =========================================================================================================================================================
         */

        static bool CylinderUGory()
        {
            if ((PLC.status.Word & 0x20) > 0)
                return true;
            else
                return false;
        }

        /* 
         * =========================================================================================================================================================
         * Nazwa:           P_PozycjaCylindra 
         * 
         * Przeznaczenie:   Odczyt pozycji cylidra - unifikacja na wybranym polu obrazu IO
         *                  
         * Parametry:       -
         * =========================================================================================================================================================
         */

        static bool KontrolaNakretki_PozycjaCylindra()
        {
            if ((PLC.status.Word & 0x08) > 0)
                return true;
            else
                return false;
        }

        static ushort P_PozycjaCylindra()
        {
            return Modbus.io.ain[3];
        }

        /* 
         * =========================================================================================================================================================
         * Nazwa:           P_PozycjaCylindra 
         * 
         * Przeznaczenie:   Odczyt pozycji cylidra - unifikacja na wybranym polu obrazu IO
         *                  
         * Parametry:       -
         * =========================================================================================================================================================
         */

        static bool KontrolaNakretki_Wydmuch()
        {
            if ((PLC.status.Word & 0x10) > 0)
                return true;
            else
                return false;
        }

        static ushort P_Wydmuch()
        {
            return Modbus.io.ain[4];
        }

        /* 
         * =========================================================================================================================================================
         * Nazwa:           textParametr_LostFocus
         * 
         * Przeznaczenie:   Zdarzenie - utracono focus na polu edycji parametrów. Trzeba wyłączyć klawiaturkę na Windows CE
         *                  
         * Parametry:       -
         * =========================================================================================================================================================
         */
        private void textParametr_LostFocus(object sender, EventArgs e)
        {
#if WindowsCE
            inputPanel.Enabled = false;
#endif
        }

        /* 
         * =========================================================================================================================================================
         * Nazwa:           textParametr_KeyDown
         * 
         * Przeznaczenie:   Zdarzenie - Wciśnięcie klawiczy w polu edycji parametru. Wywolanie funkcji ZmianaParametru z parametrem ENTER
         *                  
         * Parametry:       -
         * =========================================================================================================================================================
         */
        private void textParametr_KeyDown(object sender, KeyEventArgs e)
        {

        }

        /* 
         * =========================================================================================================================================================
         * Nazwa:           plus****_Click
         * 
         * Przeznaczenie:   Zdarzenie - Wciśnięcie klawiczy plus edycji parametru. Wywolanie funkcji ZmianaParametru z parametrem PLUS
         *                  
         * Parametry:       -
         * =========================================================================================================================================================
         */
        private void plus10000_Click(object sender, EventArgs e)
        {
            if (brak_miejsca_na_dysku)
                BrakMiejscaInfo();
            ZmianaParametruZgrzewania(10000);
        }

        private void plus1000_Click(object sender, EventArgs e)
        {
            if (brak_miejsca_na_dysku)
                BrakMiejscaInfo();
            ZmianaParametruZgrzewania(1000);
        }

        private void plus100_Click(object sender, EventArgs e)
        {
            if (brak_miejsca_na_dysku)
                BrakMiejscaInfo();
            ZmianaParametruZgrzewania(100);
        }

        private void plus10_Click(object sender, EventArgs e)
        {
            if (brak_miejsca_na_dysku)
                BrakMiejscaInfo();
            ZmianaParametruZgrzewania(10);
        }

        private void plus1_Click(object sender, EventArgs e)
        {
            if (brak_miejsca_na_dysku)
                BrakMiejscaInfo();
            ZmianaParametruZgrzewania(1);
        }

        /* 
         * =========================================================================================================================================================
         * Nazwa:           minus****_Click
         * 
         * Przeznaczenie:   Zdarzenie - Wciśnięcie klawiczy minus edycji parametru. Wywolanie funkcji ZmianaParametru z parametrem MINUS
         *                  
         * Parametry:       -
         * =========================================================================================================================================================
         */
        private void minus10000_Click(object sender, EventArgs e)
        {
            if (brak_miejsca_na_dysku)
                BrakMiejscaInfo();
            ZmianaParametruZgrzewania(-10000);
        }

        private void minus1000_Click(object sender, EventArgs e)
        {
            if (brak_miejsca_na_dysku)
                BrakMiejscaInfo();
            ZmianaParametruZgrzewania(-1000);
        }

        private void minus100_Click(object sender, EventArgs e)
        {
            if (brak_miejsca_na_dysku)
                BrakMiejscaInfo();
            ZmianaParametruZgrzewania(-100);
        }

        private void minus10_Click(object sender, EventArgs e)
        {
            if (brak_miejsca_na_dysku)
                BrakMiejscaInfo();
            ZmianaParametruZgrzewania(-10);
        }

        private void minus1_Click(object sender, EventArgs e)
        {
            if (brak_miejsca_na_dysku)
                BrakMiejscaInfo();
            ZmianaParametruZgrzewania(-1);
        }

        /* 
         * =========================================================================================================================================================
         * Nazwa:           textParametr_GotFocus
         * 
         * Przeznaczenie:   Zdarzenie - otrzymano focus na polu edycji parametrów. Trzeba wyswietlić klawiaturkę na Windows CE
         *                  
         * Parametry:       -
         * =========================================================================================================================================================
         */
        private void textParametr_GotFocus(object sender, EventArgs e)
        {
#if WindowsCE
            inputPanel = new InputPanel();
            inputPanel.Enabled = false;
#endif

        }

        /* 
 * =========================================================================================================================================================
 * Nazwa:           buttonKlawiaturka1_Click
 * 
 * Przeznaczenie:   Zdarzenie od przycisku Klawiatura - użytkownik w WIndows CE może sobie ją wyświetlić (nie przez Focus)
 *                  
 * Parametry:       -
 * =========================================================================================================================================================
 */
        private void buttonKlawiaturka1_Click(object sender, EventArgs e)
        {
#if WindowsCE
            inputPanel = new InputPanel();
            inputPanel.Enabled = false;
            textParametr.Focus();
#endif
        }

        /* 
         * =========================================================================================================================================================
         * Nazwa:           button_Licznik
         * 
         * Przeznaczenie:   Sterowanie licznikiem sztuk operatora na głównym tabie Produkcja
         *                  
         * Parametry:       -
         * =========================================================================================================================================================
         */
        private void button_Licznik_Plus1_Click(object sender, EventArgs e)
        {
            Modbus.Rozkaz(0x0104, 1);
        }

        private void button_Licznik_Plus10_Click(object sender, EventArgs e)
        {
            Modbus.Rozkaz(0x0104, 10);
        }

        private void button_Licznik_Plus100_Click(object sender, EventArgs e)
        {
            Modbus.Rozkaz(0x0104, 100);
        }

        private void button_Licznik_Plus1000_Click(object sender, EventArgs e)
        {
            Modbus.Rozkaz(0x0104, 1000);
        }

        private void button_Licznik_Plus10000_Click(object sender, EventArgs e)
        {
            Modbus.Rozkaz(0x0104, 10000);
        }

        private void button_Licznik_Minus1_Click(object sender, EventArgs e)
        {
            Modbus.Rozkaz(0x0204, 1);
        }

        private void button_Licznik_Minus10_Click(object sender, EventArgs e)
        {
            Modbus.Rozkaz(0x0204, 10);
        }

        private void button_Licznik_Minus100_Click(object sender, EventArgs e)
        {
            Modbus.Rozkaz(0x0204, 100);
        }

        private void button_Licznik_Minus1000_Click(object sender, EventArgs e)
        {
            Modbus.Rozkaz(0x0204, 1000);
        }

        private void button_Licznik_Minus10000_Click(object sender, EventArgs e)
        {
            Modbus.Rozkaz(0x0204, 10000);
        }

        private void button_LicznikZeruj_Click(object sender, EventArgs e)
        {
            Modbus.Rozkaz(0x0304, 0);
        }

        private void button_LicznikMax_Plus1_Click(object sender, EventArgs e)
        {
            Modbus.Rozkaz(0x0105, 1);
        }

        private void button_LicznikMax_Plus10_Click(object sender, EventArgs e)
        {
            Modbus.Rozkaz(0x0105, 10);
        }

        private void button_LicznikMax_Plus100_Click(object sender, EventArgs e)
        {
            Modbus.Rozkaz(0x0105, 100);
        }

        private void button_LicznikMax_Plus1000_Click(object sender, EventArgs e)
        {
            Modbus.Rozkaz(0x0105, 1000);
        }

        private void button_LicznikMax_Plus10000_Click(object sender, EventArgs e)
        {
            Modbus.Rozkaz(0x0105, 10000);
        }

        private void button_LicznikMax_Minus1_Click(object sender, EventArgs e)
        {
            Modbus.Rozkaz(0x0205, 1);
        }

        private void button_LicznikMax_Minus10_Click(object sender, EventArgs e)
        {
            Modbus.Rozkaz(0x0205, 10);
        }

        private void button_LicznikMax_Minus100_Click(object sender, EventArgs e)
        {
            Modbus.Rozkaz(0x0205, 100);
        }

        private void button_LicznikMax_Minus1000_Click(object sender, EventArgs e)
        {
            Modbus.Rozkaz(0x0205, 1000);
        }

        private void button_LicznikMax_Minus10000_Click(object sender, EventArgs e)
        {
            Modbus.Rozkaz(0x0205, 10000);
        }

        private void button_LicznikMaxZeruj_Click(object sender, EventArgs e)
        {
            Modbus.Rozkaz(0x0305, 0);
        }

        /* 
        * =========================================================================================================================================================
        * Nazwa:           Zmiana parametrów przyrządu
        * 
        * Przeznaczenie:   
        *                  
        * Parametry:       -
        * =========================================================================================================================================================
        */
        private void parprzy_plus1_Click(object sender, EventArgs e)
        {
            Modbus.Rozkaz((ushort)(0x0100 + comboParametrPrzyrzad.SelectedIndex + Modbus.PRZESUNIECIE_SM), 1);
            Tools.LogParam("Zmiana parametru przyrządu " + comboParametrPrzyrzad.SelectedItem.ToString());
        }

        private void parprzy_plus10_Click(object sender, EventArgs e)
        {
            Modbus.Rozkaz((ushort)(0x0100 + comboParametrPrzyrzad.SelectedIndex + Modbus.PRZESUNIECIE_SM), 10);
            Tools.LogParam("Zmiana parametru przyrządu " + comboParametrPrzyrzad.SelectedItem.ToString());
        }

        private void parprzy_plus100_Click(object sender, EventArgs e)
        {
            Modbus.Rozkaz((ushort)(0x0100 + comboParametrPrzyrzad.SelectedIndex + Modbus.PRZESUNIECIE_SM), 100);
            Tools.LogParam("Zmiana parametru przyrządu " + comboParametrPrzyrzad.SelectedItem.ToString());
        }

        private void parprzy_plus1000_Click(object sender, EventArgs e)
        {
            Modbus.Rozkaz((ushort)(0x0100 + comboParametrPrzyrzad.SelectedIndex + Modbus.PRZESUNIECIE_SM), 1000);
            Tools.LogParam("Zmiana parametru przyrządu " + comboParametrPrzyrzad.SelectedItem.ToString());
        }

        private void parprzy_plus10000_Click(object sender, EventArgs e)
        {
            Modbus.Rozkaz((ushort)(0x0100 + comboParametrPrzyrzad.SelectedIndex + Modbus.PRZESUNIECIE_SM), 10000);
            Tools.LogParam("Zmiana parametru przyrządu " + comboParametrPrzyrzad.SelectedItem.ToString());
        }

        private void parprzy_minus1_Click(object sender, EventArgs e)
        {
            Modbus.Rozkaz((ushort)(0x0200 + comboParametrPrzyrzad.SelectedIndex + Modbus.PRZESUNIECIE_SM), 1);
            Tools.LogParam("Zmiana parametru przyrządu " + comboParametrPrzyrzad.SelectedItem.ToString());
        }

        private void parprzy_minus10_Click(object sender, EventArgs e)
        {
            Modbus.Rozkaz((ushort)(0x0200 + comboParametrPrzyrzad.SelectedIndex + Modbus.PRZESUNIECIE_SM), 10);
            Tools.LogParam("Zmiana parametru przyrządu " + comboParametrPrzyrzad.SelectedItem.ToString());
        }

        private void parprzy_minus100_Click(object sender, EventArgs e)
        {
            Modbus.Rozkaz((ushort)(0x0200 + comboParametrPrzyrzad.SelectedIndex + Modbus.PRZESUNIECIE_SM), 100);
            Tools.LogParam("Zmiana parametru przyrządu " + comboParametrPrzyrzad.SelectedItem.ToString());
        }

        private void parprzy_minus1000_Click(object sender, EventArgs e)
        {
            Modbus.Rozkaz((ushort)(0x0200 + comboParametrPrzyrzad.SelectedIndex + Modbus.PRZESUNIECIE_SM), 1000);
            Tools.LogParam("Zmiana parametru przyrządu " + comboParametrPrzyrzad.SelectedItem.ToString());
        }

        private void parprzy_minus10000_Click(object sender, EventArgs e)
        {
            Modbus.Rozkaz((ushort)(0x0200 + comboParametrPrzyrzad.SelectedIndex + Modbus.PRZESUNIECIE_SM), 10000);
            Tools.LogParam("Zmiana parametru przyrządu " + comboParametrPrzyrzad.SelectedItem.ToString());
        }

        /* 
        * =========================================================================================================================================================
        * Nazwa:           textHaslo_GotFocus
        * 
        * Przeznaczenie:   Obsługa hasła dostępu UR
        *                  
        * Parametry:       -
        * =========================================================================================================================================================
        */
        private void textHaslo_GotFocus(object sender, EventArgs e)
        {
#if WindowsCE
            inputPanel.Enabled = true;
#endif
        }

        /* 
        * =========================================================================================================================================================
        * Nazwa:           textHaslo_KeyDown
        * 
        * Przeznaczenie:   Obsługa hasła dostępu UR
        *                  
        * Parametry:       -
        * =========================================================================================================================================================
        */
        private void textHaslo_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (textHaslo.Text == Tools.ReadSetting("Pass"))
                {
                    //poziom_ur = true;
                    timerPoziomUR.Enabled = true;
                    textHaslo.Text = "";
                    Tools.Log("Zmiana poziomu uprawnień na UR");
                }
                else
                {
                    //poziom_ur = false;
                    timerPoziomUR.Enabled = false;
                    MessageBox.Show("Niepoprawne hasło!", "Poziom uprawanień UR");
                }

#if WindowsCE
                inputPanel.Enabled = false;
#endif
            }

            if (e.KeyCode == Keys.Escape)
            {
#if WindowsCE
                inputPanel.Enabled = false;
#endif
            }
        }

        /* 
        * =========================================================================================================================================================
        * Nazwa:           textHaslo_LostFocus
        * 
        * Przeznaczenie:   Obsługa hasła dostępu UR
        *                  
        * Parametry:       -
        * =========================================================================================================================================================
        */
        private void textHaslo_LostFocus(object sender, EventArgs e)
        {
#if WindowsCE
            inputPanel.Enabled = false;
#endif
        }

        /* 
        * =========================================================================================================================================================
        * Nazwa:           timerPoziomUR_Tick
        * 
        * Przeznaczenie:   Obsługa hasła dostępu UR
        *                  
        * Parametry:       -
        * =========================================================================================================================================================
        */
        private void timerPoziomUR_Tick(object sender, EventArgs e)
        {
            //poziom_ur = false;
            timerPoziomUR.Enabled = false;
            Tools.Log("Wyłączenie uprawnień UR");
        }

        /* 
        * =========================================================================================================================================================
        * Nazwa:           buttonZmienPoziomNaUR_Click
        * 
        * Przeznaczenie:   Obsługa hasła dostępu UR
        *                  
        * Parametry:       -
        * =========================================================================================================================================================
        */
        private void buttonZmienPoziomNaUR_Click(object sender, EventArgs e)
        {
            if (textHaslo.Text == Tools.ReadSetting("Pass"))
            {
                //poziom_ur = true;
                timerPoziomUR.Enabled = true;
                textHaslo.Text = "";
                Tools.Log("Zmiana poziomu uprawnień na UR");
            }
            else
            {
                //poziom_ur = false;
                timerPoziomUR.Enabled = false;
                MessageBox.Show("Niepoprawne hasło!", "Poziom uprawanień UR");
            }

        }

        /* 
        * =========================================================================================================================================================
        * Nazwa:           buttonZmienPoziomZUR_Click
        * 
        * Przeznaczenie:   Obsługa hasła dostępu UR
        *                  
        * Parametry:       -
        * =========================================================================================================================================================
        */
        private void buttonZmienPoziomZUR_Click(object sender, EventArgs e)
        {
            //poziom_ur = false;
            timerPoziomUR.Enabled = false;
            Tools.Log("Wyłączenie uprawnień UR");
        }

        /* 
        * =========================================================================================================================================================
        * Nazwa:           buttonKlawiaturka1_Click
        * 
        * Przeznaczenie:   Obsługa hasła dostępu UR - pokazanie i schowanie klawiatury
        *                  
        * Parametry:       -
        * =========================================================================================================================================================
        */
        private void buttonKlawiaturka1_Click_1(object sender, EventArgs e)
        {
#if WindowsCE
            inputPanel = new InputPanel();
            inputPanel.Enabled = true;
            textParametr.Focus();
#endif
        }

        private void buttonKlawiaturka2_Click(object sender, EventArgs e)
        {
#if WindowsCE
            inputPanel = new InputPanel();
            inputPanel.Enabled = false;
#endif
        }

        /* 
        * =========================================================================================================================================================
        * Nazwa:           Rozkazy z przycisków do uC
        * 
        * Przeznaczenie:   
        *                  
        * Parametry:       -
        * =========================================================================================================================================================
        */

        public static bool ReferencjaPraduAktywna()
        {
            if ((PLC.status.Word & 0x01) > 0)
                return true;
            else
                return false;
        }

        public static bool ReferencjaCylindraAktywna()
        {
            if ((PLC.status.Word & 0x02) > 0)
                return true;
            else
                return false;
        }

        private void buttonPozycjaWyjsciowa_Click(object sender, EventArgs e)
        {
            Modbus.Rozkaz(1);
            Tools.LogParam("Pozycja wyjsciowa");
            koniec_procesu_cnt = 101;
            aktywacja_start = false;
            zmiana_parametru_start = false;
        }

        private void buttonPomiarRefPradu_Click(object sender, EventArgs e)
        {
            if (ReferencjaPraduAktywna() || PLC.cykl_program_akt.Word == 0)
            {
                Modbus.Rozkaz(0x030d, Convert.ToByte(numeric_ref_prad_zadane.Value));
                //Modbus.Rozkaz(2);
                Tools.LogParam("Referowanie pradu, napiecia i energii");
            }
            else
            {
                MessageBox.Show("Rozpoczęcie referencji możliwe tylko na programie 0");
            }
        }

        private void buttonPomiarCylindraIWydmuchu_Click(object sender, EventArgs e)
        {
            if (ReferencjaCylindraAktywna() || PLC.cykl_program_akt.Word == 0)
            {
                Modbus.Rozkaz(0x030e, Convert.ToByte(numeric_ref_nakr_zadane.Value));
                //Modbus.Rozkaz(3);
                Tools.LogParam("Referowanie pozycji cylindra i cisnienia wydmuchu");
            }
            else
            {
                MessageBox.Show("Rozpoczęcie referencji możliwe tylko na programie 0");
            }
        }

        private void buttonPotwierdzenieZaklocenia_Click(object sender, EventArgs e)
        {
            Modbus.Rozkaz(5);
            Tools.LogParam("Potwierdzenie zaklocenia z panela");
        }

        /* 
        * =========================================================================================================================================================
        * Nazwa:           ZapisWynikow
        * 
        * Przeznaczenie:   Zapis wyników pomiarów po zakończonym zgrzewie
        *                  
        * Parametry:       -
        * =========================================================================================================================================================
        */
        static void ZapisWynikow()
        {
            string ost_zgrzew_pomiar_energii = String.Format("{0:0.0}", (((float)(PLC.ost_zgrzew_pomiar_energii.Word) / AIN_Meas_Mul / PRZEL_ENERGIA) * MUL_Prad));
            string ost_zgrzew_energia_ref = String.Format("{0:0.0}", (((float)(Zarzadzanie.par.prog[PLC.ost_zgrzew_nr_programu.Word].Eref) / AIN_Meas_Mul / PRZEL_ENERGIA) * MUL_Prad));

            if (((err & (1 << 7)) > 0) || ((err & (1 << 8)) > 0) || ((err & (1 << 9)) > 0) || ((err & (1 << 10)) > 0) || ((err & (1 << 11)) > 0) || ((err & (1 << 12)) > 0))
            {
                // Zgrzew błędny
                Tools.LogParam("NOK! PRG " + PLC.ost_zgrzew_nr_programu.Word.ToString() +
                                ", EN " + String.Format("{0:0.0}", (((float)(PLC.ost_zgrzew_pomiar_energii.Word) / AIN_Meas_Mul / PRZEL_ENERGIA) * MUL_Prad)) + " [kWs]." +
                                " +/- " + Zarzadzanie.par.prog[PLC.ost_zgrzew_nr_programu.Word].ErefTolerance.ToString() + " [%]" +
                                ", EN R " + String.Format("{0:0.0}", (((float)(Zarzadzanie.par.prog[PLC.ost_zgrzew_nr_programu.Word].Eref) / AIN_Meas_Mul / PRZEL_ENERGIA) * MUL_Prad)) + " [kWs]." +

                                ", PR " + String.Format("{0:0.0}", (((float)(PLC.ost_zgrzew_pomiar_prad.Word) / AIN_Meas_Mul) * MUL_Prad)) + " [kA]." +
                                " +/- " + Zarzadzanie.par.prog[PLC.ost_zgrzew_nr_programu.Word].IrefTolerance.ToString() + " [%]" +
                                ", PR R " + String.Format("{0:0.0}", (((float)(Zarzadzanie.par.prog[PLC.ost_zgrzew_nr_programu.Word].Iref) / AIN_Meas_Mul) * MUL_Prad)) + " [kA]." +

                                ", NAP " + String.Format("{0:0.0}", (((float)(PLC.ost_zgrzew_pomiar_nap.Word) / AIN_Meas_Mul))) + " [V]." +
                                " +/- " + Zarzadzanie.par.prog[PLC.ost_zgrzew_nr_programu.Word].UrefTolerance.ToString() + " [%]" +
                                ", NAP R " + String.Format("{0:0.0}", (((float)(Zarzadzanie.par.prog[PLC.ost_zgrzew_nr_programu.Word].Uref) / AIN_Meas_Mul))) + " [V]."); ;

            }
            else
            {
                // Zgrzew poprawny
                Tools.LogParam("OK. PRG " + PLC.ost_zgrzew_nr_programu.Word.ToString() +
                                ", EN " + String.Format("{0:0.0}", (((float)(PLC.ost_zgrzew_pomiar_energii.Word) / AIN_Meas_Mul / PRZEL_ENERGIA) * MUL_Prad)) + " [kWs]." +
                                " +/- " + Zarzadzanie.par.prog[PLC.ost_zgrzew_nr_programu.Word].ErefTolerance.ToString() + " [%]" +
                                ", EN R " + String.Format("{0:0.0}", (((float)(Zarzadzanie.par.prog[PLC.ost_zgrzew_nr_programu.Word].Eref) / AIN_Meas_Mul / PRZEL_ENERGIA) * MUL_Prad)) + " [kWs]." +

                                ", PR " + String.Format("{0:0.0}", (((float)(PLC.ost_zgrzew_pomiar_prad.Word) / AIN_Meas_Mul) * MUL_Prad)) + " [kA]." +
                                " +/- " + Zarzadzanie.par.prog[PLC.ost_zgrzew_nr_programu.Word].IrefTolerance.ToString() + " [%]" +
                                ", PR R " + String.Format("{0:0.0}", (((float)(Zarzadzanie.par.prog[PLC.ost_zgrzew_nr_programu.Word].Iref) / AIN_Meas_Mul) * MUL_Prad)) + " [kA]." +

                                ", NAP " + String.Format("{0:0.0}", (((float)(PLC.ost_zgrzew_pomiar_nap.Word) / AIN_Meas_Mul))) + " [V]." +
                                " +/- " + Zarzadzanie.par.prog[PLC.ost_zgrzew_nr_programu.Word].UrefTolerance.ToString() + " [%]" +
                                ", NAP R " + String.Format("{0:0.0}", (((float)(Zarzadzanie.par.prog[PLC.ost_zgrzew_nr_programu.Word].Uref) / AIN_Meas_Mul))) + " [V].");
            }

        }
        /*
         *             //
                    // Wyniki pomiarów
                    //

                    // Prąd
                    label_pomiar_prad.Text = String.Format("{0:0.0}", (((float)(PLC.ost_zgrzew_pomiar_prad.Word) / AIN_Meas_Mul) * MUL_Prad));
                    label_proces_prad_ref.Text = String.Format("{0:0.0}", (((float)(Zarzadzanie.par.prog[PLC.ost_zgrzew_nr_programu.Word].Iref) / AIN_Meas_Mul) * MUL_Prad));

                    // Napięcie
                    label_pomiar_napiecie.Text = String.Format("{0:0.0}", (((float)(PLC.ost_zgrzew_pomiar_nap.Word) / AIN_Meas_Mul)));
                    label_proces_nap_ref.Text = String.Format("{0:0.0}", ((float)(Zarzadzanie.par.prog[PLC.ost_zgrzew_nr_programu.Word].Uref) / AIN_Meas_Mul));

                    // Energia
                    label_pomiar_energia.Text = String.Format("{0:0.0}", (((float)(PLC.ost_zgrzew_pomiar_energii.Word) / AIN_Meas_Mul) * MUL_Prad));
                    label_proces_ener_ref.Text = String.Format("{0:0.0}", (((float)(Zarzadzanie.par.prog[PLC.ost_zgrzew_nr_programu.Word].Eref) / AIN_Meas_Mul) * MUL_Prad));

                    // Wtopienie
                    label_wtopienie.Text = PLC.ost_zgrzew_pomiar_wtopienia.Word.ToString();
                    label_proces_wtop_ref.Text = Zarzadzanie.par.prog[PLC.ost_zgrzew_nr_programu.Word].PozycjaCylindraMinWtopienie.ToString();
        */

        /* 
        * =========================================================================================================================================================
        * Nazwa:           button_up_param_Click i button_down_param_Click
        * 
        * Przeznaczenie:   Szybsze przeglądanie parametrów
        *                  
        * Parametry:       -
        * =========================================================================================================================================================
        */
        private void button_up_param_Click(object sender, EventArgs e)
        {
            if (comboParametr.SelectedIndex > 0)
            {
                int selected = comboParametr.SelectedIndex;
                comboParametr.SelectedIndex = selected - 1;
            }
        }

        private void button_down_param_Click(object sender, EventArgs e)
        {
            if (comboParametr.SelectedIndex < (comboParametr.Items.Count - 1))
            {
                int selected = comboParametr.SelectedIndex;
                comboParametr.SelectedIndex = selected + 1;
            }
        }

        /* 
        * =========================================================================================================================================================
        * Nazwa:           textParametrPrzyrzad_GotFocus
        * 
        * Przeznaczenie:   Jezeli przy fokusie na wartosc parametrow wyskoczyla klawiatura to trzeba ja schowac
        *                  
        * Parametry:       -
        * =========================================================================================================================================================
        */
        private void textParametrPrzyrzad_GotFocus(object sender, EventArgs e)
        {
#if WindowsCE
            inputPanel = new InputPanel();
            inputPanel.Enabled = false;
#endif
        }

        /* 
         * =========================================================================================================================================================
         * Nazwa:           KontrolkiStanIO 
         * 
         * Przeznaczenie:   Funkcja, ktora ustawia kontrolki w odpowiedni stan odczytany z obrazu IO
         *                  
         * Parametry:       -
         * =========================================================================================================================================================
         */
        private void KontrolkiStanIO()
        {

            // ======================= TAB WEJŚCIA =====================

            if ((Modbus.io.i[0] & 0x01) > 0)
                label_I0_0.BackColor = Color.Green;
            else
                label_I0_0.BackColor = Color.DarkRed;

            if ((Modbus.io.i[0] & 0x02) > 0)
                label_I0_1.BackColor = Color.Green;
            else
                label_I0_1.BackColor = Color.DarkRed;

            if ((Modbus.io.i[0] & 0x04) > 0)
                label_I0_2.BackColor = Color.Green;
            else
                label_I0_2.BackColor = Color.DarkRed;

            if ((Modbus.io.i[0] & 0x08) > 0)
                label_I0_3.BackColor = Color.Green;
            else
                label_I0_3.BackColor = Color.DarkRed;

            if ((Modbus.io.i[0] & 0x10) > 0)
                label_I0_4.BackColor = Color.Green;
            else
                label_I0_4.BackColor = Color.DarkRed;

            if ((Modbus.io.i[0] & 0x20) > 0)
                label_I0_5.BackColor = Color.Green;
            else
                label_I0_5.BackColor = Color.DarkRed;

            if ((Modbus.io.i[0] & 0x40) > 0)
                label_I0_6.BackColor = Color.Green;
            else
                label_I0_6.BackColor = Color.DarkRed;

            if ((Modbus.io.i[0] & 0x80) > 0)
                label_I0_7.BackColor = Color.Green;
            else
                label_I0_7.BackColor = Color.DarkRed;

            if ((Modbus.io.i[1] & 0x01) > 0)
                label_I1_0.BackColor = Color.Green;
            else
                label_I1_0.BackColor = Color.DarkRed;

            if ((Modbus.io.i[1] & 0x02) > 0)
                label_I1_1.BackColor = Color.Green;
            else
                label_I1_1.BackColor = Color.DarkRed;

            if ((Modbus.io.i[1] & 0x04) > 0)
                label_I1_2.BackColor = Color.Green;
            else
                label_I1_2.BackColor = Color.DarkRed;

            if ((Modbus.io.i[1] & 0x08) > 0)
                label_I1_3.BackColor = Color.Green;
            else
                label_I1_3.BackColor = Color.DarkRed;

            if ((Modbus.io.i[1] & 0x10) > 0)
                label_I1_4.BackColor = Color.Green;
            else
                label_I1_4.BackColor = Color.DarkRed;

            if ((Modbus.io.i[1] & 0x20) > 0)
                label_I1_5.BackColor = Color.Green;
            else
                label_I1_5.BackColor = Color.DarkRed;

            if ((Modbus.io.i[1] & 0x40) > 0)
                label_I1_6.BackColor = Color.Green;
            else
                label_I1_6.BackColor = Color.DarkRed;

            if ((Modbus.io.i[1] & 0x80) > 0)
                label_I1_7.BackColor = Color.Green;
            else
                label_I1_7.BackColor = Color.DarkRed;

            if ((Modbus.io.i[2] & 0x01) > 0)
                label_I2_0.BackColor = Color.Green;
            else
                label_I2_0.BackColor = Color.DarkRed;

            if ((Modbus.io.i[2] & 0x02) > 0)
                label_I2_1.BackColor = Color.Green;
            else
                label_I2_1.BackColor = Color.DarkRed;

            if ((Modbus.io.i[2] & 0x04) > 0)
                label_I2_2.BackColor = Color.Green;
            else
                label_I2_2.BackColor = Color.DarkRed;

            if ((Modbus.io.i[2] & 0x08) > 0)
                label_I2_3.BackColor = Color.Green;
            else
                label_I2_3.BackColor = Color.DarkRed;

            if ((Modbus.io.i[2] & 0x10) > 0)
                label_I2_4.BackColor = Color.Green;
            else
                label_I2_4.BackColor = Color.DarkRed;

            if ((Modbus.io.i[2] & 0x20) > 0)
                label_I2_5.BackColor = Color.Green;
            else
                label_I2_5.BackColor = Color.DarkRed;

            if ((Modbus.io.i[2] & 0x40) > 0)
                label_I2_6.BackColor = Color.Green;
            else
                label_I2_6.BackColor = Color.DarkRed;

            if ((Modbus.io.i[2] & 0x80) > 0)
                label_I2_7.BackColor = Color.Green;
            else
                label_I2_7.BackColor = Color.DarkRed;

            if ((Modbus.io.i[3] & 0x01) > 0)
                label_I3_0.BackColor = Color.Green;
            else
                label_I3_0.BackColor = Color.DarkRed;

            if ((Modbus.io.i[3] & 0x02) > 0)
                label_I3_1.BackColor = Color.Green;
            else
                label_I3_1.BackColor = Color.DarkRed;

            if ((Modbus.io.i[3] & 0x04) > 0)
                label_I3_2.BackColor = Color.Green;
            else
                label_I3_2.BackColor = Color.DarkRed;

            if ((Modbus.io.i[3] & 0x08) > 0)
                label_I3_3.BackColor = Color.Green;
            else
                label_I3_3.BackColor = Color.DarkRed;

            if ((Modbus.io.i[3] & 0x10) > 0)
                label_I3_4.BackColor = Color.Green;
            else
                label_I3_4.BackColor = Color.DarkRed;

            if ((Modbus.io.i[3] & 0x20) > 0)
                label_I3_5.BackColor = Color.Green;
            else
                label_I3_5.BackColor = Color.DarkRed;


            // ======================= TAB WYJŚCIA =====================

            if ((Modbus.io.q[0] & 0x01) > 0)
                label_Q0_0.BackColor = Color.Green;
            else
                label_Q0_0.BackColor = Color.DarkRed;

            if ((Modbus.io.q[0] & 0x02) > 0)
                label_Q0_1.BackColor = Color.Green;
            else
                label_Q0_1.BackColor = Color.DarkRed;

            if ((Modbus.io.q[0] & 0x04) > 0)
                label_Q0_2.BackColor = Color.Green;
            else
                label_Q0_2.BackColor = Color.DarkRed;

            if ((Modbus.io.q[0] & 0x08) > 0)
                label_Q0_3.BackColor = Color.Green;
            else
                label_Q0_3.BackColor = Color.DarkRed;

            if ((Modbus.io.q[0] & 0x10) > 0)
                label_Q0_4.BackColor = Color.Green;
            else
                label_Q0_4.BackColor = Color.DarkRed;

            if ((Modbus.io.q[0] & 0x20) > 0)
                label_Q0_5.BackColor = Color.Green;
            else
                label_Q0_5.BackColor = Color.DarkRed;

            if ((Modbus.io.q[0] & 0x40) > 0)
                label_Q0_6.BackColor = Color.Green;
            else
                label_Q0_6.BackColor = Color.DarkRed;

            if ((Modbus.io.q[0] & 0x80) > 0)
                label_Q0_7.BackColor = Color.Green;
            else
                label_Q0_7.BackColor = Color.DarkRed;

            if ((Modbus.io.q[1] & 0x01) > 0)
                label_Q1_0.BackColor = Color.Green;
            else
                label_Q1_0.BackColor = Color.DarkRed;

            if ((Modbus.io.q[1] & 0x02) > 0)
                label_Q1_1.BackColor = Color.Green;
            else
                label_Q1_1.BackColor = Color.DarkRed;

            if ((Modbus.io.q[1] & 0x04) > 0)
                label_Q1_2.BackColor = Color.Green;
            else
                label_Q1_2.BackColor = Color.DarkRed;

            if ((Modbus.io.q[1] & 0x08) > 0)
                label_Q1_3.BackColor = Color.Green;
            else
                label_Q1_3.BackColor = Color.DarkRed;

            if ((Modbus.io.q[1] & 0x10) > 0)
                label_Q1_4.BackColor = Color.Green;
            else
                label_Q1_4.BackColor = Color.DarkRed;

            if ((Modbus.io.q[1] & 0x20) > 0)
                label_Q1_5.BackColor = Color.Green;
            else
                label_Q1_5.BackColor = Color.DarkRed;

            if ((Modbus.io.q[1] & 0x40) > 0)
                label_Q1_6.BackColor = Color.Green;
            else
                label_Q1_6.BackColor = Color.DarkRed;

            if ((Modbus.io.q[1] & 0x80) > 0)
                label_Q1_7.BackColor = Color.Green;
            else
                label_Q1_7.BackColor = Color.DarkRed;

            if ((Modbus.io.q[2] & 0x01) > 0)
                label_Q2_0.BackColor = Color.Green;
            else
                label_Q2_0.BackColor = Color.DarkRed;

            if ((Modbus.io.q[2] & 0x02) > 0)
                label_Q2_1.BackColor = Color.Green;
            else
                label_Q2_1.BackColor = Color.DarkRed;

            if ((Modbus.io.q[2] & 0x04) > 0)
                label_Q2_2.BackColor = Color.Green;
            else
                label_Q2_2.BackColor = Color.DarkRed;

            if ((Modbus.io.q[2] & 0x08) > 0)
                label_Q2_3.BackColor = Color.Green;
            else
                label_Q2_3.BackColor = Color.DarkRed;

            if ((Modbus.io.q[2] & 0x10) > 0)
                label_Q2_4.BackColor = Color.Green;
            else
                label_Q2_4.BackColor = Color.DarkRed;

            if ((Modbus.io.q[2] & 0x20) > 0)
                label_Q2_5.BackColor = Color.Green;
            else
                label_Q2_5.BackColor = Color.DarkRed;

            if ((Modbus.io.q[2] & 0x40) > 0)
                label_Q2_6.BackColor = Color.Green;
            else
                label_Q2_6.BackColor = Color.DarkRed;

            if ((Modbus.io.q[2] & 0x80) > 0)
                label_Q2_7.BackColor = Color.Green;
            else
                label_Q2_7.BackColor = Color.DarkRed;

            // ======================= TAB ANALOGI =====================

            label_ADC0.Text = Modbus.io.ain[0].ToString();
            label_AIN0.Text = String.Format("{0:0.00}", ((float)Modbus.io.ain[0] / AIN_Meas_Mul));
            label_Sygnaly_Prad.Text = String.Format("{0:0.0}", ((float)((Modbus.io.ain[0] / AIN_Meas_Mul) * MUL_Prad)));

            label_ADC1.Text = Modbus.io.ain[1].ToString();
            label_AIN1.Text = String.Format("{0:0.00}", ((float)Modbus.io.ain[1] / AIN_Meas_Mul));

            label_ADC2.Text = Modbus.io.ain[2].ToString();
            label_AIN2.Text = String.Format("{0:0.00}", ((float)Modbus.io.ain[2] / AIN_Mul));
            label_Sygnaly_CisnienieOdczyt.Text = String.Format("{0:0.0}", ((float)((Modbus.io.ain[2] / AIN_Mul) * MUL_Cisnienie)));

            label_ADC3.Text = Modbus.io.ain[3].ToString();
            label_AIN3.Text = String.Format("{0:0.00}", ((float)Modbus.io.ain[3] / AIN_Mul));

            label_ADC4.Text = Modbus.io.ain[4].ToString();
            label_AIN4.Text = String.Format("{0:0.00}", ((float)Modbus.io.ain[4] / AIN_Mul));
            label_Sygnaly_KontrolaPrzezWydmuch.Text = String.Format("{0:0.00}", ((float)((Modbus.io.ain[4] / AIN_Mul) * MUL_CisnienieWydmuchu)));

            label_DAC0.Text = Modbus.io.aout[0].ToString();
            label_AOUT0.Text = String.Format("{0:0.00}", ((float)Modbus.io.aout[0] / AOUT_Mul));
            label_Sygnaly_CisnienieZadane.Text = String.Format("{0:0.0}", ((float)((Modbus.io.aout[0] / AOUT_Mul) * MUL_Cisnienie)));

            label_DAC1.Text = Modbus.io.aout[1].ToString();
            label_AOUT1.Text = String.Format("{0:0.00}", ((float)Modbus.io.aout[1] / AOUT_Mul));

            label_pomiar_program.Text = PLC.ost_zgrzew_nr_programu.Word.ToString();
            label_pomiar_promil_ze_stepperem.Text = PLC.ost_zgrzew_prad.Word.ToString();

            //
            // Proces info
            //

            proces_PozycjaCylindra.Text = label_Sygnaly_PozycjaCylindra.Text = String.Format("{0:0.0}", ((float)((P_PozycjaCylindra() / AIN_Mul) * MUL_PozycjaCylindra)));
            proces_PozycjaGorna.Text = String.Format("{0:0.0}", ((float)(((float)PLC.cylinder_gora.Word / AIN_Mul) * MUL_PozycjaCylindra)));
            proces_PozycjaGornaTol.Text = String.Format("{0:0.0}", ((float)((float)PLC.cylinder_gora_tol.Word / AIN_Mul) * MUL_PozycjaCylindra));
            //proces_PozycjaGornaTol.Text = String.Format("{0:0.0}", ((float)((float)100 / AIN_Mul) * MUL_PozycjaCylindra));
            proces_PozycjaDolna.Text = String.Format("{0:0.0}", ((float)(((float)Zarzadzanie.par.prog[PLC.cykl_program_akt.Word].CylinderPositionDown / AIN_Mul) * MUL_PozycjaCylindra)));
            proces_PozycjaDolnaTol.Text = String.Format("{0:0.0}", ((float)(((float)Zarzadzanie.par.prog[PLC.cykl_program_akt.Word].CylinderPositionDownTolerance / AIN_Mul) * MUL_PozycjaCylindra)));

            proces_Wydmuch.Text = String.Format("{0:0.00}", ((float)(((float)P_Wydmuch() / AIN_Mul) * MUL_CisnienieWydmuchu)));
            proces_WydmuchWDole.Text = String.Format("{0:0.00}", ((float)(((float)Zarzadzanie.par.prog[PLC.cykl_program_akt.Word].ExhaustPressure / AIN_Mul) * MUL_CisnienieWydmuchu)));
            proces_WydmuchWDoleTol.Text = String.Format("{0:0.00}", ((float)(((float)Zarzadzanie.par.prog[PLC.cykl_program_akt.Word].ExhaustPressureTolerance / AIN_Mul) * MUL_CisnienieWydmuchu)));

            //if ((P_PozycjaCylindra() >= (PLC.cylinder_gora.Word - 30)) && (P_PozycjaCylindra() <= (PLC.cylinder_gora.Word + 30)))
            if (CylinderUGory())
                label_PozycjaGorna.BackColor = Color.Green;
            else
                label_PozycjaGorna.BackColor = Color.Gray;

            //if ((P_PozycjaCylindra() >= (Zarzadzanie.par.prog[PLC.cykl_program_akt.Word].CylinderPositionDown - Zarzadzanie.par.prog[PLC.cykl_program_akt.Word].CylinderPositionDownTolerance)) && (P_PozycjaCylindra() <= (Zarzadzanie.par.prog[PLC.cykl_program_akt.Word].CylinderPositionDown + Zarzadzanie.par.prog[PLC.cykl_program_akt.Word].CylinderPositionDownTolerance)))
            if (KontrolaNakretki_PozycjaCylindra())
                label_PozycjaDolna.BackColor = Color.Green;
            else
                label_PozycjaDolna.BackColor = Color.Gray;

            //if ((P_Wydmuch() >= (Zarzadzanie.par.prog[PLC.cykl_program_akt.Word].ExhaustPressure - Zarzadzanie.par.prog[PLC.cykl_program_akt.Word].ExhaustPressureTolerance)) && (P_Wydmuch() <= (Zarzadzanie.par.prog[PLC.cykl_program_akt.Word].ExhaustPressure + Zarzadzanie.par.prog[PLC.cykl_program_akt.Word].ExhaustPressureTolerance)))
            if (KontrolaNakretki_Wydmuch())
                label_WydmuchWDole.BackColor = Color.Green;
            else
                label_WydmuchWDole.BackColor = Color.Gray;

            //
            // Wyniki pomiarów
            //

            // Prąd
            label_pomiar_prad.Text = String.Format("{0:0.0}", (((float)(PLC.ost_zgrzew_pomiar_prad.Word) / AIN_Meas_Mul) * MUL_Prad));
            label_proces_prad_ref.Text = String.Format("{0:0.0}", (((float)(Zarzadzanie.par.prog[PLC.ost_zgrzew_nr_programu.Word].Iref) / AIN_Meas_Mul) * MUL_Prad));

            // Napięcie
            label_pomiar_napiecie.Text = String.Format("{0:0.0}", (((float)(PLC.ost_zgrzew_pomiar_nap.Word) / AIN_Meas_Mul)));
            label_proces_nap_ref.Text = String.Format("{0:0.0}", ((float)(Zarzadzanie.par.prog[PLC.ost_zgrzew_nr_programu.Word].Uref) / AIN_Meas_Mul));

            // Energia
            label_pomiar_energia.Text = String.Format("{0:0}", (((float)(PLC.ost_zgrzew_pomiar_energii.Word) / AIN_Meas_Mul / PRZEL_ENERGIA) * MUL_Prad));
            label_proces_ener_ref.Text = String.Format("{0:0}", (((float)(Zarzadzanie.par.prog[PLC.ost_zgrzew_nr_programu.Word].Eref) / AIN_Meas_Mul / PRZEL_ENERGIA) * MUL_Prad));

            // Wtopienie
            label_wtopienie.Text = String.Format("{0:0.00}", ((float)(((float)PLC.ost_zgrzew_pomiar_wtopienia.Word / AIN_Mul) * MUL_PozycjaCylindra)));
            label_proces_wtop_ref.Text = String.Format("{0:0.00}", ((float)(((float)Zarzadzanie.par.prog[PLC.ost_zgrzew_nr_programu.Word].Injection / AIN_Mul) * MUL_PozycjaCylindra)));
        }

        /* 
         * =========================================================================================================================================================
         * Nazwa:           Czujniki 
         * 
         * Przeznaczenie:   Funkcja wyswietlajaca stan odpytan w oknie procesu
         *                  
         * Parametry:       -
         * =========================================================================================================================================================
         */
        private void Czujniki()
        {
            ushort start_konfig = Zarzadzanie.par.prog[PLC.cykl_program_akt.Word].SensorsUpPositionConfig;
            ushort start_sygnaly = Zarzadzanie.par.prog[PLC.cykl_program_akt.Word].SensorsUpPositionSignals;
            ushort dk_konfig = Zarzadzanie.par.prog[PLC.cykl_program_akt.Word].SensorsDownPositionConfig;
            ushort dk_sygnaly = Zarzadzanie.par.prog[PLC.cykl_program_akt.Word].SensorsDownPositionSignals;

            //
            // Czujnik Start 01
            //
            if ((start_konfig & 0x01) == 0)
                labelCzujnikStart01.BackColor = Color.Gray;
            else
                if ((start_sygnaly & 0x01) == (Modbus.io.i[2] & 0x01))
                    labelCzujnikStart01.BackColor = Color.Green;
                else
                    labelCzujnikStart01.BackColor = Color.DarkRed;

            //
            // Czujnik Start 02
            //
            if ((start_konfig & 0x02) == 0)
                labelCzujnikStart02.BackColor = Color.Gray;
            else
                if ((start_sygnaly & 0x02) == (Modbus.io.i[2] & 0x02))
                    labelCzujnikStart02.BackColor = Color.Green;
                else
                    labelCzujnikStart02.BackColor = Color.DarkRed;

            //
            // Czujnik Start 03
            //
            if ((start_konfig & 0x04) == 0)
                labelCzujnikStart03.BackColor = Color.Gray;
            else
                if ((start_sygnaly & 0x04) == (Modbus.io.i[2] & 0x04))
                    labelCzujnikStart03.BackColor = Color.Green;
                else
                    labelCzujnikStart03.BackColor = Color.DarkRed;

            //
            // Czujnik Start 04
            //
            if ((start_konfig & 0x08) == 0)
                labelCzujnikStart04.BackColor = Color.Gray;
            else
                if ((start_sygnaly & 0x08) == (Modbus.io.i[2] & 0x08))
                    labelCzujnikStart04.BackColor = Color.Green;
                else
                    labelCzujnikStart04.BackColor = Color.DarkRed;

            //
            // Czujnik Start 05
            //
            if ((start_konfig & 0x10) == 0)
                labelCzujnikStart05.BackColor = Color.Gray;
            else
                if ((start_sygnaly & 0x10) == (Modbus.io.i[2] & 0x10))
                    labelCzujnikStart05.BackColor = Color.Green;
                else
                    labelCzujnikStart05.BackColor = Color.DarkRed;

            //
            // Czujnik Start 06
            //
            if ((start_konfig & 0x20) == 0)
                labelCzujnikStart06.BackColor = Color.Gray;
            else
                if ((start_sygnaly & 0x20) == (Modbus.io.i[2] & 0x20))
                    labelCzujnikStart06.BackColor = Color.Green;
                else
                    labelCzujnikStart06.BackColor = Color.DarkRed;

            //
            // Czujnik Start 07
            //
            if ((start_konfig & 0x40) == 0)
                labelCzujnikStart07.BackColor = Color.Gray;
            else
                if ((start_sygnaly & 0x40) == (Modbus.io.i[2] & 0x40))
                    labelCzujnikStart07.BackColor = Color.Green;
                else
                    labelCzujnikStart07.BackColor = Color.DarkRed;

            //
            // Czujnik Start 08
            //
            if ((start_konfig & 0x80) == 0)
                labelCzujnikStart08.BackColor = Color.Gray;
            else
                if ((start_sygnaly & 0x80) == (Modbus.io.i[2] & 0x80))
                    labelCzujnikStart08.BackColor = Color.Green;
                else
                    labelCzujnikStart08.BackColor = Color.DarkRed;

            //
            // Czujnik Start 09
            //
            if ((start_konfig & 0x0100) == 0)
                labelCzujnikStart09.BackColor = Color.Gray;
            else
                if (((start_sygnaly & 0x0100) >> 8) == (Modbus.io.i[3] & 0x01))
                    labelCzujnikStart09.BackColor = Color.Green;
                else
                    labelCzujnikStart09.BackColor = Color.DarkRed;

            //
            // Czujnik Start 10
            //
            if ((start_konfig & 0x0200) == 0)
                labelCzujnikStart10.BackColor = Color.Gray;
            else
                if (((start_sygnaly & 0x0200) >> 8) == (Modbus.io.i[3] & 0x02))
                    labelCzujnikStart10.BackColor = Color.Green;
                else
                    labelCzujnikStart10.BackColor = Color.DarkRed;

            //
            // Czujnik Start 11
            //
            if ((start_konfig & 0x0400) == 0)
                labelCzujnikStart11.BackColor = Color.Gray;
            else
                if (((start_sygnaly & 0x0400) >> 8) == (Modbus.io.i[3] & 0x04))
                    labelCzujnikStart11.BackColor = Color.Green;
                else
                    labelCzujnikStart11.BackColor = Color.DarkRed;

            //
            // Czujnik Start 12
            //
            if ((start_konfig & 0x0800) == 0)
                labelCzujnikStart12.BackColor = Color.Gray;
            else
                if (((start_sygnaly & 0x0800) >> 8) == (Modbus.io.i[3] & 0x08))
                    labelCzujnikStart12.BackColor = Color.Green;
                else
                    labelCzujnikStart12.BackColor = Color.DarkRed;

            //
            // Czujnik Start 13
            //
            if ((start_konfig & 0x1000) == 0)
                labelCzujnikStart13.BackColor = Color.Gray;
            else
                if (((start_sygnaly & 0x1000) >> 8) == (Modbus.io.i[3] & 0x10))
                    labelCzujnikStart13.BackColor = Color.Green;
                else
                    labelCzujnikStart13.BackColor = Color.DarkRed;

            //
            // Czujnik Start 14
            //
            if ((start_konfig & 0x2000) == 0)
                labelCzujnikStart14.BackColor = Color.Gray;
            else
                if (((start_sygnaly & 0x2000) >> 8) == (Modbus.io.i[3] & 0x20))
                    labelCzujnikStart14.BackColor = Color.Green;
                else
                    labelCzujnikStart14.BackColor = Color.DarkRed;

            //
            // Czujnik DK 01
            //
            if ((dk_konfig & 0x01) == 0)
                labelCzujnikDK01.BackColor = Color.Gray;
            else
                if ((dk_sygnaly & 0x01) == (Modbus.io.i[2] & 0x01))
                    labelCzujnikDK01.BackColor = Color.Green;
                else
                    labelCzujnikDK01.BackColor = Color.DarkRed;

            //
            // Czujnik DK 02
            //
            if ((dk_konfig & 0x02) == 0)
                labelCzujnikDK02.BackColor = Color.Gray;
            else
                if ((dk_sygnaly & 0x02) == (Modbus.io.i[2] & 0x02))
                    labelCzujnikDK02.BackColor = Color.Green;
                else
                    labelCzujnikDK02.BackColor = Color.DarkRed;

            //
            // Czujnik DK 03
            //
            if ((dk_konfig & 0x04) == 0)
                labelCzujnikDK03.BackColor = Color.Gray;
            else
                if ((dk_sygnaly & 0x04) == (Modbus.io.i[2] & 0x04))
                    labelCzujnikDK03.BackColor = Color.Green;
                else
                    labelCzujnikDK03.BackColor = Color.DarkRed;

            //
            // Czujnik DK 04
            //
            if ((dk_konfig & 0x08) == 0)
                labelCzujnikDK04.BackColor = Color.Gray;
            else
                if ((dk_sygnaly & 0x08) == (Modbus.io.i[2] & 0x08))
                    labelCzujnikDK04.BackColor = Color.Green;
                else
                    labelCzujnikDK04.BackColor = Color.DarkRed;

            //
            // Czujnik DK 05
            //
            if ((dk_konfig & 0x10) == 0)
                labelCzujnikDK05.BackColor = Color.Gray;
            else
                if ((dk_sygnaly & 0x10) == (Modbus.io.i[2] & 0x10))
                    labelCzujnikDK05.BackColor = Color.Green;
                else
                    labelCzujnikDK05.BackColor = Color.DarkRed;

            //
            // Czujnik DK 06
            //
            if ((dk_konfig & 0x20) == 0)
                labelCzujnikDK06.BackColor = Color.Gray;
            else
                if ((dk_sygnaly & 0x20) == (Modbus.io.i[2] & 0x20))
                    labelCzujnikDK06.BackColor = Color.Green;
                else
                    labelCzujnikDK06.BackColor = Color.DarkRed;

            //
            // Czujnik DK 07
            //
            if ((dk_konfig & 0x40) == 0)
                labelCzujnikDK07.BackColor = Color.Gray;
            else
                if ((dk_sygnaly & 0x40) == (Modbus.io.i[2] & 0x40))
                    labelCzujnikDK07.BackColor = Color.Green;
                else
                    labelCzujnikDK07.BackColor = Color.DarkRed;

            //
            // Czujnik DK 08
            //
            if ((dk_konfig & 0x80) == 0)
                labelCzujnikDK08.BackColor = Color.Gray;
            else
                if ((dk_sygnaly & 0x80) == (Modbus.io.i[2] & 0x80))
                    labelCzujnikDK08.BackColor = Color.Green;
                else
                    labelCzujnikDK08.BackColor = Color.DarkRed;

            //
            // Czujnik DK 09
            //
            if ((dk_konfig & 0x0100) == 0)
                labelCzujnikDK09.BackColor = Color.Gray;
            else
                if (((dk_sygnaly & 0x0100) >> 8) == (Modbus.io.i[3] & 0x01))
                    labelCzujnikDK09.BackColor = Color.Green;
                else
                    labelCzujnikDK09.BackColor = Color.DarkRed;

            //
            // Czujnik DK 10
            //
            if ((dk_konfig & 0x0200) == 0)
                labelCzujnikDK10.BackColor = Color.Gray;
            else
                if (((dk_sygnaly & 0x0200) >> 8) == (Modbus.io.i[3] & 0x02))
                    labelCzujnikDK10.BackColor = Color.Green;
                else
                    labelCzujnikDK10.BackColor = Color.DarkRed;

            //
            // Czujnik DK 11
            //
            if ((dk_konfig & 0x0400) == 0)
                labelCzujnikDK11.BackColor = Color.Gray;
            else
                if (((dk_sygnaly & 0x0400) >> 8) == (Modbus.io.i[3] & 0x04))
                    labelCzujnikDK11.BackColor = Color.Green;
                else
                    labelCzujnikDK11.BackColor = Color.DarkRed;

            //
            // Czujnik DK 12
            //
            if ((dk_konfig & 0x0800) == 0)
                labelCzujnikDK12.BackColor = Color.Gray;
            else
                if (((dk_sygnaly & 0x0800) >> 8) == (Modbus.io.i[3] & 0x08))
                    labelCzujnikDK12.BackColor = Color.Green;
                else
                    labelCzujnikDK12.BackColor = Color.DarkRed;

            //
            // Czujnik DK 13
            //
            if ((dk_konfig & 0x1000) == 0)
                labelCzujnikDK13.BackColor = Color.Gray;
            else
                if (((dk_sygnaly & 0x1000) >> 8) == (Modbus.io.i[3] & 0x10))
                    labelCzujnikDK13.BackColor = Color.Green;
                else
                    labelCzujnikDK13.BackColor = Color.DarkRed;

            //
            // Czujnik DK 14
            //
            if ((dk_konfig & 0x2000) == 0)
                labelCzujnikDK14.BackColor = Color.Gray;
            else
                if (((dk_sygnaly & 0x2000) >> 8) == (Modbus.io.i[3] & 0x20))
                    labelCzujnikDK14.BackColor = Color.Green;
                else
                    labelCzujnikDK14.BackColor = Color.DarkRed;

        }


        /* 
         * =========================================================================================================================================================
         * Nazwa:           ParametryCzujniki 
         * 
         * Przeznaczenie:   Funkcja wyswietlajaca konfigurację czujników z programu
         *                  
         * Parametry:       Numer programu
         * =========================================================================================================================================================
         */
        private void Wyswietlanie_ParametryCylindry(ushort prog)
        {
            ushort cylindry = Zarzadzanie.par.prog[prog].CylinderNumber;
            ushort zawory = Zarzadzanie.par.prog[prog].Valves;
            ushort konfig = Zarzadzanie.par.prog[prog].ProgramConfiguration;

            //
            // Cylindry
            //

            if ((cylindry & 0x01) == 0)
                checkBox_Cylinder_01.Checked = false;
            else
                checkBox_Cylinder_01.Checked = true;

            if ((cylindry & 0x02) == 0)
                checkBox_Cylinder_01_DodSila.Checked = false;
            else
                checkBox_Cylinder_01_DodSila.Checked = true;

            if ((cylindry & 0x04) == 0)
                checkBox_Cylinder_02.Checked = false;
            else
                checkBox_Cylinder_02.Checked = true;

            if ((cylindry & 0x08) == 0)
                checkBox_Cylinder_03.Checked = false;
            else
                checkBox_Cylinder_03.Checked = true;

            if ((cylindry & 0x10) == 0)
                checkBox_Cylinder_04.Checked = false;
            else
                checkBox_Cylinder_04.Checked = true;

            //
            // Zawory
            //

            if ((zawory & 0x01) == 0)
                checkBox_Zawor_11.Checked = false;
            else
                checkBox_Zawor_11.Checked = true;

            if ((zawory & 0x02) == 0)
                checkBox_Zawor_12.Checked = false;
            else
                checkBox_Zawor_12.Checked = true;

            if ((zawory & 0x04) == 0)
                checkBox_Zawor_21.Checked = false;
            else
                checkBox_Zawor_21.Checked = true;

            if ((zawory & 0x08) == 0)
                checkBox_Zawor_22.Checked = false;
            else
                checkBox_Zawor_22.Checked = true;

            if ((zawory & 0x10) == 0)
                checkBox_Zawor_31.Checked = false;
            else
                checkBox_Zawor_31.Checked = true;

            if ((zawory & 0x20) == 0)
                checkBox_Zawor_32.Checked = false;
            else
                checkBox_Zawor_32.Checked = true;

            if ((zawory & 0x40) == 0)
                checkBox_Zawor_41.Checked = false;
            else
                checkBox_Zawor_41.Checked = true;

            if ((zawory & 0x80) == 0)
                checkBox_Zawor_42.Checked = false;
            else
                checkBox_Zawor_42.Checked = true;

            if ((konfig & 0x01) == 0)
                checkBox_PulpitDwureczny.Checked = true;
            else
                checkBox_PulpitDwureczny.Checked = false;

            if ((konfig & 0x08) == 0)
            {
                radioButton_Wyblokowanie_WDole.Checked = true;
                radioButton_Wyblokowanie_WGorze.Checked = false;
            }
            else
            {
                radioButton_Wyblokowanie_WDole.Checked = false;
                radioButton_Wyblokowanie_WGorze.Checked = true;
            }

        }

        /* 
         * =========================================================================================================================================================
         * Nazwa:           ParametryCzujniki 
         * 
         * Przeznaczenie:   Funkcja wyswietlajaca konfigurację czujników z programu
         *                  
         * Parametry:       Numer programu
         * =========================================================================================================================================================
         */
        private void Wyswietlanie_ParametryCzujnikiStart(ushort prog)
        {
            ushort start_konfig = Zarzadzanie.par.prog[prog].SensorsUpPositionConfig;
            ushort start_sygnaly = Zarzadzanie.par.prog[prog].SensorsUpPositionSignals;

            //
            // Czujnik Start 01
            //
            if ((start_konfig & 0x01) == 0)
            {
                checkBoxStartKonfig_01.Checked = false;
                radioButtonCzujnikStart_01_0.Checked = false;
                radioButtonCzujnikStart_01_1.Checked = false;
            }
            else
            {
                checkBoxStartKonfig_01.Checked = true;
                if ((start_sygnaly & 0x01) > 0)
                {
                    radioButtonCzujnikStart_01_0.Checked = false;
                    radioButtonCzujnikStart_01_1.Checked = true;
                }
                else
                {
                    radioButtonCzujnikStart_01_0.Checked = true;
                    radioButtonCzujnikStart_01_1.Checked = false;
                }
            }

            //
            // Czujnik Start 02
            //
            if ((start_konfig & 0x02) == 0)
            {
                checkBoxStartKonfig_02.Checked = false;
                radioButtonCzujnikStart_02_0.Checked = false;
                radioButtonCzujnikStart_02_1.Checked = false;
            }
            else
            {
                checkBoxStartKonfig_02.Checked = true;
                if ((start_sygnaly & 0x02) > 0)
                {
                    radioButtonCzujnikStart_02_0.Checked = false;
                    radioButtonCzujnikStart_02_1.Checked = true;
                }
                else
                {
                    radioButtonCzujnikStart_02_0.Checked = true;
                    radioButtonCzujnikStart_02_1.Checked = false;
                }
            }

            //
            // Czujnik Start 03
            //
            if ((start_konfig & 0x04) == 0)
            {
                checkBoxStartKonfig_03.Checked = false;
                radioButtonCzujnikStart_03_0.Checked = false;
                radioButtonCzujnikStart_03_1.Checked = false;
            }
            else
            {
                checkBoxStartKonfig_03.Checked = true;
                if ((start_sygnaly & 0x04) > 0)
                {
                    radioButtonCzujnikStart_03_0.Checked = false;
                    radioButtonCzujnikStart_03_1.Checked = true;
                }
                else
                {
                    radioButtonCzujnikStart_03_0.Checked = true;
                    radioButtonCzujnikStart_03_1.Checked = false;
                }
            }

            //
            // Czujnik Start 04
            //
            if ((start_konfig & 0x08) == 0)
            {
                checkBoxStartKonfig_04.Checked = false;
                radioButtonCzujnikStart_04_0.Checked = false;
                radioButtonCzujnikStart_04_1.Checked = false;
            }
            else
            {
                checkBoxStartKonfig_04.Checked = true;
                if ((start_sygnaly & 0x08) > 0)
                {
                    radioButtonCzujnikStart_04_0.Checked = false;
                    radioButtonCzujnikStart_04_1.Checked = true;
                }
                else
                {
                    radioButtonCzujnikStart_04_0.Checked = true;
                    radioButtonCzujnikStart_04_1.Checked = false;
                }
            }

            //
            // Czujnik Start 05
            //
            if ((start_konfig & 0x10) == 0)
            {
                checkBoxStartKonfig_05.Checked = false;
                radioButtonCzujnikStart_05_0.Checked = false;
                radioButtonCzujnikStart_05_1.Checked = false;
            }
            else
            {
                checkBoxStartKonfig_05.Checked = true;
                if ((start_sygnaly & 0x10) > 0)
                {
                    radioButtonCzujnikStart_05_0.Checked = false;
                    radioButtonCzujnikStart_05_1.Checked = true;
                }
                else
                {
                    radioButtonCzujnikStart_05_0.Checked = true;
                    radioButtonCzujnikStart_05_1.Checked = false;
                }
            }

            //
            // Czujnik Start 06
            //
            if ((start_konfig & 0x20) == 0)
            {
                checkBoxStartKonfig_06.Checked = false;
                radioButtonCzujnikStart_06_0.Checked = false;
                radioButtonCzujnikStart_06_1.Checked = false;
            }
            else
            {
                checkBoxStartKonfig_06.Checked = true;
                if ((start_sygnaly & 0x20) > 0)
                {
                    radioButtonCzujnikStart_06_0.Checked = false;
                    radioButtonCzujnikStart_06_1.Checked = true;
                }
                else
                {
                    radioButtonCzujnikStart_06_0.Checked = true;
                    radioButtonCzujnikStart_06_1.Checked = false;
                }
            }

            //
            // Czujnik Start 07
            //
            if ((start_konfig & 0x40) == 0)
            {
                checkBoxStartKonfig_07.Checked = false;
                radioButtonCzujnikStart_07_0.Checked = false;
                radioButtonCzujnikStart_07_1.Checked = false;
            }
            else
            {
                checkBoxStartKonfig_07.Checked = true;
                if ((start_sygnaly & 0x40) > 0)
                {
                    radioButtonCzujnikStart_07_0.Checked = false;
                    radioButtonCzujnikStart_07_1.Checked = true;
                }
                else
                {
                    radioButtonCzujnikStart_07_0.Checked = true;
                    radioButtonCzujnikStart_07_1.Checked = false;
                }
            }

            //
            // Czujnik Start 08
            //
            if ((start_konfig & 0x80) == 0)
            {
                checkBoxStartKonfig_08.Checked = false;
                radioButtonCzujnikStart_08_0.Checked = false;
                radioButtonCzujnikStart_08_1.Checked = false;
            }
            else
            {
                checkBoxStartKonfig_08.Checked = true;
                if ((start_sygnaly & 0x80) > 0)
                {
                    radioButtonCzujnikStart_08_0.Checked = false;
                    radioButtonCzujnikStart_08_1.Checked = true;
                }
                else
                {
                    radioButtonCzujnikStart_08_0.Checked = true;
                    radioButtonCzujnikStart_08_1.Checked = false;
                }
            }

            //
            // Czujnik Start 09
            //
            if ((start_konfig & 0x100) == 0)
            {
                checkBoxStartKonfig_09.Checked = false;
                radioButtonCzujnikStart_09_0.Checked = false;
                radioButtonCzujnikStart_09_1.Checked = false;
            }
            else
            {
                checkBoxStartKonfig_09.Checked = true;
                if ((start_sygnaly & 0x100) > 0)
                {
                    radioButtonCzujnikStart_09_0.Checked = false;
                    radioButtonCzujnikStart_09_1.Checked = true;
                }
                else
                {
                    radioButtonCzujnikStart_09_0.Checked = true;
                    radioButtonCzujnikStart_09_1.Checked = false;
                }
            }

            //
            // Czujnik Start 10
            //
            if ((start_konfig & 0x200) == 0)
            {
                checkBoxStartKonfig_10.Checked = false;
                radioButtonCzujnikStart_10_0.Checked = false;
                radioButtonCzujnikStart_10_1.Checked = false;
            }
            else
            {
                checkBoxStartKonfig_10.Checked = true;
                if ((start_sygnaly & 0x200) > 0)
                {
                    radioButtonCzujnikStart_10_0.Checked = false;
                    radioButtonCzujnikStart_10_1.Checked = true;
                }
                else
                {
                    radioButtonCzujnikStart_10_0.Checked = true;
                    radioButtonCzujnikStart_10_1.Checked = false;
                }
            }

            //
            // Czujnik Start 11
            //
            if ((start_konfig & 0x400) == 0)
            {
                checkBoxStartKonfig_11.Checked = false;
                radioButtonCzujnikStart_11_0.Checked = false;
                radioButtonCzujnikStart_11_1.Checked = false;
            }
            else
            {
                checkBoxStartKonfig_11.Checked = true;
                if ((start_sygnaly & 0x400) > 0)
                {
                    radioButtonCzujnikStart_11_0.Checked = false;
                    radioButtonCzujnikStart_11_1.Checked = true;
                }
                else
                {
                    radioButtonCzujnikStart_11_0.Checked = true;
                    radioButtonCzujnikStart_11_1.Checked = false;
                }
            }

            //
            // Czujnik Start 12
            //
            if ((start_konfig & 0x800) == 0)
            {
                checkBoxStartKonfig_12.Checked = false;
                radioButtonCzujnikStart_12_0.Checked = false;
                radioButtonCzujnikStart_12_1.Checked = false;
            }
            else
            {
                checkBoxStartKonfig_12.Checked = true;
                if ((start_sygnaly & 0x800) > 0)
                {
                    radioButtonCzujnikStart_12_0.Checked = false;
                    radioButtonCzujnikStart_12_1.Checked = true;
                }
                else
                {
                    radioButtonCzujnikStart_12_0.Checked = true;
                    radioButtonCzujnikStart_12_1.Checked = false;
                }
            }

            //
            // Czujnik Start 13
            //
            if ((start_konfig & 0x1000) == 0)
            {
                checkBoxStartKonfig_13.Checked = false;
                radioButtonCzujnikStart_13_0.Checked = false;
                radioButtonCzujnikStart_13_1.Checked = false;
            }
            else
            {
                checkBoxStartKonfig_13.Checked = true;
                if ((start_sygnaly & 0x1000) > 0)
                {
                    radioButtonCzujnikStart_13_0.Checked = false;
                    radioButtonCzujnikStart_13_1.Checked = true;
                }
                else
                {
                    radioButtonCzujnikStart_13_0.Checked = true;
                    radioButtonCzujnikStart_13_1.Checked = false;
                }
            }

            //
            // Czujnik Start 14
            //
            if ((start_konfig & 0x2000) == 0)
            {
                checkBoxStartKonfig_14.Checked = false;
                radioButtonCzujnikStart_14_0.Checked = false;
                radioButtonCzujnikStart_14_1.Checked = false;
            }
            else
            {
                checkBoxStartKonfig_14.Checked = true;
                if ((start_sygnaly & 0x2000) > 0)
                {
                    radioButtonCzujnikStart_14_0.Checked = false;
                    radioButtonCzujnikStart_14_1.Checked = true;
                }
                else
                {
                    radioButtonCzujnikStart_14_0.Checked = true;
                    radioButtonCzujnikStart_14_1.Checked = false;
                }
            }
        }

        private void Wyswietlanie_ParametryCzujnikiDK(ushort prog)
        {
            ushort dk_konfig = Zarzadzanie.par.prog[prog].SensorsDownPositionConfig;
            ushort dk_sygnaly = Zarzadzanie.par.prog[prog].SensorsDownPositionSignals;

            //
            // Czujnik DK 01
            //
            if ((dk_konfig & 0x01) == 0)
            {
                checkBoxDKKonfig_001.Checked = false;
                radioButtonCzujnikDK_01_0.Checked = false;
                radioButtonCzujnikDK_01_1.Checked = false;
            }
            else
            {
                checkBoxDKKonfig_001.Checked = true;
                if ((dk_sygnaly & 0x01) > 0)
                {
                    radioButtonCzujnikDK_01_0.Checked = false;
                    radioButtonCzujnikDK_01_1.Checked = true;
                }
                else
                {
                    radioButtonCzujnikDK_01_0.Checked = true;
                    radioButtonCzujnikDK_01_1.Checked = false;
                }
            }

            //
            // Czujnik DK 02
            //
            if ((dk_konfig & 0x02) == 0)
            {
                checkBoxDKKonfig_02.Checked = false;
                radioButtonCzujnikDK_02_0.Checked = false;
                radioButtonCzujnikDK_02_1.Checked = false;
            }
            else
            {
                checkBoxDKKonfig_02.Checked = true;
                if ((dk_sygnaly & 0x02) > 0)
                {
                    radioButtonCzujnikDK_02_0.Checked = false;
                    radioButtonCzujnikDK_02_1.Checked = true;
                }
                else
                {
                    radioButtonCzujnikDK_02_0.Checked = true;
                    radioButtonCzujnikDK_02_1.Checked = false;
                }
            }

            //
            // Czujnik DK 03
            //
            if ((dk_konfig & 0x04) == 0)
            {
                checkBoxDKKonfig_03.Checked = false;
                radioButtonCzujnikDK_03_0.Checked = false;
                radioButtonCzujnikDK_03_1.Checked = false;
            }
            else
            {
                checkBoxDKKonfig_03.Checked = true;
                if ((dk_sygnaly & 0x04) > 0)
                {
                    radioButtonCzujnikDK_03_0.Checked = false;
                    radioButtonCzujnikDK_03_1.Checked = true;
                }
                else
                {
                    radioButtonCzujnikDK_03_0.Checked = true;
                    radioButtonCzujnikDK_03_1.Checked = false;
                }
            }

            //
            // Czujnik DK 04
            //
            if ((dk_konfig & 0x08) == 0)
            {
                checkBoxDKKonfig_04.Checked = false;
                radioButtonCzujnikDK_04_0.Checked = false;
                radioButtonCzujnikDK_04_1.Checked = false;
            }
            else
            {
                checkBoxDKKonfig_04.Checked = true;
                if ((dk_sygnaly & 0x08) > 0)
                {
                    radioButtonCzujnikDK_04_0.Checked = false;
                    radioButtonCzujnikDK_04_1.Checked = true;
                }
                else
                {
                    radioButtonCzujnikDK_04_0.Checked = true;
                    radioButtonCzujnikDK_04_1.Checked = false;
                }
            }

            //
            // Czujnik DK 05
            //
            if ((dk_konfig & 0x10) == 0)
            {
                checkBoxDKKonfig_05.Checked = false;
                radioButtonCzujnikDK_05_0.Checked = false;
                radioButtonCzujnikDK_05_1.Checked = false;
            }
            else
            {
                checkBoxDKKonfig_05.Checked = true;
                if ((dk_sygnaly & 0x10) > 0)
                {
                    radioButtonCzujnikDK_05_0.Checked = false;
                    radioButtonCzujnikDK_05_1.Checked = true;
                }
                else
                {
                    radioButtonCzujnikDK_05_0.Checked = true;
                    radioButtonCzujnikDK_05_1.Checked = false;
                }
            }

            //
            // Czujnik DK 06
            //
            if ((dk_konfig & 0x20) == 0)
            {
                checkBoxDKKonfig_06.Checked = false;
                radioButtonCzujnikDK_06_0.Checked = false;
                radioButtonCzujnikDK_06_1.Checked = false;
            }
            else
            {
                checkBoxDKKonfig_06.Checked = true;
                if ((dk_sygnaly & 0x20) > 0)
                {
                    radioButtonCzujnikDK_06_0.Checked = false;
                    radioButtonCzujnikDK_06_1.Checked = true;
                }
                else
                {
                    radioButtonCzujnikDK_06_0.Checked = true;
                    radioButtonCzujnikDK_06_1.Checked = false;
                }
            }

            //
            // Czujnik DK 07
            //
            if ((dk_konfig & 0x40) == 0)
            {
                checkBoxDKKonfig_07.Checked = false;
                radioButtonCzujnikDK_07_0.Checked = false;
                radioButtonCzujnikDK_07_1.Checked = false;
            }
            else
            {
                checkBoxDKKonfig_07.Checked = true;
                if ((dk_sygnaly & 0x40) > 0)
                {
                    radioButtonCzujnikDK_07_0.Checked = false;
                    radioButtonCzujnikDK_07_1.Checked = true;
                }
                else
                {
                    radioButtonCzujnikDK_07_0.Checked = true;
                    radioButtonCzujnikDK_07_1.Checked = false;
                }
            }

            //
            // Czujnik DK 08
            //
            if ((dk_konfig & 0x80) == 0)
            {
                checkBoxDKKonfig_08.Checked = false;
                radioButtonCzujnikDK_08_0.Checked = false;
                radioButtonCzujnikDK_08_1.Checked = false;
            }
            else
            {
                checkBoxDKKonfig_08.Checked = true;
                if ((dk_sygnaly & 0x80) > 0)
                {
                    radioButtonCzujnikDK_08_0.Checked = false;
                    radioButtonCzujnikDK_08_1.Checked = true;
                }
                else
                {
                    radioButtonCzujnikDK_08_0.Checked = true;
                    radioButtonCzujnikDK_08_1.Checked = false;
                }
            }

            //
            // Czujnik DK 09
            //
            if ((dk_konfig & 0x100) == 0)
            {
                checkBoxDKKonfig_09.Checked = false;
                radioButtonCzujnikDK_09_0.Checked = false;
                radioButtonCzujnikDK_09_1.Checked = false;
            }
            else
            {
                checkBoxDKKonfig_09.Checked = true;
                if ((dk_sygnaly & 0x100) > 0)
                {
                    radioButtonCzujnikDK_09_0.Checked = false;
                    radioButtonCzujnikDK_09_1.Checked = true;
                }
                else
                {
                    radioButtonCzujnikDK_09_0.Checked = true;
                    radioButtonCzujnikDK_09_1.Checked = false;
                }
            }

            //
            // Czujnik DK 10
            //
            if ((dk_konfig & 0x200) == 0)
            {
                checkBoxDKKonfig_10.Checked = false;
                radioButtonCzujnikDK_10_0.Checked = false;
                radioButtonCzujnikDK_10_1.Checked = false;
            }
            else
            {
                checkBoxDKKonfig_10.Checked = true;
                if ((dk_sygnaly & 0x200) > 0)
                {
                    radioButtonCzujnikDK_10_0.Checked = false;
                    radioButtonCzujnikDK_10_1.Checked = true;
                }
                else
                {
                    radioButtonCzujnikDK_10_0.Checked = true;
                    radioButtonCzujnikDK_10_1.Checked = false;
                }
            }

            //
            // Czujnik DK 11
            //
            if ((dk_konfig & 0x400) == 0)
            {
                checkBoxDKKonfig_11.Checked = false;
                radioButtonCzujnikDK_11_0.Checked = false;
                radioButtonCzujnikDK_11_1.Checked = false;
            }
            else
            {
                checkBoxDKKonfig_11.Checked = true;
                if ((dk_sygnaly & 0x400) > 0)
                {
                    radioButtonCzujnikDK_11_0.Checked = false;
                    radioButtonCzujnikDK_11_1.Checked = true;
                }
                else
                {
                    radioButtonCzujnikDK_11_0.Checked = true;
                    radioButtonCzujnikDK_11_1.Checked = false;
                }
            }

            //
            // Czujnik DK 12
            //
            if ((dk_konfig & 0x800) == 0)
            {
                checkBoxDKKonfig_12.Checked = false;
                radioButtonCzujnikDK_12_0.Checked = false;
                radioButtonCzujnikDK_12_1.Checked = false;
            }
            else
            {
                checkBoxDKKonfig_12.Checked = true;
                if ((dk_sygnaly & 0x800) > 0)
                {
                    radioButtonCzujnikDK_12_0.Checked = false;
                    radioButtonCzujnikDK_12_1.Checked = true;
                }
                else
                {
                    radioButtonCzujnikDK_12_0.Checked = true;
                    radioButtonCzujnikDK_12_1.Checked = false;
                }
            }

            //
            // Czujnik DK 13
            //
            if ((dk_konfig & 0x1000) == 0)
            {
                checkBoxDKKonfig_13.Checked = false;
                radioButtonCzujnikDK_13_0.Checked = false;
                radioButtonCzujnikDK_13_1.Checked = false;
            }
            else
            {
                checkBoxDKKonfig_13.Checked = true;
                if ((dk_sygnaly & 0x1000) > 0)
                {
                    radioButtonCzujnikDK_13_0.Checked = false;
                    radioButtonCzujnikDK_13_1.Checked = true;
                }
                else
                {
                    radioButtonCzujnikDK_13_0.Checked = true;
                    radioButtonCzujnikDK_13_1.Checked = false;
                }
            }

            //
            // Czujnik DK 14
            //
            if ((dk_konfig & 0x2000) == 0)
            {
                checkBoxDKKonfig_14.Checked = false;
                radioButtonCzujnikDK_14_0.Checked = false;
                radioButtonCzujnikDK_14_1.Checked = false;
            }
            else
            {
                checkBoxDKKonfig_14.Checked = true;
                if ((dk_sygnaly & 0x2000) > 0)
                {
                    radioButtonCzujnikDK_14_0.Checked = false;
                    radioButtonCzujnikDK_14_1.Checked = true;
                }
                else
                {
                    radioButtonCzujnikDK_14_0.Checked = true;
                    radioButtonCzujnikDK_14_1.Checked = false;
                }
            }

        }

        /* 
        * =========================================================================================================================================================
        * Nazwa:           ...
        * 
        * Przeznaczenie:   Zmieny indeksów w COMBO
        *                  
        * Parametry:       -
        * =========================================================================================================================================================
        */
        private void comboNrProgramu_SelectedIndexChanged(object sender, EventArgs e)
        {
        }

        private void comboParametr_SelectedIndexChanged(object sender, EventArgs e)
        {
        }


        // =========================================================================================================================================================
        private void checkBox_ListaParametrow_CheckStateChanged(object sender, EventArgs e)
        {
            WyswitlenieListy();
        }

        // =========================================================================================================================================================
        private void checkBoxStartKonfig_01_Click(object sender, EventArgs e)
        {
            Zarzadzanie.par.prog[ActiveProgEdit].SensorsUpPositionConfig ^= 0x0001;
            WyslijProgram();
            Tools.LogParam("Zmiana konfiguracji czujnika 01");
        }
        private void radioButtonCzujnikStart_01_1_Click(object sender, EventArgs e)
        {
            Zarzadzanie.par.prog[ActiveProgEdit].SensorsUpPositionSignals |= 0x0001;
            WyslijProgram();
            Tools.LogParam("Zmiana konfiguracji czujnika 01");
        }
        private void radioButtonCzujnikStart_01_0_Click(object sender, EventArgs e)
        {
            Zarzadzanie.par.prog[ActiveProgEdit].SensorsUpPositionSignals &= 0xfffe;
            WyslijProgram();
            Tools.LogParam("Zmiana konfiguracji czujnika 01");
        }

        // =========================================================================================================================================================
        private void checkBoxStartKonfig_02_Click(object sender, EventArgs e)
        {
            Zarzadzanie.par.prog[ActiveProgEdit].SensorsUpPositionConfig ^= 0x0002;
            WyslijProgram();
            Tools.LogParam("Zmiana konfiguracji czujnika 02");
        }

        private void radioButtonCzujnikStart_02_1_Click(object sender, EventArgs e)
        {
            Zarzadzanie.par.prog[ActiveProgEdit].SensorsUpPositionSignals |= 0x0002;
            WyslijProgram();
            Tools.LogParam("Zmiana konfiguracji czujnika 02");
        }

        private void radioButtonCzujnikStart_02_0_Click(object sender, EventArgs e)
        {
            Zarzadzanie.par.prog[ActiveProgEdit].SensorsUpPositionSignals &= 0xfffd;
            WyslijProgram();
            Tools.LogParam("Zmiana konfiguracji czujnika 02");
        }

        // =========================================================================================================================================================
        private void checkBoxStartKonfig_03_Click(object sender, EventArgs e)
        {
            Zarzadzanie.par.prog[ActiveProgEdit].SensorsUpPositionConfig ^= 0x0004;
            WyslijProgram();
            Tools.LogParam("Zmiana konfiguracji czujnika 03");
        }

        private void radioButtonCzujnikStart_03_1_Click(object sender, EventArgs e)
        {
            Zarzadzanie.par.prog[ActiveProgEdit].SensorsUpPositionSignals |= 0x0004;
            WyslijProgram();
            Tools.LogParam("Zmiana konfiguracji czujnika 03");
        }

        private void radioButtonCzujnikStart_03_0_Click(object sender, EventArgs e)
        {
            Zarzadzanie.par.prog[ActiveProgEdit].SensorsUpPositionSignals &= 0xfffb;
            WyslijProgram();
            Tools.LogParam("Zmiana konfiguracji czujnika 03");
        }

        // =========================================================================================================================================================
        private void checkBoxStartKonfig_04_Click(object sender, EventArgs e)
        {
            Zarzadzanie.par.prog[ActiveProgEdit].SensorsUpPositionConfig ^= 0x0008;
            WyslijProgram();
            Tools.LogParam("Zmiana konfiguracji czujnika 04");
        }

        private void radioButtonCzujnikStart_04_1_Click(object sender, EventArgs e)
        {
            Zarzadzanie.par.prog[ActiveProgEdit].SensorsUpPositionSignals |= 0x0008;
            WyslijProgram();
            Tools.LogParam("Zmiana konfiguracji czujnika 04");
        }

        private void radioButtonCzujnikStart_04_0_Click(object sender, EventArgs e)
        {
            Zarzadzanie.par.prog[ActiveProgEdit].SensorsUpPositionSignals &= 0xfff7;
            WyslijProgram();
            Tools.LogParam("Zmiana konfiguracji czujnika 04");
        }

        // =========================================================================================================================================================
        private void checkBoxStartKonfig_05_Click(object sender, EventArgs e)
        {
            Zarzadzanie.par.prog[ActiveProgEdit].SensorsUpPositionConfig ^= 0x0010;
            WyslijProgram();
            Tools.LogParam("Zmiana konfiguracji czujnika 05");
        }

        private void radioButtonCzujnikStart_05_1_Click(object sender, EventArgs e)
        {
            Zarzadzanie.par.prog[ActiveProgEdit].SensorsUpPositionSignals |= 0x0010;
            WyslijProgram();
            Tools.LogParam("Zmiana konfiguracji czujnika 05");
        }

        private void radioButtonCzujnikStart_05_0_Click(object sender, EventArgs e)
        {
            Zarzadzanie.par.prog[ActiveProgEdit].SensorsUpPositionSignals &= 0xffef;
            WyslijProgram();
            Tools.LogParam("Zmiana konfiguracji czujnika 05");
        }

        // =========================================================================================================================================================
        private void checkBoxStartKonfig_06_Click(object sender, EventArgs e)
        {
            Zarzadzanie.par.prog[ActiveProgEdit].SensorsUpPositionConfig ^= 0x0020;
            WyslijProgram();
            Tools.LogParam("Zmiana konfiguracji czujnika 06");
        }

        private void radioButtonCzujnikStart_06_1_Click(object sender, EventArgs e)
        {
            Zarzadzanie.par.prog[ActiveProgEdit].SensorsUpPositionSignals |= 0x0020;
            WyslijProgram();
            Tools.LogParam("Zmiana konfiguracji czujnika 06");
        }

        private void radioButtonCzujnikStart_06_0_Click(object sender, EventArgs e)
        {
            Zarzadzanie.par.prog[ActiveProgEdit].SensorsUpPositionSignals &= 0xffdf;
            WyslijProgram();
            Tools.LogParam("Zmiana konfiguracji czujnika 06");
        }

        // =========================================================================================================================================================
        private void checkBoxStartKonfig_07_Click(object sender, EventArgs e)
        {
            Zarzadzanie.par.prog[ActiveProgEdit].SensorsUpPositionConfig ^= 0x0040;
            WyslijProgram();
            Tools.LogParam("Zmiana konfiguracji czujnika 07");
        }

        private void radioButtonCzujnikStart_07_1_Click(object sender, EventArgs e)
        {
            Zarzadzanie.par.prog[ActiveProgEdit].SensorsUpPositionSignals |= 0x0040;
            WyslijProgram();
            Tools.LogParam("Zmiana konfiguracji czujnika 07");
        }

        private void radioButtonCzujnikStart_07_0_Click(object sender, EventArgs e)
        {
            Zarzadzanie.par.prog[ActiveProgEdit].SensorsUpPositionSignals &= 0xffbf;
            WyslijProgram();
            Tools.LogParam("Zmiana konfiguracji czujnika 07");
        }

        // =========================================================================================================================================================
        private void checkBoxStartKonfig_08_Click(object sender, EventArgs e)
        {
            Zarzadzanie.par.prog[ActiveProgEdit].SensorsUpPositionConfig ^= 0x0080;
            WyslijProgram();
            Tools.LogParam("Zmiana konfiguracji czujnika 08");
        }

        private void radioButtonCzujnikStart_08_1_Click(object sender, EventArgs e)
        {
            Zarzadzanie.par.prog[ActiveProgEdit].SensorsUpPositionSignals |= 0x0080;
            WyslijProgram();
            Tools.LogParam("Zmiana konfiguracji czujnika 08");
        }

        private void radioButtonCzujnikStart_08_0_Click(object sender, EventArgs e)
        {
            Zarzadzanie.par.prog[ActiveProgEdit].SensorsUpPositionSignals &= 0xff7f;
            WyslijProgram();
            Tools.LogParam("Zmiana konfiguracji czujnika 08");
        }

        // =========================================================================================================================================================
        private void checkBoxStartKonfig_09_Click(object sender, EventArgs e)
        {
            Zarzadzanie.par.prog[ActiveProgEdit].SensorsUpPositionConfig ^= 0x0100;
            WyslijProgram();
            Tools.LogParam("Zmiana konfiguracji czujnika 09");
        }

        private void radioButtonCzujnikStart_09_1_Click(object sender, EventArgs e)
        {
            Zarzadzanie.par.prog[ActiveProgEdit].SensorsUpPositionSignals |= 0x0100;
            WyslijProgram();
            Tools.LogParam("Zmiana konfiguracji czujnika 09");
        }

        private void radioButtonCzujnikStart_09_0_Click(object sender, EventArgs e)
        {
            Zarzadzanie.par.prog[ActiveProgEdit].SensorsUpPositionSignals &= 0xfeff;
            WyslijProgram();
            Tools.LogParam("Zmiana konfiguracji czujnika 09");
        }

        // =========================================================================================================================================================
        private void checkBoxStartKonfig_10_Click(object sender, EventArgs e)
        {
            Zarzadzanie.par.prog[ActiveProgEdit].SensorsUpPositionConfig ^= 0x0200;
            WyslijProgram();
            Tools.LogParam("Zmiana konfiguracji czujnika 10");
        }

        private void radioButtonCzujnikStart_10_1_Click(object sender, EventArgs e)
        {
            Zarzadzanie.par.prog[ActiveProgEdit].SensorsUpPositionSignals |= 0x0200;
            WyslijProgram();
            Tools.LogParam("Zmiana konfiguracji czujnika 10");
        }

        private void radioButtonCzujnikStart_10_0_Click(object sender, EventArgs e)
        {
            Zarzadzanie.par.prog[ActiveProgEdit].SensorsUpPositionSignals &= 0xfdff;
            WyslijProgram();
            Tools.LogParam("Zmiana konfiguracji czujnika 10");
        }

        // =========================================================================================================================================================
        private void checkBoxStartKonfig_11_Click(object sender, EventArgs e)
        {
            Zarzadzanie.par.prog[ActiveProgEdit].SensorsUpPositionConfig ^= 0x0400;
            WyslijProgram();
            Tools.LogParam("Zmiana konfiguracji czujnika 11");
        }

        private void radioButtonCzujnikStart_11_1_Click(object sender, EventArgs e)
        {
            Zarzadzanie.par.prog[ActiveProgEdit].SensorsUpPositionSignals |= 0x0400;
            WyslijProgram();
            Tools.LogParam("Zmiana konfiguracji czujnika 11");
        }

        private void radioButtonCzujnikStart_11_0_Click(object sender, EventArgs e)
        {
            Zarzadzanie.par.prog[ActiveProgEdit].SensorsUpPositionSignals &= 0xfbff;
            WyslijProgram();
            Tools.LogParam("Zmiana konfiguracji czujnika 11");
        }

        // =========================================================================================================================================================
        private void checkBoxStartKonfig_12_Click(object sender, EventArgs e)
        {
            Zarzadzanie.par.prog[ActiveProgEdit].SensorsUpPositionConfig ^= 0x0800;
            WyslijProgram();
            Tools.LogParam("Zmiana konfiguracji czujnika 12");
        }

        private void radioButtonCzujnikStart_12_1_Click(object sender, EventArgs e)
        {
            Zarzadzanie.par.prog[ActiveProgEdit].SensorsUpPositionSignals |= 0x0800;
            WyslijProgram();
            Tools.LogParam("Zmiana konfiguracji czujnika 12");
        }

        private void radioButtonCzujnikStart_12_0_Click(object sender, EventArgs e)
        {
            Zarzadzanie.par.prog[ActiveProgEdit].SensorsUpPositionSignals &= 0xf7ff;
            WyslijProgram();
            Tools.LogParam("Zmiana konfiguracji czujnika 12");
        }

        // =========================================================================================================================================================
        private void checkBoxStartKonfig_13_Click(object sender, EventArgs e)
        {
            Zarzadzanie.par.prog[ActiveProgEdit].SensorsUpPositionConfig ^= 0x1000;
            WyslijProgram();
            Tools.LogParam("Zmiana konfiguracji czujnika 13");
        }

        private void radioButtonCzujnikStart_13_1_Click(object sender, EventArgs e)
        {
            Zarzadzanie.par.prog[ActiveProgEdit].SensorsUpPositionSignals |= 0x1000;
            WyslijProgram();
            Tools.LogParam("Zmiana konfiguracji czujnika 13");
        }

        private void radioButtonCzujnikStart_13_0_Click(object sender, EventArgs e)
        {
            Zarzadzanie.par.prog[ActiveProgEdit].SensorsUpPositionSignals &= 0xefff;
            WyslijProgram();
            Tools.LogParam("Zmiana konfiguracji czujnika 13");
        }

        // =========================================================================================================================================================
        private void checkBoxStartKonfig_14_Click(object sender, EventArgs e)
        {
            Zarzadzanie.par.prog[ActiveProgEdit].SensorsUpPositionConfig ^= 0x2000;
            WyslijProgram();
            Tools.LogParam("Zmiana konfiguracji czujnika 14");
        }

        private void radioButtonCzujnikStart_14_1_Click(object sender, EventArgs e)
        {
            Zarzadzanie.par.prog[ActiveProgEdit].SensorsUpPositionSignals |= 0x2000;
            WyslijProgram();
            Tools.LogParam("Zmiana konfiguracji czujnika 14");
        }

        private void radioButtonCzujnikStart_14_0_Click(object sender, EventArgs e)
        {
            Zarzadzanie.par.prog[ActiveProgEdit].SensorsUpPositionSignals &= 0xdfff;
            WyslijProgram();
            Tools.LogParam("Zmiana konfiguracji czujnika 14");
        }

        // =========================================================================================================================================================
        private void checkBoxDKKonfig_001_Click(object sender, EventArgs e)
        {
            Zarzadzanie.par.prog[ActiveProgEdit].SensorsDownPositionConfig ^= 0x0001;
            WyslijProgram();
            Tools.LogParam("Zmiana konfiguracji czujnika 01");
        }
        private void radioButtonCzujnikDK_01_1_Click(object sender, EventArgs e)
        {
            Zarzadzanie.par.prog[ActiveProgEdit].SensorsDownPositionSignals |= 0x0001;
            WyslijProgram();
            Tools.LogParam("Zmiana konfiguracji czujnika 01");
        }
        private void radioButtonCzujnikDK_01_0_Click(object sender, EventArgs e)
        {
            Zarzadzanie.par.prog[ActiveProgEdit].SensorsDownPositionSignals &= 0xfffe;
            WyslijProgram();
            Tools.LogParam("Zmiana konfiguracji czujnika 01");
        }

        // =========================================================================================================================================================
        private void checkBoxDKKonfig_02_Click(object sender, EventArgs e)
        {
            Zarzadzanie.par.prog[ActiveProgEdit].SensorsDownPositionConfig ^= 0x0002;
            WyslijProgram();
            Tools.LogParam("Zmiana konfiguracji czujnika 02");
        }

        private void radioButtonCzujnikDK_02_1_Click(object sender, EventArgs e)
        {
            Zarzadzanie.par.prog[ActiveProgEdit].SensorsDownPositionSignals |= 0x0002;
            WyslijProgram();
            Tools.LogParam("Zmiana konfiguracji czujnika 02");
        }

        private void radioButtonCzujnikDK_02_0_Click(object sender, EventArgs e)
        {
            Zarzadzanie.par.prog[ActiveProgEdit].SensorsDownPositionSignals &= 0xfffd;
            WyslijProgram();
            Tools.LogParam("Zmiana konfiguracji czujnika 02");
        }

        // =========================================================================================================================================================
        private void checkBoxDKKonfig_03_Click(object sender, EventArgs e)
        {
            Zarzadzanie.par.prog[ActiveProgEdit].SensorsDownPositionConfig ^= 0x0004;
            WyslijProgram();
            Tools.LogParam("Zmiana konfiguracji czujnika 03");
        }

        private void radioButtonCzujnikDK_03_1_Click(object sender, EventArgs e)
        {
            Zarzadzanie.par.prog[ActiveProgEdit].SensorsDownPositionSignals |= 0x0004;
            WyslijProgram();
            Tools.LogParam("Zmiana konfiguracji czujnika 03");
        }

        private void radioButtonCzujnikDK_03_0_Click(object sender, EventArgs e)
        {
            Zarzadzanie.par.prog[ActiveProgEdit].SensorsDownPositionSignals &= 0xfffb;
            WyslijProgram();
            Tools.LogParam("Zmiana konfiguracji czujnika 03");
        }

        // =========================================================================================================================================================
        private void checkBoxDKKonfig_04_Click(object sender, EventArgs e)
        {
            Zarzadzanie.par.prog[ActiveProgEdit].SensorsDownPositionConfig ^= 0x0008;
            WyslijProgram();
            Tools.LogParam("Zmiana konfiguracji czujnika 04");
        }

        private void radioButtonCzujnikDK_04_1_Click(object sender, EventArgs e)
        {
            Zarzadzanie.par.prog[ActiveProgEdit].SensorsDownPositionSignals |= 0x0008;
            WyslijProgram();
            Tools.LogParam("Zmiana konfiguracji czujnika 03");
        }

        private void radioButtonCzujnikDK_04_0_Click(object sender, EventArgs e)
        {
            Zarzadzanie.par.prog[ActiveProgEdit].SensorsDownPositionSignals &= 0xfff7;
            WyslijProgram();
            Tools.LogParam("Zmiana konfiguracji czujnika 03");
        }

        // =========================================================================================================================================================
        private void checkBoxDKKonfig_05_Click(object sender, EventArgs e)
        {
            Zarzadzanie.par.prog[ActiveProgEdit].SensorsDownPositionConfig ^= 0x0010;
            WyslijProgram();
            Tools.LogParam("Zmiana konfiguracji czujnika 05");
        }

        private void radioButtonCzujnikDK_05_1_Click(object sender, EventArgs e)
        {
            Zarzadzanie.par.prog[ActiveProgEdit].SensorsDownPositionSignals |= 0x0010;
            WyslijProgram();
            Tools.LogParam("Zmiana konfiguracji czujnika 05");
        }

        private void radioButtonCzujnikDK_05_0_Click(object sender, EventArgs e)
        {
            Zarzadzanie.par.prog[ActiveProgEdit].SensorsDownPositionSignals &= 0xffef;
            WyslijProgram();
            Tools.LogParam("Zmiana konfiguracji czujnika 05");
        }

        // =========================================================================================================================================================
        private void checkBoxDKKonfig_06_Click(object sender, EventArgs e)
        {
            Zarzadzanie.par.prog[ActiveProgEdit].SensorsDownPositionConfig ^= 0x0020;
            WyslijProgram();
            Tools.LogParam("Zmiana konfiguracji czujnika 06");
        }

        private void radioButtonCzujnikDK_06_1_Click(object sender, EventArgs e)
        {
            Zarzadzanie.par.prog[ActiveProgEdit].SensorsDownPositionSignals |= 0x0020;
            WyslijProgram();
            Tools.LogParam("Zmiana konfiguracji czujnika 06");
        }

        private void radioButtonCzujnikDK_06_0_Click(object sender, EventArgs e)
        {
            Zarzadzanie.par.prog[ActiveProgEdit].SensorsDownPositionSignals &= 0xffdf;
            WyslijProgram();
            Tools.LogParam("Zmiana konfiguracji czujnika 06");
        }

        // =========================================================================================================================================================
        private void checkBoxDKKonfig_07_Click(object sender, EventArgs e)
        {
            Zarzadzanie.par.prog[ActiveProgEdit].SensorsDownPositionConfig ^= 0x0040;
            WyslijProgram();
            Tools.LogParam("Zmiana konfiguracji czujnika 07");
        }

        private void radioButtonCzujnikDK_07_1_Click(object sender, EventArgs e)
        {
            Zarzadzanie.par.prog[ActiveProgEdit].SensorsDownPositionSignals |= 0x0040;
            WyslijProgram();
            Tools.LogParam("Zmiana konfiguracji czujnika 07");
        }

        private void radioButtonCzujnikDK_07_0_Click(object sender, EventArgs e)
        {
            Zarzadzanie.par.prog[ActiveProgEdit].SensorsDownPositionSignals &= 0xffbf;
            WyslijProgram();
            Tools.LogParam("Zmiana konfiguracji czujnika 07");
        }

        // =========================================================================================================================================================
        private void checkBoxDKKonfig_08_Click(object sender, EventArgs e)
        {
            Zarzadzanie.par.prog[ActiveProgEdit].SensorsDownPositionConfig ^= 0x0080;
            WyslijProgram();
            Tools.LogParam("Zmiana konfiguracji czujnika 08");
        }

        private void radioButtonCzujnikDK_08_1_Click(object sender, EventArgs e)
        {
            Zarzadzanie.par.prog[ActiveProgEdit].SensorsDownPositionSignals |= 0x0080;
            WyslijProgram();
            Tools.LogParam("Zmiana konfiguracji czujnika 08");
        }

        private void radioButtonCzujnikDK_08_0_Click(object sender, EventArgs e)
        {
            Zarzadzanie.par.prog[ActiveProgEdit].SensorsDownPositionSignals &= 0xff7f;
            WyslijProgram();
            Tools.LogParam("Zmiana konfiguracji czujnika 08");
        }

        // =========================================================================================================================================================
        private void checkBoxDKKonfig_09_Click(object sender, EventArgs e)
        {
            Zarzadzanie.par.prog[ActiveProgEdit].SensorsDownPositionConfig ^= 0x0100;
            WyslijProgram();
            Tools.LogParam("Zmiana konfiguracji czujnika 09");
        }

        private void radioButtonCzujnikDK_09_1_Click(object sender, EventArgs e)
        {
            Zarzadzanie.par.prog[ActiveProgEdit].SensorsDownPositionSignals |= 0x0100;
            WyslijProgram();
            Tools.LogParam("Zmiana konfiguracji czujnika 09");
        }

        private void radioButtonCzujnikDK_09_0_Click(object sender, EventArgs e)
        {
            Zarzadzanie.par.prog[ActiveProgEdit].SensorsDownPositionSignals &= 0xfeff;
            WyslijProgram();
            Tools.LogParam("Zmiana konfiguracji czujnika 09");
        }

        // =========================================================================================================================================================
        private void checkBoxDKKonfig_10_Click(object sender, EventArgs e)
        {
            Zarzadzanie.par.prog[ActiveProgEdit].SensorsDownPositionConfig ^= 0x0200;
            WyslijProgram();
            Tools.LogParam("Zmiana konfiguracji czujnika 10");
        }

        private void radioButtonCzujnikDK_10_1_Click(object sender, EventArgs e)
        {
            Zarzadzanie.par.prog[ActiveProgEdit].SensorsDownPositionSignals |= 0x0200;
            WyslijProgram();
            Tools.LogParam("Zmiana konfiguracji czujnika 10");
        }

        private void radioButtonCzujnikDK_10_0_Click(object sender, EventArgs e)
        {
            Zarzadzanie.par.prog[ActiveProgEdit].SensorsDownPositionSignals &= 0xfdff;
            WyslijProgram();
            Tools.LogParam("Zmiana konfiguracji czujnika 10");
        }

        // =========================================================================================================================================================
        private void checkBoxDKKonfig_11_Click(object sender, EventArgs e)
        {
            Zarzadzanie.par.prog[ActiveProgEdit].SensorsDownPositionConfig ^= 0x0400;
            WyslijProgram();
            Tools.LogParam("Zmiana konfiguracji czujnika 11");
        }

        private void radioButtonCzujnikDK_11_1_Click(object sender, EventArgs e)
        {
            Zarzadzanie.par.prog[ActiveProgEdit].SensorsDownPositionSignals |= 0x0400;
            WyslijProgram();
            Tools.LogParam("Zmiana konfiguracji czujnika 11");
        }

        private void radioButtonCzujnikDK_11_0_Click(object sender, EventArgs e)
        {
            Zarzadzanie.par.prog[ActiveProgEdit].SensorsDownPositionSignals &= 0xfbff;
            WyslijProgram();
            Tools.LogParam("Zmiana konfiguracji czujnika 11");
        }

        // =========================================================================================================================================================
        private void checkBoxDKKonfig_12_Click(object sender, EventArgs e)
        {
            Zarzadzanie.par.prog[ActiveProgEdit].SensorsDownPositionConfig ^= 0x0800;
            WyslijProgram();
            Tools.LogParam("Zmiana konfiguracji czujnika 12");
        }

        private void radioButtonCzujnikDK_12_1_Click(object sender, EventArgs e)
        {
            Zarzadzanie.par.prog[ActiveProgEdit].SensorsDownPositionSignals |= 0x0800;
            WyslijProgram();
            Tools.LogParam("Zmiana konfiguracji czujnika 12");
        }

        private void radioButtonCzujnikDK_12_0_Click(object sender, EventArgs e)
        {
            Zarzadzanie.par.prog[ActiveProgEdit].SensorsDownPositionSignals &= 0xf7ff;
            WyslijProgram();
            Tools.LogParam("Zmiana konfiguracji czujnika 12");
        }

        // =========================================================================================================================================================
        private void checkBoxDKKonfig_13_Click(object sender, EventArgs e)
        {
            Zarzadzanie.par.prog[ActiveProgEdit].SensorsDownPositionConfig ^= 0x1000;
            WyslijProgram();
            Tools.LogParam("Zmiana konfiguracji czujnika 13");
        }

        private void radioButtonCzujnikDK_13_1_Click(object sender, EventArgs e)
        {
            Zarzadzanie.par.prog[ActiveProgEdit].SensorsDownPositionSignals |= 0x1000;
            WyslijProgram();
            Tools.LogParam("Zmiana konfiguracji czujnika 13");
        }

        private void radioButtonCzujnikDK_13_0_Click(object sender, EventArgs e)
        {
            Zarzadzanie.par.prog[ActiveProgEdit].SensorsDownPositionSignals &= 0xefff;
            WyslijProgram();
            Tools.LogParam("Zmiana konfiguracji czujnika 13");
        }

        // =========================================================================================================================================================
        private void checkBoxDKKonfig_14_Click(object sender, EventArgs e)
        {
            Zarzadzanie.par.prog[ActiveProgEdit].SensorsDownPositionConfig ^= 0x2000;
            WyslijProgram();
            Tools.LogParam("Zmiana konfiguracji czujnika 14");
        }

        private void radioButtonCzujnikDK_14_1_Click(object sender, EventArgs e)
        {
            Zarzadzanie.par.prog[ActiveProgEdit].SensorsDownPositionSignals |= 0x2000;
            WyslijProgram();
            Tools.LogParam("Zmiana konfiguracji czujnika 14");
        }

        private void radioButtonCzujnikDK_14_0_Click(object sender, EventArgs e)
        {
            Zarzadzanie.par.prog[ActiveProgEdit].SensorsDownPositionSignals &= 0xdfff;
            WyslijProgram();
            Tools.LogParam("Zmiana konfiguracji czujnika 14");
        }

        // =========================================================================================================================================================
        private void checkBox_Cylinder_01_Click(object sender, EventArgs e)
        {
            Zarzadzanie.par.prog[ActiveProgEdit].CylinderNumber ^= 0x01;
            WyslijProgram();
        }

        private void checkBox_Cylinder_01_DodSila_Click(object sender, EventArgs e)
        {
            Zarzadzanie.par.prog[ActiveProgEdit].CylinderNumber ^= 0x02;
            WyslijProgram();
        }

        private void checkBox_Cylinder_02_Click(object sender, EventArgs e)
        {
            Zarzadzanie.par.prog[ActiveProgEdit].CylinderNumber ^= 0x04;
            WyslijProgram();
        }

        private void checkBox_Cylinder_03_Click(object sender, EventArgs e)
        {
            Zarzadzanie.par.prog[ActiveProgEdit].CylinderNumber ^= 0x08;
            WyslijProgram();
        }

        private void checkBox_Cylinder_04_Click(object sender, EventArgs e)
        {
            Zarzadzanie.par.prog[ActiveProgEdit].CylinderNumber ^= 0x10;
            WyslijProgram();
        }

        // =========================================================================================================================================================
        private void checkBox_Zawor_11_Click(object sender, EventArgs e)
        {
            Zarzadzanie.par.prog[ActiveProgEdit].Valves ^= 0x01;
            WyslijProgram();
        }

        private void checkBox_Zawor_12_Click(object sender, EventArgs e)
        {
            Zarzadzanie.par.prog[ActiveProgEdit].Valves ^= 0x02;
            WyslijProgram();
        }

        private void checkBox_Zawor_21_Click(object sender, EventArgs e)
        {
            Zarzadzanie.par.prog[ActiveProgEdit].Valves ^= 0x04;
            WyslijProgram();
        }

        private void checkBox_Zawor_22_Click(object sender, EventArgs e)
        {
            Zarzadzanie.par.prog[ActiveProgEdit].Valves ^= 0x08;
            WyslijProgram();
        }

        private void checkBox_Zawor_31_Click(object sender, EventArgs e)
        {
            Zarzadzanie.par.prog[ActiveProgEdit].Valves ^= 0x10;
            WyslijProgram();
        }

        private void checkBox_Zawor_32_Click(object sender, EventArgs e)
        {
            Zarzadzanie.par.prog[ActiveProgEdit].Valves ^= 0x20;
            WyslijProgram();
        }

        private void checkBox_Zawor_41_Click(object sender, EventArgs e)
        {
            Zarzadzanie.par.prog[ActiveProgEdit].Valves ^= 0x40;
            WyslijProgram();
        }

        private void checkBox_Zawor_42_Click(object sender, EventArgs e)
        {
            Zarzadzanie.par.prog[ActiveProgEdit].Valves ^= 0x80;
            WyslijProgram();
        }

        // =========================================================================================================================================================
        private void checkBox_PulpitDwureczny_Click(object sender, EventArgs e)
        {
            Zarzadzanie.par.prog[ActiveProgEdit].ProgramConfiguration ^= 0x01;
            WyslijProgram();
        }

        private void radioButton_Wyblokowanie_WDole_Click(object sender, EventArgs e)
        {
            Zarzadzanie.par.prog[ActiveProgEdit].ProgramConfiguration &= 0xf7;
            WyslijProgram();
        }

        private void radioButton_Wyblokowanie_WGorze_Click(object sender, EventArgs e)
        {
            Zarzadzanie.par.prog[ActiveProgEdit].ProgramConfiguration |= 0x08;
            WyslijProgram();
        }

        // =========================================================================================================================================================

        private void button_PozycjaTol_Plus_Click(object sender, EventArgs e)
        {
            Zarzadzanie.par.prog[PLC.cykl_program_akt.Word].CylinderPositionDownTolerance += 10;
            MainForm.parametr_nr = PLC.cykl_program_akt.Word;
            WyslijProgram(PLC.cykl_program_akt.Word);
        }

        private void button_PozycjaTol_Minus_Click(object sender, EventArgs e)
        {
            Zarzadzanie.par.prog[PLC.cykl_program_akt.Word].CylinderPositionDownTolerance -= 10;
            MainForm.parametr_nr = PLC.cykl_program_akt.Word;
            WyslijProgram(PLC.cykl_program_akt.Word);
        }

        private void button_WydmuchTol_Plus_Click(object sender, EventArgs e)
        {
            Zarzadzanie.par.prog[PLC.cykl_program_akt.Word].ExhaustPressureTolerance += 50;
            MainForm.parametr_nr = PLC.cykl_program_akt.Word;
            WyslijProgram(PLC.cykl_program_akt.Word);
        }

        private void button_WydmuchTol_Minus_Click(object sender, EventArgs e)
        {
            Zarzadzanie.par.prog[PLC.cykl_program_akt.Word].ExhaustPressureTolerance -= 50;
            MainForm.parametr_nr = PLC.cykl_program_akt.Word;
            WyslijProgram(PLC.cykl_program_akt.Word);
        }

        // =========================================================================================================================================================

        private void checkBox_IOTest_Click(object sender, EventArgs e)
        {
            Modbus.Rozkaz(7);
            Tools.LogParam("Rozkaz - włącz/wyłącz IO Test");
        }

        private void panelPlot_Paint(object sender, PaintEventArgs e)
        {

        }

        private void picPloter_Paint(object sender, PaintEventArgs e)
        {
            Ploter.Refresh(e);
        }

        private void picPloter_MouseDown(object sender, MouseEventArgs e)
        {
            Ploter.Cursor(e.X);
            odswiez_plota = true;
        }

        private void pictureFlagaPL_Click(object sender, EventArgs e)
        {
            jezyk = LID_PL;
            PrzeladujTeksty();
            Tools.WriteSetting("lang", jezyk.ToString());
        }

        private void pictureFlagaEN_Click(object sender, EventArgs e)
        {
            jezyk = LID_EN;
            PrzeladujTeksty();
            Tools.WriteSetting("lang", jezyk.ToString());
        }

        private void pictureFlagaDE_Click(object sender, EventArgs e)
        {
            jezyk = LID_DE;
            PrzeladujTeksty();
            Tools.WriteSetting("lang", jezyk.ToString());
        }

        private void pictureFlagaES_Click(object sender, EventArgs e)
        {
            jezyk = LID_ES;
            PrzeladujTeksty();
            Tools.WriteSetting("lang", jezyk.ToString());
        }

        private void pictureFlagaHU_Click(object sender, EventArgs e)
        {
            jezyk = LID_HU;
            PrzeladujTeksty();
            Tools.WriteSetting("lang", jezyk.ToString());
        }

        private void button_prg_nr_minus_Click(object sender, EventArgs e)
        {
            if (comboNrProgramu.SelectedIndex > 0)
                comboNrProgramu.SelectedIndex--;
        }

        private void button_prg_nr_plus_Click(object sender, EventArgs e)
        {
            if (comboNrProgramu.SelectedIndex < comboNrProgramu.Items.Count - 1)
                comboNrProgramu.SelectedIndex++;
        }

        private void checkBox_Pomiary_Srednia_Click(object sender, EventArgs e)
        {
            Modbus.Rozkaz(8);
        }

        private void numeric_ref_prad_zadane_ValueChanged(object sender, EventArgs e)
        {
        }

        private void numeric_ref_nakr_zadane_ValueChanged(object sender, EventArgs e)
        {

        }

        /* 
         * =========================================================================================================================================================
         * Nazwa:           Wyswietlanie_ParametryInne 
         * 
         * Przeznaczenie:   Funkcja wyswietlajaca stany innych combo box-ów
         * =========================================================================================================================================================
         */

        private void Wyswietlanie_ParametryInne()
        {
            //
            // Ppomiary uśrednione przy refernecji nakrętki
            //
            if ((PLC.status.Word & 0x80) == 0)
                checkBox_Pomiary_Srednia.Checked = false;
            else
                checkBox_Pomiary_Srednia.Checked = true;
        }

        // =========================================================================================================================================================

        void WyswitlenieListy()
        {
            hide_param = true;
            if (checkBox_ListaParametrow.Checked)
            {
                this.comboParametr.Items.Clear();
                this.comboParametr.Items.Add(TxtParametryNazwa[PARAM_NAME_PrePressTime, jezyk]);
                this.comboParametr.Items.Add(TxtParametryNazwa[PARAM_NAME_PreWeldCurrent, jezyk]);
                this.comboParametr.Items.Add(TxtParametryNazwa[PARAM_NAME_PreWeldTime, jezyk]);
                this.comboParametr.Items.Add(TxtParametryNazwa[PARAM_NAME_PreWeldPause, jezyk]);
                this.comboParametr.Items.Add(TxtParametryNazwa[PARAM_NAME_WeldCurrent, jezyk]);
                this.comboParametr.Items.Add(TxtParametryNazwa[PARAM_NAME_WeldTime, jezyk]);
                this.comboParametr.Items.Add(TxtParametryNazwa[PARAM_NAME_PostWeldPause, jezyk]);
                this.comboParametr.Items.Add(TxtParametryNazwa[PARAM_NAME_PostWeldCurrent, jezyk]);
                this.comboParametr.Items.Add(TxtParametryNazwa[PARAM_NAME_PostWeldTime, jezyk]);
                this.comboParametr.Items.Add(TxtParametryNazwa[PARAM_NAME_PostPressTime, jezyk]);
                this.comboParametr.Items.Add(TxtParametryNazwa[PARAM_NAME_Impulses, jezyk]);
                this.comboParametr.Items.Add(TxtParametryNazwa[PARAM_NAME_ImpulsesPause, jezyk]);
                this.comboParametr.Items.Add(TxtParametryNazwa[PARAM_NAME_StepperPercent, jezyk]);
                this.comboParametr.Items.Add(TxtParametryNazwa[PARAM_NAME_StepperCounter, jezyk]);
                this.comboParametr.Items.Add(TxtParametryNazwa[PARAM_NAME_PressureSet, jezyk]);
                //this.comboParametr.Items.Add(TxtParametryNazwa[PARAM_NAME_PressureSwitch, jezyk]);
                this.comboParametr.Items.Add(TxtParametryNazwa[PARAM_NAME_CylinderPositionDown, jezyk]);
                this.comboParametr.Items.Add(TxtParametryNazwa[PARAM_NAME_CylinderPositionDownTolerance, jezyk]);
                //this.comboParametr.Items.Add(TxtParametryNazwa[PARAM_NAME_ExhaustPressure, jezyk]);
                //this.comboParametr.Items.Add(TxtParametryNazwa[PARAM_NAME_ExhaustPressureTolerance, jezyk]);
                this.comboParametr.Items.Add(TxtParametryNazwa[PARAM_NAME_Injection, jezyk]);
                this.comboParametr.Items.Add(TxtParametryNazwa[PARAM_NAME_Iref, jezyk]);
                this.comboParametr.Items.Add(TxtParametryNazwa[PARAM_NAME_Uref, jezyk]);
                this.comboParametr.Items.Add(TxtParametryNazwa[PARAM_NAME_Eref, jezyk]);
                this.comboParametr.Items.Add(TxtParametryNazwa[PARAM_NAME_IrefTolerance, jezyk]);
                this.comboParametr.Items.Add(TxtParametryNazwa[PARAM_NAME_UrefTolerance, jezyk]);
                this.comboParametr.Items.Add(TxtParametryNazwa[PARAM_NAME_ErefTolerance, jezyk]);
            }
            else
            {
                ActiveParamEdit = "";
                this.comboParametr.Items.Clear();
                this.comboParametr.Items.Add(TxtParametryNazwa[PARAM_NAME_PrePressTime, jezyk]);
                this.comboParametr.Items.Add(TxtParametryNazwa[PARAM_NAME_WeldCurrent, jezyk]);
                this.comboParametr.Items.Add(TxtParametryNazwa[PARAM_NAME_WeldTime, jezyk]);
                this.comboParametr.Items.Add(TxtParametryNazwa[PARAM_NAME_PostPressTime, jezyk]);
                this.comboParametr.Items.Add(TxtParametryNazwa[PARAM_NAME_PressureSet, jezyk]);
                //this.comboParametr.Items.Add(TxtParametryNazwa[PARAM_NAME_PressureSwitch, jezyk]);
            }
            comboParametr.SelectedIndex = 0;

            ActiveToolParamEdit = "";
            this.comboParametrPrzyrzad.Items.Clear();
            this.comboParametrPrzyrzad.Items.Add(TxtToolParametryNazwa[TOOLPARAM_NAME_Licznik, jezyk]);
            this.comboParametrPrzyrzad.Items.Add(TxtToolParametryNazwa[TOOLPARAM_NAME_LicznikMax, jezyk]);
            this.comboParametrPrzyrzad.Items.Add(TxtToolParametryNazwa[TOOLPARAM_NAME_StepperLicznik, jezyk]);
            this.comboParametrPrzyrzad.Items.Add(TxtToolParametryNazwa[TOOLPARAM_NAME_StepperOstrzezenie, jezyk]);
            this.comboParametrPrzyrzad.Items.Add(TxtToolParametryNazwa[TOOLPARAM_NAME_StepperMax, jezyk]);
            this.comboParametrPrzyrzad.Items.Add(TxtToolParametryNazwa[TOOLPARAM_NAME_Sluza, jezyk]);
            this.comboParametrPrzyrzad.Items.Add(TxtToolParametryNazwa[TOOLPARAM_NAME_LiczbaProgramow, jezyk]);
            comboParametrPrzyrzad.SelectedIndex = 0;

            hide_param = false;
        }

    }
}
