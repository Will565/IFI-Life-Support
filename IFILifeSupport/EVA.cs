using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.IO;
using KSP;


namespace IFILifeSupport
{
    public class IFILifeSupportEVA : PartModule  // Life Support Consumption Module for EVA
    {

        public bool initialized = false;
        private static double Rate_Per_Kerbal = LifeSupportRate.GetRate(); 
        
        // Right Click Info display for Part
        [KSPField(guiActive = true, guiName = "Life Support Pack Status", isPersistant = false)]
        public string lifeSupportStatus;
        [KSPField(guiActive = true, guiName = "Life Support", guiUnits = " HOURS ", guiFormat = "F2", isPersistant = false)]
        public float displayRate;


        public override void OnUpdate()
        {
            base.OnUpdate();
            Vessel active = this.part.vessel;
      
            if (active.isEVA == true)
            {
                if (active.mainBody.theName == "Kerbin" && active.altitude <= 3250)
                {
                    lifeSupportStatus = "Visor";
                }
                else
                {
                    lifeSupportStatus = "Active";
                }
                    double ResourceAval = IFIGetAllResources("LifeSupport");
                    displayRate = (float)((ResourceAval / Rate_Per_Kerbal) / 60 / 60);
                    if (displayRate >= 0 && displayRate <= 1)
                    {
                        lifeSupportStatus = "CAUTION ";
                    }
                    else if (displayRate <= 0)
                    {
                        lifeSupportStatus = "Warning!";
                    }
                    
                
            }

        }


        

 
        private double IFIGetAllResources(string IFIResource)
        {
            double IFIResourceAmt = 0.0;
            foreach (PartResource pr in this.part.Resources)
            {
                if (pr.resourceName.Equals(IFIResource))
                {
                    if (pr.flowState)
                    {
                        IFIResourceAmt += pr.amount;
                    }
                }
            }
            return IFIResourceAmt;
        }
    }
}