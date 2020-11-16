using Kingmaker.UnitLogic.Customization;
using Newtonsoft.Json.Converters;
using System;

namespace CustomBlueprints
{
    public class UnitCustomizationVariationConverter : CustomCreationConverter<UnitCustomizationVariation>
    {
        /*
         * Newtonsoft doesn't like two argument constructors
         */
        public override UnitCustomizationVariation Create(Type objectType)
        {
            return new UnitCustomizationVariation(null, Kingmaker.Blueprints.Gender.Male);
        }
    }
}
