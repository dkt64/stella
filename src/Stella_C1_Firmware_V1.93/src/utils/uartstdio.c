//*****************************************************************************
//
// uartstdio.c - Utility driver to provide simple UART console functions.
//
// Copyright (c) 2007-2009 Texas Instruments Incorporated. All rights reserved.
// Software License Agreement
//
// Texas Instruments (TI) is supplying this software for use solely and
// exclusively on TI's microcontroller products. The software is owned by
// TI and/or its suppliers, and is protected under applicable copyright
// laws. You may not combine this software with "viral" open-source
// software in order to form a larger program.
//
// THIS SOFTWARE IS PROVIDED "AS IS" AND WITH ALL FAULTS.
// NO WARRANTIES, WHETHER EXPRESS, IMPLIED OR STATUTORY, INCLUDING, BUT
// NOT LIMITED TO, IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
// A PARTICULAR PURPOSE APPLY TO THIS SOFTWARE. TI SHALL NOT, UNDER ANY
// CIRCUMSTANCES, BE LIABLE FOR SPECIAL, INCIDENTAL, OR CONSEQUENTIAL
// DAMAGES, FOR ANY REASON WHATSOEVER.
//
// This is part of revision 5450 of the Stellaris Firmware Development Package.
//
//*****************************************************************************

#include <stdarg.h>
#include "../inc/hw_ints.h"
#include "../inc/hw_memmap.h"
#include "../inc/hw_types.h"
#include "../inc/hw_uart.h"
#include "../driver/debug.h"
#include "../driver/interrupt.h"
#include "../driver/rom.h"
#include "../driver/rom_map.h"
#include "../driver/sysctl.h"
#include "../driver/uart.h"
#include "uartstdio.h"
#include "../driver/gpio.h"

//*****************************************************************************
//
// A mapping from an integer between 0 and 15 to its ASCII character
// equivalent.
//
//*****************************************************************************
static const char * const g_pcHex = "0123456789abcdef";

//*****************************************************************************
//
//! Writes a string of characters to the UART output.
//!
//! \param pcBuf points to a buffer containing the string to transmit.
//! \param ulLen is the length of the string to transmit.
//!
//! This function will transmit the string to the UART output.  The number of
//! characters transmitted is determined by the \e ulLen parameter.  This
//! function does no interpretation or translation of any characters.  Since
//! the output is sent to a UART, any LF (/n) characters encountered will be
//! replaced with a CRLF pair.
//!
//! Besides using the \e ulLen parameter to stop transmitting the string, if a
//! null character (0) is encountered, then no more characters will be
//! transmitted and the function will return.
//!
//! In non-buffered mode, this function is blocking and will not return until
//! all the characters have been written to the output FIFO.  In buffered mode,
//! the characters are written to the UART transmit buffer and the call returns
//! immediately.  If insufficient space remains in the transmit buffer,
//! additional characters are discarded.
//!
//! \return Returns the count of characters written.
//
//*****************************************************************************
int UARTwrite(const char *pcBuf, unsigned long ulLen)
{
	unsigned int uIdx;

	//
	// Send the characters
	//
	for (uIdx = 0; uIdx < ulLen; uIdx++)
	{
		//
		// If the character to the UART is \n, then add a \r before it so that
		// \n is translated to \n\r in the output.
		//

		if (pcBuf[uIdx] == '\n')
		{
			UARTCharPut(UART_DEBUG_PORT, '\r');
		}

		//
		// Send the character to the UART output.
		//
		UARTCharPut(UART_DEBUG_PORT, pcBuf[uIdx]);
	}

	//
	// Return the number of characters written.
	//
	return (uIdx);
}

//*****************************************************************************
//
//! A simple UART based printf function supporting \%c, \%d, \%p, \%s, \%u,
//! \%x, and \%X.
//!
//! \param pcString is the format string.
//! \param ... are the optional arguments, which depend on the contents of the
//! format string.
//!
//! This function is very similar to the C library <tt>fprintf()</tt> function.
//! All of its output will be sent to the UART.  Only the following formatting
//! characters are supported:
//!
//! - \%c to print a character
//! - \%d to print a decimal value
//! - \%s to print a string
//! - \%u to print an unsigned decimal value
//! - \%x to print a hexadecimal value using lower case letters
//! - \%X to print a hexadecimal value using lower case letters (not upper case
//! letters as would typically be used)
//! - \%p to print a pointer as a hexadecimal value
//! - \%\% to print out a \% character
//!
//! For \%s, \%d, \%u, \%p, \%x, and \%X, an optional number may reside
//! between the \% and the format character, which specifies the minimum number
//! of characters to use for that value; if preceded by a 0 then the extra
//! characters will be filled with zeros instead of spaces.  For example,
//! ``\%8d'' will use eight characters to print the decimal value with spaces
//! added to reach eight; ``\%08d'' will use eight characters as well but will
//! add zeroes instead of spaces.
//!
//! The type of the arguments after \e pcString must match the requirements of
//! the format string.  For example, if an integer was passed where a string
//! was expected, an error of some kind will most likely occur.
//!
//! \return None.
//
//*****************************************************************************
void Print(const char *pcString, ...)
{
	unsigned long ulIdx, ulValue, ulPos, ulCount, ulBase, ulNeg;
	char *pcStr, pcBuf[16], cFill;
	va_list vaArgP;

	//
	// Check the arguments.
	//
	ASSERT(pcString != 0);

	//
	// Start the varargs processing.
	//
	va_start(vaArgP, pcString);

	GPIOPinWrite(GPIO_PORTG_BASE, GPIO_PIN_3, GPIO_PIN_3);

	//
	// Loop while there are more characters in the string.
	//
	while (*pcString)
	{
		//
		// Find the first non-% character, or the end of the string.
		//
		for (ulIdx = 0; (pcString[ulIdx] != '%') && (pcString[ulIdx] != '\0'); ulIdx++)
		{
		}

		//
		// Write this portion of the string.
		//
		UARTwrite(pcString, ulIdx);

		//
		// Skip the portion of the string that was written.
		//
		pcString += ulIdx;

		//
		// See if the next character is a %.
		//
		if (*pcString == '%')
		{
			//
			// Skip the %.
			//
			pcString++;

			//
			// Set the digit count to zero, and the fill character to space
			// (i.e. to the defaults).
			//
			ulCount = 0;
			cFill = ' ';

			//
			// It may be necessary to get back here to process more characters.
			// Goto's aren't pretty, but effective.  I feel extremely dirty for
			// using not one but two of the beasts.
			//
			again:

			//
			// Determine how to handle the next character.
			//
			switch (*pcString++)
			{
			//
			// Handle the digit characters.
			//
			case '0':
			case '1':
			case '2':
			case '3':
			case '4':
			case '5':
			case '6':
			case '7':
			case '8':
			case '9':
			{
				//
				// If this is a zero, and it is the first digit, then the
				// fill character is a zero instead of a space.
				//
				if ((pcString[-1] == '0') && (ulCount == 0))
				{
					cFill = '0';
				}

				//
				// Update the digit count.
				//
				ulCount *= 10;
				ulCount += pcString[-1] - '0';

				//
				// Get the next character.
				//
				goto again;
			}

				//
				// Handle the %c command.
				//
			case 'c':
			{
				//
				// Get the value from the varargs.
				//
				ulValue = va_arg(vaArgP, unsigned long);

				//
				// Print out the character.
				//
				UARTwrite((char *) &ulValue, 1);

				//
				// This command has been handled.
				//
				break;
			}

				//
				// Handle the %d command.
				//
			case 'd':
			{
				//
				// Get the value from the varargs.
				//
				ulValue = va_arg(vaArgP, unsigned long);

				//
				// Reset the buffer position.
				//
				ulPos = 0;

				//
				// If the value is negative, make it positive and indicate
				// that a minus sign is needed.
				//
				if ((long) ulValue < 0)
				{
					//
					// Make the value positive.
					//
					ulValue = -(long) ulValue;

					//
					// Indicate that the value is negative.
					//
					ulNeg = 1;
				}
				else
				{
					//
					// Indicate that the value is positive so that a minus
					// sign isn't inserted.
					//
					ulNeg = 0;
				}

				//
				// Set the base to 10.
				//
				ulBase = 10;

				//
				// Convert the value to ASCII.
				//
				goto convert;
			}

				//
				// Handle the %s command.
				//
			case 's':
			{
				//
				// Get the string pointer from the varargs.
				//
				pcStr = va_arg(vaArgP, char *);

				//
				// Determine the length of the string.
				//
				for (ulIdx = 0; pcStr[ulIdx] != '\0'; ulIdx++)
				{
				}

				//
				// Write the string.
				//
				UARTwrite(pcStr, ulIdx);

				//
				// Write any required padding spaces
				//
				if (ulCount > ulIdx)
				{
					ulCount -= ulIdx;
					while (ulCount--)
					{
						UARTwrite(" ", 1);
					}
				}
				//
				// This command has been handled.
				//
				break;
			}

				//
				// Handle the %u command.
				//
			case 'u':
			{
				//
				// Get the value from the varargs.
				//
				ulValue = va_arg(vaArgP, unsigned long);

				//
				// Reset the buffer position.
				//
				ulPos = 0;

				//
				// Set the base to 10.
				//
				ulBase = 10;

				//
				// Indicate that the value is positive so that a minus sign
				// isn't inserted.
				//
				ulNeg = 0;

				//
				// Convert the value to ASCII.
				//
				goto convert;
			}

				//
				// Handle the %x and %X commands.  Note that they are treated
				// identically; i.e. %X will use lower case letters for a-f
				// instead of the upper case letters is should use.  We also
				// alias %p to %x.
				//
			case 'x':
			case 'X':
			case 'p':
			{
				//
				// Get the value from the varargs.
				//
				ulValue = va_arg(vaArgP, unsigned long);

				//
				// Reset the buffer position.
				//
				ulPos = 0;

				//
				// Set the base to 16.
				//
				ulBase = 16;

				//
				// Indicate that the value is positive so that a minus sign
				// isn't inserted.
				//
				ulNeg = 0;

				//
				// Determine the number of digits in the string version of
				// the value.
				//
				convert: for (ulIdx = 1; (((ulIdx * ulBase) <= ulValue)
						&& (((ulIdx * ulBase) / ulBase) == ulIdx)); ulIdx
						*= ulBase, ulCount--)
				{
				}

				//
				// If the value is negative, reduce the count of padding
				// characters needed.
				//
				if (ulNeg)
				{
					ulCount--;
				}

				//
				// If the value is negative and the value is padded with
				// zeros, then place the minus sign before the padding.
				//
				if (ulNeg && (cFill == '0'))
				{
					//
					// Place the minus sign in the output buffer.
					//
					pcBuf[ulPos++] = '-';

					//
					// The minus sign has been placed, so turn off the
					// negative flag.
					//
					ulNeg = 0;
				}

				//
				// Provide additional padding at the beginning of the
				// string conversion if needed.
				//
				if ((ulCount > 1) && (ulCount < 16))
				{
					for (ulCount--; ulCount; ulCount--)
					{
						pcBuf[ulPos++] = cFill;
					}
				}

				//
				// If the value is negative, then place the minus sign
				// before the number.
				//
				if (ulNeg)
				{
					//
					// Place the minus sign in the output buffer.
					//
					pcBuf[ulPos++] = '-';
				}

				//
				// Convert the value into a string.
				//
				for (; ulIdx; ulIdx /= ulBase)
				{
					pcBuf[ulPos++] = g_pcHex[(ulValue / ulIdx) % ulBase];
				}

				//
				// Write the string.
				//
				UARTwrite(pcBuf, ulPos);

				//
				// This command has been handled.
				//
				break;
			}

				//
				// Handle the %% command.
				//
			case '%':
			{
				//
				// Simply write a single %.
				//
				UARTwrite(pcString - 1, 1);

				//
				// This command has been handled.
				//
				break;
			}

				//
				// Handle all other commands.
				//
			default:
			{
				//
				// Indicate an error.
				//
				UARTwrite("ERROR", 5);

				//
				// This command has been handled.
				//
				break;
			}
			}
		}
	}

	while (UARTBusy(UART1_BASE))
	{
	}
	GPIOPinWrite(GPIO_PORTG_BASE, GPIO_PIN_3, 0);

	//
	// End the varargs processing.
	//
	va_end(vaArgP);
}

//*****************************************************************************
//
// Close the Doxygen group.
//! @}
//
//*****************************************************************************
