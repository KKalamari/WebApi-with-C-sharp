using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using MyWebApi.Models;

namespace MyWebApi
{
    [ApiController]
    [Route("[controller]")]
    public class DataForDb : ControllerBase
    {
        //connection string for the database
        private readonly string _connectionString = "Server=DESKTOP-79T840S;Database=Positions;Integrated Security =Yes;Trusted_Connection=True;TrustServerCertificate=True;";

        //method to establish a database connection 
        private SqlConnection GetSqlConnection()
        {
            SqlConnection connection = new SqlConnection(_connectionString);
            connection.Open();
            return connection;
        }

        //Endpoint 1
        [HttpPost("InsertPosition")]
        public IActionResult InsertPosition(Data data)
        {
            //check if the values for latitude and longitude are acceptable
            if (data.Latitude < -90 || data.Latitude > 90)
            {
                return BadRequest("latitude value must be within the range of -90 to 90 degrees.");
            }

            if (data.Longitude < -180 || data.Longitude > 180)
            {
                return BadRequest("longitude value must be within the range of -180 to 180 degrees.");
            }

            //establish a database connection
            SqlConnection connection = GetSqlConnection();
            SqlCommand command = new SqlCommand("INSERT INTO positions (name, latitude, longitude) VALUES (@Name, @Latitude, @Longitude)", connection);
            {
                //parameters for the SQL command
                command.Parameters.AddWithValue("@Name", data.Name);
                command.Parameters.AddWithValue("@Latitude", data.Latitude);
                command.Parameters.AddWithValue("@Longitude", data.Longitude);
                int rowsAffected = command.ExecuteNonQuery();

                return Ok();
            }
        }

        // Endpoint 2
        [HttpGet("GetPositions")]
        public IActionResult GetPositions()
        {
            List<Data> data = new List<Data>();


            // establish a database connection
            SqlConnection connection = GetSqlConnection();
            SqlCommand command = new SqlCommand("SELECT * FROM positions", connection);
            SqlDataReader reader = command.ExecuteReader();
            {
                while (reader.Read())
                {
                    data.Add(new Data
                    {
                        Name = reader["name"].ToString(),
                        Latitude = Convert.ToDouble(reader["latitude"]),
                        Longitude = Convert.ToDouble(reader["longitude"])
                    });
                }
            }

            return Ok(data);
        }

        // Endpoint 3
        [HttpGet("CalculateDistance")]
        public IActionResult CalculateDistance(string Enter_first_name, string Enter_second_name)
        {
            //retrieve positions from the database
            Data city1 = GetPositionByName(Enter_first_name);
            Data city2 = GetPositionByName(Enter_second_name);

            //check if both positions exist
            if (city1 == null || city2 == null)
            {
                return BadRequest("One or both of the positions were not found.");
            }

            //calculate distance between positions
            double distance = CalculateHaversineDistance(city1.Latitude, city1.Longitude, city2.Latitude, city2.Longitude);
            return Ok(distance);
        }

        //Method to retrieve position from the database by name
        private Data GetPositionByName(string positionName)
        {
            //establish a database connection
            SqlConnection connection = GetSqlConnection();
            SqlCommand command = new SqlCommand("SELECT * FROM positions WHERE name = @Name", connection);
            {
                //give the wanted name
                command.Parameters.AddWithValue("@Name", positionName);

                //execute and read SQL command 
                SqlDataReader reader = command.ExecuteReader();
                {
                    if (reader.Read())
                    {
                        //return the position that was found
                        return new Data
                        {
                            Name = reader["name"].ToString(),
                            Latitude = Convert.ToDouble(reader["latitude"]),
                            Longitude = Convert.ToDouble(reader["longitude"])
                        };
                    }
                }
            }

            //if city was not found return this default position with invalid latitude and longitude
            //i tried to return null but got a warning
            return new Data
            {
                Name = "City not found",
                Latitude = 91, // latitude ranges -90<x<90
                Longitude = 181 // ranges -180<x<180
            };
        }

        //method to calculate Haversine distance between two coordinates 
        //got this from chat.gpt-> https://chat.openai.com/share/61797c3e-d7ec-453a-9fab-47e0f5509624
        public static double CalculateHaversineDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double radiusEarthKilometers = 6371; // Earth's radius in kilometers
            const double degreesToRadians = Math.PI / 180.0;

            // Convert latitude and longitude from degrees to radians
            double dLat = (lat2 - lat1) * degreesToRadians;
            double dLon = (lon2 - lon1) * degreesToRadians;

            // Convert latitudes to radians
            lat1 = lat1 * degreesToRadians;
            lat2 = lat2 * degreesToRadians;

            // Calculate Haversine distance
            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                       Math.Sin(dLon / 2) * Math.Sin(dLon / 2) * Math.Cos(lat1) * Math.Cos(lat2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return radiusEarthKilometers * c;
        }
    }
}
