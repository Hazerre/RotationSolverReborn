﻿using Dalamud.Interface.Colors;
using RotationSolver.Localization;
using RotationSolver.Updaters;

namespace RotationSolver.UI;
internal partial class RotationConfigWindow
{
    private void DrawRotationTab()
    {
        ImGui.TextWrapped(LocalizationManager.RightLang.ConfigWindow_Rotation_Description);

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
    }

    private static void DrawRotations(RotationUpdater.CustomRotationGroup[] rotations)
    {
        for (int i = 0; i < rotations.Length; i++)
        {
            if (i > 0) ImGui.Separator();

            var group = rotations[i];
            var rotation = RotationUpdater.GetChooseRotation(group);

            var canAddButton = Service.Player != null
                && rotation.JobIDs.Contains((ClassJobID)Service.Player.ClassJob.Id);

            rotation.Display(group.rotations, canAddButton);
        }
    }

    internal static void DrawRotationRole(ICustomRotation rotation)
    {
        DrawTargetHostileTYpe(rotation);
        DrawSpecialRoleSettings(rotation.Job.GetJobRole(), rotation.JobIDs[0]);

        ImGui.Spacing();
    }

    private static void DrawTargetHostileTYpe(ICustomRotation rotation)
    {
        var isAllTargetAsHostile = (int)DataCenter.GetTargetHostileType(rotation.Job);
        ImGui.SetNextItemWidth(300);
        if (ImGui.Combo(LocalizationManager.RightLang.ConfigWindow_Param_RightNowTargetToHostileType + $"##HostileType{rotation.GetHashCode()}", ref isAllTargetAsHostile, new string[]
        {
             LocalizationManager.RightLang.ConfigWindow_Param_TargetToHostileType1,
             LocalizationManager.RightLang.ConfigWindow_Param_TargetToHostileType2,
             LocalizationManager.RightLang.ConfigWindow_Param_TargetToHostileType3,
        }, 3))
        {
            Service.Config.TargetToHostileTypes[rotation.Job.RowId] = (byte)isAllTargetAsHostile;
            Service.Config.Save();
        }

        if (isAllTargetAsHostile != 2 && !Service.Config.AutoOffBetweenArea)
        {
            ImGui.TextColored(ImGuiColors.DPSRed, LocalizationManager.RightLang.ConfigWindow_Param_NoticeUnexpectedCombat);
        }
    }

    private static void DrawSpecialRoleSettings(JobRole role, ClassJobID job)
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

    private static void DrawHealerSettings(ClassJobID job)
    {
        DrawDragFloat(job, LocalizationManager.RightLang.ConfigWindow_Param_HealthAreaAbility,
            () => ConfigurationHelper.GetHealAreaAbility(job),
            (value) => Service.Config.HealthAreaAbilities[job] = value,
            Service.Config.HealthAreaAbility);

        DrawDragFloat(job, LocalizationManager.RightLang.ConfigWindow_Param_HealthAreaSpell,
            () => ConfigurationHelper.GetHealAreaSpell(job),
            (value) => Service.Config.HealthAreaSpells[job] = value,
            Service.Config.HealthAreaSpell);

        DrawDragFloat(job, LocalizationManager.RightLang.ConfigWindow_Param_HealingOfTimeSubtractArea,
            () => ConfigurationHelper.GetHealingOfTimeSubtractArea(job),
            (value) => Service.Config.HealingOfTimeSubtractAreas[job] = value,
            ConfigurationHelper.HealingOfTimeSubtractAreasDefault);

        DrawDragFloat(job, LocalizationManager.RightLang.ConfigWindow_Param_HealthSingleAbility,
            () => ConfigurationHelper.GetHealSingleAbility(job),
            (value) => Service.Config.HealthSingleAbilities[job] = value,
            Service.Config.HealthSingleAbility);

        DrawDragFloat(job, LocalizationManager.RightLang.ConfigWindow_Param_HealthSingleSpell,
            () => ConfigurationHelper.GetHealSingleSpell(job),
            (value) => Service.Config.HealthSingleSpells[job] = value,
            Service.Config.HealthSingleSpell);

        DrawDragFloat(job, LocalizationManager.RightLang.ConfigWindow_Param_HealingOfTimeSubtractSingle,
            () => ConfigurationHelper.GetHealingOfTimeSubtractSingle(job),
            (value) => Service.Config.HealingOfTimeSubtractSingles[job] = value,
            ConfigurationHelper.HealingOfTimeSubtractSinglesDefault);
    }

    private static void DrawDragFloat(ClassJobID job, string desc, Func<float> getValue, Action<float> setValue, float @default)
    {
        if (getValue == null || setValue == null) return;

        var value = getValue();
        var last = value;
        DrawFloatNumber($"{desc}##{job}{desc}", ref value, @default, speed: 0.005f, description: desc);
        if(last != value)
        {
            setValue(value);
            Service.Config.Save();
        }
    }
}
