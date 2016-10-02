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

    // [KSPAddon(KSPAddon.Startup.MainMenu, true)]

    [KSPAddon(KSPAddon.Startup.Instantly, false)]
    public class ADDEVAS : UnityEngine.MonoBehaviour
    {

        private static double Rate_Per_Kerbal = LifeSupportRate.GetRate();

   
        public void Awake()
                {
            LoadingScreen screen = FindObjectOfType<LoadingScreen>();
            if (screen == null)
            {
                IFIDebug.IFIMess("IFI Can't find LoadingScreen type. Aborting execution");
                return;
            }
            List<LoadingSystem> list = LoadingScreen.Instance.loaders;

            if (list != null)
            {
                GameObject IFIObject = new GameObject("PartUpdate");
                PartUpdate loader = IFIObject.AddComponent<PartUpdate>();

                IFIDebug.IFIMess(string.Format("Adding PartUpdate to the loading screen {0}", list.Count));
                list.Insert(1, loader);

                Debug.Log(" IFI Preload LS EVA Actions Set ++++ ");
                GameEvents.onCrewOnEva.Remove(OnCrewOnEva11);
                GameEvents.onCrewOnEva.Add(OnCrewOnEva11);
                GameEvents.onCrewBoardVessel.Remove(OnCrewBoardVessel11);
                GameEvents.onCrewBoardVessel.Add(OnCrewBoardVessel11);
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
