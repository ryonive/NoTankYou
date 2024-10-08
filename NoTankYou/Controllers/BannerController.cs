﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ImGuiNET;
using KamiLib.Classes;
using KamiLib.Configuration;
using KamiLib.Extensions;
using KamiToolKit;
using KamiToolKit.Classes;
using KamiToolKit.Nodes;
using NoTankYou.Classes;
using NoTankYou.Localization;

namespace NoTankYou.Controllers;

public unsafe class BannerController() : NativeUiOverlayController(Service.AddonLifecycle, Service.Framework, Service.GameGui) {
    public BannerConfig Config { get; private set; } = new();

    private ListNode<BannerOverlayNode>? bannerListNode;
    
    protected override void PreAttach() {
        Config = BannerConfig.Load();
    }

    protected override void AttachNodes(AddonNamePlate* addonNamePlate) {
        bannerListNode = new ListNode<BannerOverlayNode> {
            Size = Config.OverlaySize,
            Position = Config.WindowPosition,
            LayoutAnchor = Config.LayoutAnchor,
            NodeFlags = NodeFlags.Clip,
            IsVisible = Config.Enabled,
            LayoutOrientation = Config.SingleLine ? LayoutOrientation.Horizontal : LayoutOrientation.Vertical,
            NodeID = 200_000,
            Color = KnownColor.White.Vector(),
            BackgroundVisible = Config.ShowListBackground,
            BackgroundColor = Config.ListBackgroundColor,
            BorderVisible = Config.ShowListBorder,
        };
        
        foreach(uint index in Enumerable.Range(0, 10)) {
            var newOverlayNode = new BannerOverlayNode(200_100u + index);
            bannerListNode.Add(newOverlayNode);
            newOverlayNode.EnableEvents(Service.AddonEventManager, (AtkUnitBase*)addonNamePlate);
        }
            
        UpdateStyle();
        System.NativeController.AttachToAddon(bannerListNode, (AtkUnitBase*)addonNamePlate, addonNamePlate->RootNode, NodePosition.AsFirstChild);
    }

    protected override void DetachNodes(AddonNamePlate* addonNamePlate) {
        if (bannerListNode is not null) {
            System.NativeController.DetachFromAddon(bannerListNode, (AtkUnitBase*)addonNamePlate);
                
            bannerListNode.Dispose();
            bannerListNode = null;
        }
    }
    
    public void DrawConfigUi()
        => Config.DrawConfigUi();
    
    public void Draw(IEnumerable<WarningState> warnings) {
        if (!Config.Enabled) return;
        if (bannerListNode is null) return;

        foreach (var bannerOverlayNode in bannerListNode) {
            bannerOverlayNode.IsVisible = false;
        }
        
        if (Config.SampleMode) {
            bannerListNode[0].AssociatedWarning = ModuleController.SampleWarning;
            bannerListNode[0].IsVisible = true; 
            return;
        }

        var filteredWarnings = Config.SoloMode ? warnings.Where(warning => warning.SourceEntityId == Service.ClientState.LocalPlayer?.EntityId) : warnings;

        switch (Config.DisplayMode) {
            case BannerOverlayDisplayMode.TopPriority:
                DrawTopPriorityWarnings(filteredWarnings);
                break;

            case BannerOverlayDisplayMode.List:
                DrawListWarnings(filteredWarnings);
                break;
        }
    }

    private void DrawListWarnings(IEnumerable<WarningState> warnings) {
        if (bannerListNode is null) return;
        
        var orderedWarnings = warnings
            .Where(warning => !Config.BlacklistedModules.Contains(warning.SourceModule))
            .OrderByDescending(warning => warning.Priority)
            .Take(Config.WarningCount)
            .ToList();

        foreach (var index in Enumerable.Range(0, 10)) {
            var bannerNode = bannerListNode[index];
            if (index > Config.WarningCount || index >= orderedWarnings.Count) continue;

            bannerNode.AssociatedWarning = orderedWarnings[index];
            bannerNode.IsVisible = true;
        }
    }
    
    private void DrawTopPriorityWarnings(IEnumerable<WarningState> warnings) {
        if (bannerListNode is null) return;

        var highestWarning = warnings
            .Where(warning => !Config.BlacklistedModules.Contains(warning.SourceModule))
            .MaxBy(warning => warning.Priority);

        if (highestWarning is null) return;

        bannerListNode[0].AssociatedWarning = highestWarning;
        bannerListNode[0].IsVisible = true;
    }

    public void Reset() {
        if (bannerListNode is null) return;

        foreach (var index in Enumerable.Range(0, 10)) {
            var node = bannerListNode[index];

            node.IsVisible = false;
        }
    }

    public void UpdateStyle() {
        if (bannerListNode is null) return;
        
        bannerListNode.Position = Config.WindowPosition;
        bannerListNode.Size = Config.OverlaySize;
        bannerListNode.Scale = new Vector2(Config.Scale, Config.Scale);
        bannerListNode.BackgroundColor = Config.ListBackgroundColor;
        bannerListNode.BackgroundVisible = Config.ShowListBackground;
        bannerListNode.BorderVisible = Config.ShowListBorder;
        bannerListNode.LayoutOrientation = Config.SingleLine ? LayoutOrientation.Horizontal : LayoutOrientation.Vertical;
        bannerListNode.LayoutAnchor = Config.LayoutAnchor;
        
        foreach (var index in Enumerable.Range(0, 10)) {
            var node = bannerListNode[index];

            node.UpdateStyle();
        } 
        
        bannerListNode.RecalculateLayout();
    }

    public void Hide() {
        if (bannerListNode is not null) {
            foreach (var bannerOverlayNode in bannerListNode) {
                bannerOverlayNode.IsVisible = false;
            }
        }
    }
}

public enum BannerOverlayDisplayMode {
    TopPriority,
    List,
}

public class BannerConfig {
    public bool Enabled = true;
    public bool SoloMode;
    public bool SampleMode;

    public Vector2 WindowPosition = new(700.0f, 400.0f);
    public float Scale = 1.0f;
    
    public BannerOverlayDisplayMode DisplayMode = BannerOverlayDisplayMode.List;
    public Vector2 OverlaySize = new(700.0f, 600.0f);
    public LayoutAnchor LayoutAnchor = LayoutAnchor.TopLeft;
    public bool SingleLine;
    public int WarningCount = 10;
    public bool ShowListBackground;
    public bool ShowListBorder;
    public Vector4 ListBackgroundColor = KnownColor.Aqua.Vector() with { W = 0.15f };
    
    public bool WarningShield = true;
    public bool WarningText = true; 
    public bool PlayerNames = true;
    public bool ShowActionName = true;
    public bool Icon = true;

    public HashSet<ModuleName> BlacklistedModules = [];

    public void DrawConfigUi() {
           var configChanged = false;
        
        ImGuiTweaks.Header(Strings.DisplayOptions);
        using (var _ = ImRaii.PushIndent()) {
            configChanged |= ImGui.Checkbox(Strings.Enable, ref Enabled);
            configChanged |= ImGuiTweaks.Checkbox(Strings.SoloMode, ref SoloMode, Strings.SoloModeHelp);
            configChanged |= ImGui.Checkbox(Strings.SampleMode, ref SampleMode);
        }
        
        ImGuiTweaks.Header(Strings.Positioning);
        using (var _ = ImRaii.PushIndent()) {
            configChanged |= ImGui.DragFloat2(Strings.Position, ref WindowPosition, 5.0f);
            configChanged |= ImGui.DragFloat2("Size", ref OverlaySize, 5.0f);
            configChanged |= ImGui.DragFloat(Strings.Scale, ref Scale, 0.01f);
            
            ImGuiHelpers.ScaledDummy(5.0f);
            
            configChanged |= ImGui.Checkbox("Show Background", ref ShowListBackground);
            configChanged |= ImGui.Checkbox("Show Border", ref ShowListBorder);
            
            ImGuiHelpers.ScaledDummy(5.0f);
            configChanged |= ImGuiTweaks.ColorEditWithDefault("Background Color", ref ListBackgroundColor, KnownColor.Aqua.Vector() with { W = 0.15f });
        }
        
        ImGuiTweaks.Header(Strings.DisplayMode);
        using (var _ = ImRaii.PushIndent()) {
            configChanged |= ImGuiTweaks.EnumCombo(Strings.DisplayMode, ref DisplayMode);
            configChanged |= ImGui.SliderInt(Strings.MaxWarnings, ref WarningCount, 1, 10);
            
            ImGuiHelpers.ScaledDummy(5.0f);

            configChanged |= ImGui.Checkbox("Single Line", ref SingleLine);
            configChanged |= ImGuiTweaks.EnumCombo("Layout Anchor", ref LayoutAnchor);
        }
        
        ImGuiTweaks.Header(Strings.DisplayStyle);
        using (var _ = ImRaii.PushIndent()) {
            configChanged |= ImGui.Checkbox(Strings.WarningShield, ref WarningShield);
            configChanged |= ImGui.Checkbox(Strings.WarningText, ref WarningText);
            configChanged |= ImGui.Checkbox(Strings.PlayerNames, ref PlayerNames);
            configChanged |= ImGui.Checkbox(Strings.ActionName, ref ShowActionName);
            configChanged |= ImGui.Checkbox(Strings.ActionIcon, ref Icon);
        }

        ImGuiTweaks.Header(Strings.ModuleBlacklist);
        using (var _ = ImRaii.PushIndent()) {
            ImGui.Columns(2);
        
            foreach (var module in Enum.GetValues<ModuleName>()[..^1]) {
                var inHashset = BlacklistedModules.Contains(module);
                if(ImGui.Checkbox(module.GetDescription(), ref inHashset)) {
                    if (!inHashset) BlacklistedModules.Remove(module);
                    if (inHashset) BlacklistedModules.Add(module);
                    configChanged = true;
                }
                ImGui.NextColumn();
            }
        
            ImGui.Columns(1);
        }

        if (configChanged) {
            Save();
            System.BannerController.UpdateStyle();
            System.BannerController.Reset();
        }
    }
    
    public static BannerConfig Load() 
        => Service.PluginInterface.LoadCharacterFile(Service.ClientState.LocalContentId, "BannerDisplay.config.json", () => new BannerConfig());

    private void Save()
        => Service.PluginInterface.SaveCharacterFile(Service.ClientState.LocalContentId, "BannerDisplay.config.json", this);
}