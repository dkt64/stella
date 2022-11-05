/*
 * tools.h
 *
 *  Created on: 2010-02-20
 *      Author: Bartek
 */

#ifndef TOOLS_H_
#define TOOLS_H_

#define TICK_1MS		20
#define TICK_10MS		10 * TICK_1MS
#define TICK_100MS		100 * TICK_1MS
#define TICK_1S			1000 * TICK_1MS

void ftoa(float f, char *buf);
unsigned long Delay(unsigned long in);
void TickInit(void);
unsigned long QuickTickRead(void);
unsigned long QuickTickElapsed(unsigned long count);
unsigned long TickRead(void);
unsigned long TickElapsed(unsigned long count);
void TickWait(unsigned long count);

#endif /* TOOLS_H_ */
