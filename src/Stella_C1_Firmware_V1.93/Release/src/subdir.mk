################################################################################
# Automatically-generated file. Do not edit!
################################################################################

# Add inputs and outputs from these tool invocations to the build variables 
C_SRCS += \
../src/comm.c \
../src/io.c \
../src/main.c \
../src/modbus.c \
../src/proces.c \
../src/qei.c \
../src/startup_gcc.c \
../src/tools.c 

OBJS += \
./src/comm.o \
./src/io.o \
./src/main.o \
./src/modbus.o \
./src/proces.o \
./src/qei.o \
./src/startup_gcc.o \
./src/tools.o 

C_DEPS += \
./src/comm.d \
./src/io.d \
./src/main.d \
./src/modbus.d \
./src/proces.d \
./src/qei.d \
./src/startup_gcc.d \
./src/tools.d 


# Each subdirectory must supply rules for building sources it contributes
src/%.o: ../src/%.c
	@echo 'Building file: $<'
	@echo 'Invoking: ARM Sourcery Windows GCC C Compiler'
	arm-none-eabi-gcc -Dgcc -I"c:\barteka\Stellaris\armgcc\arm-none-eabi\include" -I"C:\barteka\Stellaris\workspace\Stella_C1_Firmware_V1.91\src" -Os -fpack-struct -fshort-enums -Wall -fsigned-char -c -fmessage-length=0 -MMD -MP -MF"$(@:%.o=%.d)" -MT"$(@:%.o=%.d)" -mcpu=cortex-m3 -mthumb -g -gdwarf-2 -o"$@" "$<"
	@echo 'Finished building: $<'
	@echo ' '


