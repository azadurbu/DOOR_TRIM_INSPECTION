using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DOOR_TRIM_INSPECTION.Device.Light
{
    public class LightProperty
    {
        public int Level = 1000;
        public int Channel = 1;
        public string Comport = "com8";

        public void SetProperty(LightProperty property)
        {
            this.Level = property.Level;
            this.Channel = property.Channel;
            this.Comport = property.Comport;
        }

        public LightProperty Copy()
        {
            LightProperty property = new LightProperty();
            property.Level = this.Level;
            property.Channel = this.Channel;
            property.Comport = this.Comport;

            return property;
        }
    }
}
