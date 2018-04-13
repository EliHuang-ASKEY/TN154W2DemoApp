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
  * @file          accelerometer_remapping_example.c
  *
  * @date          12/15/2016
  *
  * @brief         The demo shows the example to set remapping for accelerometer sensor
  *                  Terminal configuration : 115200, 8N1
  */

/********************************************************************************/
/*                                  HEADER FILES                                */
/********************************************************************************/
#include <string.h>
#include <math.h>
#include <stdio.h>
#include <windows.h>

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
#if 0  // Ammore Remove
/** TWI Bus Clock 400kHz */
#define TWI_CLK                          400000
#endif
/* should be greater or equal to 69 bytes, page size (50) + maximum packet size(18) + 1 */
#define ARRAYSIZE                        69
#if 0  // Ammore Remove
#define EDBG_FLEXCOM                     FLEXCOM7
#define EDBG_USART                       USART7
#define EDBG_FLEXCOM_IRQ                 FLEXCOM7_IRQn
#endif
#define SENSOR_TYPE_MASK                 0x1F
#define VIRTUAL_SENSOR_SAMPLE_RATE       10
#define MAX_REPORT_LATENCY_MS            1000
#if 0  // Ammore Remove
#define DELAY_1US_CIRCLES                0x06
#define DELAY_1MS_CIRCLES                0x1BA0
#endif
#define OUT_BUFFER_SIZE                  200
#define MAX_PACKET_LENGTH                18
#if 0  // Ammore Modify
#define TICKS_IN_ONE_SECOND              3200000F
#else
#define TICKS_IN_ONE_SECOND              32000
#endif
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

/********************************************************************************/
/*                                GLOBAL VARIABLES                              */
/********************************************************************************/
/* system timestamp */
static uint32_t bhy_timestamp = 0;
static uint8_t  out_buffer[OUT_BUFFER_SIZE] = {0};
eric_value_return sensor_value,gyro_sensor_value;

/********************************************************************************/
/*                          STATIC FUNCTION DECLARATIONS                        */
/********************************************************************************/
#if 0  // Ammore Remove
static void i2c_pre_initialize(void);
static void twi_initialize(void);
static void edbg_usart_enable(void);
static void mdelay(uint32_t delay_ms);
static void udelay(uint32_t delay_us);
static void device_specific_initialization(void);
#endif
static void sensors_callback(bhy_data_generic_t * sensor_data, bhy_virtual_sensor_t sensor_id);
static void timestamp_callback(bhy_data_scalar_u16_t *new_timestamp);


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
        for (dummy = 0; dummy < DELAY_1MS_CIRCLES; dummy++)
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

    for (int8_t i = 0; i < 9;++i)
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
#endif

/*!
 * @brief This function is time stamp callback function that process data in fifo.
 *
 * @param[in]   new_timestamp
 */
static void timestamp_callback(bhy_data_scalar_u16_t *new_timestamp)
{
    /* updates the system timestamp */
    bhy_update_system_timestamp(new_timestamp, &bhy_timestamp);
}

/*!
 * @brief This function is  callback function for acquring sensor datas
 *
 * @param[in]   sensor_data
 * @param[in]   sensor_id
 */
static void sensors_callback(bhy_data_generic_t *sensor_data, bhy_virtual_sensor_t sensor_id)
{
    float   time_stamp    = 0;
    uint8_t sensor_type   = 0;
    int16_t x_raw         = 0;
    int16_t y_raw         = 0;
    int16_t z_raw         = 0;
    float   x_data        = 0;
    float   y_data        = 0;
    float   z_data        = 0;

    /* Since a timestamp is always sent before every new data, and that the callbacks   */
    /* are called while the parsing is done, then the system timestamp is always equal  */
    /* to the sample timestamp. (in callback mode only)                                 */
    time_stamp = (float)(bhy_timestamp) / TICKS_IN_ONE_SECOND;
    sensor_type = sensor_id;
    sensor_type &= SENSOR_TYPE_MASK;
    memset(out_buffer, 0, sizeof(out_buffer));

    switch(sensor_type)
    {
        case VS_TYPE_ACCELEROMETER:
            x_raw  = sensor_data->data_vector.x;
            y_raw  = sensor_data->data_vector.y;
            z_raw  = sensor_data->data_vector.z;
            /* The resolution is  15bit ,the default range is 4g, actual acceleration equals: raw_data/(exp(2,15) == 32768) */
            x_data = (float)x_raw / 32768.0f * 4.0f;
            y_data = (float)y_raw / 32768.0f * 4.0f;
            z_data = (float)z_raw / 32768.0f * 4.0f;
#if 0  // Ammore Modify
            sprintf(out_buffer,\
                    "Time:%6.3fs, Sen ID:%d, Accel X:%6.3f, Accel Y:%6.3f, Accel Z:%6.3f.\r\n",\
                    time_stamp,sensor_id,x_data,y_data,z_data);
            usart_write_line(EDBG_USART, out_buffer);
#else
            sprintf_s((char *)out_buffer, \
                      OUT_BUFFER_SIZE, \
                      "Time:%6.3fs, Sen ID:%d, Accel X:%6.3f, Accel Y:%6.3f, Accel Z:%6.3f.\r\n", \
                      time_stamp, sensor_id, x_data, y_data, z_data);
            printf("%s", out_buffer);
			sensor_value.x = x_data;
			sensor_value.y = y_data;
			sensor_value.z = z_data;
			
#endif
            break;

		case VS_TYPE_GYROSCOPE:
			x_raw = sensor_data->data_vector.x;
			y_raw = sensor_data->data_vector.y;
			z_raw = sensor_data->data_vector.z;
			/* The resolution is  15bit ,the default range is 4g, actual acceleration equals: raw_data/(exp(2,15) == 32768) */
			x_data = (float)x_raw / 32768.0f * 2000.0f;
			y_data = (float)y_raw / 32768.0f * 2000.0f;
			z_data = (float)z_raw / 32768.0f * 2000.0f;
#if 0  // Ammore Modify
			sprintf(out_buffer, \
				"Time:%6.3fs, Sen ID:%d, Accel X:%6.3f, Accel Y:%6.3f, Accel Z:%6.3f.\r\n", \
				time_stamp, sensor_id, x_data, y_data, z_data);
			usart_write_line(EDBG_USART, out_buffer);
#else
			sprintf_s((char *)out_buffer, \
				OUT_BUFFER_SIZE, \
				"Time:%6.3fs, Sen ID:%d, Accel X:%6.3f, Accel Y:%6.3f, Accel Z:%6.3f.\r\n", \
				time_stamp, sensor_id, x_data, y_data, z_data);
			printf("%s", out_buffer);
			gyro_sensor_value.x = x_data;
			gyro_sensor_value.y = y_data;
			gyro_sensor_value.z = z_data;

#endif
			break;

        default:
#if 0  // Ammore Modify
            sprintf(out_buffer,"Time:%6.3fs Unknown.\r\n",time_stamp);
            usart_write_line(EDBG_USART, out_buffer);
#else
            sprintf_s((char *)out_buffer, OUT_BUFFER_SIZE, "Time:%6.3fs Unknown.\r\n", time_stamp);
            printf("%s", out_buffer);
#endif
            break;
    }
    /* gesture recognition sensors are always one-shot, so you need to  */
}

//Eric add

eric_value_return get_acc_value()
{
	return sensor_value;
}

eric_value_return get_gyro_value()
{
	return gyro_sensor_value;
}




/*!
 * @brief     This function regroups all the initialization specific to SAM G55
 *
 */
#if 0  // Ammore Remove
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
 * @brief     main body function
 *
 * @retval   result for execution
 */
int accelerometer_remapping_example_main(void)
{
    uint8_t                   array[ARRAYSIZE];
    uint8_t                   *fifoptr                     = NULL;
    uint8_t                   bytes_left_in_fifo           = 0;
    uint16_t                  bytes_remaining              = 0;
    uint16_t                  bytes_read                   = 0;
    bhy_data_generic_t        fifo_packet;
    bhy_data_type_t           packet_type;
    BHY_RETURN_FUNCTION_TYPE  result;
    int8_t                    bhy_mapping_matrix_init[3*3]   = {0};
    int8_t                    bhy_mapping_matrix_config[3*3] = {0,1,0,-1,0,0,0,0,1};

#if 0  // Ammore Remove
    /* Initialize the SAM G55 system */
    device_specific_initialization();

    /* initializes the BHI160 and loads the RAM patch */
    bhy_driver_init(_bhi_fw/*, _bhi_fw_len*/);

    /* wait for the interrupt pin to go down then up again */
    while (ioport_get_pin_level(EXT1_PIN_GPIO_1))
    {
    }

    while (!ioport_get_pin_level(EXT1_PIN_GPIO_1))
    {
    }
#endif

    /* config mapping matrix,it is not necessary to change mapping matrix */
    bhy_mapping_matrix_get(PHYSICAL_SENSOR_INDEX_ACC, bhy_mapping_matrix_init);
    bhy_mapping_matrix_set(PHYSICAL_SENSOR_INDEX_ACC, bhy_mapping_matrix_config);
    bhy_mapping_matrix_get(PHYSICAL_SENSOR_INDEX_ACC, bhy_mapping_matrix_init);

    /* install time stamp callback */
    bhy_install_timestamp_callback(VS_WAKEUP, timestamp_callback);
    bhy_install_timestamp_callback(VS_NON_WAKEUP, timestamp_callback);

    /* enables the accelerator sensors and assigns the callback */
    bhy_install_sensor_callback(VS_TYPE_ACCELEROMETER, VS_WAKEUP, sensors_callback);
    bhy_enable_virtual_sensor(VS_TYPE_ACCELEROMETER, VS_WAKEUP, VIRTUAL_SENSOR_SAMPLE_RATE, MAX_REPORT_LATENCY_MS, \
                              VS_FLUSH_NONE, 0, 0);

	/* enables the gyroscope sensors and assigns the callback */
	bhy_install_sensor_callback(VS_TYPE_GYROSCOPE, VS_WAKEUP, sensors_callback);
	bhy_enable_virtual_sensor(VS_TYPE_GYROSCOPE, VS_WAKEUP, VIRTUAL_SENSOR_SAMPLE_RATE, MAX_REPORT_LATENCY_MS, \
		VS_FLUSH_NONE, 0, 0);

    /* continuously read and parse the fifo */
#if 0  // Ammore Modify
    while (true)
#else
    while (!OEM_IsQuit())
#endif
    {
        /* wait until the interrupt fires */
        /* unless we already know there are bytes remaining in the fifo */
#if 0  // Ammore Modify
        while ((ioport_get_pin_level(EXT1_PIN_GPIO_1) > 0) || (bytes_remaining > 0))
#else
        while ((OEM_get_pin_level() > 0) || (bytes_remaining > 0))
#endif
        {
            bhy_read_fifo(array + bytes_left_in_fifo, ARRAYSIZE - bytes_left_in_fifo, &bytes_read, &bytes_remaining);
            bytes_read           += bytes_left_in_fifo;
            fifoptr              = array;
            packet_type          = BHY_DATA_TYPE_PADDING;

            do
            {
                /* this function will call callbacks that are registered */
                result = bhy_parse_next_fifo_packet(&fifoptr, &bytes_read, &fifo_packet, &packet_type);
                /* the logic here is that if doing a partial parsing of the fifo, then we should not parse  */
                /* the last 18 bytes (max length of a packet) so that we don't try to parse an incomplete   */
                /* packet */
            } while ((result == BHY_SUCCESS) && (bytes_read > (bytes_remaining ? MAX_PACKET_LENGTH : 0)));

            bytes_left_in_fifo = 0;

            if (bytes_remaining)
            {
                /* shifts the remaining bytes to the beginning of the buffer */
                while (bytes_left_in_fifo < bytes_read)
                {
                    array[bytes_left_in_fifo++] = *(fifoptr++);
                }
            }
        }
    }

	return 0;

}
/** @}*/