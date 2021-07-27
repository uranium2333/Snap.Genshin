﻿using Newtonsoft.Json;

namespace DGP.Genshin.Models.MiHoYo.Record.Avatar
{
    internal class Constellation
    {
        [JsonProperty("id")] public int Id { get; set; }
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("icon")] public string Icon { get; set; }
        [JsonProperty("effect")] public string Effect { get; set; }
        [JsonProperty("is_actived")] public bool IsActived { get; set; }
        [JsonProperty("pos")] public int Position { get; set; }
    }
}
