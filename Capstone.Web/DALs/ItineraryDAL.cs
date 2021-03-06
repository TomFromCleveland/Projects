﻿using Capstone.Web.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace Capstone.Web.DALs
{
    public class ItineraryDAL : IItineraryDAL
    {
        private string _connectionString;

        public ItineraryDAL(string connectionString)
        {
            _connectionString = connectionString;
        }

        public ItineraryModel CreateNewItinerary(ItineraryModel itinerary)
        {
            int newItineraryAdded = 0;

            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(@"INSERT INTO itinerary (name, itinerary_date, user_id, starting_latitude, starting_longitude) 
                                                      VALUES (@name, @itineraryDate, @userID, @startingLatitude, @startingLongitude); SELECT cast(Scope_Identity() as int)", conn);

                    cmd.Parameters.AddWithValue("@name", itinerary.ItineraryName);
                    cmd.Parameters.AddWithValue("@itineraryDate", itinerary.Date);
                    cmd.Parameters.AddWithValue("@userID", itinerary.UserID);
                    cmd.Parameters.AddWithValue("@startingLatitude", itinerary.StartingLatitude);
                    cmd.Parameters.AddWithValue("@startingLongitude", itinerary.StartingLongitude);

                    newItineraryAdded = (int)cmd.ExecuteScalar();
                    itinerary.ID = newItineraryAdded;
                }
            }
            catch (SqlException e)
            {
                Console.WriteLine(e.Message);
            }
            return itinerary;
        }

        public bool AddLandmarkToItinerary(int landmarkID, int itineraryID)
        {
            int additionSuccess = 0;

            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand(@"INSERT INTO itinerary_landmark (itinerary_id, landmark_id)
                                                      VALUES (@itineraryID, @landmarkID)", conn);

                    cmd.Parameters.AddWithValue("@itineraryID", itineraryID);
                    cmd.Parameters.AddWithValue("@landmarkID", landmarkID);
                    additionSuccess = cmd.ExecuteNonQuery();
                }
            }
            catch (SqlException e)
            {
                Console.WriteLine(e.Message);
            }

            return (additionSuccess == 1);
        }

        public List<ItineraryModel> GetAllItineraries(int userId)
        {
            List<ItineraryModel> itineraries = new List<ItineraryModel>();

            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(@"SELECT * 
                                                      FROM itinerary
                                                      WHERE itinerary.user_id = @userID", conn);

                    cmd.Parameters.AddWithValue("@userID", userId);
                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {

                        itineraries.Add(new ItineraryModel()
                        {
                            ID = Convert.ToInt32(reader["id"]),
                            StartingLatitude = Convert.ToDouble(reader["starting_latitude"]),
                            StartingLongitude = Convert.ToDouble(reader["starting_longitude"]),
                            Date = Convert.ToDateTime(reader["itinerary_date"]),
                            ItineraryName = Convert.ToString(reader["name"])
                        });
                    }
                }
            }
            catch (SqlException e)
            {
                Console.WriteLine(e.Message);
            }
            return itineraries;
        }

        public bool DeleteItinerary(ItineraryModel itinerary)
        {
            bool itineraryDeletion = false;
            int linkTableDeletions = 0;

            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand(@"DELETE FROM itinerary_landmark 
                                                      WHERE itinerary_landmark.itinerary_id = @itineraryID
                                                      AND itinerary_landmark.landmark_id = @landmarkID", conn);

                    cmd.Parameters.AddWithValue("@itineraryID", itinerary.ID);
                    cmd.Parameters.Add("@landmarkID", SqlDbType.Int);

                    foreach (var landmark in itinerary.LandmarkList)
                    {
                        cmd.Parameters["@landmarkID"].Value = landmark.ID;

                        linkTableDeletions += cmd.ExecuteNonQuery();
                    }

                    cmd.CommandText = @"DELETE FROM itinerary
                                        WHERE itinerary.id = @itineraryID";

                    itineraryDeletion = (cmd.ExecuteNonQuery() != 0);
                }
            }
            catch (SqlException e)
            {
                Console.WriteLine(e.Message);
            }

            return (itineraryDeletion && (linkTableDeletions == itinerary.LandmarkList.Count));
        }

        public ItineraryModel GetItineraryDetail(int itineraryID)
        {
            ItineraryModel itinerary = new ItineraryModel();
            itinerary.LandmarkList = new List<LandmarkModel>();

            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand(@"SELECT landmark.id as landmark_id, landmark.image_name, landmark.landmark_description, landmark.name as landmark_name,
                                                      landmark.admin_approved, landmark.latitude, landmark.longitude, landmark.google_api_placeID, itinerary.id as itinerary_id, itinerary.name as itinerary_name,
                                                      itinerary.itinerary_DATE, itinerary.user_id, itinerary.starting_latitude, itinerary.starting_longitude
                                                      FROM landmark
                                                      INNER JOIN itinerary_landmark ON landmark.id = itinerary_landmark.landmark_id 
                                                      INNER JOIN itinerary ON itinerary_landmark.itinerary_id = itinerary.id
                                                      WHERE itinerary_landmark.itinerary_id = @itineraryID", conn);

                    cmd.Parameters.AddWithValue("@itineraryID", itineraryID);
                    SqlDataReader reader = cmd.ExecuteReader();




                    while (reader.Read())
                    {
                        itinerary.ID = itineraryID;
                        itinerary.Date = Convert.ToDateTime(reader["itinerary_DATE"]);
                        itinerary.ItineraryName = Convert.ToString(reader["itinerary_name"]);
                        itinerary.StartingLatitude = Convert.ToDouble(reader["starting_latitude"]);
                        itinerary.StartingLongitude = Convert.ToDouble(reader["starting_longitude"]);
                        itinerary.UserID = Convert.ToInt32(reader["user_id"]);

                        itinerary.LandmarkList.Add(new LandmarkModel()
                        {
                            ID = Convert.ToInt32(reader["landmark_id"]),
                            ImageName = Convert.ToString(reader["image_name"]),
                            IsApproved = Convert.ToBoolean(reader["admin_approved"]),
                            Description = Convert.ToString(reader["landmark_description"]),
                            Name = Convert.ToString(reader["landmark_name"]),
                            Longitude = Convert.ToDouble(reader["longitude"]),
                            Latitude = Convert.ToDouble(reader["latitude"]),
                            GooglePlacesID = Convert.ToString(reader["google_api_placeID"])
                        });
                    }


                }
            }
            catch (SqlException e)
            {
                Console.WriteLine(e.Message);
            }
            return itinerary;
        }

        public ItineraryModel GetItineraryByID(int itineraryID)
        {
            ItineraryModel itinerary = new Models.ItineraryModel();
            itinerary.LandmarkList = new List<LandmarkModel>();
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand(@"SELECT landmark.id as landmark_id, landmark.image_name, landmark.landmark_description, landmark.name as landmark_name,
                                                      landmark.admin_approved, landmark.latitude, landmark.longitude, landmark.google_api_placeID, itinerary.id as itinerary_id, itinerary.name as itinerary_name,
                                                      itinerary.itinerary_DATE, itinerary.user_id, itinerary.starting_latitude, itinerary.starting_longitude
                                                      FROM landmark
                                                      INNER JOIN itinerary_landmark ON landmark.id = itinerary_landmark.landmark_id 
                                                      INNER JOIN itinerary ON itinerary_landmark.itinerary_id = itinerary.id
                                                      WHERE itinerary_landmark.itinerary_id = @itineraryID", conn);

                    cmd.Parameters.AddWithValue("@itineraryID", itineraryID);
                    SqlDataReader reader = cmd.ExecuteReader();
                    if (!reader.HasRows)
                    {
                        reader.Close();
                        cmd = new SqlCommand(@"SELECT * FROM itinerary WHERE id=@itineraryID", conn);
                        cmd.Parameters.AddWithValue("@itineraryID", itineraryID);
                        reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            itinerary.ID = itineraryID;
                            itinerary.Date = Convert.ToDateTime(reader["itinerary_DATE"]);
                            itinerary.ItineraryName = Convert.ToString(reader["name"]);
                            itinerary.StartingLatitude = Convert.ToDouble(reader["starting_latitude"]);
                            itinerary.StartingLongitude = Convert.ToDouble(reader["starting_longitude"]);
                            itinerary.UserID = Convert.ToInt32(reader["user_id"]);
                        }
                    }
                    else
                    {

                        while (reader.Read())
                        {
                            itinerary.ID = itineraryID;
                            itinerary.Date = Convert.ToDateTime(reader["itinerary_DATE"]);
                            itinerary.ItineraryName = Convert.ToString(reader["itinerary_name"]);
                            itinerary.StartingLatitude = Convert.ToDouble(reader["starting_latitude"]);
                            itinerary.StartingLongitude = Convert.ToDouble(reader["starting_longitude"]);
                            itinerary.UserID = Convert.ToInt32(reader["user_id"]);

                            itinerary.LandmarkList.Add(new LandmarkModel()
                            {
                                ID = Convert.ToInt32(reader["landmark_id"]),
                                ImageName = Convert.ToString(reader["image_name"]),
                                IsApproved = Convert.ToBoolean(reader["admin_approved"]),
                                Description = Convert.ToString(reader["landmark_description"]),
                                Name = Convert.ToString(reader["landmark_name"]),
                                Longitude = Convert.ToDouble(reader["longitude"]),
                                Latitude = Convert.ToDouble(reader["latitude"]),
                                GooglePlacesID = Convert.ToString(reader["google_api_placeID"])
                            });
                        }
                    }

                }
            }
            catch (SqlException e)
            {
                Console.WriteLine(e.Message);
            }

            return itinerary;
        }

        public bool DeleteLandmarkFromItinerary(int landmarkID, int itineraryID)
        {
            int deletedRows = 0;
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand(@"DELETE FROM itinerary_landmark 
                                                      WHERE itinerary_landmark.itinerary_id = @itineraryID
                                                      AND itinerary_landmark.landmark_id = @landmarkID", conn);
                    cmd.Parameters.AddWithValue("@itineraryID", itineraryID);
                    cmd.Parameters.AddWithValue("@landmarkID", landmarkID);
                    deletedRows = cmd.ExecuteNonQuery();


                }
            }
            catch (SqlException e)
            {
                Console.WriteLine(e.Message);
            }


            return (deletedRows == 1);
        }

    }
}

