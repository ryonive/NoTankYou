﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Dalamud.Game.ClientState.Objects.SubKinds;
using KamiLib.Caching;
using KamiLib.Drawing;
using KamiLib.Extensions;
using KamiLib.Interfaces;
using Lumina.Excel.GeneratedSheets;
using NoTankYou.DataModels;
using NoTankYou.Interfaces;
using NoTankYou.Localization;
using NoTankYou.UserInterface.Components;
using NoTankYou.Utilities;

namespace NoTankYou.Modules;

public class CutsceneConfiguration : GenericSettings
{
}

public class Cutscene : IModule
{
    public ModuleName Name => ModuleName.Cutscene;
    public IConfigurationComponent ConfigurationComponent { get; }
    public ILogicComponent LogicComponent { get; }
    public string Command => "cutscene";
    private static CutsceneConfiguration Settings => Service.ConfigurationManager.CharacterConfiguration.Cutscene;
    public GenericSettings GenericSettings => Settings;

    public Cutscene()
    {
        ConfigurationComponent = new ModuleConfigurationComponent(this);
        LogicComponent = new ModuleLogicComponent(this);
    }

    private class ModuleConfigurationComponent : IConfigurationComponent
    {
        public ISelectable Selectable { get; }

        public ModuleConfigurationComponent(IModule parentModule)
        {
            Selectable = new ConfigurationSelectable(parentModule, this);
        }

        public void Draw()
        {
            InfoBox.Instance
                .AddTitle(Strings.Tabs_Settings)
                .AddConfigCheckbox(Strings.Labels_Enabled, Settings.Enabled)
                .AddInputInt(Strings.Labels_Priority, Settings.Priority, 0, 10)
                .Draw();
            
            InfoBox.Instance.DrawOverlaySettings(Settings);
            
            InfoBox.Instance.DrawOptions(Settings);
        }
    }

    private class ModuleLogicComponent : ILogicComponent
    {
        public IModule ParentModule { get; }
        public List<uint> ClassJobs { get; }

        private readonly OnlineStatus cutsceneStatus;

        private static readonly Dictionary<uint, Stopwatch> TimeSinceInCutscene = new();
        
        public ModuleLogicComponent(IModule parentModule)
        {
            ParentModule = parentModule;
            
            ClassJobs = LuminaCache<ClassJob>.Instance
                .Select(r => r.RowId)
                .ToList();

            cutsceneStatus = LuminaCache<OnlineStatus>.Instance.GetRow(15)!;
        }

        public WarningState? EvaluateWarning(PlayerCharacter character)
        {
            TimeSinceInCutscene.TryAdd(character.ObjectId, Stopwatch.StartNew());
            var stopwatch = TimeSinceInCutscene[character.ObjectId];

            if (stopwatch.Elapsed >= TimeSpan.FromSeconds(1) && character.HasOnlineStatus(cutsceneStatus.RowId))
            {
                return new WarningState
                {
                    MessageLong = Strings.Cutscene_WarningText,
                    MessageShort = Strings.Cutscene_WarningText,
                    IconID = cutsceneStatus.Icon,
                    IconLabel = cutsceneStatus.Name.RawString,
                    Priority = Settings.Priority.Value,
                };
            }
            else if (!character.HasOnlineStatus(cutsceneStatus.RowId))
            {
                stopwatch.Restart();
            }

            return null;
        }
    }
}