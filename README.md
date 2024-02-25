# Trails API 🏞️
[![Build Status](https://dev.azure.com/cesypozo2/Hiking%20Trails/_apis/build/status%2Fcesar2.TrailsAPI?branchName=master)](https://dev.azure.com/cesypozo2/Hiking%20Trails/_build/latest?definitionId=16&branchName=master)

This project is an API built using **ASP .NET Core Web API**. The primary goal of this project is to learn and utilize various **Azure** services and it is being used by [Trails Web Application](https://github.com/cesar2/TrailsWebApplication "Trails ASP .NET MVC Web Application")

It contains the Trail model definition and the API calls GET, POST, DELETE and PUT. It has been deployed in Azure as a **Linux Web App**.

## Class Trail

The `Trail` class represents a trail in the Trails application. It contains the following attributes:

- `Id` (string): The unique identifier of the trail.
- `Name` (string): The name of the trail.
- `Distance` (double): The distance of the trail in kilometres.
- `Duration` (double): The duration of the trail in hours.
- `ElevationGain` (double): The elevation gain of the trail in meters.
- `ElevationLoss` (double): The elevation loss of the trail in meters.
- `MaxElevation` (double): The maximum elevation of the trail in meters.
- `MinElevation` (double): The minimum elevation of the trail in meters.
- `Difficulty` (DifficultyLevel): The difficulty level of the trail. It can be one of the following values: Easy, Moderate, Difficult.
- `Description` (string): The description of the trail.
- `ImageFile` (IFormFile): The image file associated with the trail.
- `ImageUrl` (string): The URL of the image associated with the trail.
- `GPXFile` (IFormFile): The GPX file associated with the trail.
- `GPXUrl` (string): The URL of the GPX file associated with the trail.
- `Waypoint` (nested class): A nested class representing a waypoint on the trail. It contains the following attributes:
  - `Latitude` (double): The latitude of the waypoint.
  - `Longitude` (double): The longitude of the waypoint.

The `Trail` class provides multiple constructors to initialize its attributes. It also includes JSON properties for serialization and deserialization.

These are the operations implemented:

![image](https://github.com/cesar2/TrailsAPI/assets/5868552/0423413c-ccb3-40a6-b474-bd52d87a6bc6)
