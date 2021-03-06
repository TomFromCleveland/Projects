﻿using Capstone.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capstone.Web.DALs
{
    public interface ILandmarkDAL
    {

        List<LandmarkModel> GetAllApprovedLandmarks();
        LandmarkModel GetLandmark(int landmarkID);
        bool SubmitNewLandmark(LandmarkModel landmark);
        List<LandmarkModel> GetAllUnapprovedLandmarks();
        bool ApproveLandmarks(List<LandmarkModel> landmark);
    }
}
