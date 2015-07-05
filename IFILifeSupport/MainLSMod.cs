using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.IO;
using KSP;


namespace IFILifeSupport
{
    public class IFILifeSupport : PartModule  // Life Support Consumption Module
    {

        public bool initialized = false;
        public static int HoursPerDay { get { return GameSettings.KERBIN_TIME ? 6 : 24; } } // Make sure LS remaining Display conforms to Kerbin time setting.

        // Right Click Info display for Part
        [KSPField(guiActive = true, guiName = "Life Support Status", isPersistant = false)]
        public string lifeSupportStatus;
        [KSPField(guiActive = true, guiName = "Life Support", guiUnits = " Days ", guiFormat = "F2", isPersistant = false)]
        public float displayRate;
        [KSPField(guiActive = false, isPersistant = true)]
        public bool RescueFlag;


#if !DEBUG
        // Debug Button for right click info - TO BE removed after testing.
        [KSPField(guiActive = true, guiName = "Debug Logging", isPersistant = false)]
        public string DebugStatus = "Disabled";
        [KSPEvent(name = "ToggleDebug", active = true, guiActive = true, guiActiveUnfocused = true, guiName = "LS Debug Info")]

        public void ToggleDebug()
        {
            if (IFIDebug.IsON)
            {
                IFIDebug.Toggle();
                DebugStatus = "Disabled";
            }
            else
            {
                IFIDebug.Toggle();
                DebugStatus = "Enabled";
            }
        }
#endif

        public override string GetInfo()
        {
            return "Interstellar Flight Inc. Life Support Systems MK XV Installed";
        }
    
 
        public override void OnUpdate()
        {
  
            #if !DEBUG 
            if (IFIDebug.IsON) { DebugStatus = "Active"; } else { DebugStatus = "Inactive"; }
            #endif

            base.OnUpdate();
            int crewCount = 0;
            crewCount = this.part.protoModuleCrew.Count;
            if (crewCount > 0 )
            {
                this.Fields[1].guiActive = true;
                Vessel active = this.part.vessel;
                if (active.mainBody.theName == "Kerbin" && active.altitude <= 12123)
                {
                    lifeSupportStatus = "Air Intake";
                }
                else
                {
                    lifeSupportStatus = "Active";
                }
                
                    double ResourceAval = IFIGetAllResources("LifeSupport");
                     displayRate = (float)((ResourceAval / (LifeSupportRate.GetRate() * IFIGetAllKerbals())) / HoursPerDay / 60 / 60);
                    
            }  //end of if crew > 0
            else if (crewCount == 0)
            {
                lifeSupportStatus = "Pod Standby";
                this.Fields[1].guiActive = false;
            }
        }


        private double IFIGetAllResources(string IFIResource)
        {
            double IFIResourceAmt = 0.0;
            Vessel active = this.part.vessel;
            foreach (Part p in active.parts)
            {
                foreach (PartResource pr in p.Resources)
                {
                    if (pr.resourceName.Equals(IFIResource))
                    {
                        if (pr.flowState)
                        {
                            IFIResourceAmt += pr.amount;
                        }
                    }
                }
            }
            return IFIResourceAmt;
        }

        private int IFIGetAllKerbals() // Find all Kerbals Hiding on Vessel. Show Life Support Remaining Tag is accurate in each pod on vessel
        {
            int KerbalCount = 0;
            Vessel active = this.part.vessel;
            try
            {
                foreach (Part p in active.parts)
                {
                    int IFIcrew = p.protoModuleCrew.Count;
                    if (IFIcrew > 0) KerbalCount += IFIcrew;
                }
            }
            catch (Exception ex) { IFIDebug.IFIMess("Vessel IFI Exception ++Finding Kerbals++MainLSMod " + ex.Message); }
            return KerbalCount;
        }

    }
}