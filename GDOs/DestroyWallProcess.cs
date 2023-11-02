using KitchenData;
using KitchenLib.Customs;
using System.Collections.Generic;

namespace KitchenRenovation.GDOs
{
    public class DestroyWallProcess : CustomProcess
    {
        public override string UniqueNameID => "Destroy Wall Process";

        public override List<(Locale, ProcessInfo)> InfoList => new()
        {
            (Locale.English, CreateProcessInfo("Destroy Wall", "<sprite name=\"nova_destroy_wall\">"))
        };
        public override bool CanObfuscateProgress => true;
    }
}
