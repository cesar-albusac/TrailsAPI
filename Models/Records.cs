namespace Trails.Models
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore.Metadata.Internal;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class Trail
    {
        [JsonProperty("id")]
        public string? Id { get; set; }

        // Properties
        [JsonProperty("name")]
        public string? Name { get; set; }

        [JsonProperty("distance")]
        public double? Distance { get; set; }

        [JsonProperty("duration")]
        public double? Duration { get; set; }

        [JsonProperty("elevationgain")]
        public double? ElevationGain { get; set; }

        [JsonProperty("elevationloss")]
        public double? ElevationLoss { get; set; }

        [JsonProperty("maxelevation")]
        public double? MaxElevation { get; set; }

        [JsonProperty("minelevation")]
        public double? MinElevation { get; set; }

        public DifficultyLevel? Difficulty { get; set; }

        [JsonProperty("description")]
        public string? Description { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        [HiddenInput]
        [Display(Name = "Portada")]
        public IFormFile? ImageFile { get; set;}

        [JsonProperty("imageUrl")]
        public string? ImageUrl { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        [HiddenInput]
        [Display(Name = "Archivo GPX")]
        public IFormFile? GPXFile { get; set;}

        [JsonProperty("GPXUrl")]
        public string? GPXUrl { get; set; }

        public class Waypoint
        {
            public double Latitude { get; set; }
            public double Longitude { get; set; }
        }

        // Enum for Difficulty Level
        public enum DifficultyLevel
        {
            Easy,
            Moderate,
            Difficult
        }

        public Trail() { }

        // Constructor 2 parameters
        public Trail(string name, double distance)
        {
            Name = name;
            Distance = distance;
        }

        // implement all constructors
        public Trail(string name, double distance, double duration, double elevationGain, double elevationLoss, double maxElevation, double minElevation, DifficultyLevel difficulty, string description, IFormFile imageUrl, IFormFile gpxUrl)
        {
            Name = name;
            Distance = distance;
            Duration = duration;
            ElevationGain = elevationGain;
            ElevationLoss = elevationLoss;
            MaxElevation = maxElevation;
            MinElevation = minElevation;
            Difficulty = difficulty;
            Description = description;
            ImageFile = imageUrl;
            GPXFile = gpxUrl;
        }

        // Constructor
        public Trail(string name, double distance, double duration, List<Waypoint> waypoints, DifficultyLevel difficulty)
        {
            Name = name;
            Distance = distance;
            Duration = duration;
            Difficulty = difficulty;
        }

    }
}
