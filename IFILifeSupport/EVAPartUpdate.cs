using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using KSP.IO;

namespace IFILifeSupport
{
    [KSPAddon(KSPAddon.Startup.MainMenu, false)]
    public class ADDEVAPARTS : UnityEngine.MonoBehaviour
    {
        private static double Rate_Per_Kerbal = LifeSupportRate.GetRate();

        private void Awake()
        {

            try
            {
                var IFIParts = PartLoader.LoadedPartsList;// .Where(p => p.category.Equals(1));
                foreach (var IFIPart in IFIParts)
                {
                    if (IFIPart.partPrefab.CrewCapacity > 0)
                    {
                        if (IFIPart.name == "kerbalEVA" || IFIPart.name == "kerbalEVAfemale")
                            AddLifeSupport(IFIPart);

                    }
                }


            }
            catch (Exception ex)
            {
                IFIDebug.IFIMess("IFI Exception Searching for Command Parts  " + ex.Message);
            }
        }

     private void AddLifeSupport(AvailablePart part11)
        {



            Part prefabPart = null;
            prefabPart = part11.partPrefab;
            IFIDebug.IFIMess("IFI Attempting to add LS Module EVA " + part11.name);
            string NAMLSmod = "IFILifeSupportEVA";
            try
            {
                if (NAMLSmod == "IFILifeSupport")
                {
                    if (!prefabPart.Modules.Contains(NAMLSmod))
                    {
                        IFIDebug.IFIMess(" IFI Preload AddModule no " + NAMLSmod + " Found Adding LS Mod");
                        PartModule module11 = part11.partPrefab.AddModule(NAMLSmod);
                    }
                }
                else
                {
                    IFIDebug.IFIMess(" IFI Preload AddModule EVA Prefab ----");
                    PartModule module11 = part11.partPrefab.AddModule(NAMLSmod);
                }
                IFIDebug.IFIMess(" IFI Preload AddModule(" + NAMLSmod + ") Fired no exception " + part11.name);
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
            double MaxLS = MaxLS = Rate_Per_Kerbal * 60 * 60 * 5;







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
                    //part11.partPrefab.SetResource(resourceNode);
                    var resource2 = prefab.AddResource(resourceNode);
                    //PartResource TEMPHOLD = prefab.AddResource(resourceNode);
                    //PartResource resource2 = prefab.AddResource(resourceNode);
                    resource2.flowState = true;
                    resource2.flowMode = PartResource.FlowMode.Both;
                    //partInfo.partConfig.AddNode(resourceNode);
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




    }
}
