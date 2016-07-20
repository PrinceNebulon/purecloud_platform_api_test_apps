using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SDK_Test
{
    [DataContract]
    public class ValueString
    {
        [DataMember(EmitDefaultValue = false, Name = "value")]
        public InstanceString Value { get; set; }

        public ValueString(string value)
        {
            Value = new InstanceString(value);
        }
        public string ToJson()
        {
            return JsonConvert.SerializeObject((object)this, Formatting.Indented);
        }
    }


    [DataContract]
    public class InstanceString
    {
        [DataMember(EmitDefaultValue = false, Name = "instance")]
        public string Instance { get; set; }

        public InstanceString(string instance)
        {
            Instance = instance;
        }
        public string ToJson()
        {
            return JsonConvert.SerializeObject((object)this, Formatting.Indented);
        }
    }
    [DataContract]
    public class ValueInt
    {
        [DataMember(EmitDefaultValue = false, Name = "value")]
        public InstanceInt Value { get; set; }

        public ValueInt(int value)
        {
            Value = new InstanceInt(value);
        }
        public string ToJson()
        {
            return JsonConvert.SerializeObject((object)this, Formatting.Indented);
        }
    }


    [DataContract]
    public class InstanceInt
    {
        [DataMember(EmitDefaultValue = false, Name = "instance")]
        public int Instance { get; set; }

        public InstanceInt(int instance)
        {
            Instance = instance;
        }
        public string ToJson()
        {
            return JsonConvert.SerializeObject((object)this, Formatting.Indented);
        }
    }
}
