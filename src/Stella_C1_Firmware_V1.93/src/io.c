//*****************************************************************************
//
// Program sterownika zgrzewania
// Modu³ odpowiedzialny za wejœcia i wyjœcia
//
//*****************************************************************************

#include "main.h"

//*****************************************************************************
// Zmienne globalne
//*****************************************************************************

volatile Image image;

volatile unsigned long adc_bufor1[ADC_BUFOR];
volatile unsigned long adc_bufor2[ADC_BUFOR];
volatile unsigned long adc_bufor[ADC_BUFOR];

//*****************************************************************************
// Funkcja konfiguruj¹ca wejœcia
//*****************************************************************************
void InputsInit(void)
{
	// IN_00 X4.01 IDX0

	GPIOPinTypeGPIOInput(GPIO_PORTD_BASE, GPIO_PIN_0);
	GPIOPadConfigSet(GPIO_PORTD_BASE, GPIO_PIN_0, GPIO_STRENGTH_2MA,
			GPIO_PIN_TYPE_STD);

	// IN_01 X4.02 PHA0

	GPIOPinTypeGPIOInput(GPIO_PORTC_BASE, GPIO_PIN_4);
	GPIOPadConfigSet(GPIO_PORTC_BASE, GPIO_PIN_4, GPIO_STRENGTH_2MA,
			GPIO_PIN_TYPE_STD);

	// IN_02 X4.03 PHB0

	GPIOPinTypeGPIOInput(GPIO_PORTF_BASE, GPIO_PIN_0);
	GPIOPadConfigSet(GPIO_PORTF_BASE, GPIO_PIN_0, GPIO_STRENGTH_2MA,
			GPIO_PIN_TYPE_STD);

	// IN_03 X4.04 IDX1

	GPIOPinTypeGPIOInput(GPIO_PORTF_BASE, GPIO_PIN_1);
	GPIOPadConfigSet(GPIO_PORTF_BASE, GPIO_PIN_1, GPIO_STRENGTH_2MA,
			GPIO_PIN_TYPE_STD);

	// IN_04 X4.05 PHA1

	GPIOPinTypeGPIOInput(GPIO_PORTG_BASE, GPIO_PIN_6);
	GPIOPadConfigSet(GPIO_PORTG_BASE, GPIO_PIN_6, GPIO_STRENGTH_2MA,
			GPIO_PIN_TYPE_STD);

	// IN_05 X4.06 PHB1

	GPIOPinTypeGPIOInput(GPIO_PORTG_BASE, GPIO_PIN_7);
	GPIOPadConfigSet(GPIO_PORTG_BASE, GPIO_PIN_7, GPIO_STRENGTH_2MA,
			GPIO_PIN_TYPE_STD);

	// IN_06 X4.07

	GPIOPinTypeGPIOInput(GPIO_PORTC_BASE, GPIO_PIN_7);
	GPIOPadConfigSet(GPIO_PORTC_BASE, GPIO_PIN_7, GPIO_STRENGTH_2MA,
			GPIO_PIN_TYPE_STD);

	// IN_07 X4.08

	GPIOPinTypeGPIOInput(GPIO_PORTC_BASE, GPIO_PIN_6);
	GPIOPadConfigSet(GPIO_PORTC_BASE, GPIO_PIN_6, GPIO_STRENGTH_2MA,
			GPIO_PIN_TYPE_STD);

	// IN_10 X4.09

	GPIOPinTypeGPIOInput(GPIO_PORTC_BASE, GPIO_PIN_5);
	GPIOPadConfigSet(GPIO_PORTC_BASE, GPIO_PIN_5, GPIO_STRENGTH_2MA,
			GPIO_PIN_TYPE_STD);

	// IN_11 X4.10

	GPIOPinTypeGPIOInput(GPIO_PORTA_BASE, GPIO_PIN_2);
	GPIOPadConfigSet(GPIO_PORTA_BASE, GPIO_PIN_2, GPIO_STRENGTH_2MA,
			GPIO_PIN_TYPE_STD);

	// IN_12 X4.11

	GPIOPinTypeGPIOInput(GPIO_PORTA_BASE, GPIO_PIN_3);
	GPIOPadConfigSet(GPIO_PORTA_BASE, GPIO_PIN_3, GPIO_STRENGTH_2MA,
			GPIO_PIN_TYPE_STD);

	// IN_13 X4.12

	GPIOPinTypeGPIOInput(GPIO_PORTA_BASE, GPIO_PIN_4);
	GPIOPadConfigSet(GPIO_PORTA_BASE, GPIO_PIN_4, GPIO_STRENGTH_2MA,
			GPIO_PIN_TYPE_STD);

	// IN_14 X4.13

	GPIOPinTypeGPIOInput(GPIO_PORTA_BASE, GPIO_PIN_5);
	GPIOPadConfigSet(GPIO_PORTA_BASE, GPIO_PIN_5, GPIO_STRENGTH_2MA,
			GPIO_PIN_TYPE_STD);

	// IN_15 X4.14

	GPIOPinTypeGPIOInput(GPIO_PORTA_BASE, GPIO_PIN_6);
	GPIOPadConfigSet(GPIO_PORTA_BASE, GPIO_PIN_6, GPIO_STRENGTH_2MA,
			GPIO_PIN_TYPE_STD);

	// IN_16 X4.15

	GPIOPinTypeGPIOInput(GPIO_PORTA_BASE, GPIO_PIN_7);
	GPIOPadConfigSet(GPIO_PORTA_BASE, GPIO_PIN_7, GPIO_STRENGTH_2MA,
			GPIO_PIN_TYPE_STD);

	// IN_17 X4.16

	GPIOPinTypeGPIOInput(GPIO_PORTG_BASE, GPIO_PIN_5);
	GPIOPadConfigSet(GPIO_PORTG_BASE, GPIO_PIN_5, GPIO_STRENGTH_2MA,
			GPIO_PIN_TYPE_STD);
}

//*****************************************************************************
// Get - odczyt stanu wejœæ
//*****************************************************************************

unsigned char GetHard_SelectButton(void)
{
	return GPIOPinRead(GPIO_PORTF_BASE, GPIO_PIN_4);
}

unsigned char GetHard_I00(void)
{
	return GPIOPinRead(GPIO_PORTD_BASE, GPIO_PIN_0) >> 0;
}

unsigned char GetHard_I01(void)
{
	return GPIOPinRead(GPIO_PORTC_BASE, GPIO_PIN_4) >> 3;
}

unsigned char GetHard_I02(void)
{
	return GPIOPinRead(GPIO_PORTF_BASE, GPIO_PIN_0) << 2;
}

unsigned char GetHard_I03(void)
{
	return GPIOPinRead(GPIO_PORTF_BASE, GPIO_PIN_1) << 2;
}

unsigned char GetHard_I04(void)
{
	return GPIOPinRead(GPIO_PORTG_BASE, GPIO_PIN_6) >> 2;
}

unsigned char GetHard_I05(void)
{
	return GPIOPinRead(GPIO_PORTG_BASE, GPIO_PIN_7) >> 2;
}

unsigned char GetHard_I06(void)
{
	return GPIOPinRead(GPIO_PORTC_BASE, GPIO_PIN_7) >> 1;
}

unsigned char GetHard_I07(void)
{
	return GPIOPinRead(GPIO_PORTC_BASE, GPIO_PIN_6) << 1;
}

unsigned char GetHard_I10(void)
{
	return GPIOPinRead(GPIO_PORTC_BASE, GPIO_PIN_5) >> 5;
}

unsigned char GetHard_I11(void)
{
	return GPIOPinRead(GPIO_PORTA_BASE, GPIO_PIN_2) >> 1;
}

unsigned char GetHard_I12(void)
{
	return GPIOPinRead(GPIO_PORTA_BASE, GPIO_PIN_3) >> 1;
}

unsigned char GetHard_I13(void)
{
	return GPIOPinRead(GPIO_PORTA_BASE, GPIO_PIN_4) >> 1;
}

unsigned char GetHard_I14(void)
{
	return GPIOPinRead(GPIO_PORTA_BASE, GPIO_PIN_5) >> 1;
}

unsigned char GetHard_I15(void)
{
	return GPIOPinRead(GPIO_PORTA_BASE, GPIO_PIN_6) >> 1;
}

unsigned char GetHard_I16(void)
{
	return GPIOPinRead(GPIO_PORTA_BASE, GPIO_PIN_7) >> 1;
}

unsigned char GetHard_I17(void)
{
	return GPIOPinRead(GPIO_PORTG_BASE, GPIO_PIN_5) << 2;
}

//*****************************************************************************
// Funkcja konfiguruj¹ca wyjœcia
//*****************************************************************************
void OutputsInit(void)
{
	image.q[0] = 0;
	image.q[1] = 0;
	image.q[2] = 0;

	// -------------------------------------------------------------
	// X8
	// -------------------------------------------------------------

	// OUT_00 X8.01 PWM0
	GPIOPinTypeGPIOOutput(GPIO_PORTG_BASE, GPIO_PIN_2);
	GPIOPadConfigSet(GPIO_PORTG_BASE, GPIO_PIN_2, GPIO_STRENGTH_2MA,
			GPIO_PIN_TYPE_STD);

	// OUT_01 X8.02 PWM1
	GPIOPinTypeGPIOOutput(GPIO_PORTD_BASE, GPIO_PIN_1);
	GPIOPadConfigSet(GPIO_PORTD_BASE, GPIO_PIN_1, GPIO_STRENGTH_2MA,
			GPIO_PIN_TYPE_STD);

	// OUT_02 X8.03 PWM2
	GPIOPinTypeGPIOOutput(GPIO_PORTH_BASE, GPIO_PIN_0);
	GPIOPadConfigSet(GPIO_PORTH_BASE, GPIO_PIN_0, GPIO_STRENGTH_2MA,
			GPIO_PIN_TYPE_STD);

	// OUT_03 X8.04 PWM3
	GPIOPinTypeGPIOOutput(GPIO_PORTH_BASE, GPIO_PIN_1);
	GPIOPadConfigSet(GPIO_PORTH_BASE, GPIO_PIN_1, GPIO_STRENGTH_2MA,
			GPIO_PIN_TYPE_STD);

	// OUT_04 X8.05 PWM4
	GPIOPinTypeGPIOOutput(GPIO_PORTF_BASE, GPIO_PIN_2);
	GPIOPadConfigSet(GPIO_PORTF_BASE, GPIO_PIN_2, GPIO_STRENGTH_2MA,
			GPIO_PIN_TYPE_STD);

	// OUT_05 X8.06 PWM5
	GPIOPinTypeGPIOOutput(GPIO_PORTF_BASE, GPIO_PIN_3);
	GPIOPadConfigSet(GPIO_PORTF_BASE, GPIO_PIN_3, GPIO_STRENGTH_2MA,
			GPIO_PIN_TYPE_STD);

	// OUT_06 X8.07 CCP0
	GPIOPinTypeGPIOOutput(GPIO_PORTB_BASE, GPIO_PIN_0);
	GPIOPadConfigSet(GPIO_PORTB_BASE, GPIO_PIN_0, GPIO_STRENGTH_2MA,
			GPIO_PIN_TYPE_STD);

	// OUT_07 X8.08 CCP1
	GPIOPinTypeGPIOOutput(GPIO_PORTF_BASE, GPIO_PIN_6);
	GPIOPadConfigSet(GPIO_PORTF_BASE, GPIO_PIN_6, GPIO_STRENGTH_2MA,
			GPIO_PIN_TYPE_STD);

	// OUT_10 X8.09 CCP2
	GPIOPinTypeGPIOOutput(GPIO_PORTB_BASE, GPIO_PIN_1);
	GPIOPadConfigSet(GPIO_PORTB_BASE, GPIO_PIN_1, GPIO_STRENGTH_2MA,
			GPIO_PIN_TYPE_STD);

	// OUT_11 X8.10 CCP3
	GPIOPinTypeGPIOOutput(GPIO_PORTG_BASE, GPIO_PIN_4);
	GPIOPadConfigSet(GPIO_PORTG_BASE, GPIO_PIN_4, GPIO_STRENGTH_2MA,
			GPIO_PIN_TYPE_STD);

	// OUT_12 X8.11
	GPIOPinTypeGPIOOutput(GPIO_PORTF_BASE, GPIO_PIN_5);
	GPIOPadConfigSet(GPIO_PORTF_BASE, GPIO_PIN_5, GPIO_STRENGTH_2MA,
			GPIO_PIN_TYPE_STD);

	// OUT_13 X8.12
	GPIOPinTypeGPIOOutput(GPIO_PORTH_BASE, GPIO_PIN_3);
	GPIOPadConfigSet(GPIO_PORTH_BASE, GPIO_PIN_3, GPIO_STRENGTH_2MA,
			GPIO_PIN_TYPE_STD);

	// -------------------------------------------------------------
	// X9
	// -------------------------------------------------------------

	// OUT_14 X9.01
	GPIOPinTypeGPIOOutput(GPIO_PORTH_BASE, GPIO_PIN_2);
	GPIOPadConfigSet(GPIO_PORTH_BASE, GPIO_PIN_2, GPIO_STRENGTH_2MA,
			GPIO_PIN_TYPE_STD);

	// OUT_15 X9.02
	GPIOPinTypeGPIOOutput(GPIO_PORTB_BASE, GPIO_PIN_6);
	GPIOPadConfigSet(GPIO_PORTB_BASE, GPIO_PIN_6, GPIO_STRENGTH_2MA,
			GPIO_PIN_TYPE_STD);

	// OUT_16 X9.03
	GPIOPinTypeGPIOOutput(GPIO_PORTB_BASE, GPIO_PIN_5);
	GPIOPadConfigSet(GPIO_PORTB_BASE, GPIO_PIN_5, GPIO_STRENGTH_2MA,
			GPIO_PIN_TYPE_STD);

	// OUT_17 X9.04
	GPIOPinTypeGPIOOutput(GPIO_PORTB_BASE, GPIO_PIN_4);
	GPIOPadConfigSet(GPIO_PORTB_BASE, GPIO_PIN_4, GPIO_STRENGTH_2MA,
			GPIO_PIN_TYPE_STD);
}

//*****************************************************************************
// Set, Reset i Get - Wyjœcia
//*****************************************************************************

// ----------------------------------------------------------------------------
// Q00
// ----------------------------------------------------------------------------
void SetHard_Q00(void)
{
	GPIOPinWrite(GPIO_PORTG_BASE, GPIO_PIN_2, GPIO_PIN_2);
}

void ResetHard_Q00(void)
{
	GPIOPinWrite(GPIO_PORTG_BASE, GPIO_PIN_2, 0);
}

unsigned char GetHard_Q00(void)
{
	return GPIOPinRead(GPIO_PORTG_BASE, GPIO_PIN_2) >> 2;
}

// ----------------------------------------------------------------------------
// Q01
// ----------------------------------------------------------------------------
void SetHard_Q01(void)
{
	GPIOPinWrite(GPIO_PORTD_BASE, GPIO_PIN_1, GPIO_PIN_1);
}

void ResetHard_Q01(void)
{
	GPIOPinWrite(GPIO_PORTD_BASE, GPIO_PIN_1, 0);
}

unsigned char GetHard_Q01(void)
{
	return GPIOPinRead(GPIO_PORTD_BASE, GPIO_PIN_1) >> 0;
}

// ----------------------------------------------------------------------------
// Q02
// ----------------------------------------------------------------------------
void SetHard_Q02(void)
{
	GPIOPinWrite(GPIO_PORTH_BASE, GPIO_PIN_0, GPIO_PIN_0);
}

void ResetHard_Q02(void)
{
	GPIOPinWrite(GPIO_PORTH_BASE, GPIO_PIN_0, 0);
}

unsigned char GetHard_Q02(void)
{
	return GPIOPinRead(GPIO_PORTH_BASE, GPIO_PIN_0) << 2;
}

// ----------------------------------------------------------------------------
// Q03
// ----------------------------------------------------------------------------
void SetHard_Q03(void)
{
	GPIOPinWrite(GPIO_PORTH_BASE, GPIO_PIN_1, GPIO_PIN_1);
}

void ResetHard_Q03(void)
{
	GPIOPinWrite(GPIO_PORTH_BASE, GPIO_PIN_1, 0);
}

unsigned char GetHard_Q03(void)
{
	return GPIOPinRead(GPIO_PORTH_BASE, GPIO_PIN_1) << 2;
}

// ----------------------------------------------------------------------------
// Q04
// ----------------------------------------------------------------------------
void SetHard_Q04(void)
{
	GPIOPinWrite(GPIO_PORTF_BASE, GPIO_PIN_2, GPIO_PIN_2);
}

void ResetHard_Q04(void)
{
	GPIOPinWrite(GPIO_PORTF_BASE, GPIO_PIN_2, 0);
}

unsigned char GetHard_Q04(void)
{
	return GPIOPinRead(GPIO_PORTF_BASE, GPIO_PIN_2) << 2;
}

// ----------------------------------------------------------------------------
// Q05
// ----------------------------------------------------------------------------
void SetHard_Q05(void)
{
	GPIOPinWrite(GPIO_PORTF_BASE, GPIO_PIN_3, GPIO_PIN_3);
}

void ResetHard_Q05(void)
{
	GPIOPinWrite(GPIO_PORTF_BASE, GPIO_PIN_3, 0);
}

unsigned char GetHard_Q05(void)
{
	return GPIOPinRead(GPIO_PORTF_BASE, GPIO_PIN_3) << 2;
}

// ----------------------------------------------------------------------------
// Q06
// ----------------------------------------------------------------------------
void SetHard_Q06(void)
{
	GPIOPinWrite(GPIO_PORTB_BASE, GPIO_PIN_0, GPIO_PIN_0);
}

void ResetHard_Q06(void)
{
	GPIOPinWrite(GPIO_PORTB_BASE, GPIO_PIN_0, 0);
}

unsigned char GetHard_Q06(void)
{
	return GPIOPinRead(GPIO_PORTB_BASE, GPIO_PIN_0) << 6;
}

// ----------------------------------------------------------------------------
// Q07
// ----------------------------------------------------------------------------
void SetHard_Q07(void)
{
	GPIOPinWrite(GPIO_PORTF_BASE, GPIO_PIN_6, GPIO_PIN_6);
}

void ResetHard_Q07(void)
{
	GPIOPinWrite(GPIO_PORTF_BASE, GPIO_PIN_6, 0);
}

unsigned char GetHard_Q07(void)
{
	return GPIOPinRead(GPIO_PORTF_BASE, GPIO_PIN_6) << 1;
}

// ----------------------------------------------------------------------------
// Q10
// ----------------------------------------------------------------------------
void SetHard_Q10(void)
{
	GPIOPinWrite(GPIO_PORTB_BASE, GPIO_PIN_1, GPIO_PIN_1);
}

void ResetHard_Q10(void)
{
	GPIOPinWrite(GPIO_PORTB_BASE, GPIO_PIN_1, 0);
}

unsigned char GetHard_Q10(void)
{
	return GPIOPinRead(GPIO_PORTB_BASE, GPIO_PIN_1) >> 1;
}

// ----------------------------------------------------------------------------
// Q11
// ----------------------------------------------------------------------------
void SetHard_Q11(void)
{
	GPIOPinWrite(GPIO_PORTG_BASE, GPIO_PIN_4, GPIO_PIN_4);
}

void ResetHard_Q11(void)
{
	GPIOPinWrite(GPIO_PORTG_BASE, GPIO_PIN_4, 0);
}

unsigned char GetHard_Q11(void)
{
	return GPIOPinRead(GPIO_PORTG_BASE, GPIO_PIN_4) >> 3;
}

// ----------------------------------------------------------------------------
// Q12
// ----------------------------------------------------------------------------
void SetHard_Q12(void)
{
	GPIOPinWrite(GPIO_PORTF_BASE, GPIO_PIN_5, GPIO_PIN_5);
}

void ResetHard_Q12(void)
{
	GPIOPinWrite(GPIO_PORTF_BASE, GPIO_PIN_5, 0);
}

unsigned char GetHard_Q12(void)
{
	return GPIOPinRead(GPIO_PORTF_BASE, GPIO_PIN_5) >> 3;
}

// ----------------------------------------------------------------------------
// Q13
// ----------------------------------------------------------------------------
void SetHard_Q13(void)
{
	GPIOPinWrite(GPIO_PORTH_BASE, GPIO_PIN_3, GPIO_PIN_3);
}

void ResetHard_Q13(void)
{
	GPIOPinWrite(GPIO_PORTH_BASE, GPIO_PIN_3, 0);
}

unsigned char GetHard_Q13(void)
{
	return GPIOPinRead(GPIO_PORTH_BASE, GPIO_PIN_3) >> 0;
}

// ----------------------------------------------------------------------------
// Q14
// ----------------------------------------------------------------------------
void SetHard_Q14(void)
{
	GPIOPinWrite(GPIO_PORTH_BASE, GPIO_PIN_2, GPIO_PIN_2);
}

void ResetHard_Q14(void)
{
	GPIOPinWrite(GPIO_PORTH_BASE, GPIO_PIN_2, 0);
}

unsigned char GetHard_Q14(void)
{
	return GPIOPinRead(GPIO_PORTH_BASE, GPIO_PIN_2) << 2;
}

// ----------------------------------------------------------------------------
// Q15
// ----------------------------------------------------------------------------
void SetHard_Q15(void)
{
	GPIOPinWrite(GPIO_PORTB_BASE, GPIO_PIN_6, GPIO_PIN_6);
}

void ResetHard_Q15(void)
{
	GPIOPinWrite(GPIO_PORTB_BASE, GPIO_PIN_6, 0);
}

unsigned char GetHard_Q15(void)
{
	return GPIOPinRead(GPIO_PORTB_BASE, GPIO_PIN_6) >> 1;
}

// ----------------------------------------------------------------------------
// Q16
// ----------------------------------------------------------------------------
void SetHard_Q16(void)
{
	GPIOPinWrite(GPIO_PORTB_BASE, GPIO_PIN_5, GPIO_PIN_5);
}

void ResetHard_Q16(void)
{
	GPIOPinWrite(GPIO_PORTB_BASE, GPIO_PIN_5, 0);
}

unsigned char GetHard_Q16(void)
{
	return GPIOPinRead(GPIO_PORTB_BASE, GPIO_PIN_5) << 1;
}

// ----------------------------------------------------------------------------
// Q17
// ----------------------------------------------------------------------------
void SetHard_Q17(void)
{
	GPIOPinWrite(GPIO_PORTB_BASE, GPIO_PIN_4, GPIO_PIN_4);
}

void ResetHard_Q17(void)
{
	GPIOPinWrite(GPIO_PORTB_BASE, GPIO_PIN_4, 0);
}

unsigned char GetHard_Q17(void)
{
	return GPIOPinRead(GPIO_PORTB_BASE, GPIO_PIN_4) << 3;
}

//*****************************************************************************
// Obraz wejœæ wyjœæ
//*****************************************************************************

unsigned char GetHard_I0(void)
{
	return GetHard_I00() + GetHard_I01() + GetHard_I02() + GetHard_I03()
			+ GetHard_I04() + GetHard_I05() + GetHard_I06() + GetHard_I07();
}

unsigned char GetHard_I1(void)
{
	return GetHard_I10() + GetHard_I11() + GetHard_I12() + GetHard_I13()
			+ GetHard_I14() + GetHard_I15() + GetHard_I16() + GetHard_I17();
}

unsigned char GetHard_Q0(void)
{
	return GetHard_Q00() + GetHard_Q01() + GetHard_Q02() + GetHard_Q03()
			+ GetHard_Q04() + GetHard_Q05() + GetHard_Q06() + GetHard_Q07();
}

unsigned char GetHard_Q1(void)
{
	return GetHard_Q10() + GetHard_Q11() + GetHard_Q12() + GetHard_Q13()
			+ GetHard_Q14() + GetHard_Q15() + GetHard_Q16() + GetHard_Q17();
}

void SetHard_Q0(byte out)
{
	/*
	 * Impuls zgrzewaj¹cy
	 */
	if ((out & 0x01) > 0)
		SetHard_Q00();
	else
		ResetHard_Q00();

	/*
	 * MV1
	 */
	if ((out & 0x02) > 0)
		SetHard_Q01();
	else
		ResetHard_Q01();

	/*
	 * MV2
	 */
	if ((out & 0x04) > 0)
		SetHard_Q02();
	else
		ResetHard_Q02();

	/*
	 * MV3
	 */
	if ((out & 0x08) > 0)
		SetHard_Q03();
	else
		ResetHard_Q03();

	/*
	 * MV4
	 */
	if ((out & 0x10) > 0)
		SetHard_Q04();
	else
		ResetHard_Q04();

	/*
	 * ¯¹danie na wyblokowanie
	 */
	if ((out & 0x20) > 0)
		SetHard_Q05();
	else
		ResetHard_Q05();

	/*
	 * VZ - przep³yw pr¹du
	 */
	if ((out & 0x40) > 0)
		SetHard_Q06();
	else
		ResetHard_Q06();

	/*
	 * FK
	 */
	if ((out & 0x80) > 0)
		SetHard_Q07();
	else
		ResetHard_Q07();
}

void SetHard_Q1(unsigned char out)
{
	if ((out & 0x01) > 0)
		SetHard_Q10();
	else
		ResetHard_Q10();

	if ((out & 0x02) > 0)
		SetHard_Q11();
	else
		ResetHard_Q11();

	if ((out & 0x04) > 0)
		SetHard_Q12();
	else
		ResetHard_Q12();

	if ((out & 0x08) > 0)
		SetHard_Q13();
	else
		ResetHard_Q13();

	if ((out & 0x10) > 0)
		SetHard_Q14();
	else
		ResetHard_Q14();

	if ((out & 0x20) > 0)
		SetHard_Q15();
	else
		ResetHard_Q15();

	if ((out & 0x40) > 0)
		SetHard_Q16();
	else
		ResetHard_Q16();

	if ((out & 0x80) > 0)
		SetHard_Q17();
	else
		ResetHard_Q17();
}

//*****************************************************************************
// Obs³uga SPI
//*****************************************************************************

void SPI_Init(void)
{
	// CS IO1
	GPIOPinTypeGPIOOutput(GPIO_PORTB_BASE, GPIO_PIN_3);
	GPIOPadConfigSet(GPIO_PORTB_BASE, GPIO_PIN_3, GPIO_STRENGTH_2MA,
			GPIO_PIN_TYPE_STD);
	GPIOPinWrite(GPIO_PORTB_BASE, GPIO_PIN_3, GPIO_PIN_3);

	// CS IO2
	GPIOPinTypeGPIOOutput(GPIO_PORTB_BASE, GPIO_PIN_2);
	GPIOPadConfigSet(GPIO_PORTB_BASE, GPIO_PIN_2, GPIO_STRENGTH_2MA,
			GPIO_PIN_TYPE_STD);
	GPIOPinWrite(GPIO_PORTB_BASE, GPIO_PIN_2, GPIO_PIN_2);

	// CS DAC
	GPIOPinTypeGPIOOutput(GPIO_PORTE_BASE, GPIO_PIN_1);
	GPIOPadConfigSet(GPIO_PORTE_BASE, GPIO_PIN_1, GPIO_STRENGTH_2MA,
			GPIO_PIN_TYPE_STD);
	GPIOPinWrite(GPIO_PORTE_BASE, GPIO_PIN_1, GPIO_PIN_1);

	// CONFIG SPI
	SysCtlPeripheralEnable(SYSCTL_PERIPH_SSI1);
	GPIOPinTypeSSI(GPIO_PORTE_BASE, GPIO_PIN_0 + GPIO_PIN_2 + GPIO_PIN_3);
	SSIConfigSetExpClk(SSI1_BASE, SysCtlClockGet(), SSI_FRF_MOTO_MODE_0,
			SSI_MODE_MASTER, 200000, 16);
	SSIEnable(SSI1_BASE);

	// KONFIGURACJA IO
	// PORTA JAKO WEJSCIA
	Delay(10000);
	SPI_EmptyFIFO();
	GPIOPinWrite(GPIO_PORTB_BASE, GPIO_PIN_2, 0);
	SSIDataPut(SSI1_BASE, 0x4000);
	SSIDataPut(SSI1_BASE, 0xff00);
	SPI_Wait();
	GPIOPinWrite(GPIO_PORTB_BASE, GPIO_PIN_2, GPIO_PIN_2);

	// PORTB JAKO WYJSCIA
	Delay(10000);
	SPI_EmptyFIFO();
	GPIOPinWrite(GPIO_PORTB_BASE, GPIO_PIN_2, 0);
	SSIDataPut(SSI1_BASE, 0x4001);
	SSIDataPut(SSI1_BASE, 0x0000);
	SPI_Wait();
	GPIOPinWrite(GPIO_PORTB_BASE, GPIO_PIN_2, GPIO_PIN_2);

	// PORTA JAKO WEJSCIA
	Delay(10000);
	SPI_EmptyFIFO();
	GPIOPinWrite(GPIO_PORTB_BASE, GPIO_PIN_3, 0);
	SSIDataPut(SSI1_BASE, 0x4000);
	SSIDataPut(SSI1_BASE, 0xff00);
	SPI_Wait();
	GPIOPinWrite(GPIO_PORTB_BASE, GPIO_PIN_3, GPIO_PIN_3);

	// PORTB JAKO WEJSCIA
	Delay(10000);
	SPI_EmptyFIFO();
	GPIOPinWrite(GPIO_PORTB_BASE, GPIO_PIN_3, 0);
	SSIDataPut(SSI1_BASE, 0x4001);
	SSIDataPut(SSI1_BASE, 0xff00);
	SPI_Wait();
	GPIOPinWrite(GPIO_PORTB_BASE, GPIO_PIN_3, GPIO_PIN_3);
}

//*****************************************************************************
// Obs³uga SPI
//*****************************************************************************

void SPI_Wait()
{
	while (SSIBusy(SSI1_BASE))
	{
	}
}

//*****************************************************************************
void SPI_EmptyFIFO()
{
	unsigned int x;
	unsigned long dummy;

	while (HWREG(SSI1_BASE + SSI_O_SR) & SSI_SR_RNE)
	{
		dummy = HWREG(SSI1_BASE + SSI_O_DR);
	}

	for (x = 0; x < 8; x++)
	{
		dummy = HWREG(SSI1_BASE + SSI_O_DR);
	}
}

//*****************************************************************************
void SPI_DAC(unsigned int dac_a, unsigned int dac_b)
{
	// DAC A
	//Delay(10000);
	SPI_EmptyFIFO();
	GPIOPinWrite(GPIO_PORTE_BASE, GPIO_PIN_1, 0);
	SSIDataPut(SSI1_BASE, 0x3000 + dac_a);
	SPI_Wait();
	GPIOPinWrite(GPIO_PORTE_BASE, GPIO_PIN_1, GPIO_PIN_1);

	// DAC B
	//Delay(10000);
	SPI_EmptyFIFO();
	GPIOPinWrite(GPIO_PORTE_BASE, GPIO_PIN_1, 0);
	SSIDataPut(SSI1_BASE, 0xb000 + dac_b);
	SPI_Wait();
	GPIOPinWrite(GPIO_PORTE_BASE, GPIO_PIN_1, GPIO_PIN_1);
	SPI_EmptyFIFO();
}

// port a wejœcia
// port b wyjœcia
// 1 input
// 0 output
// 0X00 IODIRA
// 0X01 IODIRB
// 0X12 GPIOA
// 0X13 GPIOB

// 1 BAJT - OPCODE ZAWSZE NAJPIERW = 0x40 zapis, 0x41 odczyt
// 2 BAJT - ADRES REJESTRU NP. 0X00 IODIRA
// 3 BAJT - ZAPIS DANYCH LUB ODCZYT

//*****************************************************************************
void SPI_Outputs(byte out2)
{
	// WYJSCIA NA MODU£ IO2
	Delay(10000);
	SPI_EmptyFIFO();
	GPIOPinWrite(GPIO_PORTB_BASE, GPIO_PIN_2, 0);
	SSIDataPut(SSI1_BASE, 0x4013);
	SSIDataPut(SSI1_BASE, out2 << 8);
	SPI_Wait();
	GPIOPinWrite(GPIO_PORTB_BASE, GPIO_PIN_2, GPIO_PIN_2);
	SPI_EmptyFIFO();
}

//*****************************************************************************
void SPI_Inputs(unsigned char* in2, unsigned char* in3, unsigned char* in4)
{
	unsigned int x;
	unsigned int fifo[8];

	for (x = 0; x < 8; x++)
		fifo[x] = 0;

	// WEJSCIA Z MODU£U IO2
	// --------------------------------------------------

	//Delay(10000);
	//SPI_EmptyFIFO();
	GPIOPinWrite(GPIO_PORTB_BASE, GPIO_PIN_2, 0);
	SSIDataPut(SSI1_BASE, 0x4112);
	SSIDataPut(SSI1_BASE, 0x0000);
	SPI_Wait();
	GPIOPinWrite(GPIO_PORTB_BASE, GPIO_PIN_2, GPIO_PIN_2);

	for (x = 0; x < 8; x++)
		fifo[x] = HWREG(SSI1_BASE + SSI_O_DR);
	/*
	 for (x = 0; x < 8; x++)
	 UARTprintf("%04x.", fifo[x]);
	 UARTprintf("\n");
	 */

	*in2 = (unsigned char) ((fifo[1] >> 8) & 0xff);

	// WEJSCIA Z MODU£U IO1
	// --------------------------------------------------

	//Delay(10000);
	//SPI_EmptyFIFO();
	GPIOPinWrite(GPIO_PORTB_BASE, GPIO_PIN_3, 0);
	SSIDataPut(SSI1_BASE, 0x4112);
	SSIDataPut(SSI1_BASE, 0x0000);
	SPI_Wait();
	GPIOPinWrite(GPIO_PORTB_BASE, GPIO_PIN_3, GPIO_PIN_3);

	for (x = 0; x < 8; x++)
		fifo[x] = HWREG(SSI1_BASE + SSI_O_DR);
	/*
	 for (x = 0; x < 8; x++)
	 UARTprintf("%04x.", fifo[x]);
	 UARTprintf("\n\n");
	 */

	*in3 = (unsigned char) ((fifo[1] >> 8) & 0xff);
	*in4 = (unsigned char) (fifo[1] & 0xff);

	//SPI_EmptyFIFO();
}

//*****************************************************************************
// Obs³uga ADC
//*****************************************************************************

//*****************************************************************************
// Inicjacja - wersja pierwsza
//*****************************************************************************

void ADC_Init(void)
{
	SysCtlPeripheralEnable(SYSCTL_PERIPH_ADC0);

	//SysCtlADCSpeedSet(SYSCTL_ADCSPEED_1MSPS);

	//ADCHardwareOversampleConfigure(ADC0_BASE, 2);

	// *******************************************************************
	// Konfiguracja pomiaru pr¹du i napiêcia
	// Sekwenser nr1, trigger softwarewy, priorytet 1
	// *******************************************************************
	ADCSequenceConfigure(ADC0_BASE, 1, ADC_TRIGGER_PROCESSOR, 1);
	// Sekwencja nr1:
	// Krok 0: ADC0
	ADCSequenceStepConfigure(ADC0_BASE, 1, 0, ADC_CTL_CH0);
	// Krok 1: ADC1 + END
	ADCSequenceStepConfigure(ADC0_BASE, 1, 1, ADC_CTL_CH1 | ADC_CTL_END);
	// W³¹czenie sekwencji nr 1
	ADCSequenceEnable(ADC0_BASE, 1);

	// *******************************************************************
	// Konfiguracja pomiaru AIN i temperatury procka
	// Sekwenser nr2, trigger softwarewy, priorytet 0
	// *******************************************************************
	ADCSequenceConfigure(ADC0_BASE, 2, ADC_TRIGGER_PROCESSOR, 0);
	// Sekwencja nr2:
	// Krok 0: ADC2
	ADCSequenceStepConfigure(ADC0_BASE, 2, 0, ADC_CTL_CH2);
	// Krok 1: ADC3
	ADCSequenceStepConfigure(ADC0_BASE, 2, 1, ADC_CTL_CH3);
	// Krok 2: ADC4
	ADCSequenceStepConfigure(ADC0_BASE, 2, 2, ADC_CTL_CH4);
	// Krok 4: Temp
	ADCSequenceStepConfigure(ADC0_BASE, 2, 3, ADC_CTL_TS | ADC_CTL_END);
	// W³¹czenie sekwencji nr 2
	ADCSequenceEnable(ADC0_BASE, 2);
}

//*****************************************************************************
// Inicjacja - wersja druga
//*****************************************************************************

void ADC_Init2(void)
{
	SysCtlPeripheralEnable(SYSCTL_PERIPH_ADC0);
	SysCtlADCSpeedSet(SYSCTL_ADCSPEED_1MSPS);

	// *******************************************************************
	// Konfiguracja analogów - pomiar i reszta razem
	// Sekwenser nr0, trigger softwarewy, priorytet 0
	// *******************************************************************
	ADCSequenceConfigure(ADC0_BASE, 0, ADC_TRIGGER_PROCESSOR, 0);
	// Sekwencja nr1:
	// Krok 0: ADC0
	ADCSequenceStepConfigure(ADC0_BASE, 0, 0, ADC_CTL_CH0);
	// Krok 1: ADC1
	ADCSequenceStepConfigure(ADC0_BASE, 0, 1, ADC_CTL_CH1);
	// Krok 2: ADC2
	ADCSequenceStepConfigure(ADC0_BASE, 0, 2, ADC_CTL_CH2);
	// Krok 3: ADC3
	ADCSequenceStepConfigure(ADC0_BASE, 0, 3, ADC_CTL_CH3);
	// Krok 4: ADC4
	ADCSequenceStepConfigure(ADC0_BASE, 0, 4, ADC_CTL_CH4);
	// Krok 5: Temp
	ADCSequenceStepConfigure(ADC0_BASE, 0, 5, ADC_CTL_TS | ADC_CTL_END);

	// W³¹czenie sekwencji nr 1
	ADCSequenceEnable(ADC0_BASE, 0);

}

//*****************************************************************************
// Inicjacja - wersja trzecia
//*****************************************************************************

void ADC_Init3(void)
{
	SysCtlPeripheralEnable(SYSCTL_PERIPH_ADC0);

	// *******************************************************************
	// Sekwenser nr1, trigger softwarewy, priorytet 0
	// *******************************************************************
	ADCSequenceConfigure(ADC0_BASE, 1, ADC_TRIGGER_PROCESSOR, 0);
	ADCSequenceStepConfigure(ADC0_BASE, 1, 0, ADC_CTL_CH0);
	ADCSequenceStepConfigure(ADC0_BASE, 1, 1, ADC_CTL_CH1);
	ADCSequenceStepConfigure(ADC0_BASE, 1, 2, ADC_CTL_CH2);
	ADCSequenceStepConfigure(ADC0_BASE, 1, 3, ADC_CTL_CH3 | ADC_CTL_END);
	ADCSequenceEnable(ADC0_BASE, 1);

	// *******************************************************************
	// Sekwenser nr2, trigger softwarewy, priorytet 0
	// *******************************************************************
	ADCSequenceConfigure(ADC0_BASE, 2, ADC_TRIGGER_PROCESSOR, 0);
	ADCSequenceStepConfigure(ADC0_BASE, 2, 0, ADC_CTL_CH0);
	ADCSequenceStepConfigure(ADC0_BASE, 2, 1, ADC_CTL_CH1);
	ADCSequenceStepConfigure(ADC0_BASE, 2, 2, ADC_CTL_CH4);
	ADCSequenceStepConfigure(ADC0_BASE, 2, 3, ADC_CTL_TS | ADC_CTL_END);
	ADCSequenceEnable(ADC0_BASE, 2);
}

//*****************************************************************************
// Pomiar i odczyt - wersja trzecia
//*****************************************************************************

void ADC_Start1(void)
{
	ADCProcessorTrigger(ADC0_BASE, 1);
}

void ADC_Start2(void)
{
	ADCProcessorTrigger(ADC0_BASE, 2);
}

void ADC_Odczyt1(unsigned long buf[])
{
	ADCSequenceDataGet(ADC0_BASE, 1, buf);
}

void ADC_Odczyt2(unsigned long buf[])
{
	ADCSequenceDataGet(ADC0_BASE, 2, buf);
}

//*****************************************************************************
// Pomiar i odczyt - wersja druga
//*****************************************************************************

//*****************************************************************************
void ADC_Start(void)
{
	ADCProcessorTrigger(ADC0_BASE, 0);
}

//*****************************************************************************
void ADC_Odczyt(unsigned long buf[])
{
	ADCSequenceDataGet(ADC0_BASE, 0, buf);
}

//*****************************************************************************
void ADC_Pomiar_Start(void)
{
	ADCProcessorTrigger(ADC0_BASE, 1);
}

//*****************************************************************************
void ADC_Pomiar_Odczyt(unsigned long buf[])
{
	ADCSequenceDataGet(ADC0_BASE, 1, buf);
}

//*****************************************************************************
void ADC_AIN_Start(void)
{
	ADCProcessorTrigger(ADC0_BASE, 2);
}

//*****************************************************************************
void ADC_AIN_Odczyt(unsigned long buf[])
{
	ADCSequenceDataGet(ADC0_BASE, 2, buf);
}

//*****************************************************************************
void IO_Test(void)
{
	// Manipulacje
	// -----------------
	//image.aout[0] = 4095; // 0xfff daje 2,048V -> 9,23V
	image.aout[1] = 10000;

	// Ustawienie wyjœæ
	// -----------------
	if (GET_IOTEST)
	{
		image.q[2] = image.i[4] | 0x80;
	}

	SPI_Outputs(image.q[2]);

	if (image.aout[0] > 10000)
		image.aout[0] = 10000;
	if (image.aout[1] > 10000)
		image.aout[1] = 10000;

	if (GET_IOTEST)
		SPI_DAC(3000, 3000);
	else
		SPI_DAC(ANALOG_DAC(image.aout[0]), ANALOG_DAC(image.aout[1]));

	// Odczyt wejœæ
	// -----------------
	SPI_Inputs(&image.i[2], &image.i[3], &image.i[4]);
}

//*****************************************************************************
// Zapalenie i gaszenie diody STATUS
//*****************************************************************************
void LedOn(void)
{
	GPIOPinWrite(GPIO_PORTF_BASE, GPIO_PIN_7, GPIO_PIN_7);
}

void LedOff(void)
{
	GPIOPinWrite(GPIO_PORTF_BASE, GPIO_PIN_7, 0);
}

void LedSwitch(void)
{
	if (GPIOPinRead(GPIO_PORTF_BASE, GPIO_PIN_7) == 0)
		GPIOPinWrite(GPIO_PORTF_BASE, GPIO_PIN_7, GPIO_PIN_7);
	else
		GPIOPinWrite(GPIO_PORTF_BASE, GPIO_PIN_7, 0);
}

char Status(void)
{
	char x;

	x = GPIOPinRead(GPIO_PORTF_BASE, GPIO_PIN_4);

	if (x > 0)
		return 1;
	else
		return 0;
}

