using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlimeCharacterForDutch
{
    internal class Animation : IDisposable
    {
        private readonly ImageBuffer? buffer;
        private readonly System.Windows.Forms.Timer? timer;
        private readonly Form main;

        private bool isDisposed = false;
        public bool isLooped = false;
        public bool isPlaying = false;

        public Animation(Form main, string n, int delay, params string[] paths) {
            buffer = new ImageBuffer(main, n, paths);
            timer = new System.Windows.Forms.Timer() { Interval = delay };
            this.main = main;

            timer.Tick += Timer_Tick;
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            if (isDisposed) return;
            if (buffer == null) return;

            if (buffer.IsAtEnd() && !isLooped) timer?.Stop();
            main.Invoke(() => { main.BackgroundImage = buffer.Advance(); });
        }

        public void Play()
        {
            if (isDisposed) return;
            timer?.Start();
            isPlaying = true;
        }

        public void Stop()
        {
            if (isDisposed) return;
            timer?.Stop();
            isPlaying = false;
        }

        public void Dispose()
        {
            if (isDisposed) return;
            if (timer != null) { timer.Stop(); timer.Tick -= Timer_Tick; };

            buffer?.Dispose();
            timer?.Dispose();
            isDisposed = true;
            isPlaying = false;
        }
    }
}
