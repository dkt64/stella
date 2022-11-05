/*
 * Proces.c
 *
 *  Created on: 2010-02-20
 *      Author: Bartek
 */

#include "main.h"

volatile word time_index;
volatile bool time_0 = false;
volatile bool time_1 = false;

//
// BLINK
//

bool blink_1_Hz = false;
bool blink_2_Hz = false;
bool blink_5_Hz = false;

//
// Zmienne sluzy
//

bool sluza_start = false;
word sluza_czas = 0;
bool sluza_check = false;
bool sluza_started = false;
bool bez_sluzy = false;
bool sluza_opt = false;
bool sluza_klt = false;
byte licznik_opt = 1;
word sluza_opt_czas = 0;

// parametry zgrzewania
WeldParam par[16];
WeldParam prog;

// Tablice do obliczenia referencji
word RefTabPrad[16][REF_TAB_SIZE];
word RefTabNapiecie[16][REF_TAB_SIZE];
word RefTabEnergia[16][REF_TAB_SIZE];
word RefTabCylinder[16][REF_TAB_SIZE];
word RefTabWydmuch[16][REF_TAB_SIZE];
word RefTabWtopienie[16][REF_TAB_SIZE];

volatile byte liczba_impulsow;
volatile byte pauza_impulsow;
volatile byte czas_pauzy_impulsow;
volatile byte czas_docisku_wstepnego;
volatile byte czas_docisku_koncowego;
volatile byte czas_podgrzewania;
volatile byte czas_pauzy_przed_zgrzewaniem;
volatile byte czas_zgrzewania;
volatile byte czas_pauzy_po_zgrzewaniu;
volatile byte czas_dogrzewania;
volatile word prad_podgrzewania;
volatile word prad_zgrzewania;
volatile word prad_dogrzewania;
volatile word kat;
volatile word kat_podgrzewanie;
volatile word kat_zgrzewanie;
volatile word kat_dogrzewanie;

byte ostatni_program = 0;
bool z_pradem = false;
byte z_pradem_wszystkie = 0;

word rozkaz;
word rozkaz_arg;

bool synchro_jest = false;
byte synchro_cnt = 0;

//
// Zmienna do sumowania probek z jednego pó³okresu
//
dword pomiar_prad_sredni_polokres = 0;
dword pomiar_nap_srednie_polokres = 0;
dword pomiar_wtopienia_polokres = 0;
byte pomiar_numer_polokresu = 0;

//
// Suma srednich wartosci z kazdego polokresu za caly czas zgrzewania
//
dword pomiar_polokres_prad;
dword pomiar_polokres_nap;
byte pomiar_numer_zmierzonego_polokresu = 0;

//
// Wartoœci koncowe srednia calkowitego pomiaru
//
dword pomiar_prad;
dword pomiar_nap;
dword pomiar_energia;

//
// Zmienna ustawiana w Modbusie (gdy odczytano program)
// Czeka na ni¹ proces na koñcu
// Mo¿na by zmieniæ w APTerminal - odczyt programu po FK, ale jeszcze nie mam FK
// Kasowana przy kolejnym zgrzewie
//

bool wyslano_wyniki = false;

//
// Start programu - flaga
//
bool start = true;

//
// Rozkaz kasuja blad
//
bool kasuj_blad = false;

//
// Kontrola dynamiczna czujników
// word, który ma zapisane wszytkie aktywne czujniki z ca³ego cyklu
// Wpisane sa jedynki w bity, które s¹ u¿ywane podczas cyklu
// Je¿eli gdziekolwiek w konfigu start lub dk w ka¿dym programie w cyklu
// jest jedynka to sprawdzamy czy wystpi zero, je¿eli w konfigu jest 0 to sprawdzamy jednynki
//

word kontrola_dyn_konfig_1 = 0;
word kontrola_dyn_1 = 0;
word kontrola_dyn_konfig_0 = 0;
word kontrola_dyn_0 = 0;

//
// Czas trwania impulsu na tyrystory
// Ze znakiem, gyd¿ wa¿na jest kontrola mniejszego od zero
//

char impuls = 0;

//
// Testy synchronizacji
//

word sync_short = 0xffff;
word sync_long = 0;

//
// Tablica wartosi skutecznych z calego cyklu
//

word wykres_wzor[WYKRES_TAB_SIZE];
word wykres_prad[WYKRES_TAB_SIZE];
word wykres_nap[WYKRES_TAB_SIZE];
word wykres_wtopienia[WYKRES_TAB_SIZE];
byte wykres_index;

//
// Pomiary czasów
//
word mess1, mess2;

//
// Pomiary analogowe
//
unsigned long adc0_suma, adc1_suma, adc2_suma, adc3_suma, adc4_suma, adct_suma;
unsigned long adc0_srednia, adc1_srednia, adc2_srednia, adc3_srednia,
		adc4_srednia, adct_srednia;
word adc0_index, adc1_index, adc2_index, adc3_index, adc4_index, adct_index;

// Offsety analogowe

#define adc0_offset 5
#define adc1_offset 5
#define adc2_offset 10
#define adc3_offset 10
#define adc4_offset 10
#define adct_offset 0

//
// Merker - je¿eli zgrzew OK to pamiêtamy o zwiêkdzeniu licznika zgrzewów
//

bool nast_program = false;

//
// Liczba refernecji przepisanych podczas rozkazu
byte ref_prad_cnt0 = 0;
byte ref_nakr_cnt0 = 0;

//
// Info starcie aplikacji APTerminal
//

bool aktywacja_apterminal = false;
bool aktywacja_ok = false;

//
// Kodowanie
//
word kod_podlaczony = 0, kod_aktywny = 0;
bool zmiana_kodu = false;

//
// Licznik dla wykrycia zwarcia tyrystorow
//
word licznik_zwarcie;

//
// Zmienne do obliczenia wynikow zgrzewania
//
float energia_zad = 0.0f;
float energia_akt = 0.0f;
float energia_tol = 0.0f;

float prad_zad = 0.0f;
float prad_akt = 0.0f;
float prad_tol = 0.0f;

float nap_zad = 0.0f;
float nap_akt = 0.0f;
float nap_tol = 0.0f;

//
// Ustalenie liczby programow z pradem (przy sterowaniu mamy mniejsza
// ilosc zgrzewow niz programow
//

byte liczba_zgrzewow;

// Wtopienie aktualne
dword wtop, wtop_temp;

//*****************************************************************************
// Inicjacja przerwañ procesowych
//*****************************************************************************
void ProcesInit(void)
{
	//
	// Inicjacja Timera0
	// Podstawowy Timer do obs³ugi procesu
	// 2500 - iloœæ cykli na przerwanie 50.000.000 Hz / 2500 = 20 kHz = 200 przerwañ na jeden pó³okres
	//
	SysCtlPeripheralEnable(SYSCTL_PERIPH_TIMER0);
	TimerConfigure(TIMER0_BASE, TIMER_CFG_32_BIT_PER);
	TimerLoadSet(TIMER0_BASE, TIMER_A, PROCES_INT);
	IntEnable(INT_TIMER0A);
	TimerIntEnable(TIMER0_BASE, TIMER_TIMA_TIMEOUT);
	TimerEnable(TIMER0_BASE, TIMER_A);

	//
	// Inicjacja Timera1
	// Timer do obliczania czasów
	//
	SysCtlPeripheralEnable(SYSCTL_PERIPH_TIMER1);
	TimerConfigure(TIMER1_BASE, TIMER_CFG_16_BIT_PAIR | TIMER_CFG_A_PERIODIC);
	TimerPrescaleSet(TIMER1_BASE, TIMER_A, 50);
	TimerLoadSet(TIMER1_BASE, TIMER_A, 0xffff);
	TimerEnable(TIMER1_BASE, TIMER_A);

	//
	// Inicjacja przaerwania od wejœcia synchronizuj¹ego
	//
	GPIOPinIntEnable(GPIO_PORTD_BASE, GPIO_PIN_0);
	GPIOIntTypeSet(GPIO_PORTD_BASE, GPIO_PIN_0, GPIO_FALLING_EDGE);
	IntEnable(INT_GPIOD);
}

//*****************************************************************************
// Przerwanie od wejœcia synchronizuj¹cego
//*****************************************************************************
void SynchroInt(void)
{
	//
	// FK na testy
	//

	//P_FK(true);

	GPIOPinIntClear(GPIO_PORTD_BASE, GPIO_PIN_0);

	image.sm[SM_LICZNIK_PRZERWAN_SYNC]++;
	image.sm[SM_INDEX] = time_index;
	time_index = 0;
	time_0 = false;
	time_1 = false;

	synchro_jest = true;

	//
	// Wy³¹czamy zap³on tyrystorów - dadatkowo dla bezpieczenstwa
	//
	P_Impuls_Hard(false);
	P_Impuls(false);
	impuls = 0;

	//
	// Testy sycnhro
	//

	if (image.sm[SM_INDEX] > sync_long)
		sync_long = image.sm[SM_INDEX];

	if (image.sm[SM_INDEX] < sync_short)
		sync_short = image.sm[SM_INDEX];

	//image.sm[SM_TEST1] = sync_short;
	//image.sm[SM_TEST2] = sync_long;
}

//*****************************************************************************
// Przerwanie zegara 20kHz
//*****************************************************************************
void ProcesInt(void)
{
	//
	// Potwierdzenie przerwania
	//
	TimerIntClear(TIMER0_BASE, TIMER_TIMA_TIMEOUT);

	//mess1 = us;

	//
	// Wy³¹czamy zap³on tyrystorów
	//

	//P_Impuls(false);

	//
	// Zmienne procesowe
	//
	tick++;
	time_index++;

	//
	// Obs³uga wejœæ wyjœæ i obrazu IO
	//
	ProcesIO();

	if (GET_IOTEST == false)
	{
		//
		// Badanie zbocza restu bledu
		//
		P_ResetBledu();

		//
		// Sprawdzenie gotowœci maszyny i ustawienie bitów zak³óceñ
		//
		Zaklocenia();
		Gotowosc();

		//
		// Pomiary analogów
		//
		Pomiar3();

		//
		// Blink
		//
		Blink();
	}

	//
	// Tutaj raz na okres obs³uga procesu i pomiary analogowe
	//
	if (time_index >= INDEX_0 && time_0 == false)
	{
		time_0 = true;
		//mess2 = us;

		kod_podlaczony = (((image.i[3] & 0xc0) >> 6) + (image.i[4] << 2));

		if (GET_IOTEST == false)
		{
			// Pomiar dotyczy zawsze poprzedniego okresu i musi byæ na pocz¹tku, gdy¿ tutaj wyliczane s¹
			// wartoœci do obrazu AIN
			Pomiar_Obliczenia();

			SterowaniePrzyrzadem();
			Sluza();
			KontrolaNakretki();
			Cisnienie();
			CylinderUGory();
			Wyblokowanie();
			KontrolaDynamiczna_ZbieranieSygnalow();
			Proces();
			// Wa¿na kolejnoœæ - zeby nie przechodzil do nowego programu
			Cykl();
		}
		else
		{
			image.ain[0] = (adc0_suma / adc0_index) - adc0_offset;
			image.ain[1] = (adc1_suma / adc1_index) - adc1_offset;
			image.ain[2] = (adc2_suma / adc2_index) - adc2_offset;
			image.ain[3] = (adc3_suma / adc3_index) - adc3_offset;
			image.ain[4] = (adc4_suma / adc4_index) - adc4_offset;
			image.ain[5] = (adct_suma / adct_index) - adct_offset;
		}

		//
		// Pomiar czasu wykonywania procesu
		//
		//mess_res = mess - us;
		//if (mess_res > image.sm[SM_TIMER1])
		//image.sm[SM_TIMER1] = mess_res;

		//
		// Fk na testy
		//
		//P_FK(false);
		//p_test2 = mess2 - us;
	}

	if (p_stanprocesu == ETPROC_ZGRZEWANIE_PODGRZEWANIE)
		kat = kat_podgrzewanie;
	else if (p_stanprocesu == ETPROC_ZGRZEWANIE_ZGRZEWANIE)
		kat = kat_zgrzewanie;
	else if (p_stanprocesu == ETPROC_ZGRZEWANIE_DOGRZEWANIE)
		kat = kat_dogrzewanie;
	else
		kat = 0xffff;

	//
	// Zap³on tyrystorów
	//
	if (time_1 == false && time_index >= kat && z_pradem && p_gotowa
			&& (p_stanprocesu == ETPROC_ZGRZEWANIE_DOGRZEWANIE || p_stanprocesu
					== ETPROC_ZGRZEWANIE_PODGRZEWANIE || p_stanprocesu
					== ETPROC_ZGRZEWANIE_ZGRZEWANIE))
	{
		P_Impuls(true);
	}

	// Wylaczenie tyrystorow
	if (time_index >= INDEX_1000 + IMPULS_TIME)
	{
		P_Impuls(false);
	}

	//
	// Obs³uga RSow
	//
	Uart0_GetBytes();
	Uart1_GetBytes();
	Uart2_GetBytes();

	// Obsluga rozkazów z Modbusa
	//
	Rozkaz();

	//p_test1 = mess1 - us;

	//
	// Semafor dla pêtli g³ównej
	//
	semaphore = false;
}

//*****************************************************************************
// Reset procesu - wszystkie cylindry i inne wyjscia na 0
//*****************************************************************************
void ProcesReset(void)
{
	wyslano_wyniki = false;
	P_FK(false);
	P_VZ(false);

	P_CylindryStart(false);
}

//*****************************************************************************
// Proces zgrzewania
//*****************************************************************************
void Proces(void)
{
	// ========================================================================
	switch (p_stanprocesu)
	{

	case ETPROC_CZEKAM_NA_BRAK_STARTU:
		// ====================================================================

		ProcesReset();

		if (P_Start() == false)
			p_stanprocesu = ETPROC_CZEKAM_NA_POZWOLENIE_STARTU;

		break;

		// ====================================================================
	case ETPROC_CZEKAM_NA_POZWOLENIE_STARTU:
		// ====================================================================

		ProcesReset();

		if (p_gotowa && (err1 == 0) && (P_ZezwolenieProgramowania() == false))// ???????????? && (P_DK() == false))
			p_stanprocesu = ETPROC_CZEKAM_NA_CYLINDER_W_GORZE;

		break;

		// ====================================================================
	case ETPROC_CZEKAM_NA_CYLINDER_W_GORZE:
		// ====================================================================

		ProcesReset();

		if (GET_CYL_U_GORY)
		{
			UstalLiczbeZgrzewow();

			if (nast_program == true)
			{
				nast_program = false;
				p_aktualny_program++;
			}

			p_stanprocesu = ETPROC_CZEKAM_NA_START;
		}

		break;

		// ====================================================================
	case ETPROC_CZEKAM_NA_START:
		// ====================================================================

		ProcesReset();

		if (p_gotowa && P_Start() && err1 == 0)// ?????????????? && (P_DK() == false))
		{
			ProgramKopiuj();
			ObliczParametry();

			p_stanprocesu = ETPROC_CZEKAM_NA_CZUJNIKI_START;

			//
			// Brak zezwolenia ze Sluzy
			//
			if ((p_sluza > 0) && (sluza_check == true || sluza_started == true)
					&& (P_SluzaSygnal() == false))
				ZaklocenieSet(ERR_SLUZA);
			if ((p_sluza > 0) && (sluza_check == true) && (P_SluzaSygnal()
					== true))
				ZaklocenieReset(ERR_SLUZA);

			//
			// B³¹d sluzy optycznej - nie prze³o¿ono detalu
			//
			if ((sluza_opt) && (sluza_check == true) && (licznik_opt == 0))
				ZaklocenieSet(ERR_SLUZA_OPT_ZERO);

			if (KontrolaDynamiczna_Sprawdzenie() == true)
			{
				ZaklocenieReset(ERR_KONTROLA_CZUJNIKOW);
			}
		}

		if (p_gotowa == false)
			p_stanprocesu = ETPROC_CZEKAM_NA_BRAK_STARTU;

		if (P_ZezwolenieProgramowania() == true)
			p_stanprocesu = ETPROC_CZEKAM_NA_POZWOLENIE_STARTU;

		if (!GET_CYL_U_GORY)
		{
			p_stanprocesu = ETPROC_CZEKAM_NA_CYLINDER_W_GORZE;
		}

		break;

		// ====================================================================
	case ETPROC_CZEKAM_NA_CZUJNIKI_START:
		// ====================================================================

		if (p_gotowa && P_Start() && P_CzujnikiZezwoleniePrzyStarcie())
		{
			if (prog.NumerCylindra > 0)
			{
				p_stanprocesu = ETPROC_CZEKAM_NA_WYBLOKOWANIE;
			}
			else
			{
				nast_program = true;

				if ((prog.Konfig & KONFIG_BEZ_STARTU) > 0)
					p_stanprocesu = ETPROC_CZEKAM_NA_POZWOLENIE_STARTU;
				else
					p_stanprocesu = ETPROC_CZEKAM_NA_BRAK_STARTU;
			}
		}
		else if (P_Start() == false)
			p_stanprocesu = ETPROC_CZEKAM_NA_START;
		else if (p_gotowa == false)
			p_stanprocesu = ETPROC_CZEKAM_NA_BRAK_STARTU;

		break;

		// ====================================================================
	case ETPROC_CZEKAM_NA_DK:
		// ====================================================================

		if (p_gotowa && P_Start())
		{
			P_CylindryStart(true);

			if (GET_CISNIENIE_OSIAGNIETE)
			{
				p_stanprocesu = ETPROC_ZGRZEWANIE_DOCISK_WSTEPNY;
			}
		}
		else
		{
			P_CylindryStart(false);
			p_stanprocesu = ETPROC_CZEKAM_NA_BRAK_STARTU;
		}

		break;

		// ====================================================================
	case ETPROC_ZGRZEWANIE_DOCISK_WSTEPNY:
		// ====================================================================

		if (p_gotowa && P_Start())
		{
			P_CylindryStart(true);

			if (GET_CISNIENIE_OSIAGNIETE)
			{
				if (czas_docisku_wstepnego > 0)
					czas_docisku_wstepnego--;

				if (czas_docisku_wstepnego == 0)
				{
					p_stanprocesu = ETPROC_CZEKAM_NA_CZUJNIKI_DK;
				}
			}
		}
		else
		{
			P_CylindryStart(false);
			p_stanprocesu = ETPROC_CZEKAM_NA_BRAK_STARTU;
		}

		break;

		// ====================================================================
	case ETPROC_CZEKAM_NA_CZUJNIKI_DK:
		// ====================================================================

		if (p_gotowa && P_Start())
		{
			P_CylindryStart(true);

			if (GET_CISNIENIE_OSIAGNIETE)
			{
				if (P_CzujnikiZezwoleniePrzyDK() == true)
				{
					p_stanprocesu = ETPROC_CZEKAM_NA_KONTROLA_NAKRETKI;
				}
			}
		}
		else
		{
			P_CylindryStart(false);
			p_stanprocesu = ETPROC_CZEKAM_NA_BRAK_STARTU;
		}

		break;

		// ====================================================================
	case ETPROC_CZEKAM_NA_KONTROLA_NAKRETKI:
		// ====================================================================

		if (p_gotowa && P_Start())
		{
			P_CylindryStart(true);

			if (GET_CISNIENIE_OSIAGNIETE && GET_REFERUJ_CYLINDER)
			{
				ReferujCylinder();

				sluza_start = false;
				sluza_check = false;
			}

			if (GET_CISNIENIE_OSIAGNIETE && GET_NAKR_POZ && GET_NAKR_WYD)
			{
				// reset wartoœci wtopienia
				p_wtopienie_poczatkowe = p_cylinder_akt;
				p_ostzgrzew_pom_wtop = 0;
				wtop = 0;

				if (czas_podgrzewania > 0 && prad_podgrzewania > 0)
					p_stanprocesu = ETPROC_ZGRZEWANIE_PODGRZEWANIE;
				else if (czas_pauzy_przed_zgrzewaniem > 0)
					p_stanprocesu = ETPROC_ZGRZEWANIE_PODGRZEWANIE_PAUZA;
				else if (czas_zgrzewania > 0 && prad_zgrzewania > 0)
					p_stanprocesu = ETPROC_ZGRZEWANIE_ZGRZEWANIE;
				else if (czas_pauzy_po_zgrzewaniu > 0)
					p_stanprocesu = ETPROC_ZGRZEWANIE_DOGRZEWANIE_PAUZA;
				else if (czas_dogrzewania > 0 && prad_dogrzewania > 0)
					p_stanprocesu = ETPROC_ZGRZEWANIE_DOGRZEWANIE;
				else
					p_stanprocesu = ETPROC_ZGRZEWANIE_KONIEC;
			}
		}
		else
		{
			P_CylindryStart(false);
			p_stanprocesu = ETPROC_CZEKAM_NA_BRAK_STARTU;
		}

		break;

		// ====================================================================
	case ETPROC_CZEKAM_NA_WYBLOKOWANIE:
		// ====================================================================

		if ((prog.Konfig & KONFIG_WYBLOKOWANIE_GORA) > 0)
		{
			if (P_Start() && P_WyblokowanieZezwolenie())
				p_stanprocesu = ETPROC_CZEKAM_NA_DK;

			if (P_Start() == false)
				p_stanprocesu = ETPROC_CZEKAM_NA_BRAK_STARTU;
		}
		else
		{
			if (p_gotowa && P_Start())
			{
				P_CylindryStart(true);

				if (P_WyblokowanieZezwolenie())
				{
					p_stanprocesu = ETPROC_CZEKAM_NA_DK;
				}
			}
			else
			{
				P_CylindryStart(false);
				p_stanprocesu = ETPROC_CZEKAM_NA_BRAK_STARTU;
			}

		}

		break;

		// ====================================================================
	case ETPROC_ZGRZEWANIE_PODGRZEWANIE:
		// ====================================================================

		if (czas_podgrzewania > 0)
		{
			czas_podgrzewania--;
		}

		if (czas_podgrzewania == 0)
		{
			if (czas_pauzy_przed_zgrzewaniem > 0)
			{
				p_stanprocesu = ETPROC_ZGRZEWANIE_PODGRZEWANIE_PAUZA;
			}
			else if (czas_zgrzewania > 0 && prad_zgrzewania > 0)
				p_stanprocesu = ETPROC_ZGRZEWANIE_ZGRZEWANIE;
			else if (czas_pauzy_po_zgrzewaniu > 0)
			{
				p_stanprocesu = ETPROC_ZGRZEWANIE_DOGRZEWANIE_PAUZA;
			}
			else if (czas_dogrzewania > 0 && prad_dogrzewania > 0)
				p_stanprocesu = ETPROC_ZGRZEWANIE_DOGRZEWANIE;
			else
				p_stanprocesu = ETPROC_ZGRZEWANIE_KONIEC;
		}

		if (p_gotowa == false)
		{
			ZaklocenieSet(ERR_PROCES_PRZERWANY);
			p_stanprocesu = ETPROC_ZGRZEWANIE_KONIEC;
		}

		break;

		// ====================================================================
	case ETPROC_ZGRZEWANIE_PODGRZEWANIE_PAUZA:
		// ====================================================================

		if (czas_pauzy_przed_zgrzewaniem > 0)
		{
			czas_pauzy_przed_zgrzewaniem--;
		}

		if (czas_pauzy_przed_zgrzewaniem == 0)
		{
			if (czas_zgrzewania > 0 && prad_zgrzewania > 0)
				p_stanprocesu = ETPROC_ZGRZEWANIE_ZGRZEWANIE;
			else if (czas_pauzy_po_zgrzewaniu > 0)
				p_stanprocesu = ETPROC_ZGRZEWANIE_DOGRZEWANIE_PAUZA;
			else if (czas_dogrzewania > 0 && prad_dogrzewania > 0)
				p_stanprocesu = ETPROC_ZGRZEWANIE_DOGRZEWANIE;
			else
				p_stanprocesu = ETPROC_ZGRZEWANIE_KONIEC;
		}

		if (p_gotowa == false)
		{
			ZaklocenieSet(ERR_PROCES_PRZERWANY);
			p_stanprocesu = ETPROC_ZGRZEWANIE_KONIEC;
		}

		break;

		// ====================================================================
	case ETPROC_ZGRZEWANIE_ZGRZEWANIE:
		// ====================================================================

		if (czas_zgrzewania > 0)
		{
			czas_zgrzewania--;
		}

		if (czas_zgrzewania == 0)
		{
			//
			// Sprawdzenie czy wiêcej impulsów
			// Jezeli tak to etap Pauza
			//
			if (liczba_impulsow > 1 && pauza_impulsow > 0)
			{
				p_stanprocesu = ETPROC_ZGRZEWANIE_ZGRZEWANIE_PAUZA;
				czas_pauzy_impulsow = pauza_impulsow;
			}
			else
			{
				//
				// Nastepny etap po zgrzaniu wszystkich impulsów
				//
				if (czas_pauzy_po_zgrzewaniu > 0)
				{
					p_stanprocesu = ETPROC_ZGRZEWANIE_DOGRZEWANIE_PAUZA;
				}
				else if (czas_dogrzewania > 0 && prad_dogrzewania > 0)
					p_stanprocesu = ETPROC_ZGRZEWANIE_DOGRZEWANIE;
				else if (czas_docisku_koncowego > 0)
					p_stanprocesu = ETPROC_ZGRZEWANIE_DOCISK_KONCOWY;
				else
					p_stanprocesu = ETPROC_ZGRZEWANIE_KONIEC;
			}
		}

		if (p_gotowa == false)
		{
			ZaklocenieSet(ERR_PROCES_PRZERWANY);
			p_stanprocesu = ETPROC_ZGRZEWANIE_KONIEC;
		}

		break;

		// ====================================================================
	case ETPROC_ZGRZEWANIE_ZGRZEWANIE_PAUZA:
		// ====================================================================

		if (czas_pauzy_impulsow > 0)
		{
			czas_pauzy_impulsow--;
		}

		if (czas_pauzy_impulsow == 0)
		{
			if (liczba_impulsow > 0)
				liczba_impulsow--;

			czas_zgrzewania = prog.CzasZgrzewania * 2;

			p_stanprocesu = ETPROC_ZGRZEWANIE_ZGRZEWANIE;
		}

		if (p_gotowa == false)
		{
			ZaklocenieSet(ERR_PROCES_PRZERWANY);
			p_stanprocesu = ETPROC_ZGRZEWANIE_KONIEC;
		}

		break;

		// ====================================================================
	case ETPROC_ZGRZEWANIE_DOGRZEWANIE_PAUZA:
		// ====================================================================

		if (czas_pauzy_po_zgrzewaniu > 0)
		{
			czas_pauzy_po_zgrzewaniu--;
		}

		if (czas_pauzy_po_zgrzewaniu == 0)
		{
			if (czas_dogrzewania > 0 && prad_dogrzewania > 0)
				p_stanprocesu = ETPROC_ZGRZEWANIE_DOGRZEWANIE;
			else if (czas_docisku_koncowego > 0)
				p_stanprocesu = ETPROC_ZGRZEWANIE_DOCISK_KONCOWY;
			else
				p_stanprocesu = ETPROC_ZGRZEWANIE_KONIEC;
		}

		if (p_gotowa == false)
		{
			ZaklocenieSet(ERR_PROCES_PRZERWANY);
			p_stanprocesu = ETPROC_ZGRZEWANIE_KONIEC;
		}

		break;

		// ====================================================================
	case ETPROC_ZGRZEWANIE_DOGRZEWANIE:
		// ====================================================================

		if (czas_dogrzewania > 0)
		{
			czas_dogrzewania--;
		}

		if (czas_dogrzewania == 0)
		{
			if (czas_docisku_koncowego > 0)
				p_stanprocesu = ETPROC_ZGRZEWANIE_DOCISK_KONCOWY;
			else
				p_stanprocesu = ETPROC_ZGRZEWANIE_KONIEC;
		}

		if (p_gotowa == false)
		{
			ZaklocenieSet(ERR_PROCES_PRZERWANY);
			p_stanprocesu = ETPROC_ZGRZEWANIE_KONIEC;
		}

		break;

		// ====================================================================
	case ETPROC_ZGRZEWANIE_DOCISK_KONCOWY:
		// ====================================================================

		if (czas_docisku_koncowego > 0)
		{
			czas_docisku_koncowego--;
		}

		if (czas_docisku_koncowego == 0)
		{
			p_stanprocesu = ETPROC_ZGRZEWANIE_KONIEC;
		}

		if (p_gotowa == false)
		{
			ZaklocenieSet(ERR_PROCES_PRZERWANY);
			p_stanprocesu = ETPROC_ZGRZEWANIE_KONIEC;
		}

		break;

		// ====================================================================
	case ETPROC_ZGRZEWANIE_KONIEC:
		// ====================================================================

		if (z_pradem)
			p_licznik_steppera_akt++;

		if (WynikiZgrzewania() == true || z_pradem == false)
			p_stanprocesu = ETPROC_ZGRZEW_OK;
		else
			p_stanprocesu = ETPROC_ZGRZEW_NOK;

		P_CylindryStart(false);
		P_FK(true);

		break;

		// ====================================================================
	case ETPROC_ZGRZEW_OK:
		// ====================================================================

		P_CylindryStart(false);

		nast_program = true;

		if (z_pradem == true)
			z_pradem_wszystkie++;

		p_stanprocesu = ETPROC_KONIEC_PROCESU;

		break;

		// ====================================================================
	case ETPROC_ZGRZEW_NOK:
		// ====================================================================

		P_CylindryStart(false);

		nast_program = true;

		p_stanprocesu = ETPROC_KONIEC_PROCESU;

		break;

		// ====================================================================
	case ETPROC_KONIEC_PROCESU:
		// ====================================================================

		P_CylindryStart(false);

		if (wyslano_wyniki == true)
		{
			if ((prog.Konfig & KONFIG_BEZ_STARTU) > 0)
				p_stanprocesu = ETPROC_CZEKAM_NA_POZWOLENIE_STARTU;
			else
				p_stanprocesu = ETPROC_CZEKAM_NA_BRAK_STARTU;
		}

		break;

	}

}

//*****************************************************************************
// Odczyt obrazu IO i zapis obrazu wyjsc na hardware
//*****************************************************************************
void ProcesIO(void)
{
	if (GET_IOTEST)
	{
		image.q[0] = image.i[0] | image.i[2];
		image.q[1] = image.i[1] | image.i[3];
	}
	/*
	 else
	 {
	 image.q[0] = GetHard_Q0();
	 image.q[1] = GetHard_Q1();
	 }
	 */

	SetHard_Q0(image.q[0]);
	SetHard_Q1(image.q[1]);

	image.i[0] = GetHard_I0();
	image.i[1] = GetHard_I1();
}

//*****************************************************************************
// Obsluga rozkazów bezpoœrednich wys³anych przez Modbusa
//*****************************************************************************
void RozkazBezposredni(byte rozkaz)
{
	switch (rozkaz)
	{
	//
	// Pozycja wyjœciowa
	//
	case 1:
		ProcesReset();
		p_aktualny_program = 0;
		p_stanprocesu = ETPROC_CZEKAM_NA_BRAK_STARTU;
		z_pradem_wszystkie = 0;
		sluza_start = false;
		if (p_sluza > 0)
			sluza_check = true;
		else
			sluza_check = false;

	case 2:
		break;

	case 3:
		break;

		//
		// Start programu - pierwsza aktywacja
		//
	case 4:

		aktywacja_apterminal = true;
		kod_aktywny = kod_podlaczony;
		if (p_sluza > 0)
			sluza_check = true;
		else
			sluza_check = false;

		//p_cylinder_gora = p_cylinder_akt;
		//referuj_nakretki = true;
		//referuj_prad = true;

		break;

		//
		// Kasowanie b³êdu z aplikacji
		//
	case 5:
		kasuj_blad = true;
		break;

		//
		// Kasowanie aktywnego przyrz¹du
		//
	case 6:
		aktywacja_programyzaladowane = false;
		aktywacja_apterminal = false;
		break;

		//
		// W³¹cz/wy³¹cz IOTest
		//
	case 7:

		if (GET_IOTEST)
			RESET_IOTEST;
		else
			SET_IOTEST;
		break;

		//
		// W³¹cz/wy³¹cz POMIAR TOLERANCJI PRZY REFEROWANIU
		//
	case 8:

		if (GET_POMIAR_TOLERANCJI)
			RESET_POMIAR_TOLERANCJI;
		else
			SET_POMIAR_TOLERANCJI;
		break;

	}
}

//*****************************************************************************
// Obsluga rozkazów bezpoœrednich wys³anych przez Modbusa
//*****************************************************************************
void RozkazAkcja(byte adres, byte wartosc)
{
	switch (adres)
	{

	//
	// Referencja pr¹du
	//
	case 0x0d:

		if (GET_REFERUJ_PRAD)
		{
			RESET_REFERUJ_PRAD;
			//ResetReferencjiPradu(wartosc);
		}
		else
		{
			SET_REFERUJ_PRAD;
			ResetReferencjiPradu(wartosc);
		}
		break;

		//
		// Referencja nakrêtek
		//
	case 0x0e:
		if (GET_REFERUJ_CYLINDER)
		{
			RESET_REFERUJ_CYLINDER;
			//ResetReferencjiCylindra(wartosc);
		}
		else
		{
			SET_REFERUJ_CYLINDER;
			ResetReferencjiCylindra(wartosc);
		}

		break;
	}
}

//*****************************************************************************
// Obsluga rozkazów przez Modbus'a
//*****************************************************************************
void Rozkaz(void)
{
	byte code, address;

	code = (byte) ((rozkaz & 0xff00) >> 8);
	address = (byte) (rozkaz & 0xff);

	if (rozkaz)
	{
		switch (code)
		{
		//
		// rozkazy bezpoœrednie
		//
		case 0:
			RozkazBezposredni(address);
			break;

			//
			// dodawanie
			//
		case 1:
			if (image.sm[address] + rozkaz_arg <= 65535)
				image.sm[address] += rozkaz_arg;
			break;

			//
			// odejmowanie
			//
		case 2:
			if (image.sm[address] >= rozkaz_arg)
				image.sm[address] -= rozkaz_arg;
			else
				image.sm[address] = 0;
			break;

			//
			// wpisywanie
			//
		case 3:
			image.sm[address] = rozkaz_arg;
			RozkazAkcja(address, rozkaz_arg);
			break;

		default:
			break;
		}

		rozkaz = 0;
		image.sm[0] = 0;
		image.sm[1] = 0;
	}
}

//*****************************************************************************
// Zak³ócenia - ustawienie odpowiedniego bitu w wordzie obrazu
// Parametr - nr zaklocenia
//*****************************************************************************
void ZaklocenieSet(byte nr)
{
	if (nr < 16)
		err0 |= 1 << nr;
	if (nr < 32 && nr >= 16)
		err1 |= 1 << (nr - 16);
}

void ZaklocenieReset(byte nr)
{
	if (nr < 16)
		err0 &= ~(1 << nr);
	if (nr < 32 && nr >= 16)
		err1 &= ~(1 << (nr - 16));
}

void Zaklocenia(void)
{
	//
	// Brak synchronizacji z sieci¹ zasilaj¹c¹
	//
	if (synchro_jest == false)
		ZaklocenieSet(ERR_SYNCHRO);
	if (synchro_jest == true && reset_bledu)
		ZaklocenieReset(ERR_SYNCHRO);

	//
	// Zwarcie na tyrystorze
	//
	if ((image.ain[1] > (GRANICA_ZWARCIE_TYRYSTOROW)) && p_stanprocesu
			< ETPROC_ZGRZEWANIE_PODGRZEWANIE)
		licznik_zwarcie++;
	else
		licznik_zwarcie = 0;

	if ((licznik_zwarcie > LICZNIK_ZWARCIE) && (p_stanprocesu
			< ETPROC_ZGRZEWANIE_PODGRZEWANIE))
		ZaklocenieSet(ERR_ZWARCIE_TYR);

	//
	// Przerwa w obwodzie bezpieczeñstwa
	//
	if (P_NotAus() == false)
	{
		ZaklocenieSet(ERR_NOTAUS);
		P_CylindryStart(false);
		P_Impuls_Hard(false);
		P_Impuls(false);
	}
	if ((P_NotAus() == true) && reset_bledu)
		ZaklocenieReset(ERR_NOTAUS);

	//
	// Brak przep³ywu wody ch³odz¹cej
	//
	if (P_Woda() == false && (p_stanprocesu < ETPROC_ZGRZEWANIE_PODGRZEWANIE))
		ZaklocenieSet(ERR_WODA);
	if (P_Woda() == true)// && reset_bledu)
		ZaklocenieReset(ERR_WODA);

	//
	// Przekroczona temperatura transformatora zgrzewalniczego
	//
	if (P_TempTrafo() == false)
		ZaklocenieSet(ERR_TEMP_TRAFO);
	if (P_TempTrafo() == true && reset_bledu)
		ZaklocenieReset(ERR_TEMP_TRAFO);

	//
	// Przekroczona temperatura tyrystorów
	//
	if (P_TempTyr() == false)
		ZaklocenieSet(ERR_TEMP_TYR);
	if (P_TempTyr() == true && reset_bledu)
		ZaklocenieReset(ERR_TEMP_TYR);

	//
	// Brak aktywacja przyrz¹du
	//

	if (aktywacja_ok == false || zmiana_kodu)
		ZaklocenieSet(ERR_BRAK_AKTYWACJI);
	else
		ZaklocenieReset(ERR_BRAK_AKTYWACJI);

	//
	// Brak zezwolenia z Hydry
	//
	if (P_Hydra() == true && (p_stanprocesu < ETPROC_ZGRZEWANIE_PODGRZEWANIE))
		ZaklocenieSet(ERR_HYDRA);
	if (P_Hydra() == false && (reset_bledu || (P_Start() == true)))
		ZaklocenieReset(ERR_HYDRA);

	//
	// Kasowanie pozostalych bledow
	//
	if (reset_bledu)
	{
		ZaklocenieReset(ERR_PROCES_PRZERWANY);
		ZaklocenieReset(ERR_PRAD_DUZO);
		ZaklocenieReset(ERR_PRAD_MALO);
		ZaklocenieReset(ERR_NAP_DUZO);
		ZaklocenieReset(ERR_NAP_MALO);
		ZaklocenieReset(ERR_ENERGIA_DUZO);
		ZaklocenieReset(ERR_ENERGIA_MALO);
		ZaklocenieReset(ERR_SLUZA);
		ZaklocenieReset(ERR_SLUZA_OPT_DWA_RAZY);
		ZaklocenieReset(ERR_SLUZA_OPT_ZERO);
		ZaklocenieReset(ERR_KONTROLA_CZUJNIKOW);
		ZaklocenieReset(ERR_BRAK_PRZEPYWU_PRADU);
		ZaklocenieReset(ERR_BRAK_NAP_WTORNEGO);
		ZaklocenieReset(ERR_PRZEKROCZONA_GRANICA_REGULACJI);
		ZaklocenieReset(ERR_BRAK_WTOPIENIA);
		ZaklocenieReset(ERR_ZWARCIE_TYR);
		licznik_opt = 1;
	}

	if (err0 > 0 || err1 > 0)
		P_Blad(blink_1_Hz);
	else
		P_Blad(false);

	kasuj_blad = false;
}

//*****************************************************************************
// Warunki gotowosci maszyny
//*****************************************************************************
void Gotowosc(void)
{
	//
	// Sprawdzenie kodowania
	// Sprawdzenie czy siê nie zmienilo
	// Tylko przy prze³¹czonym kluczyku
	//
	//if (P_ZezwolenieProgramowania() && (kod_podlaczony != kod_aktywny))
	if (kod_podlaczony != kod_aktywny)
		zmiana_kodu = true;
	else if (kod_podlaczony == kod_aktywny)
		zmiana_kodu = false;

	//
	// Aktywacja OK, jezeli jest rozkaz z APTerminal i otrzymano ostatni program
	//
	if (aktywacja_programyzaladowane == true && aktywacja_apterminal == true
			&& zmiana_kodu == false)
		aktywacja_ok = true;
	else
		aktywacja_ok = false;

	//
	// Liczba programów musi wiêksza od zera i mniejsza od 16
	//
	if (p_liczba_programow == 0 || p_liczba_programow > 16)
		p_liczba_programow = 1;

	//
	// Sprawdzenie czy jest synchro
	// Jak nie ma to trzeba zasymulowac (time_x) ale blad bedzie nadal
	//
	if (time_index > 220)
	{
		image.sm[SM_LICZNIK_PRZERWAN_SYNC]++;
		image.sm[SM_INDEX] = time_index;

		synchro_jest = false;
		time_index = 0;
		time_0 = false;
		time_1 = false;
		P_Impuls_Hard(false);
		P_Impuls(false);
		impuls = 0;
	}

	//
	// Wyjscie Hardware OK - po nawiazaniu lacznosci mozna zalaczyc wyjscia
	//
	// Zmiana - brak ³¹czonoœæi nie wy³¹cza wyjœæ !!! Zmiana bezpieczeñstwa 2014-04-01
	//
	//if (connection == true)
	//	image.q[2] |= 0x80;
	//else
	//	image.q[2] &= ~(0x80);

	//
	// Warunek na ustwienie wyjœcia gotowœci maszyny
	//
	if (err0 == 0 && ((err1 & 0x0fff) == 0) && synchro_jest
			&& aktywacja_programyzaladowane && ((p_licznik_steppera_akt
			< p_licznik_steppera_max) || (p_licznik_steppera_max == 0))
			&& (p_licznik < p_licznik_max || p_licznik_max == 0))
	{
		P_Gotowosc(true);
		P_Gotowosc_Hard(true);
	}
	else
	{
		P_Gotowosc(false);
		P_Gotowosc_Hard(false);
	}

	//
	// Obsluga licznika stepper (wymiana elektrod)
	//
	if (P_KasujLicznikStepper() == true)
		p_licznik_steppera_akt = 0;

	if (p_licznik_steppera_ost > 0 && p_licznik_steppera_akt
			>= p_licznik_steppera_ost)
		P_LicznikStepperaOstrzezenie(true);
	else
		P_LicznikStepperaOstrzezenie(false);

	if (p_licznik_steppera_max > 0 && p_licznik_steppera_akt
			>= p_licznik_steppera_max)
		P_LicznikStepperaMax(true);
	else
		P_LicznikStepperaMax(false);

	//
	// Obsluga licznika u¿ytkownika
	//
	if (P_KasujLicznik() == true)
		p_licznik = 0;

	if (p_licznik_max > 0 && p_licznik >= p_licznik_max)
		P_LicznikOsiagniety(true);
	else
		P_LicznikOsiagniety(false);

}

//*****************************************************************************
// Obs³uga wyblokowania
//*****************************************************************************
void Wyblokowanie(void)
{
	//
	// Sygna³ VZ
	//
	if (P_ZPradem() && p_stanprocesu > ETPROC_ZGRZEWANIE_DOCISK_WSTEPNY
			&& p_stanprocesu < ETPROC_ZGRZEWANIE_KONIEC)
		P_VZ(true);
	else
		P_VZ(false);

	//
	// Sygna³ ¿¹dania do wyblokowania
	//
	if ((par[p_aktualny_program].Konfig & KONFIG_WYBLOKOWANIE_GORA) > 0)
	{
		if (P_Start() || P_Get_VZ())
			P_ZadanieWyblokowania(true);
		else
			P_ZadanieWyblokowania(false);
	}
	else
	{
		if ((P_CylindryIda() && P_Start()) || P_Get_VZ())
			P_ZadanieWyblokowania(true);
		else
			P_ZadanieWyblokowania(false);
	}
}

//*****************************************************************************
// Kopiowanie akualnego programu do struktury tymczasowej
//*****************************************************************************

void ProgramKopiuj(void)
{
	prog.CzasDociskuWstepnego = par[p_aktualny_program].CzasDociskuWstepnego;

	prog.PradPodgrzewaniaWstepnego
			= par[p_aktualny_program].PradPodgrzewaniaWstepnego;
	prog.CzasPodgrzewaniaWstepnego
			= par[p_aktualny_program].CzasPodgrzewaniaWstepnego;
	prog.PauzaPodgrzewaniaWstepnego
			= par[p_aktualny_program].PauzaPodgrzewaniaWstepnego;

	prog.PradZgrzewania = par[p_aktualny_program].PradZgrzewania;
	prog.CzasZgrzewania = par[p_aktualny_program].CzasZgrzewania;

	prog.PauzaPodgrzewaniaKoncowego
			= par[p_aktualny_program].PauzaPodgrzewaniaKoncowego;
	prog.PradPodgrzewaniaKoncowego
			= par[p_aktualny_program].PradPodgrzewaniaKoncowego;
	prog.CzasPodgrzewaniaKoncowego
			= par[p_aktualny_program].CzasPodgrzewaniaKoncowego;

	prog.CzasDociskuKoncowego = par[p_aktualny_program].CzasDociskuKoncowego;

	prog.ImpulsyIlosc = par[p_aktualny_program].ImpulsyIlosc;
	prog.ImpulsyPauza = par[p_aktualny_program].ImpulsyPauza;

	prog.StepperProcent = par[p_aktualny_program].StepperProcent;
	prog.StepperLicznik = par[p_aktualny_program].StepperLicznik;

	prog.NumerCylindra = par[p_aktualny_program].NumerCylindra;
	prog.PozycjaCylindraDol = par[p_aktualny_program].PozycjaCylindraDol;
	prog.PozycjaCylindraDolTolerancja
			= par[p_aktualny_program].PozycjaCylindraDolTolerancja;
	prog.Wtopienie = par[p_aktualny_program].Wtopienie;

	prog.CisnienieZadane = par[p_aktualny_program].CisnienieZadane;
	prog.CisnienieOsiagniete = par[p_aktualny_program].CisnienieOsiagniete;

	prog.KontrolaPrzezWydmuch = par[p_aktualny_program].KontrolaPrzezWydmuch;
	prog.KontrolaPrzezWydmuchTol
			= par[p_aktualny_program].KontrolaPrzezWydmuchTol;

	prog.Iref = par[p_aktualny_program].Iref;
	prog.IrefTolerancja = par[p_aktualny_program].IrefTolerancja;
	prog.Iakt = par[p_aktualny_program].CzasDociskuWstepnego;
	prog.Uref = par[p_aktualny_program].Iakt;
	prog.UrefTolerancja = par[p_aktualny_program].Uref;
	prog.Uakt = par[p_aktualny_program].Uakt;
	prog.Eref = par[p_aktualny_program].Eref;
	prog.ErefTolerancja = par[p_aktualny_program].ErefTolerancja;
	prog.Eakt = par[p_aktualny_program].Eakt;

	prog.CzujnikiStartKonfig = par[p_aktualny_program].CzujnikiStartKonfig;
	prog.CzujnikiStartSygnaly = par[p_aktualny_program].CzujnikiStartSygnaly;
	prog.CzujnikiDKKonfig = par[p_aktualny_program].CzujnikiDKKonfig;
	prog.CzujnikiDKSygnaly = par[p_aktualny_program].CzujnikiDKSygnaly;

	prog.Wyjscia = par[p_aktualny_program].Wyjscia;
	prog.Konfig = par[p_aktualny_program].Konfig;

	ostatni_program = p_aktualny_program;

	z_pradem = P_ZPradem();
}

//*****************************************************************************
// Obliczenie parametrów zgzewania przed rozpoczêciem
// Np. k¹t zap³onu, pr¹d ze stepperem
//*****************************************************************************
void ObliczParametry(void)
{
	byte i;

	czas_docisku_wstepnego = prog.CzasDociskuWstepnego * 2;
	czas_docisku_koncowego = prog.CzasDociskuKoncowego * 2;
	czas_podgrzewania = prog.CzasPodgrzewaniaWstepnego * 2;
	czas_pauzy_przed_zgrzewaniem = prog.PauzaPodgrzewaniaWstepnego * 2;
	czas_zgrzewania = prog.CzasZgrzewania * 2;
	czas_pauzy_po_zgrzewaniu = prog.PauzaPodgrzewaniaKoncowego * 2;
	czas_dogrzewania = prog.CzasPodgrzewaniaKoncowego * 2;

	//
	// Liczba impulsów musi wiêksza od zera
	//
	if (prog.ImpulsyIlosc == 0)
		prog.ImpulsyIlosc = 1;
	if (prog.ImpulsyPauza == 0)
		prog.ImpulsyPauza = 1;

	liczba_impulsow = prog.ImpulsyIlosc;
	pauza_impulsow = prog.ImpulsyPauza * 2;

	if (prog.StepperProcent == 0)
	{
		prad_zgrzewania = prog.PradZgrzewania;
		prad_podgrzewania = prog.PradPodgrzewaniaWstepnego;
		prad_dogrzewania = prog.PradPodgrzewaniaKoncowego;
	}
	else
	{
		prad_zgrzewania = (word) ((float) prog.PradZgrzewania
				+ (float) prog.PradZgrzewania * (float) prog.StepperProcent
						/ 100.0f * (float) p_licznik_steppera_akt
						/ (float) prog.StepperLicznik);

		prad_podgrzewania = (word) ((float) prog.PradPodgrzewaniaWstepnego
				+ (float) prog.PradPodgrzewaniaWstepnego
						* (float) prog.StepperProcent / 100.0f
						* (float) p_licznik_steppera_akt
						/ (float) prog.StepperLicznik);

		prad_dogrzewania = (word) ((float) prog.PradPodgrzewaniaKoncowego
				+ (float) prog.PradPodgrzewaniaKoncowego
						* (float) prog.StepperProcent / 100.0f
						* (float) p_licznik_steppera_akt
						/ (float) prog.StepperLicznik);
	}

	if (prad_podgrzewania > 999)
	{
		ZaklocenieSet(ERR_PRZEKROCZONA_GRANICA_REGULACJI);
		prad_podgrzewania = 999;
	}
	if (prad_podgrzewania > 999)
	{
		ZaklocenieSet(ERR_PRZEKROCZONA_GRANICA_REGULACJI);
		prad_podgrzewania = 999;
	}
	if (prad_dogrzewania > 999)
	{
		ZaklocenieSet(ERR_PRZEKROCZONA_GRANICA_REGULACJI);
		prad_dogrzewania = 999;
	}

	kat_podgrzewanie = KAT_PRZELICZNIK(prad_podgrzewania);
	kat_zgrzewanie = KAT_PRZELICZNIK(prad_zgrzewania);
	kat_dogrzewanie = KAT_PRZELICZNIK(prad_dogrzewania);

	//
	// Info o obliczonym k¹cie zap³onu
	//
	image.sm[SM_KAT] = kat_zgrzewanie;

	pomiar_prad = 0;
	pomiar_nap = 0;
	pomiar_energia = 0;
	pomiar_numer_zmierzonego_polokresu = 0;
	pomiar_polokres_prad = 0;
	pomiar_polokres_nap = 0;
	wykres_index = 0;

	for (i = 0; i < WYKRES_TAB_SIZE; i++)
	{
		wykres_wzor[i] = 0;
		wykres_prad[i] = 0;
		wykres_nap[i] = 0;
		wykres_wtopienia[i] = 0;
	}

}

//*****************************************************************************
// Tutaj wykonujemy rzeczy po zakoñczonym zgrzewie
// Sprawdzenie, który z kolei zgrzew w cyklu
// W³¹czenie obs³ugi œluzy dla ostatniego zgrzewu i Hydry
//*****************************************************************************
void UstalLiczbeZgrzewow(void)
{
	byte i;

	liczba_zgrzewow = 0;

	for (i = 0; i < p_liczba_programow; i++)
	{
		if (par[i].NumerCylindra > 0)
			liczba_zgrzewow++;
	}
}

//*****************************************************************************
// Tutaj wykonujemy rzeczy po zakoñczonym zgrzewie
// Sprawdzenie, który z kolei zgrzew w cyklu
// W³¹czenie obs³ugi œluzy dla ostatniego zgrzewu i Hydry
//*****************************************************************************
void Cykl(void)
{
	if (p_aktualny_program >= p_liczba_programow)
	{
		//
		// Tylko jezeli zgrzano wszytkie z pradem to zaliczmy zgrzew
		//
		if (z_pradem_wszystkie >= liczba_zgrzewow)
		{
			if (GET_REFERUJ_CYLINDER == false && GET_REFERUJ_PRAD == false)
				sluza_start = true;

			p_licznik++;

			if (GET_REFERUJ_PRAD && (p_ref_cnt_prad > 1))
				p_ref_cnt_prad--;
			else
			{
				RESET_REFERUJ_PRAD;
				p_ref_cnt_prad = 0;
			}
		}

		if (GET_REFERUJ_CYLINDER && (p_ref_cnt_nakr > 1))
			p_ref_cnt_nakr--;
		else
		{
			RESET_REFERUJ_CYLINDER;
			p_ref_cnt_nakr = 0;
		}

		p_aktualny_program = 0;
		z_pradem_wszystkie = 0;

		if (KontrolaDynamiczna_Sprawdzenie() == false)
		{
			ZaklocenieSet(ERR_KONTROLA_CZUJNIKOW);
		}

		KontrolaDynamiczna_OdczytKonfiguracji();
		KontrolaDynamiczna_Reset();
	}
	else
	{
	}
}

//*****************************************************************************
// Obs³uga œluzy
//*****************************************************************************
void Sluza(void)
{
	//
	// Inicjacja sluzy po koncu cyklu
	//
	if (p_sluza == 0)
	{
		bez_sluzy = true;
		sluza_opt = false;
		sluza_klt = false;
	}
	else if (p_sluza > 0 && p_sluza < 1000)
	{
		bez_sluzy = false;
		sluza_opt = true;
		sluza_klt = false;
	}
	else
	{
		bez_sluzy = true;
		sluza_opt = false;
		sluza_klt = true;
	}

	if (sluza_start == true && sluza_started == false && P_Start() == false)
	{
		sluza_start = false;
		sluza_started = true;
		sluza_check = true;

		if (p_sluza < 1000)
		{
			sluza_czas = 50;
			sluza_opt_czas = 0;
		}
		else
			sluza_czas = p_sluza / 10;

		licznik_opt = 0;
	}

	//
	// Sterowanie wyjsciem
	//
	if (sluza_czas > 0)
	{
		sluza_czas--;
		P_KoniecCyklu(true);
	}
	else
	{
		if (P_SluzaSygnal() == true && sluza_klt && sluza_started == true)
			ZaklocenieSet(ERR_SLUZA);

		sluza_started = false;
		P_KoniecCyklu(false);
	}

	//
	// Sluza optyczna
	//
	if (sluza_opt && licznik_opt > 1)
	{
		ZaklocenieSet(ERR_SLUZA_OPT_DWA_RAZY);
	}

	if (sluza_opt && P_SluzaSygnal_N() && sluza_opt_czas == 0)
	{
		licznik_opt++;
		sluza_opt_czas = p_sluza / 10;
	}

	if (sluza_opt && P_SluzaSygnal())
	{
		if (sluza_opt_czas > 0)
			sluza_opt_czas--;
	}
}

//*****************************************************************************
// Opracowanie wyników po zgrzewaniu
// Np. wartoœæ pr¹du
//*****************************************************************************
bool WynikiZgrzewania(void)
{
	bool retval = true;

	pomiar_prad = pomiar_polokres_prad / pomiar_numer_zmierzonego_polokresu;
	pomiar_nap = pomiar_polokres_nap / pomiar_numer_zmierzonego_polokresu;

	// Wyeliminowanie malego plywania wejsc
	// !!!!!!!!!!
	// Sprawdziæ
	// !!!!!!!!!!
	if (pomiar_prad < GRANICA_BRAK_PRZEPLYWU_PRADU)
		pomiar_prad = 0;

	if (pomiar_nap < GRANICA_BRAK_NAPIECIA_WTORNEGO)
		pomiar_nap = 0;

	pomiar_energia = (pomiar_prad * pomiar_nap / 100000)
			* pomiar_numer_zmierzonego_polokresu;

	p_ostzgrzew_nr_prog = ostatni_program;
	p_ostzgrzew_prad = prad_zgrzewania;
	par[ostatni_program].Iakt = p_ostzgrzew_pom_prad = (word) pomiar_prad;
	par[ostatni_program].Uakt = p_ostzgrzew_pom_nap = (word) pomiar_nap;
	par[ostatni_program].Eakt = p_ostzgrzew_pom_ener = (word) (pomiar_energia); // >> 16);

	// WTOPIENIE

	p_ostzgrzew_pom_wtop = wtop;

	if (GET_REFERUJ_CYLINDER == false && z_pradem)
	{
		// Wtopienie za ma³e
		if (p_ostzgrzew_pom_wtop < prog.Wtopienie)
		{
			retval = false;
			ZaklocenieSet(ERR_BRAK_WTOPIENIA);
		}
	}

	// PRAD

	prad_akt = (float) (par[ostatni_program].Iakt);
	prad_zad = (float) (par[ostatni_program].Iref);
	prad_tol = prad_zad * (float) (par[ostatni_program].IrefTolerancja)
			/ 100.0f;

	nap_akt = (float) (par[ostatni_program].Uakt);
	nap_zad = (float) (par[ostatni_program].Uref);
	nap_tol = nap_zad * (float) (par[ostatni_program].UrefTolerancja) / 100.0f;

	energia_akt = (float) (par[ostatni_program].Eakt);
	energia_zad = (float) (par[ostatni_program].Eref);
	energia_tol = energia_zad * (float) (par[ostatni_program].ErefTolerancja)
			/ 100.0f;

	if (GET_REFERUJ_PRAD == false && z_pradem)
	{
		// Brak przeplywu pradu
		if (p_ostzgrzew_pom_prad < GRANICA_BRAK_PRZEPLYWU_PRADU)
		{
			retval = false;
			ZaklocenieSet(ERR_BRAK_PRZEPYWU_PRADU);
		}

		// Brak napiecia
		//if (p_ostzgrzew_pom_nap < GRANICA_BRAK_NAPIECIA_WTORNEGO)
		//{
		//retval = false;
		//ZaklocenieSet(ERR_BRAK_NAP_WTORNEGO);
		//}

		// Prad za du¿y
		if (prad_akt > (prad_zad + prad_tol))
		{
			retval = false;
			ZaklocenieSet(ERR_PRAD_DUZO);
		}

		// Prad za ma³y
		if (prad_akt < (prad_zad - prad_tol))
		{
			retval = false;
			ZaklocenieSet(ERR_PRAD_MALO);
		}

		// Napiêcie za du¿e
		if (nap_akt > (nap_zad + nap_tol))
		{
			retval = false;
			ZaklocenieSet(ERR_NAP_DUZO);
		}

		// Napiêcie za ma³e
		if (nap_akt < (nap_zad - nap_tol))
		{
			retval = false;
			ZaklocenieSet(ERR_NAP_MALO);
		}

		// Energia za du¿a
		if (energia_akt > (energia_zad + energia_tol))
		{
			retval = false;
			ZaklocenieSet(ERR_ENERGIA_DUZO);
		}

		// Energia za ma³a
		if (energia_akt < (energia_zad - energia_tol))
		{
			retval = false;
			ZaklocenieSet(ERR_ENERGIA_MALO);
		}

	}
	else if (z_pradem)
	{
		ReferujPrad();
	}

	return retval;
}

//*****************************************************************************
// Pomiar wejœæ analogowych
// Naprzemiennie seq 1 i seq 2
// W kazdym seq odczyt pomiarów i 2 analogów, w sumie 4 pomiary
//*****************************************************************************

void Pomiar3(void)
{
	static bool seq = false;

	if (seq == false)
	{
		ADC_Odczyt2(adc_bufor);
		ADC_Start1();
		seq = true;
		adc0_suma += adc_bufor[0];
		adc1_suma += adc_bufor[1];
		adc4_suma += adc_bufor[2];
		adct_suma += adc_bufor[3];
		adc0_index++;
		adc1_index++;
		adc4_index++;
		adct_index++;
	}
	else
	{
		ADC_Odczyt1(adc_bufor);
		ADC_Start2();
		seq = false;
		adc0_suma += adc_bufor[0];
		adc1_suma += adc_bufor[1];
		adc2_suma += adc_bufor[2];
		adc3_suma += adc_bufor[3];
		adc0_index++;
		adc1_index++;
		adc2_index++;
		adc3_index++;
	}
}

//*****************************************************************************
// Funkcje normalizacyjne
// W przypadku korzystania z word'ów i przepe³nieñ trzeba ustaliæ wartoœci min i max
// Je¿eli wiêkszy ni¿ 15000 to równe 15000
// Je¿eli mniejszy od 0 to 0
//*****************************************************************************

dword NormalizujADC_Pomiar(long wartosc)
{
	dword retval = (dword) wartosc;

	if (wartosc > 15000)
		retval = 15000;
	else if (wartosc < 0)
		retval = 0;

	return retval;
}

dword NormalizujADC_AIN(long wartosc)
{
	dword retval = (dword) wartosc;

	if (wartosc > 10000)
		retval = 10000;
	else if (wartosc < 0)
		retval = 0;

	return retval;
}

void Pomiar_Obliczenia(void)
{
	dword prad, nap;

	image.ain[0]
			= ANALOG_ADC_POMIAR(NormalizujADC_Pomiar( ((adc0_suma / adc0_index) - adc0_offset) ));
	image.ain[1]
			= ANALOG_ADC_POMIAR(NormalizujADC_Pomiar( ((adc1_suma / adc1_index) - adc1_offset) ));
	image.ain[2]
			= ANALOG_ADC_AIN(NormalizujADC_AIN( ((adc2_suma / adc2_index) - adc2_offset) ));
	image.ain[3]
			= ANALOG_ADC_AIN(NormalizujADC_AIN( ((adc3_suma / adc3_index) - adc3_offset) ));
	image.ain[4]
			= ANALOG_ADC_AIN(NormalizujADC_AIN( ((adc4_suma / adc4_index) - adc4_offset) ));
	image.ain[5] = ((adct_suma / adct_index) - adct_offset);

	//
	// Przeliczenie temperatury
	//
	image.sm[SM_TEMPERATURA_UC] = (ADC_TEMP(image.ain[5]));

	//
	// Pomiar wtopienia
	//
	if ((p_stanprocesu >= ETPROC_ZGRZEWANIE_PODGRZEWANIE) && (p_stanprocesu
			<= ETPROC_ZGRZEWANIE_DOCISK_KONCOWY))
	{
		if (image.ain[3] > p_wtopienie_poczatkowe)
			wtop_temp = image.ain[3] - p_wtopienie_poczatkowe;
		else
			wtop_temp = p_wtopienie_poczatkowe - image.ain[3];

		// Szukamy max i zapisujemy w tab wykresu
		if (wtop_temp > wtop)
			wtop = wtop_temp;
	}

	//
	// Tylko do testów wartoœæ k¹ta
	// Tworzenie wykresu wzorcowego
	//
	if (wykres_index < WYKRES_TAB_SIZE)
	{
		if (p_stanprocesu == ETPROC_ZGRZEWANIE_PODGRZEWANIE || p_stanprocesu
				== ETPROC_ZGRZEWANIE_ZGRZEWANIE || p_stanprocesu
				== ETPROC_ZGRZEWANIE_DOGRZEWANIE)
		{
			wykres_wzor[wykres_index] = (KAT_MIN_TIME - kat) * 6;
		}
		else
		{
			wykres_wzor[wykres_index] = 0;
		}

		//
		// Tworzenie wykresu pr¹du i wartoœci
		//
		if ((p_stanprocesu >= ETPROC_ZGRZEWANIE_PODGRZEWANIE) && (p_stanprocesu
				<= ETPROC_ZGRZEWANIE_DOCISK_KONCOWY))
		{
			prad = wykres_prad[wykres_index] = image.ain[0];
			nap = wykres_nap[wykres_index] = image.ain[1];
			wykres_wtopienia[wykres_index] = wtop_temp;

			// Zwiekszenie indexu indexu
			if (wykres_index < WYKRES_TAB_SIZE)
				wykres_index++;

			if ((p_stanprocesu > ETPROC_ZGRZEWANIE_DOCISK_WSTEPNY)
					&& (p_stanprocesu < ETPROC_ZGRZEWANIE_DOCISK_KONCOWY))
			{
				pomiar_polokres_prad += prad;
				pomiar_polokres_nap += nap;
				pomiar_numer_zmierzonego_polokresu++;
			}
		}
	}

	//
	// Na koniec czyszczenie rejestrów
	//
	adc0_suma = 0;
	adc1_suma = 0;
	adc2_suma = 0;
	adc3_suma = 0;
	adc4_suma = 0;
	adct_suma = 0;
	adc0_index = 0;
	adc1_index = 0;
	adc2_index = 0;
	adc3_index = 0;
	adc4_index = 0;
	adct_index = 0;
}

/*
 //*****************************************************************************
 // Pomiar wejœæ analogowych
 //*****************************************************************************
 void Analogi(void)
 {
 ADC_AIN_Odczyt(adc_bufor2);

 image.ain[2] = adc_bufor2[0];
 image.ain[3] = adc_bufor2[1];
 image.ain[4] = adc_bufor2[2];
 image.ain[5] = 0;

 image.sm[SM_TEMPERATURA_UC] = (ADC_TEMP(adc_bufor2[3]));

 adc_bufor2[0] = adc_bufor2[1] = adc_bufor2[2] = 0;
 ADC_AIN_Start();
 }
 */
//*****************************************************************************
// Mruganie z przerwania Proces Int 20kHz
//*****************************************************************************

void KontrolaNakretki(void)
{
	if (GET_REFERUJ_CYLINDER == false)
	{
		if ((p_cylinder_akt >= (par[p_aktualny_program].PozycjaCylindraDol
				- par[p_aktualny_program].PozycjaCylindraDolTolerancja))
				&& (p_cylinder_akt
						<= (par[p_aktualny_program].PozycjaCylindraDol
								+ par[p_aktualny_program].PozycjaCylindraDolTolerancja)))
			SET_NAKR_POZ;
		else
			RESET_NAKR_POZ;
	}
	else
		SET_NAKR_POZ;

	/*
	 if (GET_REFERUJ_CYLINDER == false)
	 {
	 if ((p_wydmuch_akt >= (par[p_aktualny_program].KontrolaPrzezWydmuch
	 - par[p_aktualny_program].KontrolaPrzezWydmuchTol))
	 && (p_wydmuch_akt
	 <= (par[p_aktualny_program].KontrolaPrzezWydmuch
	 + par[p_aktualny_program].KontrolaPrzezWydmuchTol)))
	 SET_NAKR_WYD;
	 else

	 RESET_NAKR_WYD;
	 }
	 else
	 SET_NAKR_WYD;
	 */
	SET_NAKR_WYD;
}

void ResetReferencjiPradu(byte wartosc)
{
	int i, j;

	if (wartosc == 0)
		wartosc = 1;

	p_ref_cnt_prad = ref_prad_cnt0 = wartosc;

	for (i = 0; i < 16; i++)
	{
		for (j = 0; j < REF_TAB_SIZE; j++)
		{
			RefTabWtopienie[i][j] = 0;
			RefTabPrad[i][j] = 0;
			RefTabNapiecie[i][j] = 0;
			RefTabEnergia[i][j] = 0;
		}
	}
}

void ResetReferencjiCylindra(byte wartosc)
{
	int i, j;

	if (wartosc == 0)
		wartosc = 1;

	p_cylinder_gora = p_cylinder_akt;
	p_ref_cnt_nakr = ref_nakr_cnt0 = wartosc;

	for (i = 0; i < 16; i++)
	{
		for (j = 0; j < REF_TAB_SIZE; j++)
		{
			RefTabCylinder[i][j] = 0;
			RefTabWydmuch[i][j] = 0;
		}
	}
}

//*****************************************************************************
// Funkcja wykonywana przy referowaniu w procesie
// Tutaj zbierane s¹ wartoœci i w przypadku dojœcia do zera wywo³ywana jest funkcja
// obliczj¹ca œredni¹
//*****************************************************************************

void ReferujCylinder()
{
	RefTabCylinder[p_aktualny_program][p_ref_cnt_nakr - 1] = p_cylinder_akt;
	RefTabWydmuch[p_aktualny_program][p_ref_cnt_nakr - 1] = p_wydmuch_akt;
	Oblicz_Ref_Nakr();
}

void ReferujPrad()
{
	RefTabWtopienie[ostatni_program][p_ref_cnt_prad - 1] = wtop;
	RefTabPrad[ostatni_program][p_ref_cnt_prad - 1] = prad_akt;
	RefTabNapiecie[ostatni_program][p_ref_cnt_prad - 1] = nap_akt;
	RefTabEnergia[ostatni_program][p_ref_cnt_prad - 1] = energia_akt;
	Oblicz_Ref_Prad();
}

//*****************************************************************************
// Obliczanie œredniej z pomiarów referencyjnych referowania
//*****************************************************************************

void Oblicz_Ref_Nakr()
{
	int i, j;

	dword srednia_cylinder = 0, srednia_wydmuch = 0;
	word min_cyl, min_wyd, max_cyl, max_wyd;

	if (ref_nakr_cnt0 > 0)
	{
		// Obliczenie sumy i œredniej, oraz wyznaczenie tolerancji
		min_wyd = min_cyl = 0xffff;
		max_wyd = max_cyl = 0;
		srednia_cylinder = 0;
		srednia_wydmuch = 0;

		for (j = p_ref_cnt_nakr - 1; j < ref_nakr_cnt0; j++)
		{
			srednia_cylinder += RefTabCylinder[p_aktualny_program][j];
			srednia_wydmuch += RefTabWydmuch[p_aktualny_program][j];

			if (RefTabCylinder[p_aktualny_program][j] > max_cyl)
				max_cyl = RefTabCylinder[p_aktualny_program][j];
			if (RefTabCylinder[p_aktualny_program][j] < min_cyl)
				min_cyl = RefTabCylinder[p_aktualny_program][j];

			if (RefTabWydmuch[p_aktualny_program][j] > max_wyd)
				max_wyd = RefTabWydmuch[p_aktualny_program][j];
			if (RefTabWydmuch[p_aktualny_program][j] < min_wyd)
				min_wyd = RefTabWydmuch[p_aktualny_program][j];
		}

		srednia_cylinder /= (ref_nakr_cnt0 - p_ref_cnt_nakr + 1);
		srednia_wydmuch /= (ref_nakr_cnt0 - p_ref_cnt_nakr + 1);

		prog.PozycjaCylindraDol = par[p_aktualny_program].PozycjaCylindraDol
				= (word) srednia_cylinder;
		prog.KontrolaPrzezWydmuch
				= par[p_aktualny_program].KontrolaPrzezWydmuch
						= (word) srednia_wydmuch;

		if (GET_POMIAR_TOLERANCJI && ref_nakr_cnt0 > 1)
		{
			if ((max_cyl != 0) && (min_cyl != 0xffff) && (max_cyl >= min_cyl))
			{
				prog.PozycjaCylindraDolTolerancja
						= par[p_aktualny_program].PozycjaCylindraDolTolerancja
								= (max_cyl - min_cyl) * 2 / 3;
			}

			if ((max_cyl != 0) && (min_cyl != 0xffff) && (max_cyl < min_cyl))
			{
				prog.PozycjaCylindraDolTolerancja
						= par[p_aktualny_program].PozycjaCylindraDolTolerancja
								= (min_cyl - max_cyl) * 2 / 3;
			}

			if ((max_wyd != 0) && (min_wyd != 0xffff) && (max_wyd >= min_wyd))
			{
				prog.KontrolaPrzezWydmuchTol
						= par[p_aktualny_program].KontrolaPrzezWydmuchTol
								= (max_wyd - min_wyd) * 2 / 3;
			}

			if ((max_wyd != 0) && (min_wyd != 0xffff) && (max_wyd < min_wyd))
			{
				prog.KontrolaPrzezWydmuchTol
						= par[p_aktualny_program].KontrolaPrzezWydmuchTol
								= (min_wyd - max_wyd) * 2 / 3;
			}
		}
	}
}

void Oblicz_Ref_Prad()
{
	int i, j;

	dword srednia_wtop = 0, srednia_prad = 0, srednia_napiecie = 0,
			srednia_energia = 0;

	if (ref_prad_cnt0 > 0)
	{
		srednia_wtop = 0;
		srednia_prad = 0;
		srednia_napiecie = 0;
		srednia_energia = 0;

		for (j = p_ref_cnt_prad - 1; j < ref_prad_cnt0; j++)
		{
			srednia_wtop += RefTabWtopienie[ostatni_program][j];
			srednia_prad += RefTabPrad[ostatni_program][j];
			srednia_napiecie += RefTabNapiecie[ostatni_program][j];
			srednia_energia += RefTabEnergia[ostatni_program][j];
		}

		srednia_wtop /= (ref_prad_cnt0 - p_ref_cnt_prad + 1);
		srednia_prad /= (ref_prad_cnt0 - p_ref_cnt_prad + 1);
		srednia_napiecie /= (ref_prad_cnt0 - p_ref_cnt_prad + 1);
		srednia_energia /= (ref_prad_cnt0 - p_ref_cnt_prad + 1);

		prog.Wtopienie = par[ostatni_program].Wtopienie = (word) srednia_wtop
				* 2 / 3;
		prog.Iref = par[ostatni_program].Iref = (word) srednia_prad;
		prog.Uref = par[ostatni_program].Uref = (word) srednia_napiecie;
		prog.Eref = par[ostatni_program].Eref = (word) srednia_energia;
	}
}

//*****************************************************************************
// Wynik porównania pozycji cylindraq - je¿eli w górze to true
//*****************************************************************************

void CylinderUGory(void)
{
	if (P_CylinderUGory() || ((p_cylinder_akt > p_cylinder_gora
			- p_cylinder_tol) && (p_cylinder_akt < p_cylinder_gora
			+ p_cylinder_tol)))
		SET_CYL_U_GORY;
	else
		RESET_CYL_U_GORY;
}

//*****************************************************************************
// WYnik porównania ciœnienia zadanego z osi¹gniêtym
//*****************************************************************************

void Cisnienie(void)
{
	word cisnienie_10plus = par[p_aktualny_program].CisnienieZadane
			+ par[p_aktualny_program].CisnienieZadane / 20;
	word cisnienie_10minus = par[p_aktualny_program].CisnienieZadane
			- par[p_aktualny_program].CisnienieZadane / 20;

	image.aout[0] = (par[p_aktualny_program].CisnienieZadane);

	if (par[p_aktualny_program].CisnienieZadane > 0)
	{
		if (P_DK() && (p_cisnienie_akt >= cisnienie_10minus)
				&& (p_cisnienie_akt <= cisnienie_10plus))
		{
			SET_CISNIENIE_OSIAGNIETE;
		}
		else
		{
			RESET_CISNIENIE_OSIAGNIETE;
		}
	}
	else
	{
		if (P_DK())
		{
			SET_CISNIENIE_OSIAGNIETE;
		}
		else
		{
			RESET_CISNIENIE_OSIAGNIETE;
		}
	}
}

//*****************************************************************************
// Odczyt konfiguracji czujników z cyklu i ustsawienie zmiennych w celu szukania
// póŸniejeszego na odpowiednich bitach
//*****************************************************************************
void KontrolaDynamiczna_OdczytKonfiguracji(void)
{
	byte i,j;

	kontrola_dyn_konfig_1 = 0;
	kontrola_dyn_konfig_0 = 0;

	for (i = 0; i < LICZBA_PROGRAMOW; i++)
	{
		for (j = 0; j < 16; j++)
		{
		if ((par[i].CzujnikiStartKonfig & (1 << j)) > 0
				&& (par[i].CzujnikiStartSygnaly & (1 << j)) > 0)
			kontrola_dyn_konfig_1 |= (1 << j);

		if ((par[i].CzujnikiStartKonfig & (1 << j)) > 0
				&& (par[i].CzujnikiStartSygnaly & (1 << j)) == 0)
			kontrola_dyn_konfig_0 |= (1 << j);

		if ((par[i].CzujnikiDKKonfig & (1 << j)) > 0
				&& (par[i].CzujnikiDKSygnaly & (1 << j)) > 0)
			kontrola_dyn_konfig_1 |= (1 << j);

		if ((par[i].CzujnikiDKKonfig & (1 << j)) > 0
				&& (par[i].CzujnikiDKSygnaly & (1 << j)) == 0)
			kontrola_dyn_konfig_0 |= (1 << j);
		}
	}
}

//*****************************************************************************
// Reset kontroli dynamicznej
// Chcemy skasowaæ wszystkie bity w wordzie wiêc na pocz¹tku trzeba ustawiæ te co trzeba
//*****************************************************************************
void KontrolaDynamiczna_Reset(void)
{
	kontrola_dyn_1 = kontrola_dyn_konfig_1;
	kontrola_dyn_0 = kontrola_dyn_konfig_0;
}

//*****************************************************************************
// Odczyt konfiguracji czujników z cyklu i ustawienie zmiennych w celu szukania
// póŸniejeszego na odpowiednich bitach
//*****************************************************************************
void KontrolaDynamiczna_ZbieranieSygnalow(void)
{
	// Kasujemy bit w kontroli 1 je¿eli pojawi siê 0 na danym bicie
	kontrola_dyn_1 &= P_Czujniki();
	kontrola_dyn_0 &= ~P_Czujniki();
}

//*****************************************************************************
// Sprawdzenie wyników kontroli dynamicznej
//*****************************************************************************
bool KontrolaDynamiczna_Sprawdzenie(void)
{
	if ((kontrola_dyn_1 == 0) && (kontrola_dyn_0 == 0))
		return true;
	else
		return false;
}

//*****************************************************************************
// Odczyt wejœæ procesowych i sterowanie wyjsciami procesowymi
// Zalezny od werji programu
//*****************************************************************************

void SterowaniePrzyrzadem(void)
{
	if (ZezwolenieSterowaniaPrzyrzadem() && ((par[p_aktualny_program].Wyjscia
			& 0x01) > 0))
	{
		image.q[1] |= 0x10;
	}
	else
		image.q[1] &= ~0x10;

	if (ZezwolenieSterowaniaPrzyrzadem() && ((par[p_aktualny_program].Wyjscia
			& 0x02) > 0))
	{
		image.q[1] |= 0x20;
	}
	else
		image.q[1] &= ~0x20;

	if (ZezwolenieSterowaniaPrzyrzadem() && ((par[p_aktualny_program].Wyjscia
			& 0x04) > 0))
	{
		image.q[1] |= 0x40;
	}
	else
		image.q[1] &= ~0x40;

	if (ZezwolenieSterowaniaPrzyrzadem() && ((par[p_aktualny_program].Wyjscia
			& 0x08) > 0))
	{
		image.q[1] |= 0x80;
	}
	else
		image.q[1] &= ~0x80;

	if (ZezwolenieSterowaniaPrzyrzadem() && ((par[p_aktualny_program].Wyjscia
			& 0x10) > 0))
	{
		image.q[2] |= 0x01;
	}
	else
		image.q[2] &= ~0x01;

	if (ZezwolenieSterowaniaPrzyrzadem() && ((par[p_aktualny_program].Wyjscia
			& 0x20) > 0))
	{
		image.q[2] |= 0x02;
	}
	else
		image.q[2] &= ~0x02;

	if (ZezwolenieSterowaniaPrzyrzadem() && ((par[p_aktualny_program].Wyjscia
			& 0x40) > 0))
	{
		image.q[2] |= 0x04;
	}
	else
		image.q[2] &= ~0x04;

	if (ZezwolenieSterowaniaPrzyrzadem() && ((par[p_aktualny_program].Wyjscia
			& 0x80) > 0))
	{
		image.q[2] |= 0x08;
	}
	else
		image.q[2] &= ~0x08;

}

bool ZezwolenieSterowaniaPrzyrzadem(void)
{
	return P_Start();
}

void Blink(void)
{
	static word cnt_1_Hz = 0;
	static word cnt_2_Hz = 0;
	static word cnt_5_Hz = 0;

	//
	// 1Hz
	//
	if (cnt_1_Hz++ > 20000)
		cnt_1_Hz = 0;

	if (cnt_1_Hz > 10000)
		blink_1_Hz = true;
	else
		blink_1_Hz = false;
	//
	// 2Hz
	//
	if (cnt_2_Hz++ > 10000)
		cnt_2_Hz = 0;

	if (cnt_2_Hz > 5000)
		blink_2_Hz = true;
	else
		blink_2_Hz = false;
	//
	// 5Hz
	//
	if (cnt_5_Hz++ > 4000)
		cnt_5_Hz = 0;

	if (cnt_5_Hz > 2000)
		blink_5_Hz = true;
	else
		blink_5_Hz = false;
}

bool P_Start_P(void)
{
	bool retval = false;

	static byte prev = 0;
	static byte act = 0;

	act = image.i[0] & 0x02;

	if ((prev == 0) && (act > 0))
		retval = true;
	else
		retval = false;

	prev = act;

	return retval;
}

bool P_Start(void)
{
	if ((image.i[0] & 0x02) > 0)
		return true;
	else
		return false;
}

bool P_DK_P(void)
{
	bool retval = false;

	static byte prev = 0;
	static byte act = 0;

	act = image.i[0] & 0x04;

	if ((prev == 0) && (act > 0))
		retval = true;
	else
		retval = false;

	prev = act;

	return retval;
}

bool P_DK(void)
{
	if ((image.i[0] & 0x04) > 0)
		return true;
	else
		return false;
}

bool P_WyblokowanieZezwolenie(void)
{
	if ((image.i[0] & 0x08) > 0)
		return true;
	else
		return false;
}

bool P_ResetBledu(void)
{
	static byte prev = 0;
	static byte act = 0;

	act = image.i[0] & 0x10;

	if ((prev == 0) && (act > 0))
		kasuj_blad = true;

	prev = act;

	return kasuj_blad;
}

bool P_NotAus(void)
{
	if ((image.i[0] & 0x20) > 0)
		return true;
	else
		return false;
}

bool P_Woda(void)
{
	if ((image.i[0] & 0x40) > 0)
		return true;
	else
		return false;
}

bool P_TempTrafo(void)
{
	if ((image.i[0] & 0x80) > 0)
		return true;
	else
		return false;
}

bool P_TempTyr(void)
{
	if ((image.i[1] & 0x01) > 0)
		return true;
	else
		return false;
}

bool P_Hydra(void)
{
	if ((image.i[1] & 0x40) > 0)
		return true;
	else
		return false;
}

bool P_ZPradem(void)
{
	if ((image.i[1] & 0x02) > 0)
		return true;
	else
		return false;
}

bool P_ZezwolenieProgramowania(void)
{
	if ((image.i[1] & 0x04) > 0)
		return true;
	else
		return false;
}

bool P_KasujLicznik(void)
{
	bool retval = false;

	static byte prev = 0;
	static byte act = 0;

	act = image.i[1] & 0x08;

	if ((prev == 0) && (act > 0))
		retval = true;
	else
		retval = false;

	prev = act;

	return retval;
}

bool P_KasujLicznikStepper(void)
{
	bool retval = false;

	static byte prev = 0;
	static byte act = 0;

	act = image.i[1] & 0x10;

	if ((prev == 0) && (act > 0))
		retval = true;
	else
		retval = false;

	prev = act;

	return retval;
}

bool P_CylinderUGory(void)
{
	if ((image.i[1] & 0x20) > 0)
		return true;
	else
		return false;
}

bool P_SluzaSygnal(void)
{
	if ((image.i[1] & 0x80) > 0)
		return true;
	else
		return false;
}

bool P_SluzaSygnal_N(void)
{
	bool retval = false;

	static byte prev = 0;
	static byte act = 0;

	act = image.i[1] & 0x80;

	if ((prev > 0) && (act == 0))
		retval = true;
	else
		retval = false;

	prev = act;

	return retval;
}

word P_Czujniki(void)
{
	return (image.i[2] + ((image.i[3] & 0x3f) << 8));
}

bool P_CzujnikiZezwoleniePrzyStarcie(void)
{
	if ((prog.CzujnikiStartKonfig & prog.CzujnikiStartSygnaly) == (P_Czujniki()
			& prog.CzujnikiStartKonfig))
		return true;
	else
		return false;
}

bool P_CzujnikiZezwoleniePrzyDK(void)
{
	if ((prog.CzujnikiDKKonfig & prog.CzujnikiDKSygnaly) == (P_Czujniki()
			& prog.CzujnikiDKKonfig))
		return true;
	else
		return false;
}

void P_Impuls_Hard(bool in)
{
	if (in == true)
		SetHard_Q00();
	else
		ResetHard_Q00();
}

void P_Impuls(bool in)
{
	if (in == true)
		image.q[0] |= 0x01;
	else
		image.q[0] &= ~0x01;
}

void P_CylindryStart_Hard(bool in)
{
	if (in && ((prog.NumerCylindra & 0x01) > 0))
		SetHard_Q01();
	else
		ResetHard_Q01();

	if (in && ((prog.NumerCylindra & 0x02) > 0))
		SetHard_Q02();
	else
		ResetHard_Q02();

	if (in && ((prog.NumerCylindra & 0x04) > 0))
		image.q[2] |= 0x10;
	else
		image.q[2] &= ~0x10;

	if (in && ((prog.NumerCylindra & 0x08) > 0))
		image.q[2] |= 0x20;
	else
		image.q[2] &= ~0x20;

	if (in && ((prog.NumerCylindra & 0x10) > 0))
		image.q[2] |= 0x40;
	else
		image.q[2] &= ~0x40;
}

void P_CylindryStart(bool in)
{
	if (in && ((prog.NumerCylindra & 0x01) > 0))
		image.q[0] |= 0x02;
	else
		image.q[0] &= ~0x02;

	if (in && ((prog.NumerCylindra & 0x02) > 0))
		image.q[0] |= 0x04;
	else
		image.q[0] &= ~0x04;

	if (in && ((prog.NumerCylindra & 0x04) > 0))
		image.q[2] |= 0x10;
	else
		image.q[2] &= ~0x10;

	if (in && ((prog.NumerCylindra & 0x08) > 0))
		image.q[2] |= 0x20;
	else
		image.q[2] &= ~0x20;

	if (in && ((prog.NumerCylindra & 0x10) > 0))
		image.q[2] |= 0x40;
	else
		image.q[2] &= ~0x40;
}

bool P_CylindryIda(void)
{
	if ((GetHard_Q01() > 0) || (GetHard_Q02() > 0) || ((image.q[2] & 0x10) > 0)
			|| ((image.q[2] & 0x20) > 0) || ((image.q[2] & 0x40) > 0))
		return true;
	else
		return false;
}

void P_ZadanieWyblokowania_Hard(bool in)
{
	if (in == true)
		SetHard_Q03();
	else
		ResetHard_Q03();
}

void P_ZadanieWyblokowania(bool in)
{
	if (in == true)
		image.q[0] |= 0x08;
	else
		image.q[0] &= ~0x08;
}

void P_VZ_Hard(bool in)
{
	if (in == true)
		SetHard_Q04();
	else
		ResetHard_Q04();
}

void P_VZ(bool in)
{
	if (in == true)
		image.q[0] |= 0x10;
	else
		image.q[0] &= ~0x10;
}

bool P_Get_VZ_Hard(void)
{
	if (GetHard_Q04() > 0)
		return true;
	else
		return false;
}

bool P_Get_VZ(void)
{
	if (image.q[0] & 0x10 > 0)
		return true;
	else
		return false;
}

void P_FK_Hard(bool in)
{
	if (in == true)
		SetHard_Q05();
	else
		ResetHard_Q05();
}

void P_FK(bool in)
{
	if (in == true)
		image.q[0] |= 0x20;
	else
		image.q[0] &= ~0x20;
}

void P_KoniecCyklu_Hard(bool in)
{
	if (in == true)
		SetHard_Q06();
	else
		ResetHard_Q06();
}

void P_KoniecCyklu(bool in)
{
	if (in == true)
		image.q[0] |= 0x40;
	else
		image.q[0] &= ~0x40;
}

void P_Blad_Hard(bool in)
{
	if (in == true)
		SetHard_Q07();
	else
		ResetHard_Q07();
}

void P_Blad(bool in)
{
	if (in == true)
		image.q[0] |= 0x80;
	else
		image.q[0] &= ~0x80;
}

void P_Gotowosc_Hard(bool in)
{
	if (in == true)
		SetHard_Q10();
	else
		ResetHard_Q10();
}

void P_Gotowosc(bool in)
{
	if (in == true)
		image.q[1] |= 0x01;
	else
		image.q[1] &= ~0x01;
}

void P_LicznikOsiagniety_Hard(bool in)
{
	if (in == true)
		SetHard_Q11();
	else
		ResetHard_Q11();
}

void P_LicznikOsiagniety(bool in)
{
	if (in == true)
		image.q[1] |= 0x02;
	else
		image.q[1] &= ~0x02;
}

void P_LicznikStepperaOstrzezenie_Hard(bool in)
{
	if (in == true)
		SetHard_Q12();
	else
		ResetHard_Q12();
}

void P_LicznikStepperaOstrzezenie(bool in)
{
	if (in == true)
		image.q[1] |= 0x04;
	else
		image.q[1] &= ~0x04;
}

void P_LicznikStepperaMax_Hard(bool in)
{
	if (in == true)
		SetHard_Q13();
	else
		ResetHard_Q13();
}

void P_LicznikStepperaMax(bool in)
{
	if (in == true)
		image.q[1] |= 0x08;
	else
		image.q[1] &= ~0x08;
}

void P_SterowaniePrzyrzadem_Hard(void)
{
	if (ZezwolenieSterowaniaPrzyrzadem() && ((prog.Wyjscia & 0x01) > 0))
	{
		SetHard_Q14();
	}
	else
		ResetHard_Q14();

	if (ZezwolenieSterowaniaPrzyrzadem() && ((prog.Wyjscia & 0x02) > 0))
	{
		SetHard_Q15();
	}
	else
		ResetHard_Q15();

	if (ZezwolenieSterowaniaPrzyrzadem() && ((prog.Wyjscia & 0x04) > 0))
	{
		SetHard_Q16();
	}
	else
		ResetHard_Q16();

	if (ZezwolenieSterowaniaPrzyrzadem() && ((prog.Wyjscia & 0x08) > 0))
	{
		SetHard_Q17();
	}
	else
		ResetHard_Q17();

	if (ZezwolenieSterowaniaPrzyrzadem() && ((prog.Wyjscia & 0x10) > 0))
	{
		image.q[2] |= 0x01;
	}
	else
		image.q[2] &= ~0x01;

	if (ZezwolenieSterowaniaPrzyrzadem() && ((prog.Wyjscia & 0x20) > 0))
	{
		image.q[2] |= 0x02;
	}
	else
		image.q[2] &= ~0x02;

	if (ZezwolenieSterowaniaPrzyrzadem() && ((prog.Wyjscia & 0x40) > 0))
	{
		image.q[2] |= 0x04;
	}
	else
		image.q[2] &= ~0x04;

	if (ZezwolenieSterowaniaPrzyrzadem() && ((prog.Wyjscia & 0x80) > 0))
	{
		image.q[2] |= 0x08;
	}
	else
		image.q[2] &= ~0x08;

}

