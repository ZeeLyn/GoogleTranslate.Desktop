using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace GoogleTranslate.Desktop
{
    /// <summary>
    /// 热键注册帮助
    /// </summary>
    public class HotKeyHelper
    {
        /// <summary>
        /// 记录快捷键注册项的唯一标识符
        /// </summary>
        private static readonly Dictionary<EHotKeySetting, int> HotKeySettingsDic = new Dictionary<EHotKeySetting, int>();

        public static int GetHotKeySetting(EHotKeySetting key)
        {
            return HotKeySettingsDic[key];
        }

        /// <summary>
        /// 注册热键
        /// </summary>
        /// <param name="hotKeyModel">热键待注册项</param>
        /// <param name="hWnd">窗口句柄</param>
        /// <returns>成功返回true，失败返回false</returns>
        public static bool RegisterHotKey(HotKeyModel hotKeyModel, IntPtr hWnd)
        {
            var fsModifierKey = new ModifierKeys();
            var hotKeySetting = (EHotKeySetting)Enum.Parse(typeof(EHotKeySetting), hotKeyModel.Name);

            if (!HotKeySettingsDic.ContainsKey(hotKeySetting))
            {
                // 全局原子不会在应用程序终止时自动删除。每次调用GlobalAddAtom函数，必须相应的调用GlobalDeleteAtom函数删除原子。
                if (HotKeyManager.GlobalFindAtom(hotKeySetting.ToString()) != 0)
                {
                    HotKeyManager.GlobalDeleteAtom(HotKeyManager.GlobalFindAtom(hotKeySetting.ToString()));
                }
                HotKeySettingsDic[hotKeySetting] = HotKeyManager.GlobalAddAtom(hotKeySetting.ToString());
            }
            else
            {
                // 注销旧的热键
                HotKeyManager.UnregisterHotKey(hWnd, HotKeySettingsDic[hotKeySetting]);
            }
            if (!hotKeyModel.IsUsable)
                return true;

            // 注册热键
            if (hotKeyModel.IsSelectCtrl && !hotKeyModel.IsSelectShift && !hotKeyModel.IsSelectAlt)
            {
                fsModifierKey = ModifierKeys.Control;
            }
            else if (!hotKeyModel.IsSelectCtrl && hotKeyModel.IsSelectShift && !hotKeyModel.IsSelectAlt)
            {
                fsModifierKey = ModifierKeys.Shift;
            }
            else if (!hotKeyModel.IsSelectCtrl && !hotKeyModel.IsSelectShift && hotKeyModel.IsSelectAlt)
            {
                fsModifierKey = ModifierKeys.Alt;
            }
            else if (hotKeyModel.IsSelectCtrl && hotKeyModel.IsSelectShift && !hotKeyModel.IsSelectAlt)
            {
                fsModifierKey = ModifierKeys.Control | ModifierKeys.Shift;
            }
            else if (hotKeyModel.IsSelectCtrl && !hotKeyModel.IsSelectShift && hotKeyModel.IsSelectAlt)
            {
                fsModifierKey = ModifierKeys.Control | ModifierKeys.Alt;
            }
            else if (!hotKeyModel.IsSelectCtrl && hotKeyModel.IsSelectShift && hotKeyModel.IsSelectAlt)
            {
                fsModifierKey = ModifierKeys.Shift | ModifierKeys.Alt;
            }
            else if (hotKeyModel.IsSelectCtrl && hotKeyModel.IsSelectShift && hotKeyModel.IsSelectAlt)
            {
                fsModifierKey = ModifierKeys.Control | ModifierKeys.Shift | ModifierKeys.Alt;
            }

            return HotKeyManager.RegisterHotKey(hWnd, HotKeySettingsDic[hotKeySetting], fsModifierKey, hotKeyModel.SelectKey);
        }

    }
}
