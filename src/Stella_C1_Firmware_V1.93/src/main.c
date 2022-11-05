//*****************************************************************************
//
// Program sterownika zgrzewania
// Impuls synchronizuj¹cy na zboczu narastaj¹cym
//
// Bardzo wa¿ne - optymalizacja na rozmiar !!!
//
//*****************************************************************************

#include "main.h"

//*****************************************************************************
// Zmienne globalne
//*****************************************************************************

#define VERSION 0x1930

/*
 * 1.92 (2016-10-16) - Naprawa wtopienia
 * 1.93 (2018-04-10) - Naprawa kontroli dynamicznej
 */

//*****************************************************************************
// Zmienne ogólnego przeznaczenia
//*****************************************************************************

char str[100], tempstr[100];
unsigned int x;
word timer1, timer2;
word i;

bool connection = false, aktywacja_programyzaladowane = false;
volatile byte frames = 0;
bool wypisz = false;

//*****************************************************************************
// TICKi
//*****************************************************************************

volatile unsigned long tickTest, tickAlive, tickDisplay, tickComm, tickUpdate;

//*****************************************************************************
// Rozpoznawanie komend przez RS-a
//*****************************************************************************

void Komendy(char *buf)
{
	// Komenda: Gada do mnie panel - odpowiadam mu
	// ------------------------------------------------------------------------
	if (ustrstr(buf, "Tu PC") != NULL)
	{
		Print("Tu sterowniczek\n");
	}
}

//*****************************************************************************
// Drukowanie parametrow na RS-a
//*****************************************************************************
void DrukujParametry(int nr)
{
	Print("Parametry programu nr %d: \n", nr);
	Print("-CzasDociskuWstepnego       = %d \n", par[nr].CzasDociskuWstepnego);
	Print("-PradZgrzewania             = %d \n", par[nr].PradZgrzewania);
	Print("-CzasZgrzewania             = %d \n", par[nr].CzasZgrzewania);
	Print("-PauzaPodgrzewaniaKoncowego = %d \n",
			par[nr].PauzaPodgrzewaniaKoncowego);
	Print("-PradPodgrzewaniaKoncowego  = %d \n",
			par[nr].PradPodgrzewaniaKoncowego);
}

//*****************************************************************************
// Wyci¹gniêcie informacji o systemie i wypisanie ich przez RS-a
//*****************************************************************************

void DrukujInformacje(void)
{
	char sync[] =
	{ 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa,
			0xaa, 0xaa, 0xaa, 0xaa, 0x00 };

	Print(sync);
	Print("\n*** START PROGRAMU *** \n");
	Print("Clock      = %10d [Hz]\n", SysCtlClockGet());
	Print("FlashSize  = %10d [bytes]\n", SysCtlFlashSizeGet());
	Print("SRAMSize   = %10d [bytes]\n", SysCtlSRAMSizeGet());
	Print("ResetCause = %10d \n", SysCtlResetCauseGet());

	Print("Device Id0 =   %08x \n", HWREG(SYSCTL_DID0));
	Print("Device Id1 =   %08x \n", HWREG(SYSCTL_DID1));
	Print("BOR Contr. =   %08x \n", HWREG(SYSCTL_PBORCTL));

	Print("Sizeof image  = %d \n", sizeof(Image));
	Print("Sizeof par    = %d \n", sizeof(WeldParam));
}

//*****************************************************************************
// PROGRAM G£ÓWNY
//*****************************************************************************
int main(void)
{
	int i, j;

	// Ustawienie zegara na 50 MHz
	SysCtlClockSet(SYSCTL_SYSDIV_4 | SYSCTL_USE_PLL | SYSCTL_OSC_MAIN
			| SYSCTL_XTAL_8MHZ);

	// W³¹czenie wszystkich GPIO
	SysCtlPeripheralEnable(SYSCTL_PERIPH_GPIOA);
	SysCtlPeripheralEnable(SYSCTL_PERIPH_GPIOB);
	SysCtlPeripheralEnable(SYSCTL_PERIPH_GPIOC);
	SysCtlPeripheralEnable(SYSCTL_PERIPH_GPIOD);
	SysCtlPeripheralEnable(SYSCTL_PERIPH_GPIOE);
	SysCtlPeripheralEnable(SYSCTL_PERIPH_GPIOF);
	SysCtlPeripheralEnable(SYSCTL_PERIPH_GPIOG);
	SysCtlPeripheralEnable(SYSCTL_PERIPH_GPIOH);

	OutputsInit();
	InputsInit();

	//
	// Stale
	//

	p_version = VERSION;
	p_cylinder_tol = POZYCJA_GORNA_CYLINDRA_TOL;	// 200 mV

	// Inicjacja UARTów
	Uarts_Init();

	// Inicjacja portu diody STATUS
	GPIOPinTypeGPIOOutput(GPIO_PORTF_BASE, GPIO_PIN_7);
	GPIOPadConfigSet(GPIO_PORTG_BASE, GPIO_PIN_7, GPIO_STRENGTH_8MA,
			GPIO_PIN_TYPE_STD);

	LedOff();

	// Inicjacja wejœcia dla przycisku SELECT
	GPIOPinTypeGPIOInput(GPIO_PORTF_BASE, GPIO_PIN_4);
	GPIOPadConfigSet(GPIO_PORTF_BASE, GPIO_PIN_4, GPIO_STRENGTH_2MA,
			GPIO_PIN_TYPE_STD);

	// Pare informacji na RS-a
	DrukujInformacje();

	// Kasuj info o przyczynie ostatniego resetu
	SysCtlResetCauseClear(SYSCTL_CAUSE_LDO | SYSCTL_CAUSE_SW
			| SYSCTL_CAUSE_WDOG | SYSCTL_CAUSE_BOR | SYSCTL_CAUSE_POR
			| SYSCTL_CAUSE_EXT);

	// Inicjacja packietów Modbus'a
	ModbusInitPack(&uart0_modpack);
	ModbusInitPack(&uart1_modpack);
	ModbusInitPack(&uart2_modpack);

	SPI_Init();
	ADC_Init3();

	// Pierwsze odczytanie obrazu
	IO_Test();
	ProcesIO();

	ProcesInit();

	// Inicjacje ticków
	TickInit();
	tickUpdate = tickComm = tickTest = tickAlive = tickDisplay = TICK_ALIVE;

	// W³¹czenie przerwañ
	IntMasterEnable();

	//*****************************************************************************
	// PÊTLA G£ÓWNA WHILE(1)
	//*****************************************************************************

	Print("Main Loop started...\n");


	// Za³¹czenie wyjœæ po uruchomieniu
	image.q[2] |= 0x80;

	while (true)
	{
		IO_Test();

		ModbusService_Uart0(&uart0_modpack);
		ModbusService_Uart1(&uart1_modpack);
		ModbusService_Uart2(&uart2_modpack);

		if (TickElapsed(tickAlive) > TICK_ALIVE)
		{
			if (frames > 1)
				connection = true;
			else
				connection = false;

			frames = 0;

			tickAlive = TickRead();
		}

		/*
		 if (TickElapsed(tickTest) > TICK_TEST)
		 {
		 if (P_Start() == false && pomiar_tab_zmierzono > 0)
		 {
		 Print("Pomiar: \n");
		 for (i = 0; i < pomiar_tab_zmierzono; i++)
		 {
		 Print("%d \n", pomiar_tab[i]);
		 }
		 pomiar_tab_zmierzono = 0;
		 }

		 tickTest = TickRead();
		 }
		 */
	}

}
