/*
 * modbus.c
 *
 *  Created on: 2010-02-21
 *      Author: Bartek
 */

#include "main.h"

byte slave_id = 2;
volatile MODPACK uart0_modpack;
volatile MODPACK uart1_modpack;
volatile MODPACK uart2_modpack;

// Synchroznizacja przepisania obrazu funkcji main i irq
volatile bool semaphore = false;

void ModbusInitPack(MODPACK *pack)
{
	pack->address_lo = false;
	pack->byte_count = 0;
	pack->crc_computed = 0;
	//pack->crc_err_counter = 0;
	pack->crc_lo = false;
	//pack->crc_ok_counter = 0;
	pack->crc_received = 0;
	pack->func = 0;
	pack->id = 0;
	pack->len = 0;
	pack->lenm = 0;
	pack->quantity = 0;
	pack->quantity_lo = false;
	pack->start_address = 0;
	pack->status = PARSED_EMPTY;
}

void ModbusPrintPack(MODPACK *pack)
{
	Print("pack->address_lo      = %d \n", pack->address_lo);
	Print("pack->byte_count      = %d \n", pack->byte_count);
	Print("pack->crc_computed    = %d \n", pack->crc_computed);
	Print("pack->crc_err_counter = %d \n", pack->crc_err_counter);
	Print("pack->crc_lo          = %d \n", pack->crc_lo);
	Print("pack->crc_ok_counter  = %d \n", pack->crc_ok_counter);
	Print("pack->crc_received    = %d \n", pack->crc_received);
	Print("pack->func            = %d \n", pack->func);
	Print("pack->id              = %d \n", pack->id);
	Print("pack->len             = %d \n", pack->len);
	Print("pack->lenm            = %d \n", pack->lenm);
	Print("pack->quantity        = %d \n", pack->quantity);
	Print("pack->quantity_lo     = %d \n", pack->quantity_lo);
	Print("pack->start_address   = %d \n", pack->start_address);
	Print("pack->status          = %d \n", pack->status);
}

bool ModbusOK(MODPACK *pack)
{
	if (pack->crc_received == pack->crc_computed)
		return true;
	else
		return false; // false
}

void ModbusCRC(MODPACK *pack)
{
	word CRCFull = 0xFFFF;
	char CRCLSB;
	int i, j;

	//mess1 = us;

	for (i = 0; i < pack->lenm; i++)
	{
		CRCFull = (word) (CRCFull ^ pack->message[i]);

		for (j = 0; j < 8; j++)
		{
			CRCLSB = (char) (CRCFull & 0x0001);
			CRCFull = (word) ((CRCFull >> 1) & 0x7FFF);

			if (CRCLSB == 1)
				CRCFull = (word) (CRCFull ^ 0xA001);
		}
	}

	pack->crc_computed = CRCFull;

	//p_test1 = us - mess1;
}
// $01$10$00$01$00$02$04$ff$ff$ff$ff$91$c9
//   1  2  3  4  5  6  7  8  9  a  b  c  d

// 1 id 		01
// 2 kod 		10
// 3 adr hi 	00
// 4 adr lo		01
// 5 qua hi		00
// 6 qua lo		02
// 7 bytes		04
// 8 val1 hi	ff
// 9 val1 lo	ff
// a val2 hi	ff
// b val2 lo	ff
// c crc hi		91
// d crc lo		c9


WartosciStaleWObrazie()
{
	p_cylinder_tol = POZYCJA_GORNA_CYLINDRA_TOL;
}

//*****************************************************************************
// Przygotowanie odpowiedzi
//*****************************************************************************
bool ModbusResponse(MODPACK *pack)
{
	word i = 0, prog = 0;
	byte *ptr;

	ptr = null;

	//Print("glowa = %d, ogon = %d \n", uart0_glowa, uart0_ogon);


	switch (pack->func)
	{

	// ****************************
	// ZAPIS DO HOLDING REGISTERS
	// ****************************
	case 0x10:

		//Print("Tworze odpowidz na funkcje 0x10");
		// ZAPIS BAJTÓW Z RAMKI MODBUS NA OBRAZ LUB PARAMETRY


		// adres 0 to Image
		if ((pack->start_address == 0x0) && (pack->len <= sizeof(Image)))
		{
			//Print("Ustawienie w IO \n");
			ptr = (byte*) &image;
		}

		// adres 1000 to Parametry
		if ((pack->start_address >= 0x1000) && (pack->start_address <= 0x1f00)
				&& (pack->len <= sizeof(WeldParam)))
		{
			prog = (pack->start_address - 0x1000) / 0x100;
			//Print("Ustawienie parametrow programu nr %d, pack->len = %d, pack->start_address = %d \n", prog, pack->len, pack->start_address);
			ptr = (byte*) &par[prog];

			if (prog == 15)
				aktywacja_programyzaladowane = true;
		}

		if (ptr != null)
		{
			//Print("len = %d \n", pack->len);

			//Print("Adres tablicy z parametrami = %d \n", ptr);

			//
			// !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
			// Synchronizacja wpisania obrazu z przerwaniem
			//
			semaphore = true;
			while (semaphore == true)
			{
			}
			//
			// !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
			//

			for (i = 0; i < pack->len; i++)
			{
				*ptr++ = pack->buf[i];
			}

			WartosciStaleWObrazie();

			if (pack->start_address == 0x0)
			{
				if (image.sm[0] != 0)
				{
					rozkaz = image.sm[0];
					rozkaz_arg = image.sm[1];

					//Print("Rozkaz = %d, arg = %d \n", rozkaz, rozkaz_arg);
				}
			}

			// TWORZENIE RAMKI Z ODPOWIEDZI¥
			pack->lenm = 6;

			ModbusCRC(pack);

			pack->message[pack->lenm++] = (byte) (pack->crc_computed);
			pack->message[pack->lenm++] = (byte) (pack->crc_computed >> 8);

			//if ((pack->start_address >= 0x1000) && (pack->start_address
			//		<= 0x1f00))
			//	DrukujParametry(prog);
		}

		break;

		// ****************************
		// ODCZYT HOLDING REGISTERS
		// ****************************
	case 0x03:

		//Print("Tworze odpowidz na funkcje 0x03");
		// TWORZENIE RAMKI Z ODPOWIEDZI¥
		// adres 0 to Image
		if ((pack->start_address == 0x0) && (pack->byte_count <= sizeof(Image)))
		{
			//Print("Odczyt IO \n");
			ptr = (byte*) &image;
		}

		// adres 1000 to Parametry
		if ((pack->start_address >= 0x1000) && (pack->start_address <= 0x1f00)
				&& (pack->byte_count <= sizeof(WeldParam)))
		{
			ptr = (byte*) &par[(pack->start_address - 0x1000) / 0x100];
			wyslano_wyniki = true;
			//Uart1_SendChar('.');
			//Print("Byl odczyt programu startaddress = %d \n", pack->start_address);
			//Print("Odczyt parametrow programu nr %d, pack->byte_count = %d, pack->start_address = %d, prog = %d \n", (pack->start_address - 0x1000) / 0x100, pack->byte_count, pack->start_address, (pack->start_address - 0x1000) / 0x100);
		}

		// adres 5000 Wykres wzorcowy
		if ((pack->start_address == 0x5000) && (pack->byte_count
				<= WYKRES_TAB_SIZE * 2))
		{
			ptr = (byte*) &wykres_wzor[0];
			if ((wykres_index * 2) > WYKRES_TAB_SIZE)
				pack->byte_count = WYKRES_TAB_SIZE;
			else
				pack->byte_count = wykres_index * 2;
		}

		// adres 5100 Wykres pr¹du
		if ((pack->start_address == 0x5100) && (pack->byte_count
				<= WYKRES_TAB_SIZE * 2))
		{
			ptr = (byte*) &wykres_prad[0];
			if ((wykres_index * 2) > WYKRES_TAB_SIZE)
				pack->byte_count = WYKRES_TAB_SIZE;
			else
				pack->byte_count = wykres_index * 2;
		}

		// adres 5200 Wykres napiêcia
		if ((pack->start_address == 0x5200) && (pack->byte_count
				<= WYKRES_TAB_SIZE * 2))
		{
			ptr = (byte*) &wykres_nap[0];
			if ((wykres_index * 2) > WYKRES_TAB_SIZE)
				pack->byte_count = WYKRES_TAB_SIZE;
			else
				pack->byte_count = wykres_index * 2;
		}

		// adres 5200 Wykres wtopienia
		if ((pack->start_address == 0x5300) && (pack->byte_count
				<= WYKRES_TAB_SIZE * 2))
		{
			ptr = (byte*) &wykres_wtopienia[0];
			if ((wykres_index * 2) > WYKRES_TAB_SIZE)
				pack->byte_count = WYKRES_TAB_SIZE;
			else
				pack->byte_count = wykres_index * 2;
		}

		if (ptr != null)
		{
			pack->lenm = 2;
			pack->message[pack->lenm++] = pack->byte_count;

			for (i = 0; i < pack->byte_count; i++)
			{
				pack->message[pack->lenm++] = *ptr;
				ptr++;
			}

			ModbusCRC(pack);

			pack->message[pack->lenm++] = (byte) (pack->crc_computed);
			pack->message[pack->lenm++] = (byte) (pack->crc_computed >> 8);

			//Print("CRC computed = 0x%x(%d) \n", pack->crc_computed, pack->crc_computed);
		}
		break;

	default:
		break;
	}

	//Print("glowa = %d, ogon = %d \n", uart0_glowa, uart0_ogon);

	if (ptr != null)
		return true;
	else
		return false;
}

//*****************************************************************************
// Odbiór dannych Modbus
//*****************************************************************************
void ModbusInput(MODPACK *pack, byte newbyte)
{
	switch (pack->status)
	{

	case PARSED_DATA:
		if (pack->crc_lo == false)
		{
			pack->crc_received = (word) (newbyte);
			pack->crc_lo = true;
		}
		else
		{
			pack->crc_received |= (word) (newbyte << 8);
			pack->status = PARSED_FULL;

			//if (pack->func == 0x10)
			//Print("Parsed full \n");
		}
		break;
		//*****************************************************************************

	case PARSED_BYTECOUNT:
		switch (pack->func)
		{
		case 0x10:
			if ((pack->len < pack->byte_count) && (pack->len < MAX_MODBUSPACK))
			{
				pack->message[pack->lenm++] = newbyte;
				pack->buf[pack->len++] = newbyte;
			}
			if ((pack->len >= pack->byte_count)
					|| (pack->len >= MAX_MODBUSPACK))
			{
				pack->status = PARSED_DATA;
			}
			//Uart1_SendChar('d');
			//Print("len = %d \n", pack->len);
			break;
		default:
			break;
		}
		break;
		//*****************************************************************************

	case PARSED_QUANTITY:
		switch (pack->func)
		{
		case 0x10:
			pack->message[pack->lenm++] = newbyte;
			pack->byte_count = newbyte;
			if (pack->byte_count > MAX_MODBUSPACK)
				pack->byte_count = MAX_MODBUSPACK;

			//ModbusPrintPack(pack);

			pack->status = PARSED_BYTECOUNT;
			break;
		default:
			break;
		}
		break;
		//*****************************************************************************

	case PARSED_ADDRESS:
		switch (pack->func)
		{
		case 0x10:
			if (pack->quantity_lo == false)
			{
				pack->message[pack->lenm++] = newbyte;
				pack->quantity = (word) (newbyte << 8);
				pack->quantity_lo = true;
			}
			else
			{
				pack->message[pack->lenm++] = newbyte;
				pack->quantity |= (word) (newbyte);
				pack->status = PARSED_QUANTITY;

				//pack->byte_count = (byte) ((pack->quantity) * 2);

			}
			break;
		case 0x03:
			if (pack->quantity_lo == false)
			{
				pack->message[pack->lenm++] = newbyte;
				pack->quantity = (word) (newbyte << 8);
				pack->quantity_lo = true;
			}
			else
			{
				pack->message[pack->lenm++] = newbyte;
				pack->quantity |= (word) (newbyte);
				pack->byte_count = (byte) ((pack->quantity) * 2);

				if (pack->byte_count > MAX_MODBUSPACK)
					pack->byte_count = MAX_MODBUSPACK;

				pack->status = PARSED_DATA;
			}
			break;
		default:
			break;
		}
		break;
		//*****************************************************************************

	case PARSED_FUNC:
		if (pack->address_lo == false)
		{
			pack->message[pack->lenm++] = newbyte;
			pack->start_address = (word) (newbyte << 8);
			pack->address_lo = true;
		}
		else
		{
			pack->message[pack->lenm++] = newbyte;
			pack->start_address |= (word) (newbyte);
			pack->status = PARSED_ADDRESS;
		}
		break;
		//*****************************************************************************

	case PARSED_ID:
		if (newbyte == 0x10 || newbyte == 0x03)
		{
			pack->message[pack->lenm++] = newbyte;
			pack->func = newbyte;
			pack->status = PARSED_FUNC;
			//Uart1_SendChar('x');

			//pack->start_address = 0;
			//pack->quantity = 0;
			//pack->byte_count = 0;
			//pack->address_lo = false;
			//pack->quantity_lo = false;
			//pack->crc_lo = false;
		}
		else
		{
			pack->status = PARSED_EMPTY;

			//pack->len = 0;
			//pack->lenm = 0;
		}
		break;
		//*****************************************************************************

	case PARSED_EMPTY:
		if (newbyte == slave_id)
		{
			pack->message[pack->lenm++] = newbyte;
			pack->status = PARSED_ID;
		}
		break;
		//*****************************************************************************

		//case PARSED_FULL:
		//default:
		//pack->len = 0;
		//pack->lenm = 0;
		//pack->status = PARSED_EMPTY;
	}

	if (pack->len > MAX_MODBUSPACK || pack->lenm > MAX_MODBUSMESSAGE)
	{
		//pack->len = 0;
		//pack->lenm = 0;
		pack->status = PARSED_EMPTY;
	}
}

void ModbusService_Uart0(MODPACK *pack)
{
	byte data, counter = 0;

	// Sprawdzamy czy jest cos w buforze
	if (uart0_glowa != uart0_ogon)
	{

		while (uart0_glowa != uart0_ogon && (counter++ < MODBUS_COUNT_READ))
		{
			//LedSwitch();

			// Odczytaujemy bajt do zmiennej
			data = uart0_bufor[uart0_ogon++];

			if (uart0_ogon >= UART_BUFOR_SIZE)
				uart0_ogon = 0;

			// Obsluga wejscia danych modbus
			ModbusInput(pack, data);

			//ModbusPrintPack(pack);
			//Uart1_SendChar('x');

			// Jezeli odebrano wszystko obsluga rozkazu modbus
			if (pack->status == PARSED_FULL)
			{
				frames++;

				// Sprawdzenie CRC
				ModbusCRC(pack);

				/*
				 if (pack->func == 0x10)
				 {
				 ModbusPrintPack(pack);
				 }
				 */

				// Jezeli CRC sie zgadza to mozemy przygotowac odpowiedŸ
				if (ModbusOK(pack) == true)
				{
					/*
					 if (pack->func == 0x10)
					 {
					 Print("CRC OK rozkaz 0x10 \n");
					 }
					 */

					// Zwiekszenie licznika poprawnych odbiorow danych
					pack->crc_ok_counter++;

					// Analizujemy dane i tworzymy odpowiedŸ
					if (ModbusResponse(pack) == true)
					{
						Uart0_SendBytes(pack->message, pack->lenm);

						/*
						 if (pack->func == 0x10)
						 {
						 Uart1_SendChar('.');
						 }
						 */

						/*
						 if (pack->func == 0x10)
						 {
						 Print("Wyslalem: ");

						 Uart1_SendBytes(pack->message, pack->lenm);

						 Print(".");
						 }
						 */
					}
				}
				else
				{
					// Jezeli suma kontrolna sie NIE zgadza to zwikszamy licznik bledych odbiorow
					pack->crc_err_counter++;
				}

				// Inicjacja packietu poprzez usatwienie ststus EMPTY - po ka¿dym odbiorze pe³nego pakietu
				pack->status = PARSED_EMPTY;
			}

			if (pack->status == PARSED_EMPTY)
			{
				ModbusInitPack(pack);
			}
		}
	}
}

void ModbusService_Uart1(MODPACK *pack)
{
	byte data, counter = 0;

	// Sprawdzamy czy jest cos w buforze
	if (uart1_glowa != uart1_ogon)
	{
		while (uart1_glowa != uart1_ogon && (counter++ < MODBUS_COUNT_READ))
		{

			// Odczytaujemy bajt do zmiennej
			data = uart1_bufor[uart1_ogon++];

			if (uart1_ogon >= UART_BUFOR_SIZE)
				uart1_ogon = 0;

			// Obsluga wejscia danych modbus
			ModbusInput(pack, data);

			// Jezeli odebrano wszystko obsluga rozkazu modbus
			if (pack->status == PARSED_FULL)
			{
				frames++;

				// Sprawdzenie CRC
				ModbusCRC(pack);

				// Jezeli CRC sie zgadza to mozemy przygotowac odpowiedŸ
				if (ModbusOK(pack) == true)
				{
					// Zwiekszenie licznika poprawnych odbiorow danych
					pack->crc_ok_counter++;

					// Analizujemy dane i tworzymy odpowiedŸ
					if (ModbusResponse(pack) == true)
					{
						Uart1_SendBytes(pack->message, pack->lenm);
					}
				}
				else
				{
					// Jezeli suma kontrolna sie NIE zgadza to zwikszamy licznik bledych odbiorow
					pack->crc_err_counter++;
				}

				// Inicjacja packietu poprzez usatwienie ststus EMPTY - po ka¿dym odbiorze pe³nego pakietu
				pack->status = PARSED_EMPTY;
			}

			if (pack->status == PARSED_EMPTY)
			{
				ModbusInitPack(pack);
			}
		}
	}
}

void ModbusService_Uart2(MODPACK *pack)
{
	byte data, counter = 0;

	// Sprawdzamy czy jest cos w buforze
	if (uart2_glowa != uart2_ogon)
	{

		while (uart2_glowa != uart2_ogon && (counter++ < MODBUS_COUNT_READ))
		{
			// Odczytaujemy bajt do zmiennej
			data = uart2_bufor[uart2_ogon++];

			if (uart2_ogon >= UART_BUFOR_SIZE)
				uart2_ogon = 0;

			// Obsluga wejscia danych modbus
			ModbusInput(pack, data);

			// Jezeli odebrano wszystko obsluga rozkazu modbus
			if (pack->status == PARSED_FULL)
			{
				frames++;

				// Sprawdzenie CRC
				ModbusCRC(pack);

				// Jezeli CRC sie zgadza to mozemy przygotowac odpowiedŸ
				if (ModbusOK(pack) == true)
				{
					// Zwiekszenie licznika poprawnych odbiorow danych
					pack->crc_ok_counter++;

					// Analizujemy dane i tworzymy odpowiedŸ
					if (ModbusResponse(pack) == true)
					{
						Uart2_SendBytes(pack->message, pack->lenm);
					}
				}
				else
				{
					// Jezeli suma kontrolna sie NIE zgadza to zwikszamy licznik bledych odbiorow
					pack->crc_err_counter++;
				}

				// Inicjacja packietu poprzez usatwienie ststus EMPTY - po ka¿dym odbiorze pe³nego pakietu
				pack->status = PARSED_EMPTY;
			}

			if (pack->status == PARSED_EMPTY)
			{
				ModbusInitPack(pack);
			}
		}
	}
}

