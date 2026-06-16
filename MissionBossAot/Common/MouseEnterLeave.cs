using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MissionBossAot.Common
{
    internal class MouseEnterLeave
    {
        public static void ButtonMouseEnterLightGray(Button button)
        {
            ((Button)button).Background = System.Windows.Media.Brushes.LightGray;
        }

        public static void ButtonMouseEnterFirebrick(Button button)
        {
            ((Button)button).Background = System.Windows.Media.Brushes.Firebrick;
        }

        public static void ButtonMouseLeave(Button button)
        {
            ((Button)button).Background = System.Windows.Media.Brushes.Transparent;
        }

        public static void SystemMenuMouseEnter(Image image, TextBlock textBlock)
        {
            image.Width += 2;
            image.Height += 2;
            textBlock.FontSize = 14;
            textBlock.FontWeight = FontWeights.SemiBold;
        }

        public static void SystemMenuMouseLeave(Image image, TextBlock textBlock)
        {
            image.Width -= 2;
            image.Height -= 2;
            textBlock.FontSize = 12;
            textBlock.FontWeight = FontWeights.Bold;
        }
    }
}
