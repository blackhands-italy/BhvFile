using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using System.Windows.Forms;
using System.Linq;

namespace BHVEditor
{
    public static class BhvDebugManager
    {
        public static List<StateDebugInfo> AllStates = new List<StateDebugInfo>();

        public class StateDebugInfo
        {
            public string StateName { get; set; }
            public List<NextStateInfo> NextStates { get; set; }
        }

        public class NextStateInfo
        {
            public string NextStateName { get; set; }
            public List<string> Conditions { get; set; }
        }

        public static void LoadIntegratedJson(string filePath)
        {
            try
            {
                string json = File.ReadAllText(filePath);
                var root = JsonConvert.DeserializeObject<Dictionary<string, List<StateDebugInfo>>>(json);
                if (root != null && root.ContainsKey("AllStates"))
                {
                    AllStates = root["AllStates"];
                    MessageBox.Show($"成功載入 {AllStates.Count} 個狀態。");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("讀取 JSON 失敗: " + ex.Message);
            }
        }

        public static string GetStateName(int index) =>
            (index >= 0 && index < AllStates.Count) ? AllStates[index].StateName : "";
        // 需要添加一个新的方法来获取状态的所有转换信息
        // 修复后的方法：获取调试转换信息
        public static List<Transition> GetDebugTransitions(int stateIndex)
        {
            var transitions = new List<Transition>();

            if (stateIndex >= 0 && stateIndex < AllStates.Count)
            {
                var stateInfo = AllStates[stateIndex];

                if (stateInfo.NextStates != null)
                {
                    foreach (var nextState in stateInfo.NextStates)
                    {
                        // 安全解析状态索引
                        int targetStateIndex = ParseStateIndex(nextState.NextStateName);
                        if (targetStateIndex >= 0)
                        {
                            var transition = new Transition
                            {
                                StateIndex = targetStateIndex
                            };

                            // 修复：将 List<string> 转换为 List<Condition>
                            if (nextState.Conditions != null)
                            {
                                transition.Conditions = ConvertToConditionList(nextState.Conditions);
                            }
                            else
                            {
                                transition.Conditions = new List<Condition>();
                            }

                            transitions.Add(transition);
                        }
                    }
                }
            }
            return transitions;
        }
        // 辅助方法：安全解析状态索引
        private static int ParseStateIndex(string stateName)
        {
            if (string.IsNullOrEmpty(stateName))
                return -1;

            // 尝试直接解析数字
            if (int.TryParse(stateName, out int result))
                return result;

            // 尝试从 "State123" 这样的格式中提取数字
            string numbers = new string(stateName.Where(char.IsDigit).ToArray());
            if (int.TryParse(numbers, out int parsedNumber))
                return parsedNumber;

            // 如果都无法解析，返回-1表示无效
            return -1;
        }

        // 辅助方法：将 List<string> 转换为 List<Condition>
        private static List<Condition> ConvertToConditionList(List<string> stringConditions)
        {
            var conditions = new List<Condition>();

            if (stringConditions != null)
            {
                foreach (var conditionStr in stringConditions)
                {
                    conditions.Add(new Condition
                    {
                        // 根据您的 Condition 类结构设置相应属性
                        // 这里假设 Condition 类有一个 Expression 属性
                        Expression = conditionStr
                    });
                }
            }

            return conditions;
        }

        // 新增方法：获取特定状态的转换数量
        public static int GetTransitionCount(int stateIndex)
        {
            if (stateIndex >= 0 && stateIndex < AllStates.Count)
            {
                return AllStates[stateIndex]?.NextStates?.Count ?? 0;
            }
            return 0;
        }

        // 新增方法：获取特定转换的条件列表
        public static List<string> GetTransitionConditions(int fromStateIndex, int toStateIndex)
        {
            if (fromStateIndex >= 0 && fromStateIndex < AllStates.Count)
            {
                var stateInfo = AllStates[fromStateIndex];
                var nextState = stateInfo?.NextStates?
                    .FirstOrDefault(ns => ParseStateIndex(ns.NextStateName) == toStateIndex);

                return nextState?.Conditions ?? new List<string>();
            }
            return new List<string>();
        }
    }
}