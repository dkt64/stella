/*
 * modbus.h
 *
 *  Created on: 2010-02-21
 *      Author: Bartek
 */

#ifndef MODBUS_H_
#define MODBUS_H_

#define MODBUS_COUNT_READ	32
#define MAX_MODBUSPACK 		256-9
#define MAX_MODBUSMESSAGE	256

#define PARSED_EMPTY 		0
#define PARSED_ID	 		1
#define PARSED_FUNC			2
#define PARSED_ADDRESS		3
#define PARSED_QUANTITY		4
#define PARSED_BYTECOUNT	5
#define PARSED_DATA			6
#define PARSED_FULL			10

typedef struct
{
	byte status; 			// status odebranej ramki
	byte len; 				// aktualna ilo�� bajt�w w tablicy buf
	byte id;				// odebrany id urz�dzenia
	byte func;				// odebrana funkcja
	word start_address;		// adres spod kt�rego chcemy odczytac dane
	word quantity;			// liczba rejestr�w do odczytania
	byte byte_count;			// liczba rejestr�w do odczytania
	byte buf[MAX_MODBUSPACK]; 	// report or command string
	word crc_received;				// odebrany crc
	word crc_computed;				// obliczony crc
	bool address_lo;		// merker
	bool quantity_lo;		// merker
	bool crc_lo;			// merker
	byte message[MAX_MODBUSMESSAGE];	// ca�a wiadomo�� do obliczenia crc
	byte lenm;							// index dla ca�ej wiadomo�ci
	word crc_err_counter;
	word crc_ok_counter;
} MODPACK;

void ModbusInitPack(MODPACK *pack);
bool ModbusOK(MODPACK *pack);
void ModbusCRC(MODPACK *pack);
bool ModbusResponse(MODPACK *pack);
void ModbusInput(MODPACK *pack, byte newbyte);
void ModbusService_Uart0(MODPACK *pack);
void ModbusService_Uart1(MODPACK *pack);
void ModbusService_Uart2(MODPACK *pack);

#endif /* MODBUS_H_ */
