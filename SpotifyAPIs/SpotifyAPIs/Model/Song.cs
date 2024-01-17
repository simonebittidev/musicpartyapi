using System;
using Newtonsoft.Json;

namespace SpotifyAPIs.Model
{
	public class Song
	{
        [JsonProperty(PropertyName = "image_url")]
        public string ImageUrl { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        public Song()
		{
		}
	}
}

