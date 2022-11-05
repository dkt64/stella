/*
 * tools.c
 *
 *  Created on: 2010-02-20
 *      Author: Bartek
 */

volatile unsigned long tick;

//*****************************************************************************
// Funkcje narzêdziowe
//*****************************************************************************

void ftoa(float f, char *buf)
{
	int pos = 0, ix, dp, num;
	if (f < 0)
	{
		buf[pos++] = '-';
		f = -f;
	}
	dp = 0;
	while (f >= 10.0)
	{
		f = f / 10.0;
		dp++;
	}
	for (ix = 1; ix < 8; ix++)
	{
		num = f;
		f = f - num;
		if (num > 9)
			buf[pos++] = '#';
		else
			buf[pos++] = '0' + num;
		if (dp == 0)
			buf[pos++] = '.';
		f = f * 10.0;
		dp--;
	}
}

unsigned long Delay(unsigned long in)
{
	unsigned long x, i;

	x = 0;

	for (i = 0; i < in; i++)
	{
		x = i * 23;
	}

	return x;
}

// *****************************************************************************
// Obs³uga ticków
// *****************************************************************************

void TickInit(void)
{
	// Konfiguracja Tick'a
	//SysTickPeriodSet(50000); // 50000 cykli zegara = prze³adowanie co 1 ms
	SysTickEnable();
	//SysTickIntEnable();
	tick = 0;
}

unsigned long QuickTickRead(void)
{
	unsigned long t;

	t = SysTickValueGet();
	return (t);
}

unsigned long QuickTickElapsed(unsigned long count)
{
	return (count - QuickTickRead()) / 50;
}

unsigned long TickRead(void)
{
	unsigned long t;

	t = tick;
	return (t);
}

unsigned long TickElapsed(unsigned long count)
{
	return (TickRead() - count);
}

void TickWait(unsigned long count)
{
	unsigned long start_count;

	start_count = TickRead();

	while (TickElapsed(start_count) <= count)
	{
	}
}


