﻿using DGP.Snap.Framework.Attributes.DataModel;
using Newtonsoft.Json;

namespace DGP.Genshin.Models.MiHoYo.UserInfo
{
    [JsonModel]
    public class PrivacyInvisible
    {
        [JsonProperty("post")] public bool Post { get; set; }
        [JsonProperty("collect")] public bool Collect { get; set; }
        [JsonProperty("watermark")] public bool Watermark { get; set; }
        [JsonProperty("reply")] public bool Reply { get; set; }
        [JsonProperty("post_and_instant")] public bool PostAndInstant { get; set; }
    }

}