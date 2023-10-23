﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace FTAnalyzer.Exports
{
    [JsonObject(MemberSerialization.OptIn)]
    public interface IJsonFamily
    {
        [JsonProperty]
        string FamilyID { get; }
        [JsonProperty]
        string HusbandID { get; }
        [JsonProperty]
        string Husband { get; }
        [JsonProperty]
        string WifeID { get; }
        [JsonProperty]
        string Wife { get; }
        [JsonProperty]
        string Marriage { get; }
    }
}
