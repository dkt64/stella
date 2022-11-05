/*
 * comm.h
 *
 *  Created on: 2010-02-20
 *      Author: Bartek
 */

#ifndef COMM_H_
#define COMM_H_

#define MAX_UART_STRING 	256
#define UART_COUNT_READ		32

void Uarts_Init(void);
void Uart0_SendBytes(byte *str, byte length);
void Uart1_SendBytes(byte *str, word length);
void Uart2_SendBytes(byte *str, word length);
void Uart0_SendString(char *str);
void Uart1_SendString(char *str);
void Uart2_SendString(char *str);
void Uart0_GetBytes(void);
void Uart1_GetBytes(void);
void Uart2_GetBytes(void);
void Uart0_SendChar(char ch);
void Uart1_SendChar(char ch);
void Uart2_SendChar(char ch);

#endif /* COMM_H_ */
