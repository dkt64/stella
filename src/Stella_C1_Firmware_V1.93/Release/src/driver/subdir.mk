################################################################################
# Automatically-generated file. Do not edit!
################################################################################

# Add inputs and outputs from these tool invocations to the build variables 
C_SRCS += \
../src/driver/adc.c \
../src/driver/cpu.c \
../src/driver/gpio.c \
../src/driver/interrupt.c \
../src/driver/qei.c \
../src/driver/ssi.c \
../src/driver/sysctl.c \
../src/driver/systick.c \
../src/driver/timer.c \
../src/driver/uart.c \
../src/driver/watchdog.c 

OBJS += \
./src/driver/adc.o \
./src/driver/cpu.o \
./src/driver/gpio.o \
./src/driver/interrupt.o \
./src/driver/qei.o \
./src/driver/ssi.o \
./src/driver/sysctl.o \
./src/driver/systick.o \
./src/driver/timer.o \
./src/driver/uart.o \
./src/driver/watchdog.o 

C_DEPS += \
./src/driver/adc.d \
./src/driver/cpu.d \
./src/driver/gpio.d \
./src/driver/interrupt.d \
./src/driver/qei.d \
./src/driver/ssi.d \
./src/driver/sysctl.d \
./src/driver/systick.d \
./src/driver/timer.d \
./src/driver/uart.d \
./src/driver/watchdog.d 


# Each subdirectory must supply rules for building sources it contributes
src/driver/%.o: ../src/driver/%.c
	@echo 'Building file: $<'
	@echo 'Invoking: ARM Sourcery Windows GCC C Compiler'
	arm-none-eabi-gcc -Dgcc -I"c:\barteka\Stellaris\armgcc\arm-none-eabi\include" -I"C:\barteka\Stellaris\workspace\Stella_C1_Firmware_V1.91\src" -Os -fpack-struct -fshort-enums -Wall -fsigned-char -c -fmessage-length=0 -MMD -MP -MF"$(@:%.o=%.d)" -MT"$(@:%.o=%.d)" -mcpu=cortex-m3 -mthumb -g -gdwarf-2 -o"$@" "$<"
	@echo 'Finished building: $<'
	@echo ' '


