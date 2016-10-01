using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using KSP.IO;



namespace IFILifeSupport
{
    public static class IFIDebug
    {
        public static bool IsON = true;
        public static void IFIMess(string IFIMessage)
        {
            if (IsON)
            {
                Debug.Log("*IFI DEBUG--" + IFIMessage);
            }
        }
        public static void Toggle()
        {
            if (IsON) { IsON = false; } else { IsON = true; }
        }
        
    }



    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class ADDEVAS : UnityEngine.MonoBehaviour
    {

        private static double Rate_Per_Kerbal = LifeSupportRate.GetRate();

   
                public void Awake()
                {
                    Debug.Log(" IFI Preload LS EVA Install started ++++ ");
                        GameEvents.onCrewOnEva.Remove(OnCrewOnEva11);
                        GameEvents.onCrewOnEva.Add(OnCrewOnEva11);
                        GameEvents.onCrewBoardVessel.Remove(OnCrewBoardVessel11);
                        GameEvents.onCrewBoardVessel.Add(OnCrewBoardVessel11);

            // Add Module to Command Parts AND eVA PreFabs
            try
            {
                var IFIParts = PartLoader.LoadedPartsList;// .Where(p => p.category.Equals(1));
                foreach (var IFIPart in IFIParts)
                {
                    if (IFIPart.partPrefab.CrewCapacity > 0)
                    {
                        AddLifeSupport(IFIPart);    
                                     
                    }
                }
                
                
            }
            catch (Exception ex)
            {
                IFIDebug.IFIMess("IFI Exception Searching for Command Parts  " + ex.Message);
            }

#if !DEBUG
            if (IFIDebug.IsON) { IFIDebug.Toggle(); }
#endif
        }


        private void AddLifeSupport(AvailablePart part11)
        {

            

            Part prefabPart = null; 
            prefabPart = part11.partPrefab;
            IFIDebug.IFIMess("IFI Attempting to add LS Module to part: " + part11.name);
            string NAMLSmod = null;
            if (part11.name == "kerbalEVA" || part11.name == "kerbalEVAfemale") { NAMLSmod = "IFILifeSupportEVA"; } else {  NAMLSmod = "IFILifeSupport"; }
            try
            {
                if (NAMLSmod == "IFILifeSupport")
                {
                if (!prefabPart.Modules.Contains(NAMLSmod))
                {
                    IFIDebug.IFIMess(" IFI Preload AddModule no "+ NAMLSmod +" Found Adding LS Mod");
                    PartModule module11 = part11.partPrefab.AddModule(NAMLSmod);
                }
                }
                else
                {
                    IFIDebug.IFIMess(" IFI Preload AddModule EVA Prefab ----");
                    PartModule module11 = part11.partPrefab.AddModule(NAMLSmod);
                }
                IFIDebug.IFIMess(" IFI Preload AddModule("+ NAMLSmod+") Fired no exception " + part11.name);
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Object reference not set"))
                {
                    IFIDebug.IFIMess(" IFI Preload ++Module++  Module(" + NAMLSmod + ") Added to " + part11.name);
                }
                else
                {
                    IFIDebug.IFIMess("EVA IFI Exception +addmodule+ " + part11.name + "--" + ex.Message);
                }
            }
            addLSResources(part11);
        }

  
        private void addLSResources(AvailablePart part11)
        {

            Part prefabPart = null;
            prefabPart = part11.partPrefab;
            IFIDebug.IFIMess("IFI Attempting to add LS Resource to part: " + part11.name);
            double MaxLS = 0;
            if (part11.name == "kebalEVA" || part11.name == "kerbalEVAfemale")
            {
                 MaxLS = Rate_Per_Kerbal * 60 * 60 * 5;
            }
            else
            {
                 MaxLS = 3 * prefabPart.CrewCapacity;
            }


           




            //Part prefab = part11.partPrefab; 

            ConfigNode resourceNode = new ConfigNode("RESOURCE");

            resourceNode.AddValue("name", "LifeSupport");
            resourceNode.AddValue("amount", MaxLS);
            resourceNode.AddValue("maxAmount", MaxLS);

            try
            {


                


                var partInfo = PartLoader.getPartInfoByName(part11.name);
                var prefab = partInfo.partPrefab;
                if (prefab != null)
                {
                    IFIDebug.IFIMess("EVA IFI Is part valid " + prefab.name + "-----");
                   // part11.partPrefab.SetResource(resourceNode);
                   var resource2 = prefab.AddResource(resourceNode);
                    //PartResource TEMPHOLD = prefab.AddResource(resourceNode);
                    //PartResource resource2 = prefab.AddResource(resourceNode);
                    resource2.flowState = true;
                    resource2.flowMode = PartResource.FlowMode.Both;
                  //  partInfo.partConfig.AddNode(resourceNode);
                }

                //RES_O2.SetInfo(PartResourceLibrary.Instance.resourceDefinitions["Oxygen"]);
                //RES_O2.amount = 8;
                //RES_O2.maxAmount = 8;
            }
                catch (Exception ex)
                {
                    IFIDebug.IFIMess("EVA IFI Exception +addresource+ " + part11.name + "-----" + ex.Message);


                }

          
            
            }
        private void OnCrewBoardVessel11(GameEvents.FromToAction<Part, Part> action)
        {
            IFIDebug.IFIMess(" IFI DEBUG -- OnCrewBoardVessel fired ----");
            double IFIResourceAmt = 0.0;
            double IFIResElectric = 0.0;
            foreach (PartResource pr in action.from.Resources)
            {
                string IIResource = pr.resourceName;
                IFIDebug.IFIMess(" Resource Name " + IIResource);
                if (IIResource == "LifeSupport")
                {
                    IFIResourceAmt += pr.amount;
                }
                else if (IIResource == "ElectricCharge")
                {
                   // IFIResElectric += pr.amount;
                }
            }
            IFIDebug.IFIMess(" Electric Found " + Convert.ToString(IFIResElectric));
            IFIResourceAmt = action.from.RequestResource("LifeSupport", IFIResourceAmt);
            IFIResourceAmt = action.to.RequestResource("LifeSupport", 0.0 - IFIResourceAmt);
            //IFIResElectric = (action.from.RequestResource("ElectricCharge", IFIResElectric)) - 0.001;
            //IFIResElectric = action.to.RequestResource("ElectricCharge", 0.0 - IFIResElectric);
            IFIDebug.IFIMess("IFI Life Support Message: EVA - Ended - " + action.from.name + " Boarded Vessel - LS Return = " + Convert.ToString(IFIResourceAmt) + " and  Electric" + Convert.ToString(IFIResElectric));
        }

        private void OnCrewOnEva11(GameEvents.FromToAction<Part, Part> action) //Kerbal goes on EVA takes LS With them
        {

            IFIDebug.IFIMess("IFI DEBUG -- OnCrewOnEva fired ----");
            double resourceRequest = 0.0;//* Take 4 hours of LS on each eva.
            double IFIResElectric = resourceRequest * 1.5;
            double IFIResReturn = 0.0;
            try
            {
                foreach (PartResource pr in action.to.Resources)
                {

                    if (pr.resourceName.Equals("LifeSupport"))
                    {
                        pr.amount = pr.maxAmount;
                        resourceRequest += pr.maxAmount;
                    }


                }
            }
            catch (Exception ex) { IFIDebug.IFIMess(" IFI Exception +ON EVA RESOURCE TRANSFER+ " + ex.Message); }
            //IFIResReturn = action.from.RequestResource("ElectricCharge", resourceRequest * 1.5);
            //IFIResElectric -= IFIResReturn;
            //IFIResReturn = action.to.RequestResource("ElectricCharge", IFIResElectric);
            //IFIResElectric = resourceRequest * 1.5;
            //IFIResElectric -= IFIResReturn;
            IFIResReturn = 0.0;
            IFIResReturn = action.from.RequestResource("LifeSupport", resourceRequest);
            resourceRequest -= IFIResReturn;
            resourceRequest = action.to.RequestResource("LifeSupport", resourceRequest);
            IFIDebug.IFIMess("IFI Life Support Message: EVA - Started - " + action.to.name + " Exited Vessel - Took " + Convert.ToString(IFIResReturn) + " Life Support  and " + Convert.ToString(IFIResElectric) + " Electric Charge ");
        }
    }

}
