using System;

namespace DOOR_TRIM_INSPECTION.Device.DIO.AJIN
{
    internal class AJIN_DIO
    {
        private const int DATA_LENGTH = 16;
        private uint[] _input = new uint[DATA_LENGTH];
        private uint[] _output = new uint[DATA_LENGTH];

        public uint[] INPUT_DATA { get { return _input; } }
        public uint[] OUTPUT_DATA { get { return _output; } }


        public bool LoadDevice()
        {
            //++
            // Initialize library 
            if (CAXL.AxlOpen(7) == (uint)AXT_FUNC_RESULT.AXT_RT_SUCCESS)
            {
                uint uStatus = 0;

                if (CAXD.AxdInfoIsDIOModule(ref uStatus) == (uint)AXT_FUNC_RESULT.AXT_RT_SUCCESS)
                {
                    if ((AXT_EXISTENCE)uStatus == AXT_EXISTENCE.STATUS_EXIST)
                    {
                        int nModuleCount = 0;

                        if (CAXD.AxdInfoGetModuleCount(ref nModuleCount) == (uint)AXT_FUNC_RESULT.AXT_RT_SUCCESS)
                        {
                            return true;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            else
            {
                return false;
            }

            return false;
        }

        /// <summary>
        /// _output 값 쓰기
        /// </summary>
        public void SendDIOByte()
        {
            uint data = 0;
            for (int i = 0; i < 16; i++)
            {
                data += (uint)_output[i] << i;
            }
            SendDIOByte(data);
        }

        /// <summary>
        /// MAX = 0xFFFF
        /// </summary>
        /// <param name="uValue"></param>
        public void SendDIOByte(uint uValue)
        {
            int nModuleCount = 0;

            for (int idx = 0; idx < DATA_LENGTH; idx++)
                _output[idx] = Convert.ToUInt16(uValue >> idx & 1);

            CAXD.AxdInfoGetModuleCount(ref nModuleCount);

            if (nModuleCount > 0)
            {
                int nBoardNo = 0;
                int nModulePos = 0;
                uint uModuleID = 0;

                CAXD.AxdInfoGetModule(0, ref nBoardNo, ref nModulePos, ref uModuleID);

                switch ((AXT_MODULE)uModuleID)
                {
                    case AXT_MODULE.AXT_SIO_DO32P:
                    case AXT_MODULE.AXT_SIO_DO32T:
                    case AXT_MODULE.AXT_SIO_RDO32:
                        CAXD.AxdoWriteOutportWord(0, 16, uValue);
                        break;
                    case AXT_MODULE.AXT_SIO_DB32P:
                    case AXT_MODULE.AXT_SIO_DB32T:
                    case AXT_MODULE.AXT_SIO_RDB128MLII:
                        CAXD.AxdoWriteOutportWord(0, 0, uValue);
                        break;
                }
            }
        }

        public void ReadData()
        {
            uint OutView = 0;
            uint InView = 0;
            int nBoardNo = 0;
            int nModulePos = 0;
            uint uModuleID = 0;

            CAXD.AxdInfoGetModule(0, ref nBoardNo, ref nModulePos, ref uModuleID);
            switch ((AXT_MODULE)uModuleID)
            {
                case AXT_MODULE.AXT_SIO_DO32P:
                case AXT_MODULE.AXT_SIO_DO32T:
                case AXT_MODULE.AXT_SIO_RDO32:
                case AXT_MODULE.AXT_SIO_DO32T_P:
                case AXT_MODULE.AXT_SIO_RDO32RTEX:
                    CAXD.AxdoReadOutportWord(0, 0, ref InView);
                    CAXD.AxdoReadOutportWord(0, 1, ref OutView);
                    break;

                default:
                    CAXD.AxdiReadInportWord(0, 0, ref InView);
                    CAXD.AxdiReadInportWord(0, 1, ref OutView);
                    break;
            }

            for (int idx = 0; idx < DATA_LENGTH; idx++)
            {
                _input[idx] = Convert.ToUInt16(InView >> idx & 1);
                _output[idx] = Convert.ToUInt16(OutView >> idx & 1);
            }
        }
    }
}
