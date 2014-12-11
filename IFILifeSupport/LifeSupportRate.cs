using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using KSP.IO;
using KSP;

namespace IFILifeSupport
{
    public static class LifeSupportRate
    {
        private static double Rate_Per_Kerbal = 0.000046;
        public static double GetRate()
        {
            double Hold_Rate = 0.0;
            Hold_Rate = GetTechRate();
            return Hold_Rate;
        }
        private static double GetTechRate()
        {
            double Adjustment = 1.00;
            if (ResearchAndDevelopment.Instance != null)
            {
                Adjustment = 1.5;
                if (ResearchAndDevelopment.GetTechnologyState("advScienceTech") == RDTech.State.Available)
                { Adjustment = 0.90; }
                else if (ResearchAndDevelopment.GetTechnologyState("advExploration") == RDTech.State.Available)
                { Adjustment = 0.95; }
                else if (ResearchAndDevelopment.GetTechnologyState("scienceTech") == RDTech.State.Available)
                { Adjustment = 1.00; }
            }

            return Rate_Per_Kerbal * Adjustment;
        }
       
 
    }
}
