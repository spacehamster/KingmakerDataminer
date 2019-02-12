using Kingmaker.UnitLogic.Customization;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomBlueprints
{
    public class UnitCustomizationVariationConverter : CustomCreationConverter<UnitCustomizationVariation>
    {
        public override UnitCustomizationVariation Create(Type objectType)
        {
            return new UnitCustomizationVariation(null, Kingmaker.Blueprints.Gender.Male);
        }
    }
}
