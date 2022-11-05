//*****************************************************************************
// Obs³uga komunikacji (UART)
//*****************************************************************************

#include "main.h"

volatile byte uart0_bufor[UART_BUFOR_SIZE];
word uart0_bufor_przepelnienie;
volatile word uart0_ogon, uart0_glowa;

volatile byte uart1_bufor[UART_BUFOR_SIZE];
word uart1_bufor_przepelnienie;
volatile word uart1_ogon, uart1_glowa;

volatile byte uart2_bufor[UART_BUFOR_SIZE];
word uart2_bufor_przepelnienie;
volatile word uart2_ogon, uart2_glowa;

#define uart0_baud 115200
#define uart1_baud 115200
#define uart2_baud 115200

//*****************************************************************************
// Inicjacja UARTów
//*****************************************************************************

void Uarts_Init(void)
{
	uart0_ogon = uart0_glowa = uart1_ogon = uart1_glowa = uart2_ogon
			= uart2_glowa = 0;
	uart0_bufor_przepelnienie = uart1_bufor_przepelnienie
			= uart2_bufor_przepelnienie = 0;

	// Konfiguracja pinów
	GPIOPinTypeUART(GPIO_PORTA_BASE, GPIO_PIN_0 | GPIO_PIN_1);
	GPIOPinTypeUART(GPIO_PORTD_BASE, GPIO_PIN_2 | GPIO_PIN_3);
	GPIOPinTypeUART(GPIO_PORTG_BASE, GPIO_PIN_0 | GPIO_PIN_1);
	// Inicjacja wyjœcia dla nadawania RS485
	GPIOPinTypeGPIOOutput(GPIO_PORTG_BASE, GPIO_PIN_3);
	GPIOPadConfigSet(GPIO_PORTG_BASE, GPIO_PIN_3, GPIO_STRENGTH_2MA,
			GPIO_PIN_TYPE_STD);
	GPIOPinWrite(GPIO_PORTG_BASE, GPIO_PIN_3, 0);

	// ******************************* UART 0 ******************************************
	// STDIO na RS232
	SysCtlPeripheralEnable(SYSCTL_PERIPH_UART0);
	UARTConfigSetExpClk(UART0_BASE, SysCtlClockGet(), uart0_baud,
			(UART_CONFIG_WLEN_8 | UART_CONFIG_STOP_ONE | UART_CONFIG_PAR_NONE));
	UARTFIFOEnable(UART0_BASE);
	UARTEnable(UART0_BASE);

	// ******************************* UART 1 ******************************************
	// RS485
	SysCtlPeripheralEnable(SYSCTL_PERIPH_UART1);
	UARTConfigSetExpClk(UART1_BASE, SysCtlClockGet(), uart1_baud,
			(UART_CONFIG_WLEN_8 | UART_CONFIG_STOP_ONE | UART_CONFIG_PAR_NONE));
	UARTFIFOEnable(UART1_BASE);
	UARTEnable(UART1_BASE);

	// ******************************* UART 2 ******************************************
	// RS422
	SysCtlPeripheralEnable(SYSCTL_PERIPH_UART2);
	UARTConfigSetExpClk(UART2_BASE, SysCtlClockGet(), uart2_baud,
			(UART_CONFIG_WLEN_8 | UART_CONFIG_STOP_ONE | UART_CONFIG_PAR_NONE));
	UARTFIFOEnable(UART2_BASE);
	UARTEnable(UART2_BASE);
}

//*****************************************************************************
// Wysy³a znak na Uart'a
//*****************************************************************************
void Uart0_SendChar(char ch)
{
	UARTCharPut(UART0_BASE, ch);
}
void Uart1_SendChar(char ch)
{
	GPIOPinWrite(GPIO_PORTG_BASE, GPIO_PIN_3, GPIO_PIN_3);

	UARTCharPut(UART1_BASE, ch);

	while (UARTBusy(UART1_BASE))
	{
	}
	GPIOPinWrite(GPIO_PORTG_BASE, GPIO_PIN_3, 0);
}
void Uart2_SendChar(char ch)
{
	UARTCharPut(UART2_BASE, ch);
}

//*****************************************************************************
// Wysy³a tablicê o odpowiedniej dlugosci na Uart'a
//*****************************************************************************
void Uart0_SendBytes(byte *str, byte length)
{
	byte x;

	for (x = 0; x < length; x++)
	{
		UARTCharPut(UART0_BASE, str[x]);
	}
}

void Uart1_SendBytes(byte *str, word length)
{
	uint x;

	GPIOPinWrite(GPIO_PORTG_BASE, GPIO_PIN_3, GPIO_PIN_3);

	for (x = 0; x < length; x++)
	{
		UARTCharPut(UART1_BASE, str[x]);
	}
	while (UARTBusy(UART1_BASE))
	{
	}

	GPIOPinWrite(GPIO_PORTG_BASE, GPIO_PIN_3, 0);
}

void Uart2_SendBytes(byte *str, word length)
{
	uint x;

	for (x = 0; x < length; x++)
	{
		UARTCharPut(UART2_BASE, str[x]);
	}
}

//*****************************************************************************
// Wysy³a ³añcuch na Uart'a
//*****************************************************************************
void Uart0_SendString(char *str)
{
	word x = 0;

	while ((*str != '\n') && (*str != 0) && (x++ < MAX_UART_STRING))
	{
		UARTCharPut(UART0_BASE, *str++);
	}

	if (*str == '\n')
	{
		UARTCharPut(UART0_BASE, '\r');
		UARTCharPut(UART0_BASE, '\n');
	}
}

void Uart1_SendString(char *str)
{
	word x = 0;

	GPIOPinWrite(GPIO_PORTG_BASE, GPIO_PIN_3, GPIO_PIN_3);

	while ((*str != '\n') && (*str != 0) && (x++ < MAX_UART_STRING))
	{
		UARTCharPut(UART1_BASE, *str++);
	}

	if (*str == '\n')
	{
		UARTCharPut(UART1_BASE, '\r');
		UARTCharPut(UART1_BASE, '\n');
	}

	while (UARTBusy(UART1_BASE))
	{
	}

	GPIOPinWrite(GPIO_PORTG_BASE, GPIO_PIN_3, 0);
}

void Uart2_SendString(char *str)
{
	word x = 0;

	while ((*str != '\n') && (*str != 0) && (x++ < MAX_UART_STRING))
	{
		UARTCharPut(UART2_BASE, *str++);
	}

	if (*str == '\n')
	{
		UARTCharPut(UART2_BASE, '\r');
		UARTCharPut(UART2_BASE, '\n');
	}
}

//*****************************************************************************
// Odbiór danych z Uart'a do bufora
//*****************************************************************************
void Uart0_GetBytes(void)
{
	long c;
	byte counter = 0;

	if (UARTCharsAvail(UART0_BASE) == true)
	{
		while (UARTCharsAvail(UART0_BASE) == true && (counter++
				< UART_COUNT_READ))
		{
			c = UARTCharGetNonBlocking(UART0_BASE);

			uart0_bufor[uart0_glowa++] = (byte) c;

			LedSwitch();

			//Uart1_SendChar('x');

			if (uart0_glowa >= UART_BUFOR_SIZE)
			{
				uart0_glowa = 0;
				//Print("Przepelnienie na Uart0 \n");
			}
		}
	}
}

void Uart1_GetBytes(void)
{
	long c;
	byte counter = 0;

	if (UARTCharsAvail(UART1_BASE) == true)
	{
		while (UARTCharsAvail(UART1_BASE) == true && (counter++
				< UART_COUNT_READ))
		{
			c = UARTCharGetNonBlocking(UART1_BASE);

			uart1_bufor[uart1_glowa++] = (byte) c;

			LedSwitch();

			if (uart1_glowa >= UART_BUFOR_SIZE)
			{
				uart1_glowa = 0;
			}
		}
	}
}

void Uart2_GetBytes(void)
{
	long c;
	byte counter = 0;

	if (UARTCharsAvail(UART2_BASE) == true)
	{
		while (UARTCharsAvail(UART2_BASE) == true && (counter++
				< UART_COUNT_READ))
		//if (UARTCharsAvail(UART2_BASE) == true)
		{
			c = UARTCharGetNonBlocking(UART2_BASE);

			uart2_bufor[uart2_glowa++] = (byte) c;

			LedSwitch();

			if (uart2_glowa >= UART_BUFOR_SIZE)
			{
				uart2_glowa = 0;
			}
		}
	}
}

