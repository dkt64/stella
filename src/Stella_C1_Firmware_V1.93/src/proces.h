/*
 * Proces.h
 *
 *  Created on: 2010-02-20
 *      Author: Bartek
 */

#ifndef PROCES_H_
#define PROCES_H_

//
// Sta³e
//

// D³ugoœæ tablicy o uœredniania referencji
#define REF_TAB_SIZE	11

#define WYKRES_TAB_SIZE 100

/*
 * Sta³e do procesu
 */

#define PROCES_INT 2500		// 200 razy na pó³okres

// INDEX zawiera wartosci od 0 do 200 w jednym polokresie
#define INDEX_0				50
#define INDEX_1000			180
#define IMPULS_TIME			5
#define KAT_ZAKRES			1000

#define KAT_MAX_TIME		(INDEX_0 + IMPULS_TIME)
#define KAT_MIN_TIME		(INDEX_1000 - IMPULS_TIME)
#define KAT_MAX_TIMER		(KAT_MAX_TIME * 100)
#define KAT_MIN_TIMER		(KAT_MIN_TIME * 100)
#define KAT_MAX_MIN		 	(KAT_MIN_TIMER - KAT_MAX_TIMER)
#define KAT_PRZELICZNIK(x)	(KAT_MIN_TIMER - (x)*(KAT_MAX_MIN/KAT_ZAKRES)) / 100

// *******************************************************************************
// Przeliczniki ADC
// *******************************************************************************

// DAC przy tescie N=3000 napiêcie 7.46V i 7.42, czyli 7.44
#define DAC						248
#define ANALOG_DAC(x)			((x)*100/DAC)

// Przy napiêciu 8.5V wartoœæ ADC = 922 co daje 914,4
//#define ADC_AIN					9219
//#define ANALOG_ADC_AIN(x)		((x)*ADC_AIN/1000)
#define ADC_AIN					9400
#define ANALOG_ADC_AIN(x)		((x)*ADC_AIN/1000)

// Przy napiêciu 8.5V wartoœæ ADC = 575 co daje 914,4 ?
//#define ADC_POMIAR				1478
//#define ANALOG_ADC_POMIAR(x)	((x)*ADC_POMIAR/100)
#define ADC_POMIAR				1500
#define ANALOG_ADC_POMIAR(x)	((x)*ADC_POMIAR/100)

#define GRANICA_BRAK_PRZEPLYWU_PRADU   	100
#define GRANICA_BRAK_NAPIECIA_WTORNEGO 	100

#define GRANICA_ZWARCIE_TYRYSTOROW		500 // (mV)
#define LICZNIK_ZWARCIE 				10000  // (20kHz) * 10000 = 0,5sek

// *******************************************************************************
// Bity w parametrach
// *******************************************************************************

/*
 * Konfig:
 * -    bit 0:
 *      1 -> przejscie do kolejnego programu bez puszczenia pulpitu 2-recznego
 *      0 -> przejscie do kolejnego programu z koniecznoœci¹ puszczenia pulpitu 2-recznego
 */

#define KONFIG_BEZ_STARTU 			0x01
#define KONFIG_WYBLOKOWANIE_GORA 	0x08

//
//Makra upraszczaj¹ce kod
//

#define reset_bledu 				(kasuj_blad == true)

#define p_liczba_programow 			image.sm[SM_CYKL_LICZBA_PROG]
#define p_aktualny_program			image.sm[SM_CYKL_PROGRAM_AKT]
#define p_stanprocesu				image.sm[SM_PROGRAM_STAN]

#define p_licznik					image.sm[SM_LICZNIK]
#define p_licznik_max				image.sm[SM_LICZNIK_MAX]
#define p_licznik_steppera_akt		image.sm[SM_LICZNIK_STEPPER]
#define p_licznik_steppera_ost		image.sm[SM_LICZNIK_STEPPER_OST]
#define p_licznik_steppera_max		image.sm[SM_LICZNIK_STEPPER_MAX]

#define p_sluza						image.sm[SM_SLUZA]

#define p_poz_wyj					image.sm[SM_POZ_WYJ]

#define p_ostzgrzew_nr_prog			image.sm[SM_OSTZGRZEW_NR_PROGRAMU]
#define p_ostzgrzew_prad			image.sm[SM_OSTZGRZEW_PRAD]
#define p_ostzgrzew_pom_prad		image.sm[SM_OSTZGRZEW_POMIAR_PRAD]
#define p_ostzgrzew_pom_nap			image.sm[SM_OSTZGRZEW_POMIAR_NAP]
#define p_ostzgrzew_pom_ener		image.sm[SM_OSTZGRZEW_POMIAR_ENER]
#define p_ostzgrzew_pom_wtop		image.sm[SM_OSTZGRZEW_POMIAR_WTOP]

#define p_cylinder_gora				image.sm[SM_CYLINDER_GORA]
#define p_cylinder_akt				image.ain[3]
#define p_cylinder_tol				image.sm[SM_CYLINDER_GORA_TOL]
//#define p_cylinder_tol				500

#define p_wydmuch_akt				image.ain[4]

#define p_cisnienie_akt				image.ain[2]

#define p_status					image.sm[SM_STATUS]

#define err0						image.sm[SM_ERR0]
#define err1						image.sm[SM_ERR1]

//#define p_gotowa					GetHard_Q10()
#define p_gotowa					(image.q[1] & 0x01)

#define p_wtopienie_poczatkowe		image.sm[SM_WTOPIENIE_POCZATKOWE]
#define p_version					image.sm[SM_VERSION]

#define p_ref_cnt_prad				image.sm[SM_REF_CNT_PRAD]
#define p_ref_cnt_nakr				image.sm[SM_REF_CNT_NAKR]

/*
 * Sprawdzilem - min. 195 max. 207
 */

/*
 * NumerCylindra
 * bit0 - Cylinder g³ówny					Q0.1
 * bit1 - Cylinder g³ówny zwiêkszona si³a	Q0.2
 * bit2 - Cylinder dodatkowy 1				Q2.4
 * bit3	- Cylinder dodatkowy 2				Q2.5
 * bit4 - Cylinder dodatkowy 3				Q2.6
 */

/*
 * Wyjscia
 * bit0 - Q1.4
 * bit1 - Q1.5
 * bit2 - Q1.6
 * bit3	- Q1.7
 * bit4 - Q2.0
 * bit5 - Q2.1
 * bit6	- Q2.2
 * bit7 - Q2.3
 */

/*
 * Przeliczniki
 *
 AOUT_Mul=411.145
 AIN_Mul=98.594
 AIN_Meas_Mul=66.466
 */

//
// Zmienne bitowe w obrazie
//

#define SET_REFERUJ_PRAD 		(p_status |= 0x01)
#define RESET_REFERUJ_PRAD 		(p_status &= ~0x01)
#define GET_REFERUJ_PRAD 		(p_status & 0x01)

#define SET_REFERUJ_CYLINDER 	(p_status |= 0x02)
#define RESET_REFERUJ_CYLINDER 	(p_status &= ~0x02)
#define GET_REFERUJ_CYLINDER 	((p_status & 0x02) >> 1)

#define SET_IOTEST 				(p_status |= 0x04)
#define RESET_IOTEST 			(p_status &= ~0x04)
#define GET_IOTEST 				((p_status & 0x04) >> 2)

#define SET_NAKR_POZ			(p_status |= 0x08)
#define RESET_NAKR_POZ 			(p_status &= ~0x08)
#define GET_NAKR_POZ			((p_status & 0x08) >> 3)

#define SET_NAKR_WYD			(p_status |= 0x10)
#define RESET_NAKR_WYD 			(p_status &= ~0x10)
#define GET_NAKR_WYD			((p_status & 0x10) >> 4)

#define SET_CYL_U_GORY			(p_status |= 0x20)
#define RESET_CYL_U_GORY		(p_status &= ~0x20)
#define GET_CYL_U_GORY			((p_status & 0x20) >> 5)

#define SET_CISNIENIE_OSIAGNIETE	(p_status |= 0x40)
#define RESET_CISNIENIE_OSIAGNIETE	(p_status &= ~0x40)
#define GET_CISNIENIE_OSIAGNIETE	((p_status & 0x40) >> 6)

#define SET_POMIAR_TOLERANCJI		(p_status |= 0x80)
#define RESET_POMIAR_TOLERANCJI		(p_status &= ~0x80)
#define GET_POMIAR_TOLERANCJI		((p_status & 0x80) >> 7)

//
// ZMIENNE MODYFIKOWALNE W OBRAZIE SM
//

#define SM_ROZKAZ					0
#define SM_ROZKAZ_ARG				1
#define SM_ROZKAZ_RES1				2
#define SM_ROZKAZ_RES2				3

#define SM_LICZNIK					4	//przyrz¹d
#define SM_LICZNIK_MAX				5	//przyrz¹d
#define SM_LICZNIK_STEPPER			6	//przyrz¹d
#define SM_LICZNIK_STEPPER_OST		7	//przyrz¹d
#define SM_LICZNIK_STEPPER_MAX		8	//przyrz¹d
#define SM_SLUZA					9	//przyrz¹d
#define	SM_CYKL_LICZBA_PROG			10	//przyrz¹d
#define SM_CYLINDER_GORA			11	//przyrz¹d
#define	SM_STATUS					12	//przyrz¹d
#define SM_REF_CNT_PRAD				13	//przyrz¹d
#define	SM_REF_CNT_NAKR				14	//przyrz¹d
#define	SM_CYLINDER_GORA_TOL		15	//przyrz¹d
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

enum ErrNr
{
	ERR_SYNCHRO,
	ERR_NOTAUS,
	ERR_WODA,
	ERR_TEMP_TRAFO,
	ERR_TEMP_TYR,
	ERR_PROCES_PRZERWANY,
	ERR_KONTROLA_CZUJNIKOW,
	ERR_ENERGIA_MALO,
	ERR_ENERGIA_DUZO,
	ERR_PRAD_MALO,
	ERR_PRAD_DUZO,
	ERR_NAP_MALO,
	ERR_NAP_DUZO,
	ERR_ZWARCIE_TYR,
	ERR_BRAK_NAP_WTORNEGO,
	ERR_BRAK_PRZEPYWU_PRADU,
	ERR_BRAK_WTOPIENIA,
	ERR_BRAK_AKTYWACJI,
	ERR_SLUZA_OPT_DWA_RAZY,
	ERR_PRZEKROCZONA_GRANICA_REGULACJI,
	ERR_20,
	ERR_21,
	ERR_22,
	ERR_23,

	// Od tego miejsca tylko bledy przy starcie nie zatrzymuj¹ce procesu
	ERR_HYDRA,
	ERR_SLUZA,
	ERR_SLUZA_OPT_ZERO,
	ERR_27,
	ERR_28,
	ERR_29,
	ERR_30,
	ERR_31
};

enum EtapProcesu
{
	ETPROC_CZEKAM_NA_BRAK_STARTU,
	ETPROC_CZEKAM_NA_POZWOLENIE_STARTU,
	ETPROC_CZEKAM_NA_CYLINDER_W_GORZE,
	ETPROC_CZEKAM_NA_START,
	ETPROC_CZEKAM_NA_CZUJNIKI_START,
	ETPROC_CZEKAM_NA_DK,
	ETPROC_CZEKAM_NA_CZUJNIKI_DK,
	ETPROC_CZEKAM_NA_KONTROLA_NAKRETKI,
	ETPROC_CZEKAM_NA_WYBLOKOWANIE,
	ETPROC_ZGRZEWANIE_DOCISK_WSTEPNY,
	ETPROC_ZGRZEWANIE_PODGRZEWANIE,
	ETPROC_ZGRZEWANIE_PODGRZEWANIE_PAUZA,
	ETPROC_ZGRZEWANIE_ZGRZEWANIE,
	ETPROC_ZGRZEWANIE_ZGRZEWANIE_PAUZA,
	ETPROC_ZGRZEWANIE_DOGRZEWANIE_PAUZA,
	ETPROC_ZGRZEWANIE_DOGRZEWANIE,
	ETPROC_ZGRZEWANIE_DOCISK_KONCOWY,
	ETPROC_ZGRZEWANIE_KONIEC,
	ETPROC_ZGRZEW_OK,
	ETPROC_ZGRZEW_NOK,
	ETPROC_KONIEC_PROCESU
};

typedef struct _WeldParam
{
	byte CzasDociskuWstepnego;

	word PradPodgrzewaniaWstepnego;
	byte CzasPodgrzewaniaWstepnego;
	byte PauzaPodgrzewaniaWstepnego;

	word PradZgrzewania;
	byte CzasZgrzewania;

	byte PauzaPodgrzewaniaKoncowego;
	word PradPodgrzewaniaKoncowego;
	byte CzasPodgrzewaniaKoncowego;

	byte CzasDociskuKoncowego;

	byte ImpulsyIlosc;
	byte ImpulsyPauza;

	byte StepperProcent;
	word StepperLicznik;

	byte NumerCylindra;

	word PozycjaCylindraDol;
	word PozycjaCylindraDolTolerancja;

	word Wtopienie;

	word CisnienieZadane;
	word CisnienieOsiagniete;

	word KontrolaPrzezWydmuch;
	word KontrolaPrzezWydmuchTol;

	word Iref;
	byte IrefTolerancja;
	word Iakt; // read-only
	word Uref;
	byte UrefTolerancja;
	word Uakt; // read-only
	word Eref;
	byte ErefTolerancja;
	word Eakt; // read-only

	word CzujnikiStartKonfig;
	word CzujnikiStartSygnaly;
	word CzujnikiDKKonfig;
	word CzujnikiDKSygnaly;
	word Wyjscia;
	word Konfig;
} WeldParam;

void ProcesInit(void);
void SynchroInt(void);
void ProcesInt(void);
void Pomiar(void);
void Analogi(void);
void ProcesIO(void);
void Proces(void);
void ProgramKopiuj(void);
void ObliczParametry(void);
bool P_Start(void);
bool P_DK(void);
bool P_WyblokowanieZezwolenie(void);
bool P_ResetBledu(void);
bool P_NotAus(void);
bool P_Woda(void);
bool P_TempTrafo(void);
bool P_TempTyr(void);
bool P_Hydra(void);
bool P_ZPradem(void);
bool P_ZezwolenieProgramowania(void);
bool P_KasujLicznik(void);
bool P_KasujLicznikStepper(void);
bool P_CylinderUGory(void);
bool P_BlokadaZHydry(void);
bool P_SluzaSygnal(void);
word P_Czujniki(void);
bool P_CzujnikiZezwoleniePrzyStarcie(void);
bool P_CzujnikiZezwoleniePrzyDK(void);
void P_Impuls_Hard(bool in);
void P_CylindryStart_Hard(bool in);
void P_ZadanieWyblokowania_Hard(bool in);
void P_VZ_Hard(bool in);
void P_FK_Hard(bool in);
void P_KoniecCyklu_Hard(bool in);
void P_Blad_Hard(bool in);
void P_Gotowosc_Hard(bool in);
void P_LicznikOsiagniety_Hard(bool in);
void P_LicznikStepperaOstrzezenie_Hard(bool in);
void P_LicznikStepperaMax_Hard(bool in);
bool ZezwolenieSterowaniaPrzyrzadem(void);
bool WynikiZgrzewania(void);
void Pomiar_SredniaZaPolokres(void);
bool P_PozycjaCylindraDK_OK(void);
bool P_WydmuchDK_OK(void);
bool P_Get_VZ(void);
bool P_ZezwolenieSluza(void);
void Cykl(void);
void Sluza(void);
void Blink(void);
bool P_SluzaSygnal_N(void);
void KontrolaDynamiczna_OdczytKonfiguracji(void);
void KontrolaDynamiczna_Reset(void);
void KontrolaDynamiczna_ZbieranieSygnalow(void);
bool KontrolaDynamiczna_Sprawdzenie(void);
bool P_CylindryIda(void);
bool P_Start_P(void);
void ADC_Start();
void ADC_Odczyt(unsigned long *);
void ResetReferencjiPradu(byte);
void ResetReferencjiCylindra(byte);
void ObliczWtopienie();
void ReferujCylinder();
void SterowaniePrzyrzadem(void);
void KontrolaNakretki(void);
void ReferujPrad();
void CylinderUGory(void);
void Cisnienie(void);
void P_Blad(bool);
void P_Impuls(bool);
void P_Gotowosc(bool);
void UstalLiczbeZgrzewow(void);

// *****************
// Jakies starocie:
// *****************

/*

 #define PROCES_INT 5000	// 10 razy na pó³okres
 // Timer0 tyka 200 razy na 10ms (1 pó³okres)
 #define MOC_MAX_TIME		40		// Minimalna wartoœæ timera i maksymalna moc
 #define MOC_MIN_TIME		180		// Maksymalna wartoœæ timera i minimalna moc
 #define MOC_ZAKRES			1000	// Zakres regulacji (taki jak na sterownikach zgrzewania)
 // Obliczenia dla wartoœci timera
 #define MOC_MAX_TIMER		MOC_MAX_TIME * 100
 #define MOC_MIN_TIMER		MOC_MIN_TIME * 100
 #define MOC_MAX_MIN		 	(MOC_MIN_TIMER - MOC_MAX_TIMER)
 #define MOC_PRZELICZNIK(x)	(MOC_MIN_TIMER - (x)*(MOC_MAX_MIN/MOC_ZAKRES)) / 100
 */

#endif /* PROCES_H_ */
