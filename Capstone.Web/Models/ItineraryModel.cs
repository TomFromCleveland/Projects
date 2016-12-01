﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Capstone.Web.Models
{
    public class ItineraryModel
    {
        public int ID { get; set; }
        public int StartingLatitude { get; set; }
        public int StartingLongitude { get; set; }
        public List<LandmarkModel> LandmarkList { get; set; }
        public int UserID { get; set; }
        public string Name { get; set; }
        public DateTime Date { get; set; }
    }
}