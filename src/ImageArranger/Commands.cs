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
        public static RoutedCommand SaveCommand = new RoutedCommand();
        public static RoutedCommand SaveAsCommand = new RoutedCommand();
        public static RoutedCommand QuitCommand = new RoutedCommand();
        public static RoutedCommand FullScreenCommand = new RoutedCommand();

        static Commands()
        {
            NewCommand.InputGestures.Add(new KeyGesture(Key.N, ModifierKeys.Control, "Ctrl+N"));
            SaveCommand.InputGestures.Add(new KeyGesture(Key.S, ModifierKeys.Control, "Ctrl+S"));
            SaveAsCommand.InputGestures.Add(new KeyGesture(Key.S, ModifierKeys.Control|ModifierKeys.Shift, "Ctrl+Shift+S"));
            QuitCommand.InputGestures.Add(new KeyGesture(Key.Q, ModifierKeys.Control, "Ctrl+Q"));
            FullScreenCommand.InputGestures.Add(new KeyGesture(Key.F, ModifierKeys.Control, "Ctrl+F"));
        }
    }
}
