using DOOR_TRIM_INSPECTION.Class;
using DOOR_TRIM_INSPECTION.Device.DIO.AJIN;
using System;

namespace DOOR_TRIM_INSPECTION.Device.DIO
{
    public class AJIN_DIO_comm
    {
        private AJIN_DIO dio = new AJIN_DIO();
        private const int DATA_INPUT_LENGTH = 4;
        private const int DATA_OUTPUT_LENGTH = 16;
        private uint[] _input = new uint[DATA_INPUT_LENGTH];
        private uint[] _output = new uint[DATA_OUTPUT_LENGTH];

        public void Open()
        {
#if PLC_TEST
#else
            try
            {
                dio.LoadDevice();
            }
            catch(Exception ex) { Machine.logger.WriteException(eLogType.ERROR, ex); }
#endif
        }

        public void GetDIOData()
        {
            dio.ReadData();
            for (int i = 0; i < DATA_INPUT_LENGTH; i++)
            {
                _input[i] = dio.INPUT_DATA[i];
            }
            for (int i = 0; i < DATA_OUTPUT_LENGTH; i++)
            {
                _output[i] = dio.OUTPUT_DATA[i];
            }
        }

        public void SendProductType(DOOR type)
        {
            //ClearSetData();
            if (type == DOOR.FRONT_LEFT)
            {
                _output[(int)DIO_OUTPUT_DATA.MODEL_FRONT_REAR] = 0;
                _output[(int)DIO_OUTPUT_DATA.MODEL_LEFT_RIGHT] = 0;
            }
            else if (type == DOOR.FRONT_RIGHT)
            {
                _output[(int)DIO_OUTPUT_DATA.MODEL_FRONT_REAR] = 0;
                _output[(int)DIO_OUTPUT_DATA.MODEL_LEFT_RIGHT] = 1;
            }
            else if (type == DOOR.REAR_LEFT)
            {
                _output[(int)DIO_OUTPUT_DATA.MODEL_FRONT_REAR] = 1;
                _output[(int)DIO_OUTPUT_DATA.MODEL_LEFT_RIGHT] = 0;
            }
            else
            {
                _output[(int)DIO_OUTPUT_DATA.MODEL_FRONT_REAR] = 1;
                _output[(int)DIO_OUTPUT_DATA.MODEL_LEFT_RIGHT] = 1;
            }
            WriteData();
            //dio.SendDIOByte();
        }
        //public DOOR GetProductType()
        //{
        //    GetDIOData();

        //    if (_input[0] == 0 && _input[1] == 0)
        //        return DOOR.FRONT_RIGHT;
        //    else if (_input[0] == 0 && _input[1] == 1)
        //        return DOOR.FRONT_LEFT;
        //    else if (_input[0] == 1 && _input[1] == 0)
        //        return DOOR.REAR_RIGHT;
        //    else
        //        return DOOR.REAR_LEFT;

        //}

        public bool GetCaptureStart()
        {
            GetDIOData();
            return _input[0] == 1;
        }

        public DateTime GetSampleOutTime()
        {
            int waitTime = 0;
            while (true)
            {
                GetDIOData();


                if (_input[2] != 1 && waitTime < 10000) // WAIT UNTIL INPUT 2 IS SET OR 10 SECOND PASSED
                {

                    waitTime += 1;
                    System.Threading.Thread.Sleep(1);

                }
                else
                {
                    Console.WriteLine($"Time {waitTime} to go out after inspection");
                    break;
                }
            }
            return DateTime.Now;
        }

        public void WriteData()
        {
            for (int i = 0; i < DATA_OUTPUT_LENGTH; i++)
            {
                dio.OUTPUT_DATA[i] = _output[i];
            }
            dio.SendDIOByte();
        }
        public void ClearData()
        {
            SetOutputData(0);
            WriteData();
        }

        private void ClearSetData()
        {
            for (int idx = 0; idx < DATA_OUTPUT_LENGTH; idx++)
                _output[idx] = 0;
        }

        public void SendSetStartMove()
        {
            _output[(int)DIO_OUTPUT_DATA.START_MOVE] = 1;
            WriteData();
        }

        public void SendSetLightRest(bool isReset)
        {
            _output[(int)DIO_OUTPUT_DATA.LIGHT_RESET] = isReset ? (uint)1 : 0;
            Machine.logger.Write(eLogType.SEQ, "SendSetLightRest : " + _output[(int)DIO_OUTPUT_DATA.LIGHT_RESET]);
            WriteData();
        }

        public void SendResetStartMove()
        {
            _output[(int)DIO_OUTPUT_DATA.START_MOVE] = 0;
            WriteData();
        }

        public void SendResult(bool isOk)
        {
            //ClearSetData();
            _output[(int)DIO_OUTPUT_DATA.RESULT_OK] = 0;
            _output[(int)DIO_OUTPUT_DATA.RESULT_NG] = 0;
            if (isOk)
                _output[(int)DIO_OUTPUT_DATA.RESULT_OK] = 1;
            else
                _output[(int)DIO_OUTPUT_DATA.RESULT_NG] = 1;

            WriteData();
        }
        public void SendResetResult()
        {
            _output[(int)DIO_OUTPUT_DATA.RESULT_OK] = 0;
            _output[(int)DIO_OUTPUT_DATA.RESULT_NG] = 0;
            WriteData();
        }

        /// <summary>
        /// 0x0000~0xFFFF
        /// </summary>
        /// <param name="outputData"></param>
        public void SetOutputData(string outputData)
        {
            uint data = uint.Parse(outputData);
            SetOutputData(data);
        }

        /// <summary>
        /// 0x0000~0x000F
        /// </summary>
        /// <param name="data"></param>
        public void SetOutputData(uint data)
        {
            for (int idx = 0; idx < DATA_OUTPUT_LENGTH; idx++)
                _output[idx] = Convert.ToUInt16(data >> idx & 1);
        }

        /// <summary>
        /// 0 or 1 Max Length
        /// </summary>
        /// <param name="data"></param>
        public void SetOutputData(uint[] data)
        {
            for (int idx = 0; idx < DATA_OUTPUT_LENGTH; idx++)
            {
                if (idx < DATA_OUTPUT_LENGTH)
                    _output[idx] = data[idx];
                else
                    _output[idx] = 0;
            }
        }

    }
}
