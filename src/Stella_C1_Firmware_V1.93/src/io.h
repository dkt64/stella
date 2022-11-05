/*
 * io.h
 *
 *  Created on: 2009-07-12
 *      Author: Bartek
 */

#ifndef IO_H_
#define IO_H_

/*
 * Definicje STA£YCH
 */
#define ADC_BUFOR 8

#define ADC_TEMP(macarg) (147.5 - ((225.0 * macarg)/1024.0))

/*
 * Definicje funkcji
 */

void InputsInit(void);
void OutputsInit(void);
void SPI_Init(void);
void SPI_Wait();
void SPI_EmptyFIFO();
void SPI_DAC(unsigned int dac_a, unsigned int dac_b);
void SPI_Outputs(unsigned char out2);
void SPI_Inputs(unsigned char* in2, unsigned char* in3, unsigned char* in4);
void IO_Test(void);
void ADC_Init(void);
unsigned char GetHard_SelectButton(void);
void LedOn(void);
void LedOff(void);
void LedSwitch(void);
char Status(void);
void ADC_Pomiar_Start(void);
void ADC_Pomiar_Odczyt(unsigned long buf[]);
void ADC_AIN_Start(void);
void ADC_AIN_Odczyt(unsigned long buf[]);
unsigned char GetHard_I0(void);
unsigned char GetHard_I1(void);
unsigned char GetHard_Q0(void);
unsigned char GetHard_Q1(void);
void SetHard_Q0(byte out);
void SetHard_Q1(unsigned char out);

void SetHard_Q00(void);
void ResetHard_Q00(void);
unsigned char GetHard_Q00(void);

void SetHard_Q01(void);
void ResetHard_Q01(void);
unsigned char GetHard_Q01(void);

void SetHard_Q02(void);
void ResetHard_Q02(void);
unsigned char GetHard_Q02(void);

void SetHard_Q03(void);
void ResetHard_Q03(void);
unsigned char GetHard_Q03(void);

void SetHard_Q04(void);
void ResetHard_Q04(void);
unsigned char GetHard_Q04(void);

void SetHard_Q05(void);
void ResetHard_Q05(void);
unsigned char GetHard_Q05(void);

void SetHard_Q06(void);
void ResetHard_Q06(void);
unsigned char GetHard_Q06(void);

void SetHard_Q07(void);
void ResetHard_Q07(void);
unsigned char GetHard_Q07(void);

void SetHard_Q10(void);
void ResetHard_Q10(void);
unsigned char GetHard_Q10(void);

void SetHard_Q11(void);
void ResetHard_Q11(void);
unsigned char GetHard_Q11(void);

void SetHard_Q12(void);
void ResetHard_Q12(void);
unsigned char GetHard_Q12(void);

void SetHard_Q13(void);
void ResetHard_Q13(void);
unsigned char GetHard_Q13(void);

void SetHard_Q14(void);
void ResetHard_Q14(void);
unsigned char GetHard_Q14(void);

void SetHard_Q15(void);
void ResetHard_Q15(void);
unsigned char GetHard_Q15(void);

void SetHard_Q16(void);
void ResetHard_Q16(void);
unsigned char GetHard_Q16(void);

void SetHard_Q17(void);
void ResetHard_Q17(void);
unsigned char GetHard_Q17(void);

void ADC_Init3(void);
void ADC_Start1(void);
void ADC_Start2(void);
void ADC_Odczyt1(unsigned long buf[]);
void ADC_Odczyt2(unsigned long buf[]);
void SPI_Inputs(unsigned char*, unsigned char*, unsigned char*);

/*
 * Definicje nowych typów
 */



typedef struct _Image
{
	word sm[IOSIZE_SM];
	byte q[IOSIZE_Q];
	word aout[IOSIZE_AOUT];
	byte i[IOSIZE_I];
	word ain[IOSIZE_AIN];
} Image;

// ********************************************************************
// WEJSCIA I WYJŒCIA - OPIS
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
 * I11 [X4.10] [image.i[1] & 0x02] - Z PR¥DEM
 * I12 [X4.11] [image.i[1] & 0x04] - ZEZWOLENIE PROGRAMOWANIA (KLUCZYK)
 * I13 [X4.12] [image.i[1] & 0x08] - KASUJ LICZNIK 1
 * I14 [X4.13] [image.i[1] & 0x10] - KASUJ LICZNIKI STEPERÓW (Wymiana elektrody)
 * I15 [X4.14] [image.i[1] & 0x20] - CYLINDER U GÓRY
 * I16 [X4.15] [image.i[1] & 0x40] - BLOKADA Z HYDRY
 * I17 [X4.16] [image.i[1] & 0x80] - CZUJNIK ŒLUZY
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
 * Q02 [X8.03] - MV2 - zwiêkszona si³a
 * Q03 [X8.04] - Wyblokowanie - ¿¹danie
 * Q04 [X8.05] - VZ
 * Q05 [X8.06] - FK - Koniec zgrzewu
 * Q06 [X8.07] - Licznik - Koniec cyklu
 * Q07 [X8.08] - BLAD ZGRZEWANIA
 * Q10 [X8.09] - GOTOWOSC (STATYCZNA PRZY STARCIE (WODA ITP.) - LAMPKA ZIELONA)
 * Q11 [X8.10] - LICZNIK u¿yutkownika osi¹gniêty (LAMPKA ¯Ó£TA)
 * Q12 [X8.11] - LICZNIK STEPPERA ostrze¿enie (LAMPKA ¿ó³ta)
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
 *
 */

#endif /* IO_H_ */
