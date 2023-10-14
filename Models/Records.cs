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
        public TimeSpan EstimatedDuration { get; set; }
        public List<string> Waypoints { get; set; }
        public DifficultyLevel Difficulty { get; set; }
        public string StartingPoint { get; set; }
        public string EndingPoint { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set;}
        public string GpxFileUrl { get; set;}
        public string[] Tags { get; set;}



        // Enum for Difficulty Level
        public enum DifficultyLevel
        {
            Easy,
            Moderate,
            Difficult
        }

        public HikingRoute() { }

        // Constructor
        public HikingRoute(string name, double distance, TimeSpan duration, List<string> waypoints, DifficultyLevel difficulty)
        {
            Name = name;
            DistanceInKilometers = distance;
            EstimatedDuration = duration;
            Waypoints = waypoints;
            Difficulty = difficulty;
        }

        // Method to add a waypoint
        public void AddWaypoint(string waypoint)
        {
            Waypoints.Add(waypoint);
        }

        // Method to remove a waypoint
        public void RemoveWaypoint(string waypoint)
        {
            Waypoints.Remove(waypoint);
        }
    }
}
