using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal class FrameTrackerVK
    {
        const int INITIAL_BRANCH_COUNT = 3;
        const int INITIAL_COMMAND_COUNT = 10;

        DeviceVK _device;
        CommandListVK[][] _cmds;
        uint _index;

        public FrameTrackerVK(DeviceVK device)
        {
            _device = device;
            _cmds = new CommandListVK[INITIAL_BRANCH_COUNT][];
            for(int branchID = 0; branchID < _cmds.Length; branchID++)
                _cmds[branchID] = new CommandListVK[INITIAL_COMMAND_COUNT];
        }

        public void Track(CommandListVK cmd)
        {
            if(cmd.BranchIndex == _cmds.Length)
                Array.Resize(ref _cmds, _cmds.Length + INITIAL_BRANCH_COUNT);

            CommandListVK[] branch = _cmds[cmd.BranchIndex];

            if (cmd.Index == branch.Length)
            {
                Array.Resize(ref branch, branch.Length + INITIAL_COMMAND_COUNT);
                _cmds[cmd.BranchIndex] = branch;
            }

            branch[cmd.Index] = cmd;
        }

        internal void Reset()
        {
            _index = 0;
        }

        internal uint BranchCount { get; set; }
    }
}
