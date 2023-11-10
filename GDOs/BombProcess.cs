using KitchenData;
using KitchenLib.Customs;
using System.Collections.Generic;

namespace KitchenRenovation.GDOs
{
    public class BombProcess : CustomProcess
    {
        public override string UniqueNameID => "Bomb Process";

        public override List<(Locale, ProcessInfo)> InfoList => new()
        {
            (Locale.English, CreateProcessInfo("Bomb", "<sprite name=\"nova_bomb\">"))
        };
        public override bool CanObfuscateProgress => false;
    }
}
