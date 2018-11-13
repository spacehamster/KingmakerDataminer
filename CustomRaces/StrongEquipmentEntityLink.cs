using Harmony12;
using Kingmaker.Blueprints;
using Kingmaker.ResourceLinks;
using Kingmaker.Visual.CharacterSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomRaces
{
	//TODO: Subclassing EquipmentEntityLink and keeping a reference to EquipmentEntityLink is not required, replace
    public class StrongEquipmentEntityLink : EquipmentEntityLink
    {
        public EquipmentEntity ee;
        public StrongEquipmentEntityLink(EquipmentEntity ee, string assetID)
        {
            this.ee = ee;
            this.AssetId = assetID;
            RaceManager.assets[this.AssetId] = ee;
            RaceUtil.AddResource(ee, this.AssetId);

        }
    }
}
