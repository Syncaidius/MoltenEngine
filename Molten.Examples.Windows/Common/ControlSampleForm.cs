using System.Windows.Forms;

namespace Molten.Samples
{
    public partial class ControlSampleForm : Form
    {
        public ControlSampleForm()
        {
            InitializeComponent();
        }

        private void splitContainer1_SplitterMoved(object sender, SplitterEventArgs e)
        {

        }

        public TrackBar SliderRed => trackRed;

        public TrackBar SliderGreen => trackGreen;

        public TrackBar SliderBlue => trackBlue;
    }
}
