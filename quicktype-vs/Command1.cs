using System;
using System.ComponentModel.Design;
using System.IO;
using System.Windows.Forms;
using System.Linq;
using System.Globalization;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using System.Collections.Generic;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace quicktype_vs
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class Command1
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("3add360c-c3bd-4bd8-91cc-305f37b46106");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly Package package;

        private static readonly string[] LanguageNames = { "c++", "cpp", "cplusplus", "cs", "csharp", "elm", "go", "golang", "java", "objc", "objective-c", "objectivec", "swift", "typescript", "ts", "tsx" };

        /// <summary>
        /// Initializes a new instance of the <see cref="Command1"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        private Command1(Package package)
        {
            if (package == null)
            {
                throw new ArgumentNullException("package");
            }

            this.package = package;

            OleMenuCommandService commandService = this.ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (commandService != null)
            {
                var menuCommandID = new CommandID(CommandSet, CommandId);
                var menuItem = new MenuCommand(this.MenuItemCallback, menuCommandID);
                commandService.AddCommand(menuItem);
            }
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static Command1 Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private IServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static void Initialize(Package package)
        {
            Instance = new Command1(package);
        }

        private IWpfTextView GetWpfView()
        {
            var textManager = (IVsTextManager)ServiceProvider.GetService(typeof(SVsTextManager));
            var componentModel = (IComponentModel)this.ServiceProvider.GetService(typeof(SComponentModel));
            var editor = componentModel.GetService<IVsEditorAdaptersFactoryService>();

            textManager.GetActiveView(1, null, out IVsTextView textViewCurrent);
            return editor.GetWpfTextView(textViewCurrent);
        }

        private void Message(string message)
        {
            VsShellUtilities.ShowMessageBox(
                this.ServiceProvider,
                message,
                "Paste JSON as Code",
                OLEMSGICON.OLEMSGICON_INFO,
                OLEMSGBUTTON.OLEMSGBUTTON_OK,
                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void MenuItemCallback(object sender, EventArgs e)
        {
            var jsonText = Clipboard.GetText().Trim();
            if (jsonText.Length == 0)
            {
                Message("Cannot paste - the clipboard is empty");
                return;
            }

            var dte = Package.GetGlobalService(typeof(DTE)) as DTE;
            var doc = dte.ActiveDocument.Object() as TextDocument;
            var language = doc.Language.ToLower();

            if (!LanguageNames.Contains(language))
            {
                Message("Language \"" + doc.Language + "\" not supported");
                return;
            }

            ITextDocument document;
            this.GetWpfView().TextDataModel.DocumentBuffer.Properties.TryGetProperty(typeof(ITextDocument), out document);
            var topLevelName = Path.GetFileNameWithoutExtension(document.FilePath);

            var jsonFilename = Path.GetTempFileName();
            File.WriteAllText(jsonFilename, jsonText);

            var vsixPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            var quicktypePath = Path.Combine(vsixPath, "Resources", "quicktype.exe");

            var p = new System.Diagnostics.Process();
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.FileName = quicktypePath;
            p.StartInfo.Arguments = "--telemetry disable --lang \"" + language + "\" --top-level \"" + topLevelName + "\" \"" + jsonFilename + "\"";
            p.Start();
            string output = p.StandardOutput.ReadToEnd();
            p.WaitForExit();

            if (p.ExitCode != 0)
            {
                string error = p.StandardError.ReadToEnd();
                Message("quicktype could not process your JSON:\n\n" + error);
                return;
            }

            doc.Selection.Insert(output);
        }
    }
}
