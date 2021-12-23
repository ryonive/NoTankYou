﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImGuiNET;

namespace NoTankYou.SettingsSystem.SettingsCategories
{
    internal class KardionSettings : SettingsCategory
    {
        public KardionSettings() : base("Kardion Settings")
        {
        }

        protected override void DrawContents()
        {
            DrawKardionTab();
        }

        private static void DrawKardionTab()
        {
            ImGui.Checkbox("Enable Missing Kardion Warning", ref Service.Configuration.EnableKardionBanner);
            ImGui.Spacing();

            ImGui.Checkbox("Force Show Banner", ref Service.Configuration.ForceShowKardionBanner);
            ImGui.Spacing();

            ImGui.Checkbox("Reposition Banner", ref Service.Configuration.RepositionModeKardionBanner);
            ImGui.Spacing();
        }
    }
}
