/*
 * qei.c
 *
 * Funkcje obs³uguj¹ce enkoder inkrementalny
 *
 *  Created on: 2010-02-20
 *      Author: Bartek
 */

#include "main.h"

//*****************************************************************************
// Funkcja konfiguruj¹ca enkoder 0
//*****************************************************************************

void QEI_Init()
{
	SysCtlPeripheralEnable(SYSCTL_PERIPH_QEI0);

	// IN_00 X4.01 IDX0
	GPIOPinTypeQEI(GPIO_PORTD_BASE, GPIO_PIN_0);
	// IN_01 X4.02 PHA0
	GPIOPinTypeQEI(GPIO_PORTC_BASE, GPIO_PIN_4);
	// IN_02 X4.03 PHB0
	GPIOPinTypeQEI(GPIO_PORTF_BASE, GPIO_PIN_0);

	QEIConfigure(QEI0_BASE, (QEI_CONFIG_CAPTURE_A_B | QEI_CONFIG_RESET_IDX
			| QEI_CONFIG_QUADRATURE | QEI_CONFIG_NO_SWAP), 999);

	QEIEnable(QEI0_BASE);
}

