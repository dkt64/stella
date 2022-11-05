################################################################################
# Automatically-generated file. Do not edit!
################################################################################

-include ../makefile.init

RM := cs-rm -rf

# All of the sources participating in the build are defined here
-include sources.mk
-include subdir.mk
-include src/utils/subdir.mk
-include src/subdir.mk
-include src/driver/subdir.mk
-include objects.mk

ifneq ($(MAKECMDGOALS),clean)
ifneq ($(strip $(C_DEPS)),)
-include $(C_DEPS)
endif
ifneq ($(strip $(ASM_DEPS)),)
-include $(ASM_DEPS)
endif
ifneq ($(strip $(S_DEPS)),)
-include $(S_DEPS)
endif
endif

-include ../makefile.defs

# Add inputs and outputs from these tool invocations to the build variables 
SECONDARY_FLASH += \
Stella_C1_Firmware_V1.93.hex \

SECONDARY_LIST += \
Stella_C1_Firmware_V1.93.lst \

SECONDARY_SIZE += \
Stella_C1_Firmware_V1.93.siz \


# All Target
all: Stella_C1_Firmware_V1.93.elf secondary-outputs

# Tool invocations
Stella_C1_Firmware_V1.93.elf: $(OBJS) $(USER_OBJS)
	@echo 'Building target: $@'
	@echo 'Invoking: ARM Sourcery Windows GCC C Linker'
	arm-none-eabi-gcc -T"c:\barteka\Stellaris\workspace\Stella_C1_Firmware_V1.91\src\lm-cortex.ld" -nostartfiles -Wl,-Map,Stella_C1_Firmware_V1.93.map -mcpu=cortex-m3 -mthumb -g -gdwarf-2 -o"Stella_C1_Firmware_V1.93.elf" $(OBJS) $(USER_OBJS) $(LIBS)
	@echo 'Finished building target: $@'
	@echo ' '

Stella_C1_Firmware_V1.93.hex: Stella_C1_Firmware_V1.93.elf
	@echo 'Invoking: ARM Sourcery Windows GNU Create Flash Image'
	arm-none-eabi-objcopy -O binary Stella_C1_Firmware_V1.93.elf "Stella_C1_Firmware_V1.93.hex"
	@echo 'Finished building: $@'
	@echo ' '

Stella_C1_Firmware_V1.93.lst: Stella_C1_Firmware_V1.93.elf
	@echo 'Invoking: ARM Sourcery Windows GNU Create Listing'
	arm-none-eabi-objdump -h -S Stella_C1_Firmware_V1.93.elf >"Stella_C1_Firmware_V1.93.lst"
	@echo 'Finished building: $@'
	@echo ' '

Stella_C1_Firmware_V1.93.siz: Stella_C1_Firmware_V1.93.elf
	@echo 'Invoking: ARM Sourcery Windows GNU Print Size'
	arm-none-eabi-size  --format=berkeley Stella_C1_Firmware_V1.93.elf
	@echo 'Finished building: $@'
	@echo ' '

# Other Targets
clean:
	-$(RM) $(SECONDARY_SIZE)$(OBJS)$(C_DEPS)$(ASM_DEPS)$(SECONDARY_FLASH)$(S_DEPS)$(EXECUTABLES)$(SECONDARY_LIST) Stella_C1_Firmware_V1.93.elf
	-@echo ' '

secondary-outputs: $(SECONDARY_FLASH) $(SECONDARY_LIST) $(SECONDARY_SIZE)

.PHONY: all clean dependents
.SECONDARY:

-include ../makefile.targets
