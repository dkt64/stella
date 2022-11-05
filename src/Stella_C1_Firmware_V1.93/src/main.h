/*
 * main.h
 *
 *  Created on: 2009-06-11
 *      Author: Bartek
 */

#ifndef MAIN_H_
#define MAIN_H_

#ifndef NULL
#define NULL ((void *)0)
#endif

#ifndef null
#define null ((void *)0)
#endif

#define true	1
#define false	0
#define TRUE	1
#define FALSE	0

#define IOSIZE_SM		32
#define IOSIZE_Q		4
#define IOSIZE_AOUT		4
#define IOSIZE_I		6
#define IOSIZE_AIN		6

// Liczba programów licz¹c od 0
#define LICZBA_PROGRAMOW	15

#define POZYCJA_GORNA_CYLINDRA_TOL	200

typedef unsigned char bool;

// 8 bitów char
typedef unsigned char byte;
typedef unsigned char u8;
typedef char sbyte;
// 16 bitów short
typedef unsigned short word;
typedef unsigned short u16;
typedef unsigned short ushort;
typedef short sword;
// 32 bity long
typedef unsigned long dword;
typedef unsigned long u32;
typedef unsigned long ulong;
typedef long sdword;
// 32 bity int
typedef unsigned int uint;

#include "inc/lm3s1968.h"
#include "inc/hw_types.h"
#include "inc/hw_ints.h"
#include "inc/hw_memmap.h"
#include "inc/hw_sysctl.h"
#include "inc/hw_ssi.h"
#include "driver/debug.h"
#include "driver/sysctl.h"
#include "driver/gpio.h"
#include "driver/ssi.h"
#include "driver/uart.h"
#include "driver/systick.h"
#include "driver/interrupt.h"
#include "driver/timer.h"
#include "driver/adc.h"
#include "driver/qei.h"
#include "utils/uartstdio.h"
#include "utils/ustdlib.h"
#include "utils/ringbuf.h"
#include "utils/isqrt.h"
#include "io.h"
#include "qei.h"
#include "comm.h"
#include "proces.h"
#include "tools.h"
#include "modbus.h"

#define us ((word)TimerValueGet(TIMER1_BASE, TIMER_A))

//*****************************************************************************
// 	Definicja WA¯NYCH STA£YCH SYSTEMOWYCH
//*****************************************************************************

#define UART_BUFOR_SIZE 1024

//*****************************************************************************
// 	HARDWARE
//*****************************************************************************

#define ADC_RESOLUTION 	10
#define ADC14_MAX_VALUE	16383

//*****************************************************************************
// 	TICKi
//*****************************************************************************

#define TICK_ALIVE		TICK_1S * 3
#define TICK_DISPLAY	TICK_1S
#define TICK_TEST		TICK_1S * 5
#define TICK_COMM		TICK_100MS * 1
#define TICK_UPDATE		TICK_100MS * 2

//*****************************************************************************
// Zmienne extern
//*****************************************************************************

extern bool connection, aktywacja_programyzaladowane;
extern volatile byte frames;

extern WeldParam par[];
extern WeldParam prog;

extern word rozkaz;
extern word rozkaz_arg;

extern word timer1, timer2;

extern volatile bool semaphore;

extern volatile byte uart0_bufor[UART_BUFOR_SIZE];
extern volatile word uart0_ogon, uart0_glowa;
extern word uart0_bufor_przepelnienie;
extern volatile byte uart1_bufor[UART_BUFOR_SIZE];
extern volatile word uart1_ogon, uart1_glowa;
extern word uart1_bufor_przepelnienie;
extern volatile byte uart2_bufor[UART_BUFOR_SIZE];
extern volatile word uart2_ogon, uart2_glowa;
extern word uart2_bufor_przepelnienie;

extern byte slave_id;
volatile extern MODPACK uart0_modpack;
volatile extern MODPACK uart1_modpack;
volatile extern MODPACK uart2_modpack;

volatile extern Image image;
extern WeldParam par[16];
extern WeldParam prog;
volatile extern unsigned long adc_bufor1[];
volatile extern unsigned long adc_bufor2[];
volatile extern unsigned long adc_bufor[];
volatile extern unsigned long tick;

extern bool wyslano_wyniki;

extern word wykres_prad[];
extern word wykres_nap[];
extern word wykres_wtopienia[WYKRES_TAB_SIZE];
extern word wykres_wzor[];
extern byte pomiar_numer_zmierzonego_polokresu;
extern byte wykres_index;;

volatile extern byte connected_count;

extern word mess1, mess2;

void DrukujParametry(int nr);

extern word pomiar_tab[];
extern byte pomiar_tab_index;
extern byte pomiar_tab_zmierzono;

extern bool wypisz;

extern word RefTabCylinder[16][REF_TAB_SIZE];
extern word RefTabWydmuch[16][REF_TAB_SIZE];
extern byte ref_prad_cnt0;
extern byte ref_nakr_cnt0;

#endif /* MAIN_H_ */
