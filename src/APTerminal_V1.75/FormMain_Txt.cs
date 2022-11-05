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
    public partial class MainForm
    {
        const int LID_TABSIZE = 5;
        const int LID_PL = 0;
        const int LID_EN = 1;
        const int LID_DE = 2;
        const int LID_ES = 3;
        const int LID_HU = 4;

        const int ETPROC_CZEKAM_NA_BRAK_STARTU = 0;
        const int ETPROC_CZEKAM_NA_POZWOLENIE_STARTU = 1;
        const int ETPROC_CZEKAM_NA_CYLINDER_W_GORZE = 2;
        const int ETPROC_CZEKAM_NA_START = 3;
        const int ETPROC_CZEKAM_NA_CZUJNIKI_START = 4;
        const int ETPROC_CZEKAM_NA_DK = 5;
        const int ETPROC_CZEKAM_NA_CZUJNIKI_DK = 6;
        const int ETPROC_CZEKAM_NA_KONTROLA_NAKRETKI = 7;
        const int ETPROC_CZEKAM_NA_WYBLOKOWANIE = 8;
        const int ETPROC_ZGRZEWANIE_DOCISK_WSTEPNY = 9;
        const int ETPROC_ZGRZEWANIE_PODGRZEWANIE = 10;
        const int ETPROC_ZGRZEWANIE_PODGRZEWANIE_PAUZA = 11;
        const int ETPROC_ZGRZEWANIE_ZGRZEWANIE = 12;
        const int ETPROC_ZGRZEWANIE_ZGRZEWANIE_PAUZA = 13;
        const int ETPROC_ZGRZEWANIE_DOGRZEWANIE_PAUZA = 14;
        const int ETPROC_ZGRZEWANIE_DOGRZEWANIE = 15;
        const int ETPROC_ZGRZEWANIE_DOCISK_KONCOWY = 16;
        const int ETPROC_ZGRZEWANIE_KONIEC = 17;
        const int ETPROC_ZGRZEW_OK = 18;
        const int ETPROC_ZGRZEW_NOK = 19;
        const int ETPROC_KONIEC_PROCESU = 20;

        const int PARAM_NAME_PrePressTime = 0;
        const int PARAM_NAME_PreWeldCurrent = 1;
        const int PARAM_NAME_PreWeldTime = 2;
        const int PARAM_NAME_PreWeldPause = 3;
        const int PARAM_NAME_WeldCurrent = 4;
        const int PARAM_NAME_WeldTime = 5;
        const int PARAM_NAME_PostWeldPause = 6;
        const int PARAM_NAME_PostWeldCurrent = 7;
        const int PARAM_NAME_PostWeldTime = 8;
        const int PARAM_NAME_PostPressTime = 9;

        const int PARAM_NAME_Impulses = 10;
        const int PARAM_NAME_ImpulsesPause = 11;
        const int PARAM_NAME_StepperPercent = 12;
        const int PARAM_NAME_StepperCounter = 13;

        const int PARAM_NAME_CylinderNumber = 14;
        const int PARAM_NAME_CylinderPositionDown = 15;
        const int PARAM_NAME_CylinderPositionDownTolerance = 16;
        const int PARAM_NAME_Injection = 17;
        const int PARAM_NAME_PressureSet = 18;
        const int PARAM_NAME_PressureSwitch = 19;
        const int PARAM_NAME_ExhaustPressure = 20;
        const int PARAM_NAME_ExhaustPressureTolerance = 21;

        const int PARAM_NAME_IrefTolerance = 22;
        const int PARAM_NAME_UrefTolerance = 23;
        const int PARAM_NAME_ErefTolerance = 24;

        const int PARAM_NAME_Iref = 25;
        const int PARAM_NAME_Uref = 26;
        const int PARAM_NAME_Eref = 27;

        const int TOOLPARAM_NAME_Licznik = 0;
        const int TOOLPARAM_NAME_LicznikMax = 1;
        const int TOOLPARAM_NAME_StepperLicznik = 2;
        const int TOOLPARAM_NAME_StepperOstrzezenie = 3;
        const int TOOLPARAM_NAME_StepperMax = 4;
        const int TOOLPARAM_NAME_Sluza = 5;
        const int TOOLPARAM_NAME_LiczbaProgramow = 6;

        public int jezyk = 0;

        public string[] TxtWelcome;

        public string[,] TxtCykl;

        public string[,] TxtTabsMain;
        public string[,] TxtTabsProduction;
        public string[,] TxtTabsProcess;
        public string[,] TxtTabsTools;
        public string[,] TxtTabsParameters;
        public string[,] TxtTabsSystem;
        public string[,] TxtParametryNazwa;
        public string[,] TxtToolParametryNazwa;

        public string[] TxtLicznikZeruj;
        public string[] TxtPrzyrzad;
        public string[] TxtNrProgramu;
        public string[] TxtGora;
        public string[] TxtDol;
        public string[] TxtCzujnik;
        public string[] TxtAktualnyProgram;
        public string[] TxtPozycjaWyjsciowa;
        public string[] TxtKontrolaNakretek;
        public string[] TxtTolerancja;
        public string[] TxtPozycjaCylindra;
        public string[] TxtPozycjaCylindraGora;
        public string[] TxtPozycjaCylindraDol;
        public string[] TxtCisnienieWydmuchu;
        public string[] TxtCisnienieWydmuchuDol;
        public string[] TxtButtonPomiarPradu;
        public string[] TxtButtonPomiarNakretki;
        public string[] TxtPomiaryReferencyjne;
        public string[] TxtDaneZOstatniegoZgrzewu;
        public string[] TxtPomiarTolerancji;

        public string[] TxtPromilMocyZeStepperem;
        public string[] TxtWartoscSredniaPradu;
        public string[] TxtWartoscSredniaNapiecia;
        public string[] TxtWartoscEnergii;
        public string[] TxtWartoscWtopienia;

        public string[] TxtPrad;
        public string[] TxtNapiecie;
        public string[] TxtWtopienie;
        public string[] TxtCzas;
        public string[] TxtMoc;

        public string[] TxtAktywneZaklocenia;
        public string[] TxtButtonPotwierdzenie;

        public string[] TxtListaPrzyrzadow;
        public string[] TxtAktywny;
        public string[] TxtButtonAktywacja;
        public string[] TxtButtonUtworzenieNowego;
        public string[] TxtButtonUtworzenieKopii;
        public string[] TxtButtonSkasowanie;

        public string[] TxtAktywnyPrzyrzad;
        public string[] TxtParametr;
        public string[] TxtOpis;

        public string[] TxtKodAktywnego;
        public string[] TxtKodPodlaczonego;
        public string[] TxtButtonWyszukajKod;


        public void TextsInit()
        {
            //
            // *****************************************
            // Zmienne
            // *****************************************
            //

            TxtWelcome = new string[LID_TABSIZE];

            TxtCykl = new string[30, LID_TABSIZE];

            TxtTabsMain = new string[10, LID_TABSIZE];
            TxtTabsProduction = new string[5, LID_TABSIZE];
            TxtTabsProcess = new string[5, LID_TABSIZE];
            TxtTabsTools = new string[5, LID_TABSIZE];
            TxtTabsParameters = new string[10, LID_TABSIZE];
            TxtTabsSystem = new string[5, LID_TABSIZE];

            TxtParametryNazwa = new string[30, LID_TABSIZE];
            TxtToolParametryNazwa = new string[10, LID_TABSIZE];

            TxtLicznikZeruj = new string[LID_TABSIZE];
            TxtPrzyrzad = new string[LID_TABSIZE];
            TxtNrProgramu = new string[LID_TABSIZE];
            TxtGora = new string[LID_TABSIZE];
            TxtDol = new string[LID_TABSIZE];
            TxtCzujnik = new string[LID_TABSIZE];
            TxtAktualnyProgram = new string[LID_TABSIZE];
            TxtPozycjaWyjsciowa = new string[LID_TABSIZE];
            TxtKontrolaNakretek = new string[LID_TABSIZE];
            TxtTolerancja = new string[LID_TABSIZE];
            TxtPozycjaCylindra = new string[LID_TABSIZE];
            TxtPozycjaCylindraGora = new string[LID_TABSIZE];
            TxtPozycjaCylindraDol = new string[LID_TABSIZE];
            TxtCisnienieWydmuchu = new string[LID_TABSIZE];
            TxtCisnienieWydmuchuDol = new string[LID_TABSIZE];
            TxtButtonPomiarPradu = new string[LID_TABSIZE];
            TxtButtonPomiarNakretki = new string[LID_TABSIZE];
            TxtPomiaryReferencyjne = new string[LID_TABSIZE];
            TxtDaneZOstatniegoZgrzewu = new string[LID_TABSIZE];
            TxtPomiarTolerancji = new string[LID_TABSIZE];
            TxtPromilMocyZeStepperem = new string[LID_TABSIZE];
            TxtWartoscSredniaPradu = new string[LID_TABSIZE];
            TxtWartoscSredniaNapiecia = new string[LID_TABSIZE];
            TxtWartoscEnergii = new string[LID_TABSIZE];
            TxtWartoscWtopienia = new string[LID_TABSIZE];
            TxtPrad = new string[LID_TABSIZE];
            TxtNapiecie = new string[LID_TABSIZE];
            TxtWtopienie = new string[LID_TABSIZE];
            TxtCzas = new string[LID_TABSIZE];
            TxtMoc = new string[LID_TABSIZE];
            TxtAktywneZaklocenia = new string[LID_TABSIZE];
            TxtButtonPotwierdzenie = new string[LID_TABSIZE];

            TxtListaPrzyrzadow = new string[LID_TABSIZE];
            TxtAktywny = new string[LID_TABSIZE];
            TxtButtonAktywacja = new string[LID_TABSIZE];
            TxtButtonUtworzenieNowego = new string[LID_TABSIZE];
            TxtButtonUtworzenieKopii = new string[LID_TABSIZE];
            TxtButtonSkasowanie = new string[LID_TABSIZE];

            TxtAktywnyPrzyrzad = new string[LID_TABSIZE];
            TxtParametr = new string[LID_TABSIZE];
            TxtOpis = new string[LID_TABSIZE];

            TxtKodAktywnego = new string[LID_TABSIZE];
            TxtKodPodlaczonego = new string[LID_TABSIZE];
            TxtButtonWyszukajKod = new string[LID_TABSIZE];

            //
            // *****************************************
            // Cykl
            // *****************************************
            //
            TxtCykl[ETPROC_CZEKAM_NA_BRAK_STARTU, LID_PL] = "Czekam na puszczenie pulpitu dwuręcznego lub brak startu z PLC";
            TxtCykl[ETPROC_CZEKAM_NA_BRAK_STARTU, LID_EN] = "Waiting for releasing start signal";
            TxtCykl[ETPROC_CZEKAM_NA_BRAK_STARTU, LID_DE] = "Waiting for releasing start signal";
            TxtCykl[ETPROC_CZEKAM_NA_BRAK_STARTU, LID_ES] = "Waiting for releasing start signal";
            TxtCykl[ETPROC_CZEKAM_NA_BRAK_STARTU, LID_HU] = "Waiting for releasing start signal";

            TxtCykl[ETPROC_CZEKAM_NA_POZWOLENIE_STARTU, LID_PL] = "Czekam na zezwolenie startu (kluczyk, Hydra itd.)";
            TxtCykl[ETPROC_CZEKAM_NA_POZWOLENIE_STARTU, LID_EN] = "Waiting for start permition (programming key off, Hydra itd.)";
            TxtCykl[ETPROC_CZEKAM_NA_POZWOLENIE_STARTU, LID_DE] = "Waiting for start permition (programming key off, Hydra itd.)";
            TxtCykl[ETPROC_CZEKAM_NA_POZWOLENIE_STARTU, LID_ES] = "Waiting for start permition (programming key off, Hydra itd.)";
            TxtCykl[ETPROC_CZEKAM_NA_POZWOLENIE_STARTU, LID_HU] = "Waiting for start permition (programming key off, Hydra itd.)";

            TxtCykl[ETPROC_CZEKAM_NA_CYLINDER_W_GORZE, LID_PL] = "Czekam na pozycję górną cylindra";
            TxtCykl[ETPROC_CZEKAM_NA_CYLINDER_W_GORZE, LID_EN] = "Waiting for cylinder up (start) position";
            TxtCykl[ETPROC_CZEKAM_NA_CYLINDER_W_GORZE, LID_DE] = "Waiting for cylinder up (start) position";
            TxtCykl[ETPROC_CZEKAM_NA_CYLINDER_W_GORZE, LID_ES] = "Waiting for cylinder up (start) position";
            TxtCykl[ETPROC_CZEKAM_NA_CYLINDER_W_GORZE, LID_HU] = "Waiting for cylinder up (start) position";

            TxtCykl[ETPROC_CZEKAM_NA_START, LID_PL] = "Czekam na start z pulpitu dwuręcznego lub start z PLC";
            TxtCykl[ETPROC_CZEKAM_NA_START, LID_EN] = "Waiting for start";
            TxtCykl[ETPROC_CZEKAM_NA_START, LID_DE] = "Waiting for start";
            TxtCykl[ETPROC_CZEKAM_NA_START, LID_ES] = "Waiting for start";
            TxtCykl[ETPROC_CZEKAM_NA_START, LID_HU] = "Waiting for start";

            TxtCykl[ETPROC_CZEKAM_NA_CZUJNIKI_START, LID_PL] = "Czekam na sygnały z czujników w pozycji górnej cylindra (START)";
            TxtCykl[ETPROC_CZEKAM_NA_CZUJNIKI_START, LID_EN] = "Waiting for signals from sensors (up cylinder position)";
            TxtCykl[ETPROC_CZEKAM_NA_CZUJNIKI_START, LID_DE] = "Waiting for signals from sensors (up cylinder position)";
            TxtCykl[ETPROC_CZEKAM_NA_CZUJNIKI_START, LID_ES] = "Waiting for signals from sensors (up cylinder position)";
            TxtCykl[ETPROC_CZEKAM_NA_CZUJNIKI_START, LID_HU] = "Waiting for signals from sensors (up cylinder position)";

            TxtCykl[ETPROC_CZEKAM_NA_DK, LID_PL] = "Czekam na osiągnięcie ciśnieniea";
            TxtCykl[ETPROC_CZEKAM_NA_DK, LID_EN] = "Waiting for pressure";
            TxtCykl[ETPROC_CZEKAM_NA_DK, LID_DE] = "Waiting for pressure";
            TxtCykl[ETPROC_CZEKAM_NA_DK, LID_ES] = "Waiting for pressure";
            TxtCykl[ETPROC_CZEKAM_NA_DK, LID_HU] = "Waiting for pressure";

            TxtCykl[ETPROC_CZEKAM_NA_CZUJNIKI_DK, LID_PL] = "Czekam na sygnały z czujników w pozycji dolnej cylindra (DK)";
            TxtCykl[ETPROC_CZEKAM_NA_CZUJNIKI_DK, LID_EN] = "Waiting for signals from sensors (down cylinder position)";
            TxtCykl[ETPROC_CZEKAM_NA_CZUJNIKI_DK, LID_DE] = "Waiting for signals from sensors (down cylinder position)";
            TxtCykl[ETPROC_CZEKAM_NA_CZUJNIKI_DK, LID_ES] = "Waiting for signals from sensors (down cylinder position)";
            TxtCykl[ETPROC_CZEKAM_NA_CZUJNIKI_DK, LID_HU] = "Waiting for signals from sensors (down cylinder position)";

            TxtCykl[ETPROC_CZEKAM_NA_KONTROLA_NAKRETKI, LID_PL] = "Kontrola nakrętki";
            TxtCykl[ETPROC_CZEKAM_NA_KONTROLA_NAKRETKI, LID_EN] = "Nut control";
            TxtCykl[ETPROC_CZEKAM_NA_KONTROLA_NAKRETKI, LID_DE] = "Nut control";
            TxtCykl[ETPROC_CZEKAM_NA_KONTROLA_NAKRETKI, LID_ES] = "Nut control";
            TxtCykl[ETPROC_CZEKAM_NA_KONTROLA_NAKRETKI, LID_HU] = "Nut control";

            TxtCykl[ETPROC_CZEKAM_NA_WYBLOKOWANIE, LID_PL] = "Czekam na zezwolenie z wyblokowania";
            TxtCykl[ETPROC_CZEKAM_NA_WYBLOKOWANIE, LID_EN] = "Waiting for energy release signal (blocking controller)";
            TxtCykl[ETPROC_CZEKAM_NA_WYBLOKOWANIE, LID_DE] = "Waiting for energy release signal (blocking controller)";
            TxtCykl[ETPROC_CZEKAM_NA_WYBLOKOWANIE, LID_ES] = "Waiting for energy release signal (blocking controller)";
            TxtCykl[ETPROC_CZEKAM_NA_WYBLOKOWANIE, LID_HU] = "Waiting for energy release signal (blocking controller)";

            TxtCykl[ETPROC_ZGRZEWANIE_DOCISK_WSTEPNY, LID_PL] = "Docisk wstępny";
            TxtCykl[ETPROC_ZGRZEWANIE_DOCISK_WSTEPNY, LID_EN] = "Prepressing";
            TxtCykl[ETPROC_ZGRZEWANIE_DOCISK_WSTEPNY, LID_DE] = "Prepressing";
            TxtCykl[ETPROC_ZGRZEWANIE_DOCISK_WSTEPNY, LID_ES] = "Prepressing";
            TxtCykl[ETPROC_ZGRZEWANIE_DOCISK_WSTEPNY, LID_HU] = "Prepressing";

            TxtCykl[ETPROC_ZGRZEWANIE_PODGRZEWANIE, LID_PL] = "ZGRZEWANIE - Podgrzewanie";
            TxtCykl[ETPROC_ZGRZEWANIE_PODGRZEWANIE, LID_EN] = "WELDING - Prewelding";
            TxtCykl[ETPROC_ZGRZEWANIE_PODGRZEWANIE, LID_DE] = "WELDING - Prewelding";
            TxtCykl[ETPROC_ZGRZEWANIE_PODGRZEWANIE, LID_ES] = "WELDING - Prewelding";
            TxtCykl[ETPROC_ZGRZEWANIE_PODGRZEWANIE, LID_HU] = "WELDING - Prewelding";

            TxtCykl[ETPROC_ZGRZEWANIE_PODGRZEWANIE_PAUZA, LID_PL] = "ZGRZEWANIE - Podgrzewanie pauza";
            TxtCykl[ETPROC_ZGRZEWANIE_PODGRZEWANIE_PAUZA, LID_EN] = "WELDING - Prewelding pause";
            TxtCykl[ETPROC_ZGRZEWANIE_PODGRZEWANIE_PAUZA, LID_DE] = "WELDING - Prewelding pause";
            TxtCykl[ETPROC_ZGRZEWANIE_PODGRZEWANIE_PAUZA, LID_ES] = "WELDING - Prewelding pause";
            TxtCykl[ETPROC_ZGRZEWANIE_PODGRZEWANIE_PAUZA, LID_HU] = "WELDING - Prewelding pause";

            TxtCykl[ETPROC_ZGRZEWANIE_ZGRZEWANIE, LID_PL] = "ZGRZEWANIE - Zgrzewanie zasadnicze";
            TxtCykl[ETPROC_ZGRZEWANIE_ZGRZEWANIE, LID_EN] = "WELDING - Main welding";
            TxtCykl[ETPROC_ZGRZEWANIE_ZGRZEWANIE, LID_DE] = "WELDING - Main welding";
            TxtCykl[ETPROC_ZGRZEWANIE_ZGRZEWANIE, LID_ES] = "WELDING - Main welding";
            TxtCykl[ETPROC_ZGRZEWANIE_ZGRZEWANIE, LID_HU] = "WELDING - Main welding";

            TxtCykl[ETPROC_ZGRZEWANIE_ZGRZEWANIE_PAUZA, LID_PL] = "ZGRZEWANIE - Pauza pomiędzy impulsami zgrzewania";
            TxtCykl[ETPROC_ZGRZEWANIE_ZGRZEWANIE_PAUZA, LID_EN] = "WELDING - Pause beetwean welding impulses";
            TxtCykl[ETPROC_ZGRZEWANIE_ZGRZEWANIE_PAUZA, LID_DE] = "WELDING - Pause beetwean welding impulses";
            TxtCykl[ETPROC_ZGRZEWANIE_ZGRZEWANIE_PAUZA, LID_ES] = "WELDING - Pause beetwean welding impulses";
            TxtCykl[ETPROC_ZGRZEWANIE_ZGRZEWANIE_PAUZA, LID_HU] = "WELDING - Pause beetwean welding impulses";

            TxtCykl[ETPROC_ZGRZEWANIE_DOGRZEWANIE_PAUZA, LID_PL] = "DOGRZEWANIE - Pauza pomiędzy zgrzewaniem a dogrzewaniem";
            TxtCykl[ETPROC_ZGRZEWANIE_DOGRZEWANIE_PAUZA, LID_EN] = "POSTWELDING - Pause beetwean main welding and postwelding";
            TxtCykl[ETPROC_ZGRZEWANIE_DOGRZEWANIE_PAUZA, LID_DE] = "POSTWELDING - Pause beetwean main welding and postwelding";
            TxtCykl[ETPROC_ZGRZEWANIE_DOGRZEWANIE_PAUZA, LID_ES] = "POSTWELDING - Pause beetwean main welding and postwelding";
            TxtCykl[ETPROC_ZGRZEWANIE_DOGRZEWANIE_PAUZA, LID_HU] = "POSTWELDING - Pause beetwean main welding and postwelding";

            TxtCykl[ETPROC_ZGRZEWANIE_DOGRZEWANIE, LID_PL] = "DOGRZEWANIE - Dogrzewanie";
            TxtCykl[ETPROC_ZGRZEWANIE_DOGRZEWANIE, LID_EN] = "POSTWELDING - Postwelding";
            TxtCykl[ETPROC_ZGRZEWANIE_DOGRZEWANIE, LID_DE] = "POSTWELDING - Postwelding";
            TxtCykl[ETPROC_ZGRZEWANIE_DOGRZEWANIE, LID_ES] = "POSTWELDING - Postwelding";
            TxtCykl[ETPROC_ZGRZEWANIE_DOGRZEWANIE, LID_HU] = "POSTWELDING - Postwelding";

            TxtCykl[ETPROC_ZGRZEWANIE_DOCISK_KONCOWY, LID_PL] = "Docisk końcowy";
            TxtCykl[ETPROC_ZGRZEWANIE_DOCISK_KONCOWY, LID_EN] = "Postpressing";
            TxtCykl[ETPROC_ZGRZEWANIE_DOCISK_KONCOWY, LID_DE] = "Postpressing";
            TxtCykl[ETPROC_ZGRZEWANIE_DOCISK_KONCOWY, LID_ES] = "Postpressing";
            TxtCykl[ETPROC_ZGRZEWANIE_DOCISK_KONCOWY, LID_HU] = "Postpressing";

            TxtCykl[ETPROC_ZGRZEWANIE_KONIEC, LID_PL] = "Koniec zgrzewania - trwają obliczenia";
            TxtCykl[ETPROC_ZGRZEWANIE_KONIEC, LID_EN] = "Welding end - calculations";
            TxtCykl[ETPROC_ZGRZEWANIE_KONIEC, LID_DE] = "Welding end - calculations";
            TxtCykl[ETPROC_ZGRZEWANIE_KONIEC, LID_ES] = "Welding end - calculations";
            TxtCykl[ETPROC_ZGRZEWANIE_KONIEC, LID_HU] = "Welding end - calculations";

            TxtCykl[ETPROC_ZGRZEW_OK, LID_PL] = "Koniec zgrzewania - zgrzew poprawny";
            TxtCykl[ETPROC_ZGRZEW_OK, LID_EN] = "Welding end - weld is OK";
            TxtCykl[ETPROC_ZGRZEW_OK, LID_DE] = "Welding end - weld is OK";
            TxtCykl[ETPROC_ZGRZEW_OK, LID_ES] = "Welding end - weld is OK";
            TxtCykl[ETPROC_ZGRZEW_OK, LID_HU] = "Welding end - weld is OK";

            TxtCykl[ETPROC_ZGRZEW_NOK, LID_PL] = "Koniec zgrzewania - zgrzew niepoprawny";
            TxtCykl[ETPROC_ZGRZEW_NOK, LID_EN] = "Welding end - weld is NOT OK";
            TxtCykl[ETPROC_ZGRZEW_NOK, LID_DE] = "Welding end - weld is NOT OK";
            TxtCykl[ETPROC_ZGRZEW_NOK, LID_ES] = "Welding end - weld is NOT OK";
            TxtCykl[ETPROC_ZGRZEW_NOK, LID_HU] = "Welding end - weld is NOT OK";

            TxtCykl[ETPROC_KONIEC_PROCESU, LID_PL] = "Koniec zgrzewania - przesyłanie wyników...";
            TxtCykl[ETPROC_KONIEC_PROCESU, LID_EN] = "Welding end - results receiveing...";
            TxtCykl[ETPROC_KONIEC_PROCESU, LID_DE] = "Welding end - results receiveing...";
            TxtCykl[ETPROC_KONIEC_PROCESU, LID_ES] = "Welding end - results receiveing...";
            TxtCykl[ETPROC_KONIEC_PROCESU, LID_HU] = "Welding end - results receiveing...";

            //
            // *****************************************
            // Taby Produkcja
            // *****************************************
            //
            TxtTabsMain[0, LID_PL] = "Produkcja";
            TxtTabsMain[0, LID_EN] = "Production";
            TxtTabsMain[0, LID_DE] = "Produktion";
            TxtTabsMain[0, LID_ES] = "Producción";
            TxtTabsMain[0, LID_HU] = "Termelési";

            TxtTabsProduction[0, LID_PL] = "Licznik sztuk";
            TxtTabsProduction[0, LID_EN] = "Parts counter";
            TxtTabsProduction[0, LID_DE] = "Stückzähler";
            TxtTabsProduction[0, LID_ES] = "Contador";
            TxtTabsProduction[0, LID_HU] = "Számláló";

            TxtTabsProduction[1, LID_PL] = "Licznik sztuk max.";
            TxtTabsProduction[1, LID_EN] = "Parts counter max.";
            TxtTabsProduction[1, LID_DE] = "Stückzähler max.";
            TxtTabsProduction[1, LID_ES] = "Contador max.";
            TxtTabsProduction[1, LID_HU] = "Számláló max.";

            TxtTabsProduction[2, LID_PL] = "Wizalizacja";
            TxtTabsProduction[2, LID_EN] = "Visualization";
            TxtTabsProduction[2, LID_DE] = "Visualisierung";
            TxtTabsProduction[2, LID_ES] = "Visualización";
            TxtTabsProduction[2, LID_HU] = "Megjelenítés";

            TxtLicznikZeruj[LID_PL] = "Zeruj licznik";
            TxtLicznikZeruj[LID_EN] = "Reset counter";
            TxtLicznikZeruj[LID_DE] = "Zähler zurücksetzen";
            TxtLicznikZeruj[LID_ES] = "Reset del contador";
            TxtLicznikZeruj[LID_HU] = "Reset számláló";

            TxtPrzyrzad[LID_PL] = "Przyrząd";
            TxtPrzyrzad[LID_EN] = "Tool";
            TxtPrzyrzad[LID_DE] = "Werkzeug";
            TxtPrzyrzad[LID_ES] = "Instrumento";
            TxtPrzyrzad[LID_HU] = "Eszköz";

            TxtNrProgramu[LID_PL] = "Nr programu";
            TxtNrProgramu[LID_EN] = "Program number";
            TxtNrProgramu[LID_DE] = "Programm-Nummer";
            TxtNrProgramu[LID_ES] = "Número de programa";
            TxtNrProgramu[LID_HU] = "Program száma";

            //
            // *****************************************
            // Taby Proces
            // *****************************************
            //
            TxtTabsMain[1, LID_PL] = "Proces";
            TxtTabsMain[1, LID_EN] = "Process";
            TxtTabsMain[1, LID_DE] = "Prozess";
            TxtTabsMain[1, LID_ES] = "Proceso";
            TxtTabsMain[1, LID_HU] = "Folyamat";

            TxtTabsProcess[0, LID_PL] = "Sterowanie";
            TxtTabsProcess[0, LID_EN] = "Control";
            TxtTabsProcess[0, LID_DE] = "Steuerung";
            TxtTabsProcess[0, LID_ES] = "Control";
            TxtTabsProcess[0, LID_HU] = "Ellenőrző";

            TxtTabsProcess[1, LID_PL] = "Pomiary";
            TxtTabsProcess[1, LID_EN] = "Measurements";
            TxtTabsProcess[1, LID_DE] = "Messungen";
            TxtTabsProcess[1, LID_ES] = "Mediciones";
            TxtTabsProcess[1, LID_HU] = "Mérések";

            TxtTabsProcess[2, LID_PL] = "Wykresy";
            TxtTabsProcess[2, LID_EN] = "Charts";
            TxtTabsProcess[2, LID_DE] = "Charts";
            TxtTabsProcess[2, LID_ES] = "Gráficas";
            TxtTabsProcess[2, LID_HU] = "Telkek";

            TxtGora[LID_PL] = "Góra";
            TxtGora[LID_EN] = "Up";
            TxtGora[LID_DE] = "Oben";
            TxtGora[LID_ES] = "Arriba";
            TxtGora[LID_HU] = "Tetején";

            TxtDol[LID_PL] = "Dół";
            TxtDol[LID_EN] = "Down";
            TxtDol[LID_DE] = "Unten";
            TxtDol[LID_ES] = "Abajo";
            TxtDol[LID_HU] = "Alján";

            TxtCzujnik[LID_PL] = "Czujnik";
            TxtCzujnik[LID_EN] = "Sensor";
            TxtCzujnik[LID_DE] = "Sensor";
            TxtCzujnik[LID_ES] = "Sensor";
            TxtCzujnik[LID_HU] = "Érzékelő";

            TxtAktualnyProgram[LID_PL] = "Aktualny program";
            TxtAktualnyProgram[LID_EN] = "Actual program";
            TxtAktualnyProgram[LID_DE] = "Aktuelle Programm";
            TxtAktualnyProgram[LID_ES] = "Programa actual";
            TxtAktualnyProgram[LID_HU] = "Aktuális program";

            TxtPozycjaWyjsciowa[LID_PL] = "Pozycja wyjsciowa";
            TxtPozycjaWyjsciowa[LID_EN] = "Starting position";
            TxtPozycjaWyjsciowa[LID_DE] = "Grunstellung";
            TxtPozycjaWyjsciowa[LID_ES] = "Posición inicial";
            TxtPozycjaWyjsciowa[LID_HU] = "Kiindulási pozíció";

            TxtKontrolaNakretek[LID_PL] = "Kontrola nakrętek";
            TxtKontrolaNakretek[LID_EN] = "Nut control";
            TxtKontrolaNakretek[LID_DE] = "Mutter control";
            TxtKontrolaNakretek[LID_ES] = "Control de la tuerca";
            TxtKontrolaNakretek[LID_HU] = "Ellenőrző dió";

            TxtTolerancja[LID_PL] = "Toleracja";
            TxtTolerancja[LID_EN] = "Tolerance";
            TxtTolerancja[LID_DE] = "Toleranz";
            TxtTolerancja[LID_ES] = "Tolerancia";
            TxtTolerancja[LID_HU] = "Tolerancia";

            TxtPozycjaCylindra[LID_PL] = "Pozycja cylindra";
            TxtPozycjaCylindra[LID_EN] = "Cylinder position";
            TxtPozycjaCylindra[LID_DE] = "Zylinder Positionen";
            TxtPozycjaCylindra[LID_ES] = "Coloque el cilindro";
            TxtPozycjaCylindra[LID_HU] = "Henger álláspontja";

            TxtPozycjaCylindraGora[LID_PL] = "Pozycja górna";
            TxtPozycjaCylindraGora[LID_EN] = "Top position";
            TxtPozycjaCylindraGora[LID_DE] = "Obere Position";
            TxtPozycjaCylindraGora[LID_ES] = "Posición superior";
            TxtPozycjaCylindraGora[LID_HU] = "Felső pozícióban";

            TxtPozycjaCylindraDol[LID_PL] = "Pozycja dolna";
            TxtPozycjaCylindraDol[LID_EN] = "Bottom position";
            TxtPozycjaCylindraDol[LID_DE] = "Untere Position";
            TxtPozycjaCylindraDol[LID_ES] = "Posición inferior";
            TxtPozycjaCylindraDol[LID_HU] = "Alsó pozícióban";

            TxtCisnienieWydmuchu[LID_PL] = "Ciśnienie wydmuchu";
            TxtCisnienieWydmuchu[LID_EN] = "Exhaust pressure";
            TxtCisnienieWydmuchu[LID_DE] = "Blasdruck";
            TxtCisnienieWydmuchu[LID_ES] = "Presión de soplado";
            TxtCisnienieWydmuchu[LID_HU] = "Fúj nyomás";

            TxtCisnienieWydmuchuDol[LID_PL] = "Ciśnienie w dole";
            TxtCisnienieWydmuchuDol[LID_EN] = "Pressure in bottom";
            TxtCisnienieWydmuchuDol[LID_DE] = "Blasdruck in unteren Position";
            TxtCisnienieWydmuchuDol[LID_ES] = "Presión en la posición inferior";
            TxtCisnienieWydmuchuDol[LID_HU] = "Nyomás az alsó helyzetben";

            TxtButtonPomiarPradu[LID_PL] = "Pomiar prądu, napięcia i energii";
            TxtButtonPomiarPradu[LID_EN] = "Measuring current, voltage and energy";
            TxtButtonPomiarPradu[LID_DE] = "Die Messung der Strom, Spannung und Energie";
            TxtButtonPomiarPradu[LID_ES] = "Medición de corriente, voltaje y energía";
            TxtButtonPomiarPradu[LID_HU] = "Mérő áram, feszültség és energia";

            TxtButtonPomiarNakretki[LID_PL] = "Pomiar nakrętki";
            TxtButtonPomiarNakretki[LID_EN] = "Measurement of the nut control";
            TxtButtonPomiarNakretki[LID_DE] = "Die Messung der Mutter control";
            TxtButtonPomiarNakretki[LID_ES] = "La medición del control de la tuerca";
            TxtButtonPomiarNakretki[LID_HU] = "Mérése az anya ellenőrzési";

            TxtPomiaryReferencyjne[LID_PL] = "Pomiary referencyjne";
            TxtPomiaryReferencyjne[LID_EN] = "Reference Measurements";
            TxtPomiaryReferencyjne[LID_DE] = "Referenzmessungen";
            TxtPomiaryReferencyjne[LID_ES] = "Medidas de Referencia";
            TxtPomiaryReferencyjne[LID_HU] = "Referencia mérések";

            TxtDaneZOstatniegoZgrzewu[LID_PL] = "Dane z ostatniego zgrzewu";
            TxtDaneZOstatniegoZgrzewu[LID_EN] = "Data from the last weld";
            TxtDaneZOstatniegoZgrzewu[LID_DE] = "Daten der letzten Schweißen";
            TxtDaneZOstatniegoZgrzewu[LID_ES] = "Los datos de la última soldadura";
            TxtDaneZOstatniegoZgrzewu[LID_HU] = "Az adatok az utolsó varrat";

            TxtPomiarTolerancji[LID_PL] = "Pomiar tolerancji";
            TxtPomiarTolerancji[LID_EN] = "Measuring tolerance";
            TxtPomiarTolerancji[LID_DE] = "Messtoleranz";
            TxtPomiarTolerancji[LID_ES] = "Medición de la tolerancia";
            TxtPomiarTolerancji[LID_HU] = "Mérési tűrés";

            TxtPromilMocyZeStepperem[LID_PL] = "Promil mocy ze stepperem";
            TxtPromilMocyZeStepperem[LID_EN] = "Promil power with stepper";
            TxtPromilMocyZeStepperem[LID_DE] = "Promil Leistung mit Stepper";
            TxtPromilMocyZeStepperem[LID_ES] = "Promil power with stepper";
            TxtPromilMocyZeStepperem[LID_HU] = "Promil power with stepper";

            TxtWartoscSredniaPradu[LID_PL] = "Wartość średnia prądu";
            TxtWartoscSredniaPradu[LID_EN] = "The average value of current";
            TxtWartoscSredniaPradu[LID_DE] = "Der Mittelwert der Strom";
            TxtWartoscSredniaPradu[LID_ES] = "El valor medio de la corriente";
            TxtWartoscSredniaPradu[LID_HU] = "Az átlagos értéke a áram";

            TxtWartoscSredniaNapiecia[LID_PL] = "Wartość średnia napięcia";
            TxtWartoscSredniaNapiecia[LID_EN] = "The average value of voltage";
            TxtWartoscSredniaNapiecia[LID_DE] = "Der Mittelwert der Spannung";
            TxtWartoscSredniaNapiecia[LID_ES] = "El valor medio voltaje";
            TxtWartoscSredniaNapiecia[LID_HU] = "Az átlagos értéke feszültség";

            TxtWartoscEnergii[LID_PL] = "Wartość energii";
            TxtWartoscEnergii[LID_EN] = "The value of energy";
            TxtWartoscEnergii[LID_DE] = "Der Wert der Energie";
            TxtWartoscEnergii[LID_ES] = "El valor de la energía";
            TxtWartoscEnergii[LID_HU] = "Az érték az energia";

            TxtWartoscWtopienia[LID_PL] = "Wartość wtopienia";
            TxtWartoscWtopienia[LID_EN] = "The value of fusion";
            TxtWartoscWtopienia[LID_DE] = "Der Wert der Fusion";
            TxtWartoscWtopienia[LID_ES] = "El valor de la fusión";
            TxtWartoscWtopienia[LID_HU] = "Az érték a fúziós";

            TxtPrad[LID_PL] = "Prąd";
            TxtPrad[LID_EN] = "Current";
            TxtPrad[LID_DE] = "Strom";
            TxtPrad[LID_ES] = "Corriente";
            TxtPrad[LID_HU] = "Áram";

            TxtNapiecie[LID_PL] = "Napięcie";
            TxtNapiecie[LID_EN] = "Voltage";
            TxtNapiecie[LID_DE] = "Spannung";
            TxtNapiecie[LID_ES] = "Voltaje";
            TxtNapiecie[LID_HU] = "Feszültség";

            TxtWtopienie[LID_PL] = "Wtopienie";
            TxtWtopienie[LID_EN] = "Injection";
            TxtWtopienie[LID_DE] = "Injection";
            TxtWtopienie[LID_ES] = "Injection";
            TxtWtopienie[LID_HU] = "Injection";

            TxtCzas[LID_PL] = "Czas";
            TxtCzas[LID_EN] = "Time";
            TxtCzas[LID_DE] = "Zeit";
            TxtCzas[LID_ES] = "Tiempo";
            TxtCzas[LID_HU] = "Idő";

            TxtMoc[LID_PL] = "Moc";
            TxtMoc[LID_EN] = "Power";
            TxtMoc[LID_DE] = "Leistung";
            TxtMoc[LID_ES] = "Potencia";
            TxtMoc[LID_HU] = "Teljesítmény";

            //
            // *****************************************
            // Taby Zaklocenia
            // *****************************************
            //
            TxtTabsMain[2, LID_PL] = "Zakłócenia";
            TxtTabsMain[2, LID_EN] = "Errors";
            TxtTabsMain[2, LID_DE] = "Störungen";
            TxtTabsMain[2, LID_ES] = "Errors";
            TxtTabsMain[2, LID_HU] = "Hibaüzenetek";

            TxtAktywneZaklocenia[LID_PL] = "Aktywne zakłócenia";
            TxtAktywneZaklocenia[LID_EN] = "Active errors";
            TxtAktywneZaklocenia[LID_DE] = "Aktiv Störungen";
            TxtAktywneZaklocenia[LID_ES] = "Activo potencia";
            TxtAktywneZaklocenia[LID_HU] = "Aktív teljesítmény";

            TxtButtonPotwierdzenie[LID_PL] = "Potwierdzenie";
            TxtButtonPotwierdzenie[LID_EN] = "Acknowledgment";
            TxtButtonPotwierdzenie[LID_DE] = "Bestätigung";
            TxtButtonPotwierdzenie[LID_ES] = "Confirmación";
            TxtButtonPotwierdzenie[LID_HU] = "Megerősítés";

            //
            // *****************************************
            // Taby Przyrzady
            // *****************************************
            //
            TxtTabsMain[3, LID_PL] = "Przyrządy";
            TxtTabsMain[3, LID_EN] = "Tools";
            TxtTabsMain[3, LID_DE] = "Werkzeuge";
            TxtTabsMain[3, LID_ES] = "Herramientas";
            TxtTabsMain[3, LID_HU] = "Eszközök";

            TxtTabsTools[0, LID_PL] = "Zarządzanie";
            TxtTabsTools[0, LID_EN] = "Management";
            TxtTabsTools[0, LID_DE] = "Management";
            TxtTabsTools[0, LID_ES] = "Gestión";
            TxtTabsTools[0, LID_HU] = "Menedzsment";

            TxtTabsTools[1, LID_PL] = "Ustawienia";
            TxtTabsTools[1, LID_EN] = "Settings";
            TxtTabsTools[1, LID_DE] = "Einstellungen";
            TxtTabsTools[1, LID_ES] = "Configuración";
            TxtTabsTools[1, LID_HU] = "Beállítások";

            TxtTabsTools[2, LID_PL] = "Kodowanie";
            TxtTabsTools[2, LID_EN] = "Coding";
            TxtTabsTools[2, LID_DE] = "Kodierung";
            TxtTabsTools[2, LID_ES] = "Codificación";
            TxtTabsTools[2, LID_HU] = "Kódolás";

            TxtListaPrzyrzadow[LID_PL] = "Lista przyrządów";
            TxtListaPrzyrzadow[LID_EN] = "List of tools";
            TxtListaPrzyrzadow[LID_DE] = "Liste der Werkzeuge";
            TxtListaPrzyrzadow[LID_ES] = "Lista de herramientas";
            TxtListaPrzyrzadow[LID_HU] = "Az eszközök listájának";

            TxtAktywny[LID_PL] = "Aktywny";
            TxtAktywny[LID_EN] = "Active";
            TxtAktywny[LID_DE] = "Aktiv";
            TxtAktywny[LID_ES] = "Activo";
            TxtAktywny[LID_HU] = "Aktív";

            TxtButtonAktywacja[LID_PL] = "Aktywacja wybranego z listy";
            TxtButtonAktywacja[LID_EN] = "Activate chosen from the list";
            TxtButtonAktywacja[LID_DE] = "Aktivieren aus der Liste ausgewählt";
            TxtButtonAktywacja[LID_ES] = "Activar elegida de la lista";
            TxtButtonAktywacja[LID_HU] = "Aktiválja választott a listáról";

            TxtButtonUtworzenieNowego[LID_PL] = "Utworzenie nowego...";
            TxtButtonUtworzenieNowego[LID_EN] = "Creation of a new tool...";
            TxtButtonUtworzenieNowego[LID_DE] = "Die Schaffung neuen Werkzeuge...";
            TxtButtonUtworzenieNowego[LID_ES] = "La creación de una nueva herramienta...";
            TxtButtonUtworzenieNowego[LID_HU] = "Létrehozása egy új eszköz...";

            TxtButtonUtworzenieKopii[LID_PL] = "Utworzenie kopii aktywnego narzędzia...";
            TxtButtonUtworzenieKopii[LID_EN] = "Create a copy of the active tool...";
            TxtButtonUtworzenieKopii[LID_DE] = "Erstellen Sie eine Kopie des aktiven Werkzeugs...";
            TxtButtonUtworzenieKopii[LID_ES] = "Crea una copia de la herramienta activa...";
            TxtButtonUtworzenieKopii[LID_HU] = "Hozzon létre egy másolatot az aktív eszköz...";

            TxtButtonSkasowanie[LID_PL] = "Skasowanie wybranego z listy";
            TxtButtonSkasowanie[LID_EN] = "Deleting selected from a list";
            TxtButtonSkasowanie[LID_DE] = "Löschen aus einer Liste ausgewählt";
            TxtButtonSkasowanie[LID_ES] = "Eliminar seleccionados de una lista";
            TxtButtonSkasowanie[LID_HU] = "Törlése egy listából";

            TxtAktywnyPrzyrzad[LID_PL] = "Aktywny przyrząd";
            TxtAktywnyPrzyrzad[LID_EN] = "Active tool";
            TxtAktywnyPrzyrzad[LID_DE] = "Aktives Werkzeug";
            TxtAktywnyPrzyrzad[LID_ES] = "Active la herramienta";
            TxtAktywnyPrzyrzad[LID_HU] = "Aktív eszköz";

            TxtParametr[LID_PL] = "Parametr";
            TxtParametr[LID_EN] = "Parameter";
            TxtParametr[LID_DE] = "Parameter";
            TxtParametr[LID_ES] = "Parámetro";
            TxtParametr[LID_HU] = "Paraméter";

            TxtOpis[LID_PL] = "Opis";
            TxtOpis[LID_EN] = "Description";
            TxtOpis[LID_DE] = "Beschreibung";
            TxtOpis[LID_ES] = "Descripción";
            TxtOpis[LID_HU] = "Leírás";

            TxtKodAktywnego[LID_PL] = "Kod przyrządu aktywnego";
            TxtKodAktywnego[LID_EN] = "Code of active tool";
            TxtKodAktywnego[LID_DE] = "Code aktive Werkzeug";
            TxtKodAktywnego[LID_ES] = "Código de herramientas activa";
            TxtKodAktywnego[LID_HU] = "Kód aktív eszköz";

            TxtKodPodlaczonego[LID_PL] = "Kod przyrządu podłączonego";
            TxtKodPodlaczonego[LID_EN] = "Code connected tools";
            TxtKodPodlaczonego[LID_DE] = "Code verbunden Werkzeuge";
            TxtKodPodlaczonego[LID_ES] = "Código herramientas conectadas";
            TxtKodPodlaczonego[LID_HU] = "Kód csatlakoztatott eszközök";

            TxtButtonWyszukajKod[LID_PL] = "Wyszukaj kod w bazie danych";
            TxtButtonWyszukajKod[LID_EN] = "Find the code in the database";
            TxtButtonWyszukajKod[LID_DE] = "Finden Sie den Code in der Datenbank";
            TxtButtonWyszukajKod[LID_ES] = "Buscar el código en la base de datos";
            TxtButtonWyszukajKod[LID_HU] = "Keresse meg a kódot az adatbázisban";
            
            //
            // *****************************************
            // Taby Parametry
            // *****************************************
            //
            TxtTabsMain[4, LID_PL] = "Parametry";
            TxtTabsMain[4, LID_EN] = "Parameters";
            TxtTabsMain[4, LID_DE] = "Parameter";
            TxtTabsMain[4, LID_ES] = "Parámetros";
            TxtTabsMain[4, LID_HU] = "Paraméterek";

            TxtTabsParameters[0, LID_PL] = "Zgrzewanie";
            TxtTabsParameters[0, LID_EN] = "Welding";
            TxtTabsParameters[0, LID_DE] = "Schweißen";
            TxtTabsParameters[0, LID_ES] = "Soldadura";
            TxtTabsParameters[0, LID_HU] = "Hegesztés";

            TxtTabsParameters[1, LID_PL] = "Czujn.-Góra";
            TxtTabsParameters[1, LID_EN] = "Sens-Up";
            TxtTabsParameters[1, LID_DE] = "Sens-Oben";
            TxtTabsParameters[1, LID_ES] = "Sens.hasta";
            TxtTabsParameters[1, LID_HU] = "Érzék.tetejére";

            TxtTabsParameters[2, LID_PL] = "Czujn-Dół";
            TxtTabsParameters[2, LID_EN] = "Sens-Down";
            TxtTabsParameters[2, LID_DE] = "Sens-Unten";
            TxtTabsParameters[2, LID_ES] = "Sens.abajo";
            TxtTabsParameters[2, LID_HU] = "Érzék.alsó állásba";

            TxtTabsParameters[3, LID_PL] = "Cyl+Zaw";
            TxtTabsParameters[3, LID_EN] = "Cyl+Valv";
            TxtTabsParameters[3, LID_DE] = "Zyl+Vent";
            TxtTabsParameters[3, LID_ES] = "Cil+Vál";
            TxtTabsParameters[3, LID_HU] = "Heng+Szel";

            TxtTabsParameters[5, LID_PL] = "Konfig";
            TxtTabsParameters[5, LID_EN] = "Config";
            TxtTabsParameters[5, LID_DE] = "Konfig";
            TxtTabsParameters[5, LID_ES] = "Config";
            TxtTabsParameters[5, LID_HU] = "Konfig";

            //
            // *****************************************
            // Taby Sygnaly
            // *****************************************
            //
            TxtTabsMain[5, LID_PL] = "Syg.cyfrowe";
            TxtTabsMain[5, LID_EN] = "Digit.sign.";
            TxtTabsMain[5, LID_DE] = "Digit.sign.";
            TxtTabsMain[5, LID_ES] = "Digit.señ.";
            TxtTabsMain[5, LID_HU] = "Digit.jelek";

            TxtTabsMain[6, LID_PL] = "Syg.analog.";
            TxtTabsMain[6, LID_EN] = "Ana.sign.";
            TxtTabsMain[6, LID_DE] = "Ana.sign.";
            TxtTabsMain[6, LID_ES] = "Ana.señ.";
            TxtTabsMain[6, LID_HU] = "Ana.jelek";

            //
            // *****************************************
            // Taby System
            // *****************************************
            //
            TxtTabsMain[7, LID_PL] = "System";
            TxtTabsMain[7, LID_EN] = "System";
            TxtTabsMain[7, LID_DE] = "System";
            TxtTabsMain[7, LID_ES] = "Sistema";
            TxtTabsMain[7, LID_HU] = "Rendszer";

            TxtTabsSystem[0, LID_PL] = "Uprawanienia";
            TxtTabsSystem[0, LID_EN] = "Permissions";
            TxtTabsSystem[0, LID_DE] = "Berechtigungen";
            TxtTabsSystem[0, LID_ES] = "Permisos";
            TxtTabsSystem[0, LID_HU] = "Engedélyek";

            TxtTabsSystem[1, LID_PL] = "Info";
            TxtTabsSystem[1, LID_EN] = "Info";
            TxtTabsSystem[1, LID_DE] = "Info";
            TxtTabsSystem[1, LID_ES] = "Info";
            TxtTabsSystem[1, LID_HU] = "Info";

            TxtTabsSystem[2, LID_PL] = "Komunikacja";
            TxtTabsSystem[2, LID_EN] = "Communication";
            TxtTabsSystem[2, LID_DE] = "Kommunikation";
            TxtTabsSystem[2, LID_ES] = "Comunicación";
            TxtTabsSystem[2, LID_HU] = "Kommunikáció";

            TxtTabsSystem[3, LID_PL] = "Aplikacja";
            TxtTabsSystem[3, LID_EN] = "Application";
            TxtTabsSystem[3, LID_DE] = "Anwendung";
            TxtTabsSystem[3, LID_ES] = "Aplicación";
            TxtTabsSystem[3, LID_HU] = "Alkalmazás";

            //
            // *****************************************
            // Welcome
            // *****************************************
            //
            TxtWelcome[LID_PL] = "Witamy";
            TxtWelcome[LID_EN] = "Welcome";
            TxtWelcome[LID_DE] = "Willkommen";
            TxtWelcome[LID_ES] = "Bienvenida";
            TxtWelcome[LID_HU] = "Üdvözöljük";

            //
            // *****************************************
            // Parametry zgrzewania
            // *****************************************
            //

            //
            // PL
            //

            TxtParametryNazwa[PARAM_NAME_PrePressTime, LID_PL] = "Czas docisku wstępnego";
            TxtParametryNazwa[PARAM_NAME_PreWeldCurrent, LID_PL] = "Prąd podgrzewania";
            TxtParametryNazwa[PARAM_NAME_PreWeldTime, LID_PL] = "Czas podgrzewania";
            TxtParametryNazwa[PARAM_NAME_PreWeldPause, LID_PL] = "Pauza podgrzewania";
            TxtParametryNazwa[PARAM_NAME_WeldCurrent, LID_PL] = "Prąd zgrzewania";
            TxtParametryNazwa[PARAM_NAME_WeldTime, LID_PL] = "Czas zgrzewania";
            TxtParametryNazwa[PARAM_NAME_PostWeldPause, LID_PL] = "Pauza dogrzewania";
            TxtParametryNazwa[PARAM_NAME_PostWeldCurrent, LID_PL] = "Prąd dogrzewania";
            TxtParametryNazwa[PARAM_NAME_PostWeldTime, LID_PL] = "Czas dogrzewania";
            TxtParametryNazwa[PARAM_NAME_PostPressTime, LID_PL] = "Czas docisku końcowego";

            TxtParametryNazwa[PARAM_NAME_Impulses, LID_PL] = "Liczba impulsów";
            TxtParametryNazwa[PARAM_NAME_ImpulsesPause, LID_PL] = "Pauza impulsów";
            TxtParametryNazwa[PARAM_NAME_StepperPercent, LID_PL] = "Stepper procent";
            TxtParametryNazwa[PARAM_NAME_StepperCounter, LID_PL] = "Stepper wartość";

            TxtParametryNazwa[PARAM_NAME_PressureSet, LID_PL] = "Ciśnienie zadane";
            TxtParametryNazwa[PARAM_NAME_PressureSwitch, LID_PL] = "Ciśnienie osiągnięte";

            TxtParametryNazwa[PARAM_NAME_CylinderPositionDown, LID_PL] = "Nakr. - Pozycja cylindra";
            TxtParametryNazwa[PARAM_NAME_CylinderPositionDownTolerance, LID_PL] = "Nakr. - Pozycja cylindra - Tolerancja";
            TxtParametryNazwa[PARAM_NAME_ExhaustPressure, LID_PL] = "Nakr. - Ciśnienie wydmuchu";
            TxtParametryNazwa[PARAM_NAME_ExhaustPressureTolerance, LID_PL] = "Nakr. - Ciśnienie wydmuchu - Tolerancja";
            
            TxtParametryNazwa[PARAM_NAME_Injection, LID_PL] = "Min. wtopienie";

            TxtParametryNazwa[PARAM_NAME_IrefTolerance, LID_PL] = "Tolerancja prądu";
            TxtParametryNazwa[PARAM_NAME_UrefTolerance, LID_PL] = "Tolerancja napięcia";
            TxtParametryNazwa[PARAM_NAME_ErefTolerance, LID_PL] = "Tolerancja energii";

            TxtParametryNazwa[PARAM_NAME_Iref, LID_PL] = "Prąd zadany";
            TxtParametryNazwa[PARAM_NAME_Uref, LID_PL] = "Napięcie zadane";
            TxtParametryNazwa[PARAM_NAME_Eref, LID_PL] = "Energia zadana";

            //
            // EN
            //

            TxtParametryNazwa[PARAM_NAME_PrePressTime, LID_EN] = "Prepress time";
            TxtParametryNazwa[PARAM_NAME_PreWeldCurrent, LID_EN] = "Prewelding current";
            TxtParametryNazwa[PARAM_NAME_PreWeldTime, LID_EN] = "Prewelding time";
            TxtParametryNazwa[PARAM_NAME_PreWeldPause, LID_EN] = "Prewelding pause";
            TxtParametryNazwa[PARAM_NAME_WeldCurrent, LID_EN] = "Welding current";
            TxtParametryNazwa[PARAM_NAME_WeldTime, LID_EN] = "Welding time";
            TxtParametryNazwa[PARAM_NAME_PostWeldPause, LID_EN] = "Postwelding pause";
            TxtParametryNazwa[PARAM_NAME_PostWeldCurrent, LID_EN] = "Postwelding current";
            TxtParametryNazwa[PARAM_NAME_PostWeldTime, LID_EN] = "Postwelding time";
            TxtParametryNazwa[PARAM_NAME_PostPressTime, LID_EN] = "Postpress time";

            TxtParametryNazwa[PARAM_NAME_Impulses, LID_EN] = "Impulses number";
            TxtParametryNazwa[PARAM_NAME_ImpulsesPause, LID_EN] = "Impulses pause";
            TxtParametryNazwa[PARAM_NAME_StepperPercent, LID_EN] = "Stepper percent";
            TxtParametryNazwa[PARAM_NAME_StepperCounter, LID_EN] = "Stepper counter";

            TxtParametryNazwa[PARAM_NAME_PressureSet, LID_EN] = "Pressure set";
            TxtParametryNazwa[PARAM_NAME_PressureSwitch, LID_EN] = "Pressure switch";

            TxtParametryNazwa[PARAM_NAME_CylinderPositionDown, LID_EN] = "Nut - Cylinder position";
            TxtParametryNazwa[PARAM_NAME_CylinderPositionDownTolerance, LID_EN] = "Nut - Cylinder position - Tolerance";
            TxtParametryNazwa[PARAM_NAME_ExhaustPressure, LID_EN] = "Nut - Exhaust pressure";
            TxtParametryNazwa[PARAM_NAME_ExhaustPressureTolerance, LID_EN] = "Nut - Exhaust pressure - Tolerance";

            TxtParametryNazwa[PARAM_NAME_Injection, LID_EN] = "Min. fusion";

            TxtParametryNazwa[PARAM_NAME_IrefTolerance, LID_EN] = "Tolerance of current";
            TxtParametryNazwa[PARAM_NAME_UrefTolerance, LID_EN] = "Tolerance of voltage";
            TxtParametryNazwa[PARAM_NAME_ErefTolerance, LID_EN] = "Tolerance of energie";

            TxtParametryNazwa[PARAM_NAME_Iref, LID_EN] = "Current";
            TxtParametryNazwa[PARAM_NAME_Uref, LID_EN] = "Voltage";
            TxtParametryNazwa[PARAM_NAME_Eref, LID_EN] = "Energie";

            //
            // DE
            //

            TxtParametryNazwa[PARAM_NAME_PrePressTime, LID_DE] = "Prepress time";
            TxtParametryNazwa[PARAM_NAME_PreWeldCurrent, LID_DE] = "Prewelding current";
            TxtParametryNazwa[PARAM_NAME_PreWeldTime, LID_DE] = "Prewelding time";
            TxtParametryNazwa[PARAM_NAME_PreWeldPause, LID_DE] = "Prewelding pause";
            TxtParametryNazwa[PARAM_NAME_WeldCurrent, LID_DE] = "Welding current";
            TxtParametryNazwa[PARAM_NAME_WeldTime, LID_DE] = "Welding time";
            TxtParametryNazwa[PARAM_NAME_PostWeldPause, LID_DE] = "Postwelding pause";
            TxtParametryNazwa[PARAM_NAME_PostWeldCurrent, LID_DE] = "Postwelding current";
            TxtParametryNazwa[PARAM_NAME_PostWeldTime, LID_DE] = "Postwelding time";
            TxtParametryNazwa[PARAM_NAME_PostPressTime, LID_DE] = "Postpress time";

            TxtParametryNazwa[PARAM_NAME_Impulses, LID_DE] = "Impulses number";
            TxtParametryNazwa[PARAM_NAME_ImpulsesPause, LID_DE] = "Impulses pause";
            TxtParametryNazwa[PARAM_NAME_StepperPercent, LID_DE] = "Stepper percent";
            TxtParametryNazwa[PARAM_NAME_StepperCounter, LID_DE] = "Stepper counter";

            TxtParametryNazwa[PARAM_NAME_PressureSet, LID_DE] = "Pressure set";
            TxtParametryNazwa[PARAM_NAME_PressureSwitch, LID_DE] = "Pressure switch";

            TxtParametryNazwa[PARAM_NAME_Injection, LID_DE] = "Min. fusion";

            TxtParametryNazwa[PARAM_NAME_CylinderPositionDown, LID_DE] = "Cylinder position";
            TxtParametryNazwa[PARAM_NAME_CylinderPositionDownTolerance, LID_DE] = "Tolerance of cylinder position";
            TxtParametryNazwa[PARAM_NAME_ExhaustPressure, LID_DE] = "Exhaust pressure";
            TxtParametryNazwa[PARAM_NAME_ExhaustPressureTolerance, LID_DE] = "Tolerance of exhaust pressure";
            TxtParametryNazwa[PARAM_NAME_IrefTolerance, LID_DE] = "Tolerance of current";
            TxtParametryNazwa[PARAM_NAME_UrefTolerance, LID_DE] = "Tolerance of voltage";
            TxtParametryNazwa[PARAM_NAME_ErefTolerance, LID_DE] = "Tolerance of energie";

            TxtParametryNazwa[PARAM_NAME_Iref, LID_DE] = "Current";
            TxtParametryNazwa[PARAM_NAME_Uref, LID_DE] = "Voltage";
            TxtParametryNazwa[PARAM_NAME_Eref, LID_DE] = "Energie";

            //
            // ES
            //

            TxtParametryNazwa[PARAM_NAME_PrePressTime, LID_ES] = "Prepress time";
            TxtParametryNazwa[PARAM_NAME_PreWeldCurrent, LID_ES] = "Prewelding current";
            TxtParametryNazwa[PARAM_NAME_PreWeldTime, LID_ES] = "Prewelding time";
            TxtParametryNazwa[PARAM_NAME_PreWeldPause, LID_ES] = "Prewelding pause";
            TxtParametryNazwa[PARAM_NAME_WeldCurrent, LID_ES] = "Welding current";
            TxtParametryNazwa[PARAM_NAME_WeldTime, LID_ES] = "Welding time";
            TxtParametryNazwa[PARAM_NAME_PostWeldPause, LID_ES] = "Postwelding pause";
            TxtParametryNazwa[PARAM_NAME_PostWeldCurrent, LID_ES] = "Postwelding current";
            TxtParametryNazwa[PARAM_NAME_PostWeldTime, LID_ES] = "Postwelding time";
            TxtParametryNazwa[PARAM_NAME_PostPressTime, LID_ES] = "Postpress time";

            TxtParametryNazwa[PARAM_NAME_Impulses, LID_ES] = "Impulses number";
            TxtParametryNazwa[PARAM_NAME_ImpulsesPause, LID_ES] = "Impulses pause";
            TxtParametryNazwa[PARAM_NAME_StepperPercent, LID_ES] = "Stepper percent";
            TxtParametryNazwa[PARAM_NAME_StepperCounter, LID_ES] = "Stepper counter";

            TxtParametryNazwa[PARAM_NAME_PressureSet, LID_ES] = "Pressure set";
            TxtParametryNazwa[PARAM_NAME_PressureSwitch, LID_ES] = "Pressure switch";

            TxtParametryNazwa[PARAM_NAME_Injection, LID_ES] = "Min. fusión";

            TxtParametryNazwa[PARAM_NAME_CylinderPositionDown, LID_ES] = "Cylinder position";
            TxtParametryNazwa[PARAM_NAME_CylinderPositionDownTolerance, LID_ES] = "Tolerance of cylinder position";
            TxtParametryNazwa[PARAM_NAME_ExhaustPressure, LID_ES] = "Exhaust pressure";
            TxtParametryNazwa[PARAM_NAME_ExhaustPressureTolerance, LID_ES] = "Tolerance of exhaust pressure";
            TxtParametryNazwa[PARAM_NAME_IrefTolerance, LID_ES] = "Tolerance of current";
            TxtParametryNazwa[PARAM_NAME_UrefTolerance, LID_ES] = "Tolerance of voltage";
            TxtParametryNazwa[PARAM_NAME_ErefTolerance, LID_ES] = "Tolerance of energie";

            TxtParametryNazwa[PARAM_NAME_Iref, LID_ES] = "Current";
            TxtParametryNazwa[PARAM_NAME_Uref, LID_ES] = "Voltage";
            TxtParametryNazwa[PARAM_NAME_Eref, LID_ES] = "Energie";
            //
            // HU
            //

            TxtParametryNazwa[PARAM_NAME_PrePressTime, LID_HU] = "Prepress time";
            TxtParametryNazwa[PARAM_NAME_PreWeldCurrent, LID_HU] = "Prewelding current";
            TxtParametryNazwa[PARAM_NAME_PreWeldTime, LID_HU] = "Prewelding time";
            TxtParametryNazwa[PARAM_NAME_PreWeldPause, LID_HU] = "Prewelding pause";
            TxtParametryNazwa[PARAM_NAME_WeldCurrent, LID_HU] = "Welding current";
            TxtParametryNazwa[PARAM_NAME_WeldTime, LID_HU] = "Welding time";
            TxtParametryNazwa[PARAM_NAME_PostWeldPause, LID_HU] = "Postwelding pause";
            TxtParametryNazwa[PARAM_NAME_PostWeldCurrent, LID_HU] = "Postwelding current";
            TxtParametryNazwa[PARAM_NAME_PostWeldTime, LID_HU] = "Postwelding time";
            TxtParametryNazwa[PARAM_NAME_PostPressTime, LID_HU] = "Postpress time";

            TxtParametryNazwa[PARAM_NAME_Impulses, LID_HU] = "Impulses number";
            TxtParametryNazwa[PARAM_NAME_ImpulsesPause, LID_HU] = "Impulses pause";
            TxtParametryNazwa[PARAM_NAME_StepperPercent, LID_HU] = "Stepper percent";
            TxtParametryNazwa[PARAM_NAME_StepperCounter, LID_HU] = "Stepper counter";

            TxtParametryNazwa[PARAM_NAME_PressureSet, LID_HU] = "Pressure set";
            TxtParametryNazwa[PARAM_NAME_PressureSwitch, LID_HU] = "Pressure switch";

            TxtParametryNazwa[PARAM_NAME_Injection, LID_HU] = "Min. fúziós";

            TxtParametryNazwa[PARAM_NAME_CylinderPositionDown, LID_HU] = "Cylinder position";
            TxtParametryNazwa[PARAM_NAME_CylinderPositionDownTolerance, LID_HU] = "Tolerance of cylinder position";
            TxtParametryNazwa[PARAM_NAME_ExhaustPressure, LID_HU] = "Exhaust pressure";
            TxtParametryNazwa[PARAM_NAME_ExhaustPressureTolerance, LID_HU] = "Tolerance of exhaust pressure";
            TxtParametryNazwa[PARAM_NAME_IrefTolerance, LID_HU] = "Tolerance of current";
            TxtParametryNazwa[PARAM_NAME_UrefTolerance, LID_HU] = "Tolerance of voltage";
            TxtParametryNazwa[PARAM_NAME_ErefTolerance, LID_HU] = "Tolerance of energie";

            TxtParametryNazwa[PARAM_NAME_Iref, LID_HU] = "Current";
            TxtParametryNazwa[PARAM_NAME_Uref, LID_HU] = "Voltage";
            TxtParametryNazwa[PARAM_NAME_Eref, LID_HU] = "Energie";

            //
            // *****************************************
            // Parametry przyrządu
            // *****************************************
            //

            //
            // PL
            //

            TxtToolParametryNazwa[TOOLPARAM_NAME_Licznik, LID_PL] = "Licznik sztuk";
            TxtToolParametryNazwa[TOOLPARAM_NAME_LicznikMax, LID_PL] = "Licznik sztuk Max";
            TxtToolParametryNazwa[TOOLPARAM_NAME_StepperLicznik, LID_PL] = "Stepper Licznik";
            TxtToolParametryNazwa[TOOLPARAM_NAME_StepperOstrzezenie, LID_PL] = "Stepper Ostrzeżenie";
            TxtToolParametryNazwa[TOOLPARAM_NAME_StepperMax, LID_PL] = "Stepper Max";
            TxtToolParametryNazwa[TOOLPARAM_NAME_Sluza, LID_PL] = "Śluza";
            TxtToolParametryNazwa[TOOLPARAM_NAME_LiczbaProgramow, LID_PL] = "Liczba programów";

            //
            // EN
            //

            TxtToolParametryNazwa[TOOLPARAM_NAME_Licznik, LID_EN] = "Counter";
            TxtToolParametryNazwa[TOOLPARAM_NAME_LicznikMax, LID_EN] = "Counter Max";
            TxtToolParametryNazwa[TOOLPARAM_NAME_StepperLicznik, LID_EN] = "Stepper Counter";
            TxtToolParametryNazwa[TOOLPARAM_NAME_StepperOstrzezenie, LID_EN] = "Stepper Warning";
            TxtToolParametryNazwa[TOOLPARAM_NAME_StepperMax, LID_EN] = "Stepper Max";
            TxtToolParametryNazwa[TOOLPARAM_NAME_Sluza, LID_EN] = "Sluice";
            TxtToolParametryNazwa[TOOLPARAM_NAME_LiczbaProgramow, LID_EN] = "Number of programs";

            //
            // DE
            //

            TxtToolParametryNazwa[TOOLPARAM_NAME_Licznik, LID_DE] = "Zähler";
            TxtToolParametryNazwa[TOOLPARAM_NAME_LicznikMax, LID_DE] = "Zähler Max";
            TxtToolParametryNazwa[TOOLPARAM_NAME_StepperLicznik, LID_DE] = "Stepper Zähler";
            TxtToolParametryNazwa[TOOLPARAM_NAME_StepperOstrzezenie, LID_DE] = "Stepper Warnung";
            TxtToolParametryNazwa[TOOLPARAM_NAME_StepperMax, LID_DE] = "Stepper Max";
            TxtToolParametryNazwa[TOOLPARAM_NAME_Sluza, LID_DE] = "Schleuse";
            TxtToolParametryNazwa[TOOLPARAM_NAME_LiczbaProgramow, LID_DE] = "Anzahl der Programme";

            //
            // ES
            //

            TxtToolParametryNazwa[TOOLPARAM_NAME_Licznik, LID_ES] = "Contador";
            TxtToolParametryNazwa[TOOLPARAM_NAME_LicznikMax, LID_ES] = "Contador Max";
            TxtToolParametryNazwa[TOOLPARAM_NAME_StepperLicznik, LID_ES] = "Stepper Contador";
            TxtToolParametryNazwa[TOOLPARAM_NAME_StepperOstrzezenie, LID_ES] = "Stepper Advertencia";
            TxtToolParametryNazwa[TOOLPARAM_NAME_StepperMax, LID_ES] = "Stepper Max";
            TxtToolParametryNazwa[TOOLPARAM_NAME_Sluza, LID_ES] = "Compuerta";
            TxtToolParametryNazwa[TOOLPARAM_NAME_LiczbaProgramow, LID_ES] = "Número de programas";

            //
            // HU
            //

            TxtToolParametryNazwa[TOOLPARAM_NAME_Licznik, LID_HU] = "Pult";
            TxtToolParametryNazwa[TOOLPARAM_NAME_LicznikMax, LID_HU] = "Pult Max";
            TxtToolParametryNazwa[TOOLPARAM_NAME_StepperLicznik, LID_HU] = "Stepper Pult";
            TxtToolParametryNazwa[TOOLPARAM_NAME_StepperOstrzezenie, LID_HU] = "Stepper Figyelem";
            TxtToolParametryNazwa[TOOLPARAM_NAME_StepperMax, LID_HU] = "Stepper Max";
            TxtToolParametryNazwa[TOOLPARAM_NAME_Sluza, LID_HU] = "Zsilip";
            TxtToolParametryNazwa[TOOLPARAM_NAME_LiczbaProgramow, LID_HU] = "Programok száma";
        }

        public void PrzeladujTeksty()
        {
            label_WelcomeTxt.Text = TxtWelcome[jezyk];

            tabProdukcja.Text = TxtTabsMain[0, jezyk];
            tabPage1.Text = TxtTabsProduction[0, jezyk];
            tabPage2.Text = TxtTabsProduction[1, jezyk];
            tabPageWizu.Text = TxtTabsProduction[2, jezyk];

            tabProces.Text = TxtTabsMain[1, jezyk];
            tabSterowanie.Text = TxtTabsProcess[0, jezyk];
            tabPomiary.Text = TxtTabsProcess[1, jezyk];
            tabPlot.Text = TxtTabsProcess[2, jezyk];

            tabZaklocenia.Text = TxtTabsMain[2, jezyk];

            tabPrzyrzady.Text = TxtTabsMain[3, jezyk];
            tabZarzadzanie.Text = TxtTabsTools[0, jezyk];
            tabUstawienia.Text = TxtTabsTools[1, jezyk];
            tabKodowanie.Text = TxtTabsTools[2, jezyk];

            tabParametry.Text = TxtTabsMain[4, jezyk];
            tabParametryGlowne.Text = TxtTabsParameters[0, jezyk];
            tabParametryCzujnikiStart.Text = TxtTabsParameters[1, jezyk];
            tabParametryCzujnikiDK.Text = TxtTabsParameters[2, jezyk];
            tabParametryCylindry.Text = TxtTabsParameters[3, jezyk];
            tabParametryKonfig.Text = TxtTabsParameters[5, jezyk];

            tabSygnalyCyfrowe.Text = TxtTabsMain[5, jezyk];

            tabSygnalyAnalogowe.Text = TxtTabsMain[6, jezyk];

            tabSystem.Text = TxtTabsMain[7, jezyk];
            tabUprawnienia.Text = TxtTabsSystem[0, jezyk];
            tabInfo.Text = TxtTabsSystem[1, jezyk];
            tabKomunikacja.Text = TxtTabsSystem[2, jezyk];
            tabAplikacja.Text = TxtTabsSystem[3, jezyk];

            button_LicznikMaxZeruj.Text = button_LicznikZeruj.Text = TxtLicznikZeruj[jezyk];
            label80.Text = TxtGora[jezyk];
            label81.Text = TxtDol[jezyk];

            labelCzujnikStart01.Text = TxtCzujnik[jezyk] + " 1";
            labelCzujnikStart02.Text = TxtCzujnik[jezyk] + " 2";
            labelCzujnikStart03.Text = TxtCzujnik[jezyk] + " 3";
            labelCzujnikStart04.Text = TxtCzujnik[jezyk] + " 4";
            labelCzujnikStart05.Text = TxtCzujnik[jezyk] + " 5";
            labelCzujnikStart06.Text = TxtCzujnik[jezyk] + " 6";
            labelCzujnikStart07.Text = TxtCzujnik[jezyk] + " 7";
            labelCzujnikStart08.Text = TxtCzujnik[jezyk] + " 8";
            labelCzujnikStart09.Text = TxtCzujnik[jezyk] + " 9";
            labelCzujnikStart10.Text = TxtCzujnik[jezyk] + " 10";
            labelCzujnikStart11.Text = TxtCzujnik[jezyk] + " 11";
            labelCzujnikStart12.Text = TxtCzujnik[jezyk] + " 12";
            labelCzujnikStart13.Text = TxtCzujnik[jezyk] + " 13";
            labelCzujnikStart14.Text = TxtCzujnik[jezyk] + " 14";

            labelCzujnikDK01.Text = TxtCzujnik[jezyk] + " 1";
            labelCzujnikDK02.Text = TxtCzujnik[jezyk] + " 2";
            labelCzujnikDK03.Text = TxtCzujnik[jezyk] + " 3";
            labelCzujnikDK04.Text = TxtCzujnik[jezyk] + " 4";
            labelCzujnikDK05.Text = TxtCzujnik[jezyk] + " 5";
            labelCzujnikDK06.Text = TxtCzujnik[jezyk] + " 6";
            labelCzujnikDK07.Text = TxtCzujnik[jezyk] + " 7";
            labelCzujnikDK08.Text = TxtCzujnik[jezyk] + " 8";
            labelCzujnikDK09.Text = TxtCzujnik[jezyk] + " 9";
            labelCzujnikDK10.Text = TxtCzujnik[jezyk] + " 10";
            labelCzujnikDK11.Text = TxtCzujnik[jezyk] + " 11";
            labelCzujnikDK12.Text = TxtCzujnik[jezyk] + " 12";
            labelCzujnikDK13.Text = TxtCzujnik[jezyk] + " 13";
            labelCzujnikDK14.Text = TxtCzujnik[jezyk] + " 14";

            label43.Text = TxtAktualnyProgram[jezyk];
            buttonPozycjaWyjsciowa.Text = TxtPozycjaWyjsciowa[jezyk];
            label99.Text = TxtKontrolaNakretek[jezyk];
            label125.Text = TxtTolerancja[jezyk];
            label61.Text = TxtPozycjaCylindra[jezyk];
            label_PozycjaGorna.Text = TxtPozycjaCylindraGora[jezyk];
            label_PozycjaDolna.Text = TxtPozycjaCylindraDol[jezyk];
            label93.Text = TxtCisnienieWydmuchu[jezyk];
            label_WydmuchWDole.Text = TxtCisnienieWydmuchuDol[jezyk];
            
            buttonPomiarRefPradu.Text = TxtButtonPomiarPradu[jezyk];
            buttonPomiarCylindra.Text = TxtButtonPomiarNakretki[jezyk];
            label49.Text = TxtPomiaryReferencyjne[jezyk];
            label48.Text = TxtDaneZOstatniegoZgrzewu[jezyk];
            checkBox_Pomiary_Srednia.Text = TxtPomiarTolerancji[jezyk];
            label47.Text = TxtNrProgramu[jezyk];
            label50.Text = TxtPromilMocyZeStepperem[jezyk];
            label51.Text = TxtWartoscSredniaPradu[jezyk];
            label46.Text = TxtWartoscSredniaNapiecia[jezyk];
            label53.Text = TxtWartoscEnergii[jezyk];
            label54.Text = TxtWartoscWtopienia[jezyk];
            label_Wykresy_Prad_Txt.Text = TxtPrad[jezyk];
            label_Wykresy_Napiecie_Txt.Text = TxtNapiecie[jezyk];
            label_Wykresy_Wtopienie_Txt.Text = TxtWtopienie[jezyk];
            label111.Text = TxtCzas[jezyk];
            label116.Text = TxtNrProgramu[jezyk];
            label118.Text = TxtMoc[jezyk];
            
            label29.Text = TxtAktywneZaklocenia[jezyk];
            buttonPotwierdzenieZaklocenia.Text = TxtButtonPotwierdzenie[jezyk];

            label21.Text = TxtListaPrzyrzadow[jezyk];
            label23.Text = TxtAktywny[jezyk];
            buttonToolActivate.Text = TxtButtonAktywacja[jezyk];
            buttonToolNew.Text = TxtButtonUtworzenieNowego[jezyk];
            buttonToolNewCopy.Text = TxtButtonUtworzenieKopii[jezyk];
            buttonDelete.Text = TxtButtonSkasowanie[jezyk];

            label82.Text = TxtAktywnyPrzyrzad[jezyk];
            label83.Text = TxtParametr[jezyk];
            label113.Text = TxtOpis[jezyk];

            label105.Text = TxtKodAktywnego[jezyk];
            label106.Text = TxtKodPodlaczonego[jezyk];
            button_SzukajKodu.Text = TxtButtonWyszukajKod[jezyk];

            WyswitlenieListy();
        }

    }
}


