using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DOOR_TRIM_INSPECTION.Class
{
    public class BarCodeHelper
    {
        public BarCodeHelper(string Barcode)
        {
            if (Barcode == null)
                return;
            int idx = 0;
            ALCCD = Barcode.Substring(idx, 4); idx += 4;
            CARCD = Barcode.Substring(idx, 2); idx += 2;
            DOOR_TYPE = int.Parse(Barcode.Substring(idx, 1)); idx += 1;
            DATE = Barcode.Substring(idx, 6); idx += 6;
            SEQ = Barcode.Substring(idx, 4);
            this.Barcode = Barcode;
        }
        public string Barcode { get; set; }
        public string CARCD { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string JEPUMCD
        {
            get
            {
                int type = DOOR_TYPE % 5;
                return "DT" + type.ToString();
            }
        }
        public string ALCCD { get; set; }

        public int DOOR_TYPE { get; set; }

        public string DATE { get; set; }

        public string SEQ { get; set; }
    }

}