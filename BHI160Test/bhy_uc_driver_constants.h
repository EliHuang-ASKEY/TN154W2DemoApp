
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
  * @file          bhy_uc_driver_constants.h
  *
  * @date          12/15/2016
  *
  * @brief         header file of bhy_uc_driver.c
  *
  */


#ifndef BHY_UC_DRIVER_CONSTANTS_H_
#define BHY_UC_DRIVER_CONSTANTS_H_

#include "bhy_uc_driver_config.h"

/****************************************************************************/
/*                      Constants definitions                               */
/****************************************************************************/

/* definition of all the known IDs. User can add their own IDs to the       */
/* bhy_parse_next_fifo_packet function. follow section 15 table 28 of the   */
/* BHI160 datasheet.                                                        */
#define VS_ID_PADDING                           0
#define VS_ID_ACCELEROMETER                     1
#define VS_ID_MAGNETOMETER                      2
#define VS_ID_ORIENTATION                       3
#define VS_ID_GYROSCOPE                         4
#define VS_ID_LIGHT                             5
#define VS_ID_BAROMETER                         6
#define VS_ID_TEMPERATURE                       7
#define VS_ID_PROXIMITY                         8
#define VS_ID_GRAVITY                           9
#define VS_ID_LINEAR_ACCELERATION               10
#define VS_ID_ROTATION_VECTOR                   11
#define VS_ID_HUMIDITY                          12
#define VS_ID_AMBIENT_TEMPERATURE               13
#define VS_ID_UNCALIBRATED_MAGNETOMETER         14
#define VS_ID_GAME_ROTATION_VECTOR              15
#define VS_ID_UNCALIBRATED_GYROSCOPE            16
#define VS_ID_SIGNIFICANT_MOTION                17
#define VS_ID_STEP_DETECTOR                     18
#define VS_ID_STEP_COUNTER                      19
#define VS_ID_GEOMAGNETIC_ROTATION_VECTOR       20
#define VS_ID_HEART_RATE                        21
#define VS_ID_TILT_DETECTOR                     22
#define VS_ID_WAKE_GESTURE                      23
#define VS_ID_GLANCE_GESTURE                    24
#define VS_ID_PICKUP_GESTURE                    25
#define VS_ID_ACTIVITY                          31

#define VS_ID_ACCELEROMETER_WAKEUP                  (VS_ID_ACCELEROMETER+32)
#define VS_ID_MAGNETOMETER_WAKEUP                   (VS_ID_MAGNETOMETER+32)
#define VS_ID_ORIENTATION_WAKEUP                    (VS_ID_ORIENTATION+32)
#define VS_ID_GYROSCOPE_WAKEUP                      (VS_ID_GYROSCOPE+32)
#define VS_ID_LIGHT_WAKEUP                          (VS_ID_LIGHT+32)
#define VS_ID_BAROMETER_WAKEUP                      (VS_ID_BAROMETER+32)
#define VS_ID_TEMPERATURE_WAKEUP                    (VS_ID_TEMPERATURE+32)
#define VS_ID_PROXIMITY_WAKEUP                      (VS_ID_PROXIMITY+32)
#define VS_ID_GRAVITY_WAKEUP                        (VS_ID_GRAVITY+32)
#define VS_ID_LINEAR_ACCELERATION_WAKEUP            (VS_ID_LINEAR_ACCELERATION+32)
#define VS_ID_ROTATION_VECTOR_WAKEUP                (VS_ID_ROTATION_VECTOR+32)
#define VS_ID_HUMIDITY_WAKEUP                       (VS_ID_HUMIDITY+32)
#define VS_ID_AMBIENT_TEMPERATURE_WAKEUP            (VS_ID_AMBIENT_TEMPERATURE+32)
#define VS_ID_UNCALIBRATED_MAGNETOMETER_WAKEUP      (VS_ID_UNCALIBRATED_MAGNETOMETER+32)
#define VS_ID_GAME_ROTATION_VECTOR_WAKEUP           (VS_ID_GAME_ROTATION_VECTOR+32)
#define VS_ID_UNCALIBRATED_GYROSCOPE_WAKEUP         (VS_ID_UNCALIBRATED_GYROSCOPE+32)
#define VS_ID_SIGNIFICANT_MOTION_WAKEUP             (VS_ID_SIGNIFICANT_MOTION+32)
#define VS_ID_STEP_DETECTOR_WAKEUP                  (VS_ID_STEP_DETECTOR+32)
#define VS_ID_STEP_COUNTER_WAKEUP                   (VS_ID_STEP_COUNTER+32)
#define VS_ID_GEOMAGNETIC_ROTATION_VECTOR_WAKEUP    (VS_ID_GEOMAGNETIC_ROTATION_VECTOR+32)
#define VS_ID_HEART_RATE_WAKEUP                     (VS_ID_HEART_RATE+32)
#define VS_ID_TILT_DETECTOR_WAKEUP                  (VS_ID_TILT_DETECTOR+32)
#define VS_ID_WAKE_GESTURE_WAKEUP                   (VS_ID_WAKE_GESTURE+32)
#define VS_ID_GLANCE_GESTURE_WAKEUP                 (VS_ID_GLANCE_GESTURE+32)
#define VS_ID_PICKUP_GESTURE_WAKEUP                 (VS_ID_PICKUP_GESTURE+32)
#define VS_ID_ACTIVITY_WAKEUP                       (VS_ID_ACTIVITY+32)

#if BHY_DEBUG
#define VS_ID_DEBUG                         245
#endif
#define VS_ID_TIMESTAMP_LSW_WAKEUP          246
#define VS_ID_TIMESTAMP_MSW_WAKEUP          247
#define VS_ID_META_EVENT_WAKEUP             248
#define VS_ID_BSX_C                         249
#define VS_ID_BSX_B                         250
#define VS_ID_BSX_A                         251
#define VS_ID_TIMESTAMP_LSW                 252
#define VS_ID_TIMESTAMP_MSW                 253
#define VS_ID_META_EVENT                    254

#endif /* BHY_UC_DRIVER_CONSTANTS_H_ */
