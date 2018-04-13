/*!
  * Copyright (C) Robert Bosch. All Rights Reserved.
  *
  * <Disclaimer>
  * Common: Bosch Sensortec products are developed for the consumer goods
  * industry. They may only be used within the parameters of the respective valid
  * product data sheet.  Bosch Sensortec products are provided with the express
  * understanding that there is no warranty of fitness for a particular purpose.
  * They are not fit for use in life-sustaining, safety or security sensitive
  * systems or any system or device that may lead to bodily harm or property
  * damage if the system or device malfunctions. In addition, Bosch Sensortec
  * products are not fit for use in products which interact with motor vehicle
  * systems.  The resale and/or use of products are at the purchaser's own risk
  * and his own responsibility. The examination of fitness for the intended use
  * is the sole responsibility of the Purchaser.
  *
  * The purchaser shall indemnify Bosch Sensortec from all third party claims,
  * including any claims for incidental, or consequential damages, arising from
  * any product use not covered by the parameters of the respective valid product
  * data sheet or not approved by Bosch Sensortec and reimburse Bosch Sensortec
  * for all costs in connection with such claims.
  *
  * The purchaser must monitor the market for the purchased products,
  * particularly with regard to product safety and inform Bosch Sensortec without
  * delay of all security relevant incidents.
  *
  * Engineering Samples are marked with an asterisk (*) or (e). Samples may vary
  * from the valid technical specifications of the product series. They are
  * therefore not intended or fit for resale to third parties or for use in end
  * products. Their sole purpose is internal client testing. The testing of an
  * engineering sample may in no way replace the testing of a product series.
  * Bosch Sensortec assumes no liability for the use of engineering samples. By
  * accepting the engineering samples, the Purchaser agrees to indemnify Bosch
  * Sensortec from all claims arising from the use of engineering samples.
  *
  * Special: This software module (hereinafter called "Software") and any
  * information on application-sheets (hereinafter called "Information") is
  * provided free of charge for the sole purpose to support your application
  * work. The Software and Information is subject to the following terms and
  * conditions:
  *
  * The Software is specifically designed for the exclusive use for Bosch
  * Sensortec products by personnel who have special experience and training. Do
  * not use this Software if you do not have the proper experience or training.
  *
  * This Software package is provided `` as is `` and without any expressed or
  * implied warranties, including without limitation, the implied warranties of
  * merchantability and fitness for a particular purpose.
  *
  * Bosch Sensortec and their representatives and agents deny any liability for
  * the functional impairment of this Software in terms of fitness, performance
  * and safety. Bosch Sensortec and their representatives and agents shall not be
  * liable for any direct or indirect damages or injury, except as otherwise
  * stipulated in mandatory applicable law.
  *
  * The Information provided is believed to be accurate and reliable. Bosch
  * Sensortec assumes no responsibility for the consequences of use of such
  * Information nor for any infringement of patents or other rights of third
  * parties which may result from its use. No license is granted by implication
  * or otherwise under any patent or patent rights of Bosch. Specifications
  * mentioned in the Information are subject to change without notice.
  *
  * @file              fifo_watermark_example.c
  *
  * @date            12/15/2016
  *
  * @brief            example to test the watermark feature of the sensor.
  *                     Terminal configuration : 115200, 8N1
  */

/********************************************************************************/
/*                                  HEADER FILES                                */
/********************************************************************************/

#include <string.h>
#include <math.h>
#if 1  // Ammore Add
#include <stdio.h>
#endif

#if 0  // Ammore Remove
#include "asf.h"
#include "conf_board.h"
#include "led.h"
#endif
#include "bhy_uc_driver.h"
/* for customer , to put the firmware array */
#if 0  // Ammore Remove
#include "BHIfw.h"
#endif

/********************************************************************************/
/*                                       MACROS                                 */
/********************************************************************************/
/** TWI Bus Clock 400kHz */
#if 0  // Ammore Remove
#define TWI_CLK              400000
#endif
/* should be greater or equal to 69 bytes, page size (50) + maximum packet size(18) + 1 */
#define ARRAYSIZE            250
#define WATERMARK            200
#if 0  // Ammore Remove
#define EDBG_FLEXCOM         FLEXCOM7
#define EDBG_USART           USART7
#define EDBG_FLEXCOM_IRQ     FLEXCOM7_IRQn
#define DELAY_1US_CIRCLES    0x06
#define DELAY_1MS_CIRCLES    0x1BA0
#endif
#define FIFO_SAMPLE_RATE     100
#define REPORT_LATENCY_MS    1000
#define OUT_BUFFER_SIZE      60

/*!
 * @brief This structure holds all setting for the console
 */
#if 0  // Ammore Remove
const sam_usart_opt_t usart_console_settings =
{
    115200,
    US_MR_CHRL_8_BIT,
    US_MR_PAR_NO,
    US_MR_NBSTOP_1_BIT,
    US_MR_CHMODE_NORMAL
};
#endif

#if 1  // Ammore Add
#define Min(a,b) (a<b?a:b)
#define Max(a,b) (a>b?a:b)
#endif

/********************************************************************************/
/*                                GLOBAL VARIABLES                              */
/********************************************************************************/
#if 0  // Ammore Modify
uint8_t out_buffer[OUT_BUFFER_SIZE] = "";
#else
static char out_buffer[OUT_BUFFER_SIZE] = "";
#endif

/********************************************************************************/
/*                           STATIC FUNCTION DECLARATIONS                       */
/********************************************************************************/
#if 0  // Ammore Remove
static void i2c_pre_initialize(void);
static void twi_initialize(void);
static void edbg_usart_enable(void);
static void mdelay(uint32_t delay_ms);
static void udelay(uint32_t delay_us);
static void device_specific_initialization(void);
#endif
static void print_read_size(u16 bytes_read);

/********************************************************************************/
/*                                    FUNCTIONS                                 */
/********************************************************************************/
#if 0  // Ammore Remove
/*!
 * @brief This function is used to delay a number of microseconds actively.
 *
 * @param[in]   delay_us    microseconds to be delayed
 */
static void udelay(uint32_t delay_us)
{
    volatile uint32_t dummy;
    uint32_t calu;

    for (uint32_t u = 0; u < delay_us; u++)
    {
        for (dummy = 0; dummy < DELAY_1US_CIRCLES; dummy++)
        {
            calu++;
        }
    }
}

/*!
 * @brief This function is used to delay a number of milliseconds actively.
 *
 * @param[in]   delay_ms        milliseconds to be delayed
 */
static void mdelay(uint32_t delay_ms)
{
    volatile uint32_t dummy;
    uint32_t calu;

    for (uint32_t u = 0; u < delay_ms; u++)
    {
        for (dummy = 0; dummy < DELAY_1US_CIRCLES; dummy++)
        {
            calu++;
        }
    }
}

/*!
 * @brief     This function  issues 9 clock cycles on the SCL line
 *             so that all devices release the SDA line if they are holding it
 */
static void i2c_pre_initialize(void)
{
    ioport_set_pin_dir(EXT1_PIN_I2C_SCL, IOPORT_DIR_OUTPUT);

    for (int8_t i = 0; i < 9; ++i)
    {
        ioport_set_pin_level(EXT1_PIN_I2C_SCL, IOPORT_PIN_LEVEL_LOW);
        udelay(2);

        ioport_set_pin_level(EXT1_PIN_I2C_SCL, IOPORT_PIN_LEVEL_HIGH);
        udelay(1);
    }
}

/*!
 * @brief     This function Enable the peripheral and set TWI mode
 *
 */
static void twi_initialize(void)
{
    twi_options_t opt;

    opt.master_clk = sysclk_get_cpu_hz();
    opt.speed = TWI_CLK;
    /* Enable the peripheral and set TWI mode. */
    flexcom_enable(BOARD_FLEXCOM_TWI);
    flexcom_set_opmode(BOARD_FLEXCOM_TWI, FLEXCOM_TWI);

    if (twi_master_init(TWI4, &opt) != TWI_SUCCESS)
    {
        while (1)
        {
            ;/* Capture error */
        }
    }
}

/*!
 * @brief     This function is EDBG USART RX IRQ Handler ,just echo characters
 *
 */
static void FLEXCOM7_Handler (void)
{
    uint32_t tmp_data;

    while (usart_is_rx_ready(EDBG_USART))
    {
        usart_getchar(EDBG_USART, &tmp_data);
        usart_putchar(EDBG_USART, tmp_data);
    }
}

/*!
 * @brief     This function enable usart
 *
 */
static void edbg_usart_enable(void)
{
    flexcom_enable(EDBG_FLEXCOM);
    flexcom_set_opmode(EDBG_FLEXCOM, FLEXCOM_USART);

    usart_init_rs232(EDBG_USART, &usart_console_settings, sysclk_get_main_hz());
    usart_enable_tx(EDBG_USART);
    usart_enable_rx(EDBG_USART);

    usart_enable_interrupt(EDBG_USART, US_IER_RXRDY);
    NVIC_EnableIRQ(EDBG_FLEXCOM_IRQ);
}

/*!
 * @brief     This function regroups all the initialization specific to SAM G55
 *
 */
static void device_specific_initialization(void)
{
    /* Initialize the SAM system */
    sysclk_init();

    /* execute this function before board_init */
    i2c_pre_initialize();

    /* Initialize the board */
    board_init();

    /* configure the i2c */
    twi_initialize();

    /* configures the serial port */
    edbg_usart_enable();

    /* configures the interrupt pin GPIO */
    ioport_set_pin_dir(EXT1_PIN_GPIO_1, IOPORT_DIR_INPUT);
    ioport_set_pin_mode(EXT1_PIN_GPIO_1, IOPORT_MODE_PULLUP);
}
#endif

/*!
 * @brief     printf function
 *
 * @Param[[in]      bytes_read   number of bytes to be read
 */
static void print_read_size(u16 bytes_read)
{
    static uint16_t min_value = 0xFFFF;
    static uint16_t max_value = 0;

    min_value = Min(min_value, bytes_read);
    max_value = Max(max_value, bytes_read);

#if 0  // Ammore Modify
    sprintf(out_buffer, " Min:%04d  Current:%04d  Max:%04d\r", min_value, bytes_read, max_value);
    usart_write_line(EDBG_USART, out_buffer);
#else
    sprintf_s(out_buffer, OUT_BUFFER_SIZE," Min:%04d  Current:%04d  Max:%04d\r", min_value, bytes_read, max_value);
    printf("%s \r\n", out_buffer);
#endif
}

/*!
 * @brief     main body function
 *
 * @retval   result for execution
 */
int fifo_watermark_example_main(void)
{
    uint8_t        array[ARRAYSIZE];
    uint16_t       bytes_remaining = 0;
    uint16_t       bytes_read      = 0;

#if 0  // Ammore Remove
    /* Initialize the SAM G55 system */
    device_specific_initialization();
#endif

#if 0  // Ammore Modify
    sprintf(out_buffer, "\n\rARRAYSIZE=%d, WATERMARK=%d\n\r", ARRAYSIZE, WATERMARK);
    usart_write_line(EDBG_USART, out_buffer);
#else
    sprintf_s(out_buffer, OUT_BUFFER_SIZE,"\n\rARRAYSIZE=%d, WATERMARK=%d\n\r", ARRAYSIZE, WATERMARK);
    printf("%s \r\n", out_buffer);
#endif

#if 0  // Ammore Remove
    /* initializes the BHI160 and loads the RAM patch if */
    /* the ram patch does not output any debug         */
    /* information to the fifo, this demo will not work    */
    bhy_driver_init(_bhi_fw/*, _bhi_fw_len*/);

    /* wait for the interrupt pin to go down then up again */
    while (ioport_get_pin_level(EXT1_PIN_GPIO_1))
    {
    }

    while (!ioport_get_pin_level(EXT1_PIN_GPIO_1))
    {
    }
#endif

    /* empty the fifo for a first time. the interrupt does not contain */
    /* sensor data */
    bhy_read_fifo(array, ARRAYSIZE, &bytes_read, &bytes_remaining);

    /* Enables a streaming sensor. The goal here is to generate data */
    /* in the fifo, not to read the sensor values. */
    bhy_enable_virtual_sensor(VS_TYPE_ROTATION_VECTOR, VS_NON_WAKEUP, FIFO_SAMPLE_RATE, REPORT_LATENCY_MS, VS_FLUSH_NONE, 0, 0);

    /* Sets the watermark level */
    bhy_set_fifo_water_mark(BHY_FIFO_WATER_MARK_NON_WAKEUP, WATERMARK);

    /* continuously read and parse the fifo */
#if 0 // Ammore Modify
    while (true)
#else
    while (!OEM_IsQuit())
#endif
    {
        /* wait until the interrupt fires */
#if 0 // Ammore Modify
        while (!ioport_get_pin_level(EXT1_PIN_GPIO_1))
#else
        while (!OEM_get_pin_level())
#endif
        {
        }

        bhy_read_fifo(array, ARRAYSIZE, &bytes_read, &bytes_remaining);
        print_read_size(bytes_read);

        if (bytes_remaining)
        {
            /* The watermark level was set too high, it didn't manage to read */
#if 0  // Ammore Modify
            usart_write_line(EDBG_USART, "\n\rThe watermark level was too low, demo stopped.");
#else
            printf("\n\rThe watermark level was too low, demo stopped.");
#endif

            /* stops the demo here */
            while(1)
            {
            }
        }
    }

    return BHY_SUCCESS;
}
/** @}*/
