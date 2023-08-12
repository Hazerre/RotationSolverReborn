﻿using Dalamud.Interface.Colors;
using Dalamud.Utility;
using ECommons.ExcelServices;
using ECommons.GameHelpers;

using RotationSolver.Data;
using RotationSolver.Helpers;
using RotationSolver.Localization;
using RotationSolver.Updaters;
using System.Diagnostics;

namespace RotationSolver.UI;
internal partial class RotationConfigWindow
{
    private static void DrawRotationTab()
    {
        ImGui.TextWrapped(LocalizationManager.RightLang.ConfigWindow_Rotation_Description_Old);

        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0f, 5f));

        if (ImGui.BeginTabBar("Job Items"))
        {
            DrawRoleItems();

            ImGui.EndTabBar();
        }
        ImGui.PopStyleVar();
    }

    private static void DrawRoleItems()
    {
        foreach (var key in RotationUpdater.CustomRotationsDict.Keys)
        {
            var rotations = RotationUpdater.CustomRotationsDict[key];
            if (rotations == null || rotations.Length == 0) continue;

            if (ImGui.BeginTabItem(key.ToName()))
            {
                if (ImGui.BeginChild("Rotation Items", new Vector2(0f, -1f), true))
                {
                    DrawRotations(rotations);
                    ImGui.EndChild();
                }
                ImGui.EndTabItem();
            }
        }

        if (ImGui.BeginTabItem("Info"))
        {
            DrawInfos();
            ImGui.EndTabItem();
        }
    }

    private static void DrawRotations(CustomRotationGroup[] rotations)
    {
        for (int i = 0; i < rotations.Length; i++)
        {
            if (i > 0) ImGui.Separator();

            var group = rotations[i];
            var rotation = RotationUpdater.GetChosenRotation(group);

            var canAddButton = Player.Available
                && rotation.Jobs.Contains((Job)Player.Object.ClassJob.Id);

            rotation.Display(group.Rotations, canAddButton);
        }
    }

    internal static void DrawRotationRole(ICustomRotation rotation, bool canAddButton)
    {
        DrawTargetHostileType(rotation);

        if (rotation.Configs.Configs.Count != 0)
        {
            if (ImGui.CollapsingHeader($"{rotation.Jobs[0]} rotation settings##Settings"))
            {
                ImGui.Indent();

                DrawSpecialRoleSettings(rotation.ClassJob.GetJobRole(), rotation.Jobs[0]);
                rotation.Configs.Draw(canAddButton);

                ImGui.Unindent();

                ImGui.Spacing();
            }
        }
        else
        {
            DrawSpecialRoleSettings(rotation.ClassJob.GetJobRole(), rotation.Jobs[0]);
            rotation.Configs.Draw(canAddButton);
        }
    }

    private static void DrawTargetHostileType(ICustomRotation rotation)
    {
        var isAllTargetAsHostile = (int)DataCenter.GetTargetHostileType(rotation.ClassJob);
        ImGui.SetNextItemWidth(300);
        if (ImGui.Combo(LocalizationManager.RightLang.ConfigWindow_Param_RightNowTargetToHostileType + $"##HostileType{rotation.GetHashCode()}", ref isAllTargetAsHostile, new string[]
        {
             LocalizationManager.RightLang.ConfigWindow_Param_TargetToHostileType1,
             LocalizationManager.RightLang.ConfigWindow_Param_TargetToHostileType2,
             LocalizationManager.RightLang.ConfigWindow_Param_TargetToHostileType3,
        }, 3))
        {
            Service.Config.TargetToHostileTypes[rotation.ClassJob.RowId] = (byte)isAllTargetAsHostile;
            Service.Config.Save();
        }

        if (isAllTargetAsHostile != 2 && !Service.Config.AutoOffBetweenArea)
        {
            ImGui.TextColored(ImGuiColors.DPSRed, LocalizationManager.RightLang.ConfigWindow_Param_NoticeUnexpectedCombat);
        }
    }

    private static void DrawSpecialRoleSettings(JobRole role, Job job)
    {
        if (role == JobRole.Healer)
        {
            DrawHealerSettings(job);
        }
        else if (role == JobRole.Tank)
        {
            DrawDragFloat(job, LocalizationManager.RightLang.ConfigWindow_Param_HealthForDyingTank,
                () => ConfigurationHelper.GetHealthForDyingTank(job),
                (value) => Service.Config.HealthForDyingTanks[job] = value, 
                ConfigurationHelper.HealthForDyingTanksDefault);
        }
    }

    private static void DrawHealerSettings(Job job)
    {
        if (ImGui.BeginTable(job.ToString(), 3, ImGuiTableFlags.Borders
            | ImGuiTableFlags.Resizable
            | ImGuiTableFlags.SizingStretchProp))
        {
            ImGui.TableSetupScrollFreeze(0, 1);
            ImGui.TableNextRow(ImGuiTableRowFlags.Headers);

            ImGui.TableNextColumn();
            ImGui.TableHeader("");

            ImGui.TableNextColumn();
            ImGui.TableHeader(LocalizationManager.RightLang.ConfigWindow_Param_Normal);

            ImGui.TableNextColumn();
            ImGui.TableHeader(LocalizationManager.RightLang.ConfigWindow_Param_HOT);

            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.Text(LocalizationManager.RightLang.ConfigWindow_Param_HealthAreaAbility);

            ImGui.TableNextColumn();

            DrawDragFloat(job, nameof(Service.Config.HealthAreaAbilities),
                () => ConfigurationHelper.GetHealthAreaAbility(job),
                (value) => Service.Config.HealthAreaAbilities[job] = value,
                Service.Config.HealthAreaAbility);

            ImGui.TableNextColumn();

            DrawDragFloat(job, nameof(Service.Config.HealthAreaAbilitiesHot),
                () => ConfigurationHelper.GetHealthAreaAbilityHot(job),
                (value) => Service.Config.HealthAreaAbilitiesHot[job] = value,
                Service.Config.HealthAreaAbilityHot);

            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.Text(LocalizationManager.RightLang.ConfigWindow_Param_HealthAreaSpell);

            ImGui.TableNextColumn();

            DrawDragFloat(job, nameof(Service.Config.HealthAreaSpells),
                () => ConfigurationHelper.GetHealthAreaSpell(job),
                (value) => Service.Config.HealthAreaSpells[job] = value,
                Service.Config.HealthAreaSpell);

            ImGui.TableNextColumn();

            DrawDragFloat(job, nameof(Service.Config.HealthAreaSpellsHot),
                () => ConfigurationHelper.GetHealthAreaSpellHot(job),
                (value) => Service.Config.HealthAreaSpellsHot[job] = value,
                Service.Config.HealthAreaSpellHot);

            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.Text(LocalizationManager.RightLang.ConfigWindow_Param_HealthSingleAbility);

            ImGui.TableNextColumn();

            DrawDragFloat(job, nameof(Service.Config.HealthSingleAbilities),
                () => ConfigurationHelper.GetHealthSingleAbility(job),
                (value) => Service.Config.HealthSingleAbilities[job] = value,
                Service.Config.HealthSingleAbility);

            ImGui.TableNextColumn();

            DrawDragFloat(job, nameof(Service.Config.HealthSingleAbilitiesHot),
                () => ConfigurationHelper.GetHealthSingleAbilityHot(job),
                (value) => Service.Config.HealthSingleAbilitiesHot[job] = value,
                Service.Config.HealthSingleAbilityHot);

            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.Text(LocalizationManager.RightLang.ConfigWindow_Param_HealthSingleSpell);

            ImGui.TableNextColumn();

            DrawDragFloat(job, nameof(Service.Config.HealthSingleSpells),
                () => ConfigurationHelper.GetHealthSingleSpell(job),
                (value) => Service.Config.HealthSingleSpells[job] = value,
                Service.Config.HealthSingleSpell);

            ImGui.TableNextColumn();

            DrawDragFloat(job, nameof(Service.Config.HealthSingleSpellsHot),
                () => ConfigurationHelper.GetHealthSingleSpellHot(job),
                (value) => Service.Config.HealthSingleSpellsHot[job] = value,
                Service.Config.HealthSingleSpellHot);

            ImGui.EndTable();
        }
    }

    private static void DrawDragFloat(Job job, string desc, Func<float> getValue, Action<float> setValue, float @default)
    {
        if (getValue == null || setValue == null) return;

        var value = getValue();
        var last = value;
        DrawFloatNumber($"##{job}{desc}", ref value, @default, speed: 0.005f, description: desc);
        if(last != value)
        {
            setValue(value);
            Service.Config.Save();
        }
    }

    private static void DrawInfos()
    {
        //if (ImGuiHelper.IconButton(FontAwesomeIcon.Download, "DownloadRotationsButtonInfo"))
        //{
        //    Task.Run(async () =>
        //    {
        //        await RotationUpdater.GetAllCustomRotationsAsync(DownloadOption.MustDownload | DownloadOption.ShowList);
        //    });
        //}

        //ImGui.SameLine();
        //ImGuiHelper.Spacing();

        //DrawCheckBox(LocalizationManager.RightLang.ConfigWindow_Rotation_DownloadRotations,
        //    ref Service.Config.DownloadRotations, Service.Default.DownloadRotations);

        //if (Service.Config.DownloadRotations)
        //{
        //    ImGui.SameLine();
        //    ImGuiHelper.Spacing();

        //    DrawCheckBox(LocalizationManager.RightLang.ConfigWindow_Rotation_AutoUpdateRotations,
        //        ref Service.Config.AutoUpdateRotations, Service.Default.AutoUpdateRotations);
        //}
    }
}
