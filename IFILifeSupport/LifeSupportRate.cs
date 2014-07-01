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
            return Rate_Per_Kerbal;
        }
        public static double GetTechRate()
        {
            double Adjustment = 1.00;
            if (ResearchAndDevelopment.Instance != null)
            {
                Debug.Log("#### IFI Life Support Rate Career Mode Found ####");
                if (ResearchAndDevelopment.GetTechnologyState("advScienceTech") == RDTech.State.Available)
                { Adjustment = 0.85; Debug.Log("#### Advanced Science Tech Found ####"); }
                else if (ResearchAndDevelopment.GetTechnologyState("advExploration") == RDTech.State.Available)
                { Adjustment = 0.90; Debug.Log("#### Advanced Exploration Found ####"); }
                else if (ResearchAndDevelopment.GetTechnologyState("scienceTech") == RDTech.State.Available)
                { Adjustment = 0.95; Debug.Log("#### Science Tech Found ####"); }
            }

            return Rate_Per_Kerbal * Adjustment;
        }
       
 
    }
}
