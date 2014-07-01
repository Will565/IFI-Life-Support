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
        private bool WarpCanceled = false;
        private static double Rate_Per_Kerbal = LifeSupportRate.GetRate(); 
        private float IFICWLS = 25; // Used to track Kerbal death chance after life support runs out.
        
        // Right Click Info display for Part
        [KSPField(guiActive = true, guiName = "Life Support Pack Status", isPersistant = false)]
        public string lifeSupportStatus;
        [KSPField(guiActive = true, guiName = "Life Support", guiUnits = " HOURS ", guiFormat = "F2", isPersistant = false)]
        public float displayRate;
        [KSPField(guiActive = false, isPersistant = true)]
        private int IFITimer; // Used to track LS use on inactive vessels

        public override void OnUpdate()
        {
            if (!initialized) Initialize();
            base.OnUpdate();
            Vessel active = this.part.vessel;
            if (active.isEVA == true)
            {
                double RATE;
                if (active.mainBody.theName == "Kerbin" && active.altitude <= 3250)
                {
                    lifeSupportStatus = "Visor";
                    RATE = 0;
                }
                else
                {
                    lifeSupportStatus = "Active";
                    RATE = 1;
                }
                    int TTtest = Convert.ToInt32(Planetarium.fetch.time) - IFITimer;
                    double ResourceAval = IFIGetAllResources("LifeSupport");
                    displayRate = (float)((ResourceAval / Rate_Per_Kerbal) / 60 / 60);
                    if (!WarpCanceled && displayRate <= 2) { TimeWarp.SetRate(0, true); WarpCanceled = true; } // cancel warp once when caution/warning lvl reached
                    if (displayRate >= 0 && displayRate <= 1)
                    {
                        lifeSupportStatus = "CAUTION ";
                    }
                    else if (displayRate <= 0)
                    {
                        lifeSupportStatus = "Warning!";
                    }
                    if (TTtest >= 180) // only consume resources every 3 mins try to control lag
                    {
                        IFIDebug.IFIMess(" EVA LS Use");
                        double resourceRequest = 0.0;
                        double resourceReturn = 0.0;
                        resourceRequest = (Rate_Per_Kerbal * TTtest) * RATE;
                        double electricRequest = Rate_Per_Kerbal * TTtest * 1.2;

                        if (ResourceAval < resourceRequest)
                            {
                                double LSTest = resourceRequest - ResourceAval;
                                if (LSTest >= 2.0)  // Kill crew if resources run out
                                {
                                    IFIDebug.IFIMess(this.part.vessel.vesselName + "-EVA- Resource request Greater than Aval Resources -- " + Convert.ToString(LSTest));
                                    IFICWLS += (float)(LSTest * 10.0);
                                    CrewTest();
                                }
                                resourceRequest = ResourceAval;
                            }
                       
                            resourceReturn = this.part.RequestResource("LifeSupport", resourceRequest);
                        KerbalEVA evaPm = active.FindPartModulesImplementing<KerbalEVA>().Single();
                        if (evaPm && evaPm.lampOn)
                        {
                            IFIDebug.IFIMess(" EVA Headlamp is ON!! ");
                           electricRequest = electricRequest * 1.5;
                        }
                        double ElectricReturn = this.part.RequestResource("ElectricCharge", electricRequest);
                       
                        IFIDebug.IFIMess(this.part.vessel.vesselName+"EVA ELect Resource Return == " + Convert.ToString(ElectricReturn));

                        IFIDebug.IFIMess(this.part.vessel.vesselName+"EVA LS resource Avalible == " + Convert.ToString(ResourceAval));
                        IFIDebug.IFIMess(this.part.vessel.vesselName+"EVA LS Resource Return == " + Convert.ToString(resourceReturn));
                        if (RATE > 0 && resourceReturn <= 0 || ElectricReturn <= 0)
                        {
                            IFIDebug.IFIMess(this.part.vessel.vesselName + "EVA Crew has no LS or Charge Remaining ");
                            TimeWarp.SetRate(0, true);
                            CrewTest(); // Check for crew death
                        }
                        else { IFICWLS = 25; }
                        IFITimer = Convert.ToInt32(Planetarium.fetch.time);
                    }
                
            }

        }


        private void Initialize()
        {
            IFIDebug.IFIMess(this.part.vessel.vesselName+"EVA Init(): OnInit Fired ++ EVA");
            if (IFITimer < 1) IFITimer = Convert.ToInt32(Planetarium.fetch.time);
            initialized = true;

        }


        private void CrewTest()
        {

            float rand;

            rand = UnityEngine.Random.Range(0.0f, 100.0f);
            if (IFICWLS > rand)
            {
                ProtoCrewMember iCrew = this.part.protoModuleCrew[0];
                this.part.RemoveCrewmember(iCrew);// Remove crew from part
                iCrew.Die();// Kill crew after removal or death will reset to active.
                IFIDebug.IFIMess(part.vessel.vesselName+"EVA Kerbal Killed due to no LS - " + iCrew.name);
                this.part.explode();
            }
            IFICWLS += 5; // Increase chance of death on next check.        
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