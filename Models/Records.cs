namespace Trails.Models
{
    using Microsoft.EntityFrameworkCore.Metadata.Internal;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;

    public class Trail
    {
        // Properties
        [JsonProperty("id")]
        public string? Id { get; set; }

        [JsonProperty("name")]
        public string? Name { get; set; }

        [JsonProperty("distance")]
        public double Distance { get; set; }

        [JsonProperty("duration")]
        public double Duration { get; set; }

        [JsonProperty("elevationgain")]
        public double ElevationGain { get; set; }

        [JsonProperty("elevationloss")]
        public double ElevationLoss { get; set; }

        [JsonProperty("maxelevation")]
        public double MaxElevation { get; set; }

        [JsonProperty("minelevation")]
        public double MinElevation { get; set; }

        public DifficultyLevel Difficulty { get; set; }

        [JsonProperty("description")]
        public string? Description { get; set; }

        [JsonProperty("imageFile")]
        public string? ImageFile { get; set;}

        [JsonProperty("gpxFile")]
        public string? GPXFile { get; set;}

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

        public Trail(string id) 
        {
            Id = id;
        }

        // Constructor 2 parameters
        public Trail(string name, double distance)
        {
            Name = name;
            Distance = distance;
        }

        // implement all constructors
        public Trail(string name, double distance, double duration, double elevationGain, double elevationLoss, double maxElevation, double minElevation, DifficultyLevel difficulty, string description, string imageUrl, string gpxUrl)
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
