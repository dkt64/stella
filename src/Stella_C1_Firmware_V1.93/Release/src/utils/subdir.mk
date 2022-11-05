################################################################################
# Automatically-generated file. Do not edit!
################################################################################

# Add inputs and outputs from these tool invocations to the build variables 
C_SRCS += \
../src/utils/isqrt.c \
../src/utils/ringbuf.c \
../src/utils/uartstdio.c \
../src/utils/ustdlib.c 

OBJS += \
./src/utils/isqrt.o \
./src/utils/ringbuf.o \
./src/utils/uartstdio.o \
./src/utils/ustdlib.o 

C_DEPS += \
./src/utils/isqrt.d \
./src/utils/ringbuf.d \
./src/utils/uartstdio.d \
./src/utils/ustdlib.d 


# Each subdirectory must supply rules for building sources it contributes
src/utils/%.o: ../src/utils/%.c
	@echo 'Building file: $<'
	@echo 'Invoking: ARM Sourcery Windows GCC C Compiler'
	arm-none-eabi-gcc -Dgcc -I"c:\barteka\Stellaris\armgcc\arm-none-eabi\include" -I"C:\barteka\Stellaris\workspace\Stella_C1_Firmware_V1.91\src" -Os -fpack-struct -fshort-enums -Wall -fsigned-char -c -fmessage-length=0 -MMD -MP -MF"$(@:%.o=%.d)" -MT"$(@:%.o=%.d)" -mcpu=cortex-m3 -mthumb -g -gdwarf-2 -o"$@" "$<"
	@echo 'Finished building: $<'
	@echo ' '


