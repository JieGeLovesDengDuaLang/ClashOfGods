﻿using COG.Config.Impl;
using COG.Modules;

namespace COG.UI.SidebarText.Impl;

public class ModSettings : SidebarText
{
    public ModSettings() : base(LanguageConfig.Instance.SidebarTextMod)
    {
    }
    
    public override void ForResult(ref string result)
    {
        Objects.Clear();
        Objects.AddRange(new []
        {
            HudStringPatch.GetOptByType(CustomOption.CustomOptionType.General)
        });
    }
}