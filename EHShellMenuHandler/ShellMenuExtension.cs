using SharpShell.Attributes;
using SharpShell.SharpContextMenu;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace EHShellMenuHandler
{
    [ComVisible(true)]
    [COMServerAssociation(AssociationType.ClassOfExtension, ".exe")]
    public class ShellMenuExtension : SharpContextMenu
    {
        private ContextMenuStrip menu = new ContextMenuStrip();

        /// <summary>
        /// Determines whether the menu item can be shown for the selected item.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if item can be shown for the selected item for this instance.; 
        ///   otherwise, <c>false</c>.
        /// </returns>
        protected override bool CanShowMenu()
        {
            // We can show the item only for a single selection.
            if (SelectedItemPaths.Count() == 1)
            {
                this.UpdateMenu();
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Creates the context menu. This can be a single menu item or a tree of them.
        /// Here we create the menu based on the type of item
        /// </summary>
        /// <returns>
        /// The context menu for the shell context menu.
        /// </returns>
        protected override ContextMenuStrip CreateMenu()
        {
            menu.Items.Clear();

            bool is64bit = PEFileReader.GetPEType(SelectedItemPaths.First()) == PEType.X64 ? true : false;
            // check if the selected executable is 64 bit
            if (is64bit)
            {
                this.MenuX64();
            }
            else
            {
                this.MenuX86();
            }

            // return the menu item
            return menu;
        }

        private void MenuX64()
        {
            ToolStripMenuItem MainMenu;
            MainMenu = new ToolStripMenuItem
            {
                Text = "Start with Eroge Helper",
                Image = Resource.E
            };
            MainMenu.Click += (sender, args) => MainProcess(false);

            menu.Items.Clear();
            menu.Items.Add(MainMenu);
        }

        /// <summary>
        /// Creates the context menu when the selected .exe is 32 bit.
        /// </summary>
        protected void MenuX86()
        {
            ToolStripMenuItem MainMenu;
            MainMenu = new ToolStripMenuItem
            {
                Text = "Eroge Helper",
                Image = Resource.E
            };

            var DirectStartItem = new ToolStripMenuItem
            {
                Text = "直接启动游戏",
                Image = Resource.E
            };

            var LEStartItem = new ToolStripMenuItem
            {
                Text = "使用Locate Emulator启动",
                Image = Resource.E
            };

            DirectStartItem.Click += (sender, args) => MainProcess(false);
            LEStartItem.Click += (sender, args) => MainProcess(true);

            MainMenu.DropDownItems.Add(DirectStartItem);
            MainMenu.DropDownItems.Add(LEStartItem);

            menu.Items.Clear();
            menu.Items.Add(MainMenu);
        }

        private void MainProcess(bool useLE)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();

            #region Get Path of dll (Same as project binary Path)
            // https://stackoverflow.com/questions/52797/how-do-i-get-the-path-of-the-assembly-the-code-is-in
            string codeBase = Assembly.GetExecutingAssembly().CodeBase;
            UriBuilder uri = new UriBuilder(codeBase);
            string path = Uri.UnescapeDataString(uri.Path);
            #endregion

            var dirPath = Path.GetDirectoryName(path);
            var ErogeHelperProc = dirPath + @"\ErogeHelper.exe";
            string gamePath = SelectedItemPaths.First();
            startInfo.FileName = ErogeHelperProc;
            startInfo.Arguments = $"\"{gamePath}\"";
            startInfo.UseShellExecute = false;

            if (useLE)
            {
                // gamePath must be Args[0]
                startInfo.Arguments += " /le";
            }

            try
            {
                Process.Start(startInfo);
            }
            catch (System.ComponentModel.Win32Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        /// <summary>
        /// Updates the context menu. 
        /// </summary>
        private void UpdateMenu()
        {
            // release all resources associated to existing menu
            menu.Dispose();
            menu = CreateMenu();
        }
    }
}
