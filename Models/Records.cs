namespace Routes.Models
{
    using Microsoft.EntityFrameworkCore.Metadata.Internal;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;

    public class HikingRoute
    {
        // Properties
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        public string Name { get; set; }
        public double DistanceInKilometers { get; set; }
        public double EstimatedDuration { get; set; }
        public List<Waypoint> Waypoints { get; set; }
        public DifficultyLevel Difficulty { get; set; }
        public string StartingPoint { get; set; }
        public string EndingPoint { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set;}
        public string GpxFileUrl { get; set;}
        public string[] Tags { get; set;}

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

        public HikingRoute() { }

        // Constructor
        public HikingRoute(string name, double distance, double duration, List<Waypoint> waypoints, DifficultyLevel difficulty)
        {
            Name = name;
            DistanceInKilometers = distance;
            EstimatedDuration = duration;
            Waypoints = waypoints;
            Difficulty = difficulty;
        }

        // Method to add a waypoint
        public void AddWaypoint(Waypoint waypoint)
        {
            Waypoints.Add(waypoint);
        }

        // Method to remove a waypoint
        public void RemoveWaypoint(Waypoint waypoint)
        {
            Waypoints.Remove(waypoint);
        }
    }
}
