﻿using Capstone.Web.DALs;
using Capstone.Web.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;

namespace Capstone.Web.Controllers
{
    public class LandmarkController : Controller
    {
        private ILandmarkDAL landmarkDAL;
        // GET: Landmark
        public LandmarkController(ILandmarkDAL landmarkDAL)
        {
            this.landmarkDAL = landmarkDAL;
        }

        public ActionResult LandmarkList()
        {
            return View("LandmarkList", landmarkDAL.GetAllApprovedLandmarks());
        }

        public ActionResult LandmarkDetail(int landmarkID)
        {
            LandmarkModel landmark = landmarkDAL.GetLandmark(landmarkID);
            return View("LandmarkDetail", landmark);
        }

        public ActionResult SubmitNewLandmark()
        {
            return View("SubmitNewLandmark", new LandmarkModel());

        }

        [HttpPost]
        public ActionResult SubmissionConfirmation(LandmarkModel landmark)
        {
            string imgSrc = "https://maps.googleapis.com/maps/api/streetview?size=600x300&location=" + landmark.Latitude + "," + landmark.Longitude + "&heading=151.78&pitch=-0.76&key=" + "AIzaSyDu0VhcsrEx_f3CdQFVOC_Sw3r29lWBnYA";
            landmark.ImageName = imgSrc;

            landmark.SubmissionSuccessful = landmarkDAL.SubmitNewLandmark(landmark);
            return View("SubmissionConfirmation", landmark);
        }
        

        public ActionResult UnapprovedLandmarkList()
        {
            return View("UnapprovedLandmarkList", landmarkDAL.GetAllUnapprovedLandmarks());
        }

        [HttpPost]
        public ActionResult ApproveLandmarks(List<LandmarkModel> landmarksList)
        {
            landmarkDAL.ApproveLandmarks(landmarksList);

            //Ask Josh: should this be a redirect? why or why not?
            ModelState.Clear();
            return View("UnapprovedLandmarkList", landmarkDAL.GetAllUnapprovedLandmarks());
        }
    }
}