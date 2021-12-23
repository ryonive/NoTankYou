﻿using System.Numerics;
using Dalamud.Interface.Components;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using NoTankYou.DisplaySystem;
using NoTankYou.SettingsSystem.SettingsCategories;

namespace NoTankYou.SettingsSystem
{
    internal class SettingsWindow : Window
    {
        private Tab CurrentTab = Tab.General;
        private readonly Vector2 WindowSize = new(450, 500);

        private readonly GeneralSettings GeneralSettings = new();
        private readonly TankStanceSettings TankStanceSettings = new();
        private readonly DancePartnerSettings DancePartnerSettings = new();
        private readonly FaerieSettings FaerieSettings = new();
        private readonly KardionSettings KardionSettings = new();
        private readonly SummonerPetSettings SummonerPetSettings = new();
        private readonly BlacklistSettings BlacklistSettings = new();

        private enum Tab
        {
            General,
            TankStance,
            DancePartner,
            Faerie,
            Kardion,
            Summoner,
            Blacklist
        }

        public SettingsWindow() : base("No Tank You Settings")
        {
            IsOpen = false;

            SizeConstraints = new WindowSizeConstraints()
            {
                MinimumSize = new(WindowSize.X, WindowSize.Y),
                MaximumSize = new(WindowSize.X, WindowSize.Y)
            };

            Flags |= ImGuiWindowFlags.NoResize;
            Flags |= ImGuiWindowFlags.NoScrollbar;
            Flags |= ImGuiWindowFlags.NoScrollWithMouse;

        }

        public override void Draw()
        {
            if (!IsOpen) return;

            DrawTabs();

            switch (CurrentTab)
            {
                case Tab.General:
                    GeneralSettings.Draw();
                    break;

                case Tab.TankStance:
                    TankStanceSettings.Draw();
                    break;

                case Tab.DancePartner:
                    DancePartnerSettings.Draw();
                    break;

                case Tab.Faerie:
                    FaerieSettings.Draw();
                    break;

                case Tab.Kardion:
                    KardionSettings.Draw();
                    break;

                case Tab.Summoner:
                    SummonerPetSettings.Draw();
                    break;

                case Tab.Blacklist:
                    BlacklistSettings.Draw();
                    break;
            }

            ImGui.Separator();
            DrawSaveAndCloseButtons();
        }
        private void DrawTabs()
        {
            if (ImGui.BeginTabBar("No Tank You Tab Toolbar", ImGuiTabBarFlags.NoTooltip))
            {
                if (ImGui.BeginTabItem("General"))
                {
                    CurrentTab = Tab.General;
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("Tanks"))
                {
                    CurrentTab = Tab.TankStance;
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("DNC"))
                {
                    CurrentTab = Tab.DancePartner;
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("SCH"))
                {
                    CurrentTab = Tab.Faerie;
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("SGE"))
                {
                    CurrentTab = Tab.Kardion;
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("SMN"))
                {
                    CurrentTab = Tab.Summoner;
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("Blacklist"))
                {
                    CurrentTab = Tab.Blacklist;
                    ImGui.EndTabItem();
                }
            }
        }
        public override void OnClose()
        {
            base.OnClose();

            Service.Configuration.Save();
        }

        private void DrawSaveAndCloseButtons()
        {
            ImGui.Spacing();

            var windowSize = ImGui.GetWindowSize();
            ImGui.SetCursorPos(new Vector2(5, windowSize.Y - 30));

            if (ImGui.Button("Save", new(100, 25)))
            {
                Service.Configuration.Save();
            }

            ImGui.SameLine(ImGui.GetWindowWidth() - 155);

            if (ImGui.Button("Save & Close", new(150, 25)))
            {
                Service.Configuration.Save();
                IsOpen = false;
            }

            ImGui.Spacing();
        }

        public void Dispose()
        {

        }
    }
}
