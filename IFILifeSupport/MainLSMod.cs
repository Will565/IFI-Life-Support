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
        private bool WarpCanceled = false;
        public static int HoursPerDay { get { return GameSettings.KERBIN_TIME ? 6 : 24; } } // Make sure LS remaining Display conforms to Kerbin time setting.
        private double Rate_Per_Kerbal;
        private float IFICWLS = 15; // Used to track Kerbal death chance after life support runs out.

        // Right Click Info display for Part
        [KSPField(guiActive = true, guiName = "Life Support Status", isPersistant = false)]
        public string lifeSupportStatus;
        [KSPField(guiActive = true, guiName = "Life Support", guiUnits = " Days ", guiFormat = "F2", isPersistant = false)]
        public float displayRate;
        [KSPField(guiActive = false, isPersistant = true)]
        private int IFITimer; // Used to track LS use on inactive vessels
        [KSPField(guiActive = false, isPersistant = true)]
        private float Rate_Per_Kerbal_Hold; // USed so Vessels in flight have same Usage Rate even when R&D changes to new rate.

#if !DEBUG
        // Debug Button for right click info - TO BE removed after testing.
        [KSPField(guiActive = true, guiName = "Debug =", isPersistant = false)]
        public string DebugStatus = "Inactive";
        [KSPEvent(name = "ToggleDebug", active = true, guiActive = true, guiActiveUnfocused = true, guiName = "LS Debug Info")]
        public void ToggleDebug()
        {
            if (IFIDebug.IsON)
            {
                IFIDebug.Toggle();
                DebugStatus = "Inactive";
            }
            else
            {
                IFIDebug.Toggle();
                DebugStatus = "Active";
            }
        }
#endif

        public override string GetInfo()
        {
            return "Interstellar Flight Inc. Life Support Systems MK IX Installed";
        }

        public override void OnUpdate()
        {
  
#if !DEBUG 
            if (IFIDebug.IsON) { DebugStatus = "Active"; } else { DebugStatus = "Inactive"; }
#endif

            if (!initialized) Initialize();
            base.OnUpdate();
            int crewCount = 0;
            double RATE = 1; // used to adjust LS consumption in diferent Areas
            crewCount = this.part.protoModuleCrew.Count;
            if (crewCount > 0 || EVAReset.Status != "NO")
            {
                this.Fields[1].guiActive = true;
                Vessel active = this.part.vessel;
                if (active.mainBody.theName == "Kerbin" && active.altitude <= 12123)
                {
                    lifeSupportStatus = "Air Intake";
                    RATE = 0.50;
                }
                else
                {
                    lifeSupportStatus = "Active";
                }
                    int TTtest = Convert.ToInt32(Planetarium.fetch.time) - IFITimer;
                    double ResourceAval = IFIGetAllResources("LifeSupport");
                    displayRate = (float)((ResourceAval / (Rate_Per_Kerbal * IFIGetAllKerbals())) / HoursPerDay / 60 / 60);
                    double ElectricChargeAval = IFIGetAllResources("ElectricCharge");
                    
                    if (!WarpCanceled && displayRate <= 2) { TimeWarp.SetRate(0, true); WarpCanceled = true; } // cancel warp once when caution/warning lvl reached
                    if (WarpCanceled && displayRate > 2) { WarpCanceled = false; }
                    if (displayRate >= 0 && displayRate <= 2) { lifeSupportStatus = "CAUTION"; }
                    else if (displayRate <= 0) { lifeSupportStatus = "Warning!"; }

                    if (TTtest >= 1800 || EVAReset.Status != "NO" && EVAReset.EVAPart == part ) // only consume resources every half Hour or on enter or exit of kerbal from eva. trying to control lag with large crews and vessels
                    {
                        IFIDebug.IFIMess("####### START - Call to use Resources - - "+active.vesselName);
                        Use_Life_Support(active, crewCount, ResourceAval, TTtest, RATE);
                        IFITimer = Convert.ToInt32(Planetarium.fetch.time);
                        IFIDebug.IFIMess("####### END - Call to use Resources - - " + active.vesselName);
                    }
            }  //end of if crew > 0
            else if (crewCount == 0)
            {
                lifeSupportStatus = "Inactive";
                this.Fields[1].guiActive = false;
                IFITimer = Convert.ToInt32(Planetarium.fetch.time);
            }
        }

        private void Use_Life_Support(Vessel active, int crewCount, double ResourceAval, int TTtest, double RATE)
        {
            IFIDebug.IFIMess(" Rate per Kerbal " + Convert.ToString((float)((Rate_Per_Kerbal * 1000000)*RATE)));
            IFIDebug.IFIMess(" POD LS Number of Seconds Since last LS use == " + Convert.ToString(TTtest));
            IFIDebug.IFIMess(" POD LS Number of crew in Pod == " + Convert.ToString(crewCount));

            if (EVAReset.Status == "Exit") { crewCount += 1; EVAReset.RESET(); } // Use LS for Kerbal leaving Pod
            if (EVAReset.Status == "Enter") { crewCount -= 1; EVAReset.RESET(); } // Don't Use LS for Kerbal entering Pod

            IFIDebug.IFIMess(" POD LS Number of crew for using LS == " + Convert.ToString(crewCount));

            double resourceRequest = ((Rate_Per_Kerbal * crewCount) * TTtest) * RATE;
            double ElectricRequest = (Rate_Per_Kerbal * crewCount) * TTtest;
            if (resourceRequest > 0)
            {
                if (ResourceAval < resourceRequest)
                {
                    double LSTest = resourceRequest - ResourceAval;
                    if (LSTest >= 2.0)  // Kill crew if Life Support resource run out during unfocus time.
                    {
                        IFIDebug.IFIMess(" Resource request Greater than Aval Resources --" + Convert.ToString(LSTest));
                        IFICWLS += (float)(LSTest * 10.0);
                        CrewTest();
                    }
                    resourceRequest = ResourceAval;
                }
                double resourceReturn = active.rootPart.RequestResource("LifeSupport", resourceRequest);
                double ElectricReturn;
                if (TTtest >= 2200)       // This is a check for Solar Power after vehicle comes back active
                {
                    int CountSP = 0;
                    foreach (ModuleDeployableSolarPanel PP in active.FindPartModulesImplementing<ModuleDeployableSolarPanel>().ToList())
                    {
                        if (PP && PP.stateString == "EXTENDED") { CountSP += 1; }  // Count Extended unbroken Panels-
                    }
                    IFIDebug.IFIMess(" POD Active Solar Panel count is == " + Convert.ToString(CountSP));
                    double SolarPower = CountSP * 0.51;
                    SolarPower = SolarPower * TTtest;
                    ElectricRequest = ElectricRequest - SolarPower;
                    ElectricReturn = active.rootPart.RequestResource("ElectricCharge", ElectricRequest);
                    if (ElectricRequest >= 0 && ElectricReturn <= 0) {ElectricReturn = 0 - 1;} else {ElectricReturn = 1;} // Tell System if Electric need was met.
                }
                else
                {   // This is the normal electric use while vehicle is active
                    ElectricReturn = active.rootPart.RequestResource("ElectricCharge", ElectricRequest);
                }
                IFIDebug.IFIMess(" POD LS Resource Avalible == " + Convert.ToString(ResourceAval));
                IFIDebug.IFIMess(" POD LS Resource Return == " + Convert.ToString(resourceReturn));
                IFIDebug.IFIMess(" POD Elect Resource Return == " + Convert.ToString(ElectricReturn));

                if (resourceReturn <= 0 || ElectricReturn <= 0)
                {
                    IFIDebug.IFIMess(" POD Crew has no LS or Electric Charge Remaining");
                    TimeWarp.SetRate(0, true);
                    CrewTest(); // Check for crew death
                }
                else
                { IFICWLS = 15; } // Reset death chance if resources are avalible
            }

        }
        private void Initialize()
        {

            try
            {
                if (Rate_Per_Kerbal_Hold == 0.0)
                { Rate_Per_Kerbal = LifeSupportRate.GetTechRate(); Rate_Per_Kerbal_Hold = (float)(Rate_Per_Kerbal * 1000000); }
                else
                { Rate_Per_Kerbal = Rate_Per_Kerbal_Hold / 1000000; }
            }
            catch (Exception ex)
            { Debug.Log("IFI Exception while loading Rate Per Kerbal " + ex.Message); }
            if (IFITimer < 1) IFITimer = Convert.ToInt32(Planetarium.fetch.time);
            initialized = true;
            Vessel active = this.part.vessel;
            IFIDebug.IFIMess(this.part.vessel.vesselName+" POD Init(): OnInit Fired -- Vessel SOI is -" + active.mainBody.theName);
        }

        private void CrewTest()
        {
            float rand;
            ProtoCrewMember iCrew;
            for (int i = 0; i < this.part.protoModuleCrew.Count; i++)
            {
                rand = UnityEngine.Random.Range(0.0f, 100.0f);
                if (IFICWLS > rand)
                {
                    iCrew = this.part.protoModuleCrew[i];
                    this.part.RemoveCrewmember(iCrew);// Remove crew from part
                    iCrew.Die();  // Kill crew after removal or death will reset to active.
                    IFIDebug.IFIMess(this.part.vessel.vesselName+" POD Kerbal Killed due to no LS - " + iCrew.name);
                }
            }
            IFICWLS += 5; // Increase chance of death on next check.
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
            catch (Exception ex) { IFIDebug.IFIMess("Vessel IFI Exception ++Finding Kerbals++ " + ex.Message); }
            return KerbalCount;
        }

    }
}