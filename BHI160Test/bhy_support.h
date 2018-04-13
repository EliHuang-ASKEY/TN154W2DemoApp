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
  *
  * @file              bhy_support.h
  *
  * @date              12/19/2016
  *
  * @brief             driver on MCU for bhy
  *
  *
  */

#ifndef BHY_SUPPORT_H_
#define BHY_SUPPORT_H_

/********************************************************************************/
/*                                  HEADER FILES                                */
/********************************************************************************/
#include "bhy.h"
#if 0  // Ammore Remove
#include "twi.h"
#endif

#if 1  // Ammore Add
extern void OEM_mdelay(uint32_t ul_dly_ticks);
extern int OEM_IsQuit();
extern int8_t OEM_i2c_read_internal(uint8_t dev_addr, uint8_t reg_addr, uint8_t *rx_data, uint16_t length);
extern int8_t OEM_i2c_write_internal(uint8_t dev_addr, uint8_t reg_addr, uint8_t *reg_data, uint16_t length);
extern int OEM_get_pin_level();
#endif

/********************************************************************************/
/*                                     MACROS                                   */
/********************************************************************************/
#define RETRY_NUM                   3

/*! determines the I2C slave address of BHy
* The default I2C address of the BHy device is 0101000b (0x28). */
/* 0x28 CONFLICTS ON ATMEL DEV KITS WITH THE ONBOARD EDBG!!!!   */
#define BHY_I2C_SLAVE_ADDRESS       BHY_I2C_ADDR1
/*! the delay required to wait for BHY chip to reset */
#define BHY_RESET_DELAY_MS          UINT32_C(50)

/*! these two macros are defined for i2c read/write limitation of host */
/*! users must modify these two macros according to their own IIC hardware design */
#define I2C_ONCE_WRITE_MAX_COUNT   (8)
#define I2C_ONCE_READ_MAX_COUNT    (8)

/********************************************************************************/
/*                             FUNCTION DECLARATIONS                            */
/********************************************************************************/

/*!
* @brief        Initializes BHY smart sensor and its required connections
*
*/
int8_t bhy_initialize_support(void);

/*!
* @brief        Sends data to BHY via I2C
*
* @param[in]    dev_addr    Device I2C slave address
* @param[in]    reg_addr    Address of destination register
* @param[in]    p_wr_buf  Pointer to data buffer to be sent
* @param[in]    wr_len    Length of the data to be sent
*
* @retval       0           BHY_SUCCESS
* @retval       -1          BHY_ERROR
*
*/
int8_t bhy_i2c_write(uint8_t dev_addr, uint8_t reg_addr, uint8_t *wr_buf, uint16_t wr_len);

/*!
* @brief        Receives data from BHY on I2C
*
* @param[in]    dev_addr    Device I2C slave address
* @param[in]    reg_addr    Address of destination register
* @param[out]   p_rd_buf  Pointer to data buffer to be received
* @param[in]    rd_len    Length of the data to be received
*
* @retval       0           BHY_SUCCESS
* @retval       -1          BHY_ERROR
*
*/
int8_t bhy_i2c_read(uint8_t dev_addr, uint8_t reg_addr, uint8_t *rd_buf, uint16_t rd_len);

/*!
* @brief        Initiates a delay of the length of the argument in milliseconds
*
* @param[in]    msec    Delay length in terms of milliseconds
*
*/
void bhy_delay_msec(uint32_t msec);

/*!
* @brief        Resets the BHY chip
*
*/
void bhy_reset(void);

#endif /* BHY_SUPPORT_H_ */