using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.IO;
using KSP;
using KSP.UI.Screens;

namespace IFILifeSupport
{

[KSPAddon(KSPAddon.Startup.SpaceCentre, true)]
public class IFI_LIFESUPPORT_TRACKING : UnityEngine.MonoBehaviour
{
             private int IFITIM = 0;
             // Stock APP Toolbar - Stavell
             private ApplicationLauncherButton IFI_Button = null;
                    private Texture2D IFI_button_grn = null;
                    private Texture2D IFI_button_cau = null;
                    private Texture2D IFI_button_danger = null;
             private bool IFI_Texture_Load = false;

             private bool KerbalEVARescueDetect = false;
             private double IFITimer;
             private int IFICWLS = 25;
             private string[,] LS_Status_Hold;
             private int LS_Status_Hold_Count;
             // Make sure LS remaining Display conforms to Kerbin time setting.
             public static int HoursPerDay { get { return GameSettings.KERBIN_TIME ? 6 : 24; } }
             private bool Went_to_Main = false;

 private void OnGUIApplicationLauncherReady()
 {
            // Create the button in the KSP AppLauncher
            
     if (IFI_Button == null)
     {
         IFI_Button = ApplicationLauncher.Instance.AddModApplication(GUIToggle, GUIToggle,
                         null, null,
                         null, null,
                         ApplicationLauncher.AppScenes.TRACKSTATION | ApplicationLauncher.AppScenes.FLIGHT | ApplicationLauncher.AppScenes.SPACECENTER | ApplicationLauncher.AppScenes.MAPVIEW,
                         IFI_button_grn);
     }

     if (!HighLogic.LoadedSceneIsEditor)
     { Life_Support_Update(); }
 }

 private void GUIToggle()
 {
     Life_Support_Update();
     LifeSupportDisplay.LSDisplayActive = !LifeSupportDisplay.LSDisplayActive;       
 }

 private void ResetButton()
 {
     IFI_Button.SetTexture(IFI_button_grn);
 }

    public void Life_Support_Update()
    {
        if (HighLogic.LoadedScene == GameScenes.LOADING || HighLogic.LoadedSceneIsEditor || !HighLogic.LoadedSceneIsGame)
            return; //Don't do anything while the game is loading or in editor
        if (HighLogic.LoadedScene == GameScenes.MAINMENU)
        {
            Went_to_Main = true;
            return; // Don't Run at main menu and reset IFITimer
        }
        if (Went_to_Main)
        {
            IFITimer = Convert.ToInt32(Planetarium.GetUniversalTime());
            Went_to_Main = false;
        }
        double Elapesed_Time = IFI_Get_Elasped_Time();
        IFI_Button.SetTexture(IFI_button_grn); int LS_ALERT_LEVEL = 1;
        if (HighLogic.LoadedSceneIsFlight || HighLogic.LoadedScene == GameScenes.TRACKSTATION || HighLogic.LoadedScene == GameScenes.SPACECENTER)
        {
            LS_Status_Hold = new string[FlightGlobals.Vessels.Count(),5];
            LS_Status_Hold_Count = 0;
            //Debug.Log("######## Looking for Ships ######");
            foreach (Vessel vessel in FlightGlobals.Vessels)
            {

                if (vessel && (
                    vessel.vesselType == VesselType.Ship || vessel.vesselType == VesselType.Lander ||
                    vessel.vesselType == VesselType.Station || vessel.vesselType == VesselType.Rover ||
                    vessel.vesselType == VesselType.Base || vessel.vesselType == VesselType.Probe) ||
                    vessel.vesselType == VesselType.EVA) 
                {
                    //Debug.Log(" Found Vessel");//float distance = (float)Vector3d.Distance(vessel.GetWorldPos3D(), FlightGlobals.ActiveVessel.GetWorldPos3D());
                    string TVname;
                    int IFI_Crew =0;
                    double LSAval;
                    string IFI_Location = "";
                    double IFI_ALT = 0.0;
                    TVname = vessel.vesselName; // vessel.name;
                    if (!vessel.loaded)
                    {
                        IFI_Crew = vessel.protoVessel.GetVesselCrew().Count;
                        IFI_ALT = vessel.protoVessel.altitude;
                        IFI_Location = vessel.mainBody.theName;
                    }
                    else
                    {
                        IFI_Crew = vessel.GetCrewCount();
                        IFI_Location = vessel.mainBody.theName;
                        IFI_ALT = vessel.altitude;
                    }
                    if (IFI_Crew > 0.0)
                    {


                        double LS_Use = LifeSupportRate.GetRate();
                        if (IFI_Location == "Kerbin" && IFI_ALT <= 12123) { LS_Use *= 0.20;  }
                        if (IFI_Location == "Laythe" && IFI_ALT <= 6123) { LS_Use *= 0.60;  }
                        
                        LS_Use *= IFI_Crew;
                        LS_Use *= Elapesed_Time;
                        // IF No EC use more LS resources
                        if (IFIGetAllResources("ElectricCharge", vessel, vessel.loaded) < 0.1) { LS_Use *= 1.2;  }
                        if (LS_Use > 0.0) {
                                double rtest = IFIUSEResources("LifeSupport", vessel, vessel.loaded, LS_Use, IFI_Crew); 
                                                          
                        }


                    LSAval = IFIGetAllResources("LifeSupport",vessel,vessel.loaded);
                            //Debug.Log("Vessel with crew onboard Found: " + TVname + "   Crew=" + IFI_Crew +"    LifeSupport = "+ LSAval +"  Body:"+IFI_Location+"   ALT:"+IFI_ALT);
                            double LS_RR = LifeSupportRate.GetRate();
                            if (IFI_Location == "Kerbin" && IFI_ALT <= 12123) { LS_RR *= 0.20; }
                            if (IFI_Location == "Laythe" && IFI_ALT <= 6123) { LS_RR *= 0.60; }

                            if (IFIGetAllResources("ElectricCharge", vessel, vessel.loaded) < 0.1) { LS_RR *= 1.2; }
                            double days_rem = LSAval / IFI_Crew / LS_RR / 60 / 60 / HoursPerDay;
                    LS_Status_Hold[LS_Status_Hold_Count, 0] = TVname;
                    LS_Status_Hold[LS_Status_Hold_Count, 1] = IFI_Location;
                        string H_Crew = Convert.ToString(IFI_Crew);
                        if (vessel.vesselType == VesselType.EVA) { H_Crew = "EVA"; }
                        LS_Status_Hold[LS_Status_Hold_Count, 2] = H_Crew;
                        if (vessel.vesselType == VesselType.EVA && KerbalEVARescueDetect)
                        {
                            LS_Status_Hold[LS_Status_Hold_Count, 3] = "RESCUE";
                            LS_Status_Hold[LS_Status_Hold_Count, 4] = "RESCUE";
                            LSAval = 200.0;
                            days_rem = 10.0;
                        }
                        else
                        {
                            LS_Status_Hold[LS_Status_Hold_Count, 3] = Convert.ToString(Math.Round(LSAval, 4));
                            LS_Status_Hold[LS_Status_Hold_Count, 4] = Convert.ToString(Math.Round(days_rem, 2));
                        }
                    LS_Status_Hold_Count += 1;
                  
                    if (LS_ALERT_LEVEL < 2 && days_rem < 3) 
                    {
                        IFI_Button.SetTexture(IFI_button_cau); LS_ALERT_LEVEL = 2;
                        if (LifeSupportDisplay.WarpCancel && TimeWarp.CurrentRate > 1) { TimeWarp.SetRate(0, false); }
                    }
                    if (LS_ALERT_LEVEL < 3 && days_rem <= 1) 
                    {
                        IFI_Button.SetTexture(IFI_button_danger); LS_ALERT_LEVEL = 3;
                        if (LifeSupportDisplay.WarpCancel && TimeWarp.CurrentRate > 1) { TimeWarp.SetRate(0, false); }
                    }
                    }
               }
                
            }
          }
    }

    public void Awake()
    {
        IFITimer = Convert.ToInt32(Planetarium.GetUniversalTime());
        GameEvents.onGUIApplicationLauncherReady.Add(OnGUIApplicationLauncherReady);
        DontDestroyOnLoad(this);
        CancelInvoke();
        InvokeRepeating("display_active", 1, 1);
    }

    public void Start()
        {
            if (!IFI_Texture_Load)
            {
                if (IFI_button_grn == null)
                {
                    IFI_button_grn = new Texture2D(38, 38, TextureFormat.ARGB32, false);
                    IFI_button_cau = new Texture2D(38, 38, TextureFormat.ARGB32, false);
                    IFI_button_danger = new Texture2D(38, 38, TextureFormat.ARGB32, false);
                }
                double IHOLD = IFI_Get_Elasped_Time();

                if (GameDatabase.Instance.ExistsTexture("IFILS/Textures/IFI_LS_GRN")) IFI_button_grn = GameDatabase.Instance.GetTexture("IFILS/Textures/IFI_LS_GRN", false);
                if (GameDatabase.Instance.ExistsTexture("IFILS/Textures/IFI_LS_CAU")) IFI_button_cau = GameDatabase.Instance.GetTexture("IFILS/Textures/IFI_LS_CAU", false);
                if (GameDatabase.Instance.ExistsTexture("IFILS/Textures/IFI_LS_DAN")) IFI_button_danger = GameDatabase.Instance.GetTexture("IFILS/Textures/IFI_LS_DAN", false);
                IFI_Texture_Load = true;
            }
           
            }

    private double IFIGetAllResources(string IFIResource, Vessel IV, bool ISLoaded)
        {
            double IFIResourceAmt = 0.0;
            if (ISLoaded)
            {
                if (IV.vesselType != VesselType.EVA)
                {
                foreach (Part p in IV.parts)
                {
                    foreach (PartResource pr in p.Resources)
                    {
                        if (pr.resourceName == IFIResource)
                        {
                            if (pr.flowState)
                            {
                                IFIResourceAmt += pr.amount;
                            }
                        }
                    }
                }
            } else {
                 foreach (PartResource pr in IV.rootPart.Resources)
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
            }
            else // Kerbal EVA code
            {

                foreach (ProtoPartSnapshot p in IV.protoVessel.protoPartSnapshots)
                {

                    foreach (ProtoPartResourceSnapshot r in p.resources)
                    {
                       if (r.resourceName == IFIResource)
                        {


                            double IHold = r.amount;
                            IFIResourceAmt += IHold;
                        }
                    }
                }
              
            }

            return IFIResourceAmt;
        }

    private double IFIUSEResources(string IFIResource, Vessel IV, bool ISLoaded, double UR_Amount, int CREWHOLD)
{
            double Temp_Resource = UR_Amount;

            if (ISLoaded)
            {
               
                double ALL_Resorces = IFIGetAllResources(IFIResource, IV, true);
                if (ALL_Resorces == 0.0)
                {
                    IFI_Check_Kerbals(IV, UR_Amount); 
                    return 0.0;
                }
                if (ALL_Resorces < UR_Amount)
                {
                    double TEST_Mod = (UR_Amount - ALL_Resorces) * 100000;
                    Temp_Resource = IV.rootPart.RequestResource(IFIResource, ALL_Resorces);
                }
                else
                {
                    Temp_Resource = IV.rootPart.RequestResource(IFIResource, UR_Amount);
                }
            }
            else
            {
                KerbalEVARescueDetect = false;
                int PartCountForShip = 0;
                foreach (ProtoPartSnapshot p in IV.protoVessel.protoPartSnapshots)
                {
                    PartCountForShip++;
                    foreach (ProtoPartResourceSnapshot r in p.resources)
                    {
                        if (r.resourceName == IFIResource)
                        {
                            if (UR_Amount <= 0.0) break;
                            double IHold = r.amount;
                            // Fix for Kerbal rescue Missions
                            Int16 RescueTest = -1;
                            ConfigNode RT = p.pVesselRef.discoveryInfo;
                            System.Int16.TryParse(RT.GetValue("state"), out RescueTest);

                            if (PartCountForShip <= 2 && CREWHOLD == 1 && IHold <= 0.0 && RescueTest == 29)
                            {
                                // Add Resources to Rescue Contract POD 1 time
                                IFIDebug.IFIMess("#### IFI LIfeSupport #### Rescue POD Found with No LS Resource TAG Flagging --" + Convert.ToString(RescueTest));
                                KerbalEVARescueDetect = true;
                                IHold = r.maxAmount;                                
                                r.amount = IHold;
                                UR_Amount = 0.0;
                                return 0.0;
                            }
                            if (RescueTest == 29) { KerbalEVARescueDetect = true;
                                return 0.0; } // DO NOT USE LS ON RESCUE POD TILL player gets  CLOSE
                            UR_Amount -= IHold;
                            if (UR_Amount <= 0.0)
                            {
                                IHold -= Temp_Resource;
                                r.amount = IHold;
                                UR_Amount = 0.0;
                            }
                            else
                            {
                                r.amount = 0.0;
                            }

                            Temp_Resource = UR_Amount;
                        }
                    }
                    //
                    if (UR_Amount <= 0.0) break;
                }
                //if (IV.isEVA && )
                //{
                //    IFIDebug.IFIMess("#### IFI LIfeSupport #### Rescue Kerbal Found with No LS Resource TAG Flagging");
                //    UR_Amount = 0.0;
                //}
            }
            return UR_Amount;
}

    private void CrewTestEVA(Vessel IV, double l)
    {
        
        float rand;
        int CUR_CWLS = IFICWLS;
        CUR_CWLS += (Convert.ToInt16(l) * 10);
        rand = UnityEngine.Random.Range(0.0f, 100.0f);
        if (CUR_CWLS > rand)
        {
            if (IV.loaded)
            {
                Part p = IV.rootPart;
                ProtoCrewMember iCrew = p.protoModuleCrew[0];
                iCrew.rosterStatus = ProtoCrewMember.RosterStatus.Dead;
                p.Die();
                IFIDebug.IFIMess(" EVA Kerbal Killed due to no LS - " + iCrew.name);
                string message = "\n\n\n"; message += iCrew.name + ":\n Was killed for Life Support Failure.";
                MessageSystem.Message m = new MessageSystem.Message("Kerbal Death on EVA", message, MessageSystemButton.MessageButtonColor.RED, MessageSystemButton.ButtonIcons.ALERT);
                MessageSystem.Instance.AddMessage(m);
            }
            else
            {
              // Removed Killing Kerbals on EVA when not loaded to fix ghosting bug. 
            }
        }
        
    }
    
    private void CrewTest(int REASON, Part p, double l)
{
       int CUR_CWLS = IFICWLS;
        CUR_CWLS += (Convert.ToInt16(l) * 10);
    float rand;
    ProtoCrewMember iCrew;
    for (int i = 0; i < p.protoModuleCrew.Count; i++)
    {
        rand = UnityEngine.Random.Range(0.0f, 100.0f);
        IFIDebug.IFIMess("!!!!!!!!");
        IFIDebug.IFIMess("Testing Crew Death Crewmember=" + p.protoModuleCrew[i].name);
        IFIDebug.IFIMess("Crew Death Chance = " + Convert.ToString(CUR_CWLS));
        IFIDebug.IFIMess("Crew Death Roll = " + Convert.ToString(rand));
        IFIDebug.IFIMess("!!!!!!!!");

        if (CUR_CWLS > rand)
        {
            iCrew = p.protoModuleCrew[i];
            p.RemoveCrewmember(iCrew);// Remove crew from part
            iCrew.Die();  // Kill crew after removal or death will reset to active.
            IFIDebug.IFIMess(p.vessel.vesselName + " POD Kerbal Killed due to no LS - " + iCrew.name);
            string message = ""; message += p.vessel.vesselName + "\n\n"; message += iCrew.name + "\n Was killed due to ::";
                    message += "No Life Support Remaining";
                    message += "::";
            MessageSystem.Message m = new MessageSystem.Message("Kerbal Death from LifeSupport System", message, MessageSystemButton.MessageButtonColor.RED, MessageSystemButton.ButtonIcons.ALERT);
            MessageSystem.Instance.AddMessage(m);
        }
    }
}

    private void CrewTestProto(int REASON, ProtoPartSnapshot p, double l)
    {
        int CUR_CWLS = IFICWLS;
        CUR_CWLS += (Convert.ToInt16(l) * 10);
        float rand;
        
        ProtoCrewMember iCrew;
        for (int i = 0; i < p.protoModuleCrew.Count; i++)
        {
            rand = UnityEngine.Random.Range(0.0f, 100.0f);
            IFIDebug.IFIMess("!!!!!!!!");
            IFIDebug.IFIMess("Testing Crew Death Crewmember=" + p.protoModuleCrew[i].name);
            IFIDebug.IFIMess("Crew Death Chance = " + Convert.ToString(CUR_CWLS));
            IFIDebug.IFIMess("Crew Death Roll = " + Convert.ToString(rand));
            IFIDebug.IFIMess("!!!!!!!!");

            if (CUR_CWLS > rand)
            {
                iCrew = p.protoModuleCrew[i];
                iCrew.rosterStatus = ProtoCrewMember.RosterStatus.Dead;
                p.RemoveCrew(iCrew);
            
                IFIDebug.IFIMess(p.pVesselRef.vesselName + " POD Kerbal Killed due to no LS - " + iCrew.name);
                string message = ""; message += p.pVesselRef.vesselName + "\n\n"; message += iCrew.name + "\n Was killed due to ::";
                    message += "No Life Support Remaining";
                    message += "::";
                MessageSystem.Message m = new MessageSystem.Message("Kerbal Death from LifeSupport Failure", message, MessageSystemButton.MessageButtonColor.RED, MessageSystemButton.ButtonIcons.ALERT);
                MessageSystem.Instance.AddMessage(m);
                
            }
        }
    }

    private void IFI_Check_Kerbals(Vessel IV, double l) // Find All Kerbals Hiding on Vessel 
        {
                if (IV.vesselType == VesselType.EVA)
                {
                    try {
                       // CrewTestEVA(IV , l);
                    }
                    catch (Exception ex) { IFIDebug.IFIMess("Vessel IFI Exception ++Finding Kerbals++ eva " + ex.Message); }
                }
                else
                {
                    if (IV.loaded)
                    {
                        foreach (Part p in IV.parts)
                        {

                            int IFIcrew = p.protoModuleCrew.Count;
                            if (IFIcrew > 0) { CrewTest(0, p, l); }

                        }
                    }
                    else
                    {
                        foreach (ProtoPartSnapshot p in IV.protoVessel.protoPartSnapshots)
                        {

                            int IFIcrew = p.protoModuleCrew.Count;
                            if (IFIcrew > 0) { CrewTestProto(0, p, l); }

                        }
                    }
            }
    }

    public void display_active()
    {
        IFITIM++;
        if (!HighLogic.LoadedSceneIsEditor && ((LifeSupportDisplay.LSDisplayActive && IFITIM > 3) || (TimeWarp.CurrentRate > 800) || IFITIM > 100))
        {
            Life_Support_Update();
            IFITIM = 0;
        }
        if (HighLogic.LoadedScene == GameScenes.MAINMENU)
            Went_to_Main = true;
    }

    private void OnGUI()
        {
            if (LifeSupportDisplay.LSDisplayActive && !HighLogic.LoadedSceneIsEditor && HighLogic.LoadedSceneIsGame)
            {

                string TITLE = "IFI Vessel Life Support Status Display ";

                LifeSupportDisplay.infoWindowPos = GUILayout.Window(99988, LifeSupportDisplay.infoWindowPos, LSInfoWindow, TITLE ,LifeSupportDisplay.layoutOptions);
            }
		}
			
	private void LSInfoWindow(int windowId){
        float LS_Row = 20;
        
GUILayout.BeginHorizontal(GUILayout.Width(400f));
GUI.Label(new Rect(5, LS_Row, 132, 40), "VESSEL");
GUI.Label(new Rect(150, LS_Row, 80, 40), "LOCATION");
GUI.Label(new Rect(235, LS_Row, 58, 40), "CREW");
GUI.Label(new Rect(285, LS_Row, 112, 40), "   LIFE \nSUPPORT");
GUI.Label(new Rect(355, LS_Row, 85, 40), "     DAYS\nREMAINING");
LS_Row += 36; //14
GUILayout.EndHorizontal();

LifeSupportDisplay.infoScrollPos = GUI.BeginScrollView(new Rect(5, LS_Row, 452, 350), LifeSupportDisplay.infoScrollPos, new Rect(0, 0, 433, LS_Status_Hold_Count * 22));
if (LS_Status_Hold_Count > 0)
        {
            int LLC = 0;
            LS_Row = 4;
           
            while (LLC < LS_Status_Hold_Count)
            {
                    
                    GUI.Label(new Rect(5, LS_Row, 132, 20), LS_Status_Hold[LLC,0]);
                    GUI.Label(new Rect(160, LS_Row, 65, 20), LS_Status_Hold[LLC, 1]);
                    GUI.Label(new Rect(240, LS_Row, 58, 20), LS_Status_Hold[LLC, 2]);
                    GUI.Label(new Rect(285, LS_Row, 112, 20), LS_Status_Hold[LLC, 3]);
                    GUI.Label(new Rect(365, LS_Row, 86, 20), LS_Status_Hold[LLC, 4]);
                    LS_Row += 22;
                
                LLC++;
            }
        }
        GUI.EndScrollView();
        
        LifeSupportDisplay.WarpCancel = GUI.Toggle(new Rect(10,416,400,20),LifeSupportDisplay.WarpCancel, "Auto Cancel Warp on Low Life Support");
 //       GUILayout.BeginHorizontal(GUILayout.Width(400f));
   //     GuiUtils.SimpleTextBox("Safe Distance", autopilot.overridenSafeDistance, "m");
     //   GUILayout.EndHorizontal();
			GUI.DragWindow();
		}

    private double IFI_Get_Elasped_Time()
    {
        double CurrTime = Planetarium.GetUniversalTime();
        double IHOLD = CurrTime - IFITimer;
            
        IFITimer = CurrTime;
        return IHOLD;
    }


}
}