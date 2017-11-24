using System;
using System.Collections.Generic;

namespace CKAN.ConsoleUI.Toolkit {

    /// <summary>
    /// Object representing a menu of options for the user to choose.
    /// Displays similarly to menus from Turbo Vision.
    /// </summary>
    public class ConsolePopupMenu {

        /// <summary>
        /// Initialize the menu.
        /// </summary>
        /// <param name="opts">List of menu options for the menu</param>
        public ConsolePopupMenu(List<ConsoleMenuOption> opts)
        {
            options = opts;
            foreach (ConsoleMenuOption opt in options) {
                if (opt != null) {
                    int len = opt.Caption.Length + (
                        string.IsNullOrEmpty(opt.Key) ? 0 : 2 + opt.Key.Length
                    ) + (
                        opt.SubMenu     != null ? 3 :
                        opt.RadioActive != null ? 4 :
                        0
                    );
                    if (longestLength < len) {
                        longestLength = len;
                    }
                }
            }
        }

        /// <summary>
        /// Display the menu and handle its interactions
        /// </summary>
        /// <param name="right">X coordinate of right edge of menu</param>
        /// <param name="top">Y coordinate of top edge of menu</param>
        /// <returns>
        /// Return value of menu option selected by user
        /// </returns>
        public bool Run(int right, int top)
        {
            bool val  = true;
            bool done = false;
            do {
                Draw(right, top);
                ConsoleKeyInfo k = Console.ReadKey(true);
                switch (k.Key) {
                    case ConsoleKey.UpArrow:
                        do {
                            selectedOption = (selectedOption + options.Count - 1) % options.Count;
                        } while (options[selectedOption] == null);
                        break;
                    case ConsoleKey.DownArrow:
                        do {
                            selectedOption = (selectedOption + 1) % options.Count;
                        } while (options[selectedOption] == null);
                        break;
                    case ConsoleKey.Enter:
                        if (options[selectedOption].CloseParent) {
                            done = true;
                        }
                        if (options[selectedOption].OnExec != null) {
                            val = options[selectedOption].OnExec();
                        }
                        if (options[selectedOption].SubMenu != null) {
                            options[selectedOption].SubMenu.Run(
                                right - 2,
                                top + selectedOption + 2
                            );
                        }
                        break;
                    case ConsoleKey.F10:
                    case ConsoleKey.Escape:
                        done = true;
                        break;
                }
            } while (!done);
            return val;
        }

        private void Draw(int right, int top)
        {
            if (options.Count > 0) {
                right = FmtUtils.ConvertCoord(right, Console.WindowWidth);
                top   = FmtUtils.ConvertCoord(top,   Console.WindowHeight);
                Console.CursorVisible = false;
                // Space, vertical line, space, options, space, vertical line, space
                int w = longestLength + 6;
                // Horizontal lines before and after the options
                int h = options.Count + 2;

                Console.BackgroundColor = ConsoleTheme.Current.MenuBg;
                Console.ForegroundColor = ConsoleTheme.Current.MenuFg;
                string fullHorizLine = new string(Symbols.horizLine, longestLength + 2);
                for (int index = -1, y = top; y < top + h; ++index, ++y) {
                    Console.SetCursorPosition(right - w + 1, y);
                    // Left padding
                    Console.Write(" ");
                    if (index < 0) {
                        // Draw top line
                        Console.Write(Symbols.upperLeftCorner + fullHorizLine + Symbols.upperRightCorner);
                    } else if (index >= options.Count) {
                        // Draw bottom line
                        Console.Write(Symbols.lowerLeftCorner + fullHorizLine + Symbols.lowerRightCorner);
                    } else {
                        ConsoleMenuOption opt = options[index];
                        if (opt == null) {
                            // Draw separator
                            Console.Write(Symbols.leftTee + fullHorizLine + Symbols.rightTee);
                        } else {
                            // Draw menu option
                            Console.Write(Symbols.vertLine);
                            if (index == selectedOption) {
                                // Draw highlighted menu option
                                Console.BackgroundColor = ConsoleTheme.Current.MenuSelectedBg;
                                Console.Write(" " + AnnotatedCaption(opt) + " ");
                                Console.BackgroundColor = ConsoleTheme.Current.MenuBg;
                            } else {
                                // Draw normal menu option
                                Console.Write(" " + AnnotatedCaption(opt) + " ");
                            }
                            Console.Write(Symbols.vertLine);
                        }
                    }
                    // Right padding
                    Console.Write(" ");
                }
                ConsoleDialog.DrawShadow(right - w + 1, top, right, top + h - 1);
                DrawFooter();
                Console.SetCursorPosition(right - longestLength - 3, top + selectedOption + 1);
                Console.CursorVisible = true;
            }
        }

        private void DrawFooter()
        {
            Console.BackgroundColor = ConsoleTheme.Current.FooterBg;
            Console.ForegroundColor = ConsoleTheme.Current.FooterDescriptionFg;
            Console.SetCursorPosition(0, Console.WindowHeight - 1);
            Console.Write("  ");
            // Windows cmd.exe auto-scrolls the whole window if you draw a
            // character in the bottom right corner :(
            Console.Write((options[selectedOption]?.Tooltip ?? "").PadRight(Console.WindowWidth - Console.CursorLeft - 1));
        }

        private string AnnotatedCaption(ConsoleMenuOption opt)
        {
            if (opt.SubMenu != null) {
                return opt.Caption.PadRight(longestLength - 1) + submenuIndicator;
            } else if (opt.RadioActive != null) {
                if (opt.RadioActive()) {
                    return $"({Symbols.dot}) {opt.Caption}".PadRight(longestLength);
                } else {
                    return $"( ) {opt.Caption}".PadRight(longestLength);
                }
            } else {
                return opt.Caption.PadRight(longestLength - opt.Key.Length) + opt.Key;
            }
        }

        private List<ConsoleMenuOption> options;
        private int                     longestLength;
        private int                     selectedOption = 0;

        private static readonly string submenuIndicator = ">";
    }

    /// <summary>
    /// Object representing an option in a menu
    /// </summary>
    public class ConsoleMenuOption {

        /// <summary>
        /// Initialize the option
        /// </summary>
        /// <param name="cap">Text to show in the menu for this option</param>
        /// <param name="key">Text for hotkey to show to the right of the text</param>
        /// <param name="tt">Tooltip to show in footer when this option is highlighted</param>
        /// <param name="close">If true, close the menu after activation, otherwise keep it open</param>
        /// <param name="exec">Function to call if the user chooses this option</param>
        /// <param name="radio">If set, this option is a radio button, and this function returns its value</param>
        /// <param name="submenu">Submenu to open for this option</param>
        public ConsoleMenuOption(string cap, string key, string tt, bool close,
                Func<bool> exec = null, Func<bool> radio = null, ConsolePopupMenu submenu = null)
        {
            Caption     = cap;
            Key         = key;
            Tooltip     = tt;
            CloseParent = close;
            OnExec      = exec;
            SubMenu     = submenu;
            RadioActive = radio;
        }

        /// <summary>
        /// Text to show in the menu for this option
        /// </summary>
        public readonly string           Caption;
        /// <summary>
        /// Text for hotkey to show to the right of the text
        /// </summary>
        public readonly string           Key;
        /// <summary>
        /// Tooltip to show in footer when this option is highlighted
        /// </summary>
        public readonly string           Tooltip;
        /// <summary>
        /// If true, close the menu after activation, otherwise keep it open
        /// </summary>
        public readonly bool             CloseParent;
        /// <summary>
        /// Function to call if the user chooses this option
        /// </summary>
        public readonly Func<bool>       OnExec;
        /// <summary>
        /// If set, this option is a radio button, and this function returns its value
        /// </summary>
        public readonly Func<bool>       RadioActive;
        /// <summary>
        /// Submenu to open for this option
        /// </summary>
        public readonly ConsolePopupMenu SubMenu;
    }

}
