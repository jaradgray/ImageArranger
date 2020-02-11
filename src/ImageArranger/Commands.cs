using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ImageArranger
{
    public static class Commands
    {
        public static RoutedCommand NewCommand = new RoutedCommand();
        public static RoutedCommand QuitCommand = new RoutedCommand();

        static Commands()
        {
            NewCommand.InputGestures.Add(new KeyGesture(Key.N, ModifierKeys.Control, "Ctrl+N"));
            QuitCommand.InputGestures.Add(new KeyGesture(Key.Q, ModifierKeys.Control, "Ctrl+Q"));
        }
    }
}
