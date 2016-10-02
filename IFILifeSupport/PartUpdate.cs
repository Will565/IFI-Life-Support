using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using KSP.IO;


namespace IFILifeSupport
{
    public class PartUpdate : LoadingSystem
    {
        private static double Rate_Per_Kerbal = LifeSupportRate.GetRate();
        private bool ready;
        public static PartUpdate Instance { get; private set; }

        private void Awake()
        {

           
            if (Instance != null)
            {
                DestroyImmediate(this);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        public override float ProgressFraction()
        {
            
            return 3;
        }

        public override string ProgressTitle()
        {
            return "IFI Preload";
        }

        public override bool IsReady()
        {
            if (ready)
            {
                Debug.Log("IFI PartUpdate Finished");
#if !DEBUG
                if (IFIDebug.IsON) { IFIDebug.Toggle(); }
#endif
            }
            return ready;
        }

        public override void StartLoad()
        {
            StartLoad(false);
        }

        public void StartLoad(bool flag11)
        {
            ready = false;
            Debug.Log(" IFI Preload LifeSupport Install started ++++ ");
          
            // Add Module to Command Parts AND eVA PreFabs
            try
            {
                foreach (ConfigNode part_node in GameDatabase.Instance.GetConfigNodes("PART"))
                {
                    string CrewHold = part_node.GetValue("CrewCapacity");
                    if (CrewHold != null && CrewHold != "0")
                    {
                        AddLSModule(part_node, Int32.Parse(CrewHold));
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
            ready = true;
            
        }

       private void AddLSModule(ConfigNode CrewPart, int crewCount)
        {
            var IFIParts = CrewPart.GetNodes("MODULE");// .Where(p => p.category.Equals(1));
            bool HasMod = false;
               // Debug.Log(IFIParts.Count);
                foreach (var IFIPart in IFIParts)
                {
                string ModuleName = IFIPart.GetValue("name");
                   // Debug.Log("IFI Part Update --" + ModuleName + " -- Found");
                    if (ModuleName == "IFILifeSupport")
                    {
                    HasMod = true;
                    }
                }
            if (!HasMod)
            {
               // IFIDebug.IFIMess("IFI Attempting to add LS Module to part: " + CrewPart.GetValue("name"));
                ConfigNode IFIMOD = new ConfigNode("MODULE");
                IFIMOD.AddValue("name", "IFILifeSupport");
                CrewPart.AddNode(IFIMOD);   
            }
             HasMod = false;
            var IFIParts2 = CrewPart.GetNodes("RESOURCE");// .Where(p => p.category.Equals(1));
            // Debug.Log(IFIParts.Count);
            foreach (var IFIPart in IFIParts2)
            {
                string ModuleName = IFIPart.GetValue("name");
                //Debug.Log("IFI Part Update --" + ModuleName + " -- Found");
                if (ModuleName == "LifeSupport")
                {
                    HasMod = true;
                }
            }
            if (!HasMod)
            {
                //IFIDebug.IFIMess("IFI Attempting to add LS Resorce to part: " + CrewPart.GetValue("name"));
                ConfigNode IFIMOD = new ConfigNode("RESOURCE");
                IFIMOD.AddValue("name", "LifeSupport");
                double MaxLS = 4 * crewCount;
                IFIMOD.AddValue("amount", MaxLS);
                IFIMOD.AddValue("maxAmount", MaxLS);

                CrewPart.AddNode(IFIMOD);
            }
        }

   
}

    }
