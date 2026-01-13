using System;
using RimWorld;
using Verse;

namespace LSTC
{
    public class Building_TC_ModExt: DefModExtension
    {
        public static readonly Building_TC_ModExt Default = new Building_TC_ModExt();

        // 高功率进入阈值
        public float HighPowerEnterDelta = 10f;
        // 高功率退出阈值
        public float HighPowerExitDelta = 1f;
        // 最大线性调控温差
        public float MaxEffectiveDelta = 40f;
        // 线性倍数
        public float ChangeRate = 0.5f;
        // 最小调控温度
        public float MinTempChange = 1f;
    }

    public class Building_TC : Building_TempControl
    {
        public override void TickRare()
        {
            // 未放置或通电情况下不工作
            if (!base.Spawned || !this.compPowerTrader.PowerOn)
            {
                return;
            }

            var ext = this.def.GetModExtension<Building_TC_ModExt>() ?? Building_TC_ModExt.Default;

            float tempRoom = base.AmbientTemperature;
            float tempTarget = this.compTempControl.TargetTemperature;

            float dt = Math.Abs(tempTarget - tempRoom);
            int sign = Math.Sign(tempTarget - tempRoom);

            // 决定状态
            bool isHighPower = compTempControl.operatingAtHighPower;

            if (!isHighPower)
            {
                if (dt > ext.HighPowerEnterDelta)
                    isHighPower = true;
            }
            else
            {
                if (dt < ext.HighPowerExitDelta)
                    isHighPower = false;
            }

            // 状态只在这里写回
            compTempControl.operatingAtHighPower = isHighPower;

            // 是否工作
            if (!isHighPower)
                return;

            // 执行调温行为
            float tempChange;

            if (dt < ext.HighPowerEnterDelta)
            {
                tempChange = ext.MinTempChange;
            }
            else if (dt < ext.MaxEffectiveDelta)
            {
                tempChange = dt * ext.ChangeRate;
            }
            else
            {
                tempChange = ext.MaxEffectiveDelta * ext.ChangeRate;
            }

            tempChange *= sign;
            this.GetRoom(RegionType.Set_All).Temperature += tempChange;
        }
    }
}
