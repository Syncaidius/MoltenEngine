using Molten.Audio;
using Molten.Data;
using Molten.Graphics;
using Molten.UI;

namespace Molten.Samples
{
    public class AudioCaptureExample : SampleGame
    {

        

        public override string Description => "Demonstrates audio recording/capture";

        public AudioCaptureExample() : base("Audio Capture") { }

        protected override void OnLoadContent(ContentLoadBatch loader)
        {

        }

        private void Loader_OnCompleted(ContentLoadBatch loader)
        {

        }



        protected override void OnUpdate(Timing time)
        {
            base.OnUpdate(time);



        }



        protected override void OnDrawSprites(SpriteBatcher sb)
        {
            if (SampleFont == null)
                return;

            base.OnDrawSprites(sb);

            
        }
    }
}
