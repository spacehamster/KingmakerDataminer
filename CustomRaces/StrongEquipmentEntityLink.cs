using Kingmaker.ResourceLinks;
using Kingmaker.Visual.CharacterSystem;

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
