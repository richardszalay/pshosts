using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Management.Automation.Host;
using System.Text;

namespace RichardSzalay.Hosts.Powershell.Tests.Infrastructure
{
    public class TestPSHost : PSHost
    {
        private readonly CultureInfo currentCulture = CultureInfo.CurrentCulture;
        private readonly CultureInfo currentUICulture = CultureInfo.CurrentCulture;

        public override System.Globalization.CultureInfo CurrentCulture
        {
            get { return currentCulture; }
        }

        public override System.Globalization.CultureInfo CurrentUICulture
        {
            get { return currentUICulture; }
        }

        public override void EnterNestedPrompt()
        {
        }

        public override void ExitNestedPrompt()
        {
        }

        private readonly Guid instanceId = Guid.NewGuid();

        public override Guid InstanceId
        {
            get { return instanceId; }
        }

        public override string Name
        {
            get { return "PsHosts host fixture"; }
        }

        public override void NotifyBeginApplication()
        {
        }

        public override void NotifyEndApplication()
        {
        }

        public override void SetShouldExit(int exitCode)
        {
        }

        private TestPSHostUserInterface ui = new TestPSHostUserInterface();

        public override PSHostUserInterface UI
        {
            get { return ui; }
        }

        public override Version Version
        {
            get { return new Version(); }
        }
    }

    public class TestPSHostUserInterface : PSHostUserInterface
    {
        public override Dictionary<string, System.Management.Automation.PSObject> Prompt(string caption, string message,
            System.Collections.ObjectModel.Collection<FieldDescription> descriptions)
        {
            throw new NotImplementedException();
        }

        public event Func<string, string, Collection<ChoiceDescription>, int, int> PromptedForChoice;

        public override int PromptForChoice(string caption, string message,
            System.Collections.ObjectModel.Collection<ChoiceDescription> choices, int defaultChoice)
        {
            var handler = PromptedForChoice;

            if (handler != null)
            {
                return handler(caption, message, choices, defaultChoice);
            }
            else
            {
                throw new InvalidOperationException("Prompted for choice but no handler was assigned");
            }
        }

        public override System.Management.Automation.PSCredential PromptForCredential(string caption, string message,
            string userName, string targetName, System.Management.Automation.PSCredentialTypes allowedCredentialTypes,
            System.Management.Automation.PSCredentialUIOptions options)
        {
            throw new NotImplementedException();
        }

        public override System.Management.Automation.PSCredential PromptForCredential(string caption, string message,
            string userName, string targetName)
        {
            throw new NotImplementedException();
        }

        readonly PSHostRawUserInterface rawUI = new TestPSHostRawUserInterface();

        public override PSHostRawUserInterface RawUI
        {
            get { return rawUI; }
        }

        public override string ReadLine()
        {
            throw new NotImplementedException();
        }

        public override System.Security.SecureString ReadLineAsSecureString()
        {
            throw new NotImplementedException();
        }

        private StringBuilder writeBuffer = new StringBuilder();

        public override void Write(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string value)
        {
            writeBuffer.AppendLine(value);
        }

        public override void Write(string value)
        {
            writeBuffer.AppendLine(value);
        }

        public override void WriteDebugLine(string message)
        {
            writeBuffer.AppendLine(message);
        }

        public override void WriteErrorLine(string value)
        {
            writeBuffer.AppendLine(value);
        }

        public override void WriteLine(string value)
        {
            writeBuffer.AppendLine(value);
        }

        public override void WriteProgress(long sourceId, System.Management.Automation.ProgressRecord record)
        {
            throw new NotImplementedException();
        }

        public override void WriteVerboseLine(string message)
        {
            writeBuffer.AppendLine(message);
        }

        public override void WriteWarningLine(string message)
        {
            writeBuffer.AppendLine(message);
        }
    }

    public class TestPSHostRawUserInterface : PSHostRawUserInterface
    {

        public override ConsoleColor BackgroundColor
        {
            get;
            set;
        }

        public override Size BufferSize
        {
            get;
            set;
        }

        public override Coordinates CursorPosition
        {
            get;
            set;
        }

        public override int CursorSize
        {
            get;
            set;
        }

        public override void FlushInputBuffer()
        {
        }

        public override ConsoleColor ForegroundColor
        {
            get;
            set;
        }

        public override BufferCell[,] GetBufferContents(Rectangle rectangle)
        {
            return new BufferCell[0, 0];
        }

        public override bool KeyAvailable
        {
            get { return false; }
        }

        public override Size MaxPhysicalWindowSize
        {
            get { return new Size(); }
        }

        public override Size MaxWindowSize
        {
            get { return new Size(); }
        }

        public override KeyInfo ReadKey(ReadKeyOptions options)
        {
            throw new NotImplementedException();
        }

        public override void ScrollBufferContents(Rectangle source, Coordinates destination, Rectangle clip,
            BufferCell fill)
        {
        }

        public override void SetBufferContents(Rectangle rectangle, BufferCell fill)
        {
        }

        public override void SetBufferContents(Coordinates origin, BufferCell[,] contents)
        {
        }

        public override Coordinates WindowPosition
        {
            get;
            set;
        }

        public override Size WindowSize
        {
            get;
            set;
        }

        public override string WindowTitle
        {
            get;
            set;
        }
    }
}