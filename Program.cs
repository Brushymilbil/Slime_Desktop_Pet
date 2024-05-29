using System.Diagnostics;
using System.Numerics;

namespace SlimeCharacterForDutch
{
    internal class Program : Form
    {
        private readonly Animation eat;
        private readonly Animation move;
        private readonly Animation die;
        private readonly Animation idle;

        private readonly Lazy<Image> PineAppleImage;
        private readonly Random random = new();
        private Form? CurrentPineapple;

        private int hunger = 150;
        private bool isDragging = false;
        private bool isDown = false;
        private bool isMoving = false;

        private const int PINEAPPLE_WIDTH = 200;
        private const int PINEAPPLE_HEIGHT = 200;

        public Program()
        {
            this.BackColor = Color.FromArgb(254, 254, 254);
            this.TransparencyKey = this.BackColor;
            this.FormBorderStyle = FormBorderStyle.None;

            this.MaximumSize = new(300, 300);
            this.MinimumSize = new(300, 300);
            this.DoubleBuffered = true;

            CenterToScreen();
            idle = new Animation(this, "idle",500, Path.GetFullPath("Images\\idle.png"), Path.GetFullPath("Images\\idle2.png")) { isLooped = true };
            move = new Animation(this, "move",125, Path.GetFullPath("Images\\walk.png"), Path.GetFullPath("Images\\walk2.png"),
                    Path.GetFullPath("Images\\walk3.png"), Path.GetFullPath("Images\\walk4.png"), Path.GetFullPath("Images\\walk5.png"),
                    Path.GetFullPath("Images\\walk6.png"), Path.GetFullPath("Images\\walk7.png"))
                { isLooped = true };

            die = new Animation(this, "die",250, Path.GetFullPath("Images\\die.png"), Path.GetFullPath("Images\\die1.png"), Path.GetFullPath("Images\\die2.png"), Path.GetFullPath("Images\\die3.png"));
            eat = new Animation(this, "eat",250, Path.GetFullPath("Images\\eat.png"), Path.GetFullPath("Images\\eat2.png"), Path.GetFullPath("Images\\eat3.png"), Path.GetFullPath("Images\\eat4.png"));
            
            PineAppleImage = new Lazy<Image>(() => {
                try {
                    return Image.FromFile(Path.GetFullPath("Images\\Pineapple.png")).GetThumbnailImage(PINEAPPLE_WIDTH, PINEAPPLE_HEIGHT, null, nint.Zero);
                }
                catch (Exception e) {
                    Console.WriteLine(e.StackTrace);
                    return new Bitmap(100, 100);
                }
            });

            this.MouseDown += (object? sender, MouseEventArgs e) => {
                isDown = true;
            };

            this.MouseMove += (object? sender, MouseEventArgs e) => {
                if (!isDown) return;
                if (idle.isPlaying) idle.Stop();
                isDragging = true;

                this.Location = new(MousePosition.X - this.Size.Width/2, MousePosition.Y - this.Size.Height/2);
            };

            this.MouseUp += (object? sender, MouseEventArgs e) => {
                isDown = false;
                if (isDragging) { isDragging = false; AI(); }
            };
            AI();
        }

        private async void MoveTo()
        {
            if (isMoving) return;
            isMoving = true;

            if (!move.isPlaying) move.Play();
            if (idle.isPlaying) idle.Stop();

            int x = Screen.PrimaryScreen!.Bounds.Width;
            int y = Screen.PrimaryScreen!.Bounds.Height;

            Vector2 randomPtOnScreen = new(random.Next(this.Size.Width, x), random.Next(this.Size.Height, x));
            Vector2 start = new(this.Location.X, this.Location.Y);

            if (randomPtOnScreen.X >= x - 350)  randomPtOnScreen.X = x - 350;
            if (randomPtOnScreen.Y >= y - 350) randomPtOnScreen.Y = y - 350;

            float step = 0.1f;
            float t = 0;

            while (t < 1) {
                if (isDragging) { isMoving = false; return; }
                t += step;
                Vector2 newLoc = Vector2.Lerp(start, randomPtOnScreen, t);
                this.Location = new((int)newLoc.X, (int)newLoc.Y);

                await Task.Delay(100);
            }

            move.Stop();
            idle.Play();

            hunger -= 10;
            isMoving = false;
        }

        private async void AI()
        {
            idle.Play();

            while (hunger > 0) {
                if (isDragging) return;//don't kill the npc, just pass.
                int actionId = random.Next(1, 4);
                switch (actionId){
                    case 1:
                        MoveTo();
                        break;
                    case 2:
                        if (!idle.isPlaying) idle.Play();
                        await Task.Delay(1000);
                        break;
                    case 3:
                        if (CurrentPineapple != null) break;
                        CurrentPineapple = new Form()
                        {
                            BackgroundImage = PineAppleImage.Value,
                            BackColor = Color.FromArgb(254, 254, 254),
                            TransparencyKey = Color.FromArgb(254, 254, 254),
                            FormBorderStyle = FormBorderStyle.None,
                            Size = new(PINEAPPLE_WIDTH, PINEAPPLE_HEIGHT),
                            Location = new(random.Next(PINEAPPLE_WIDTH, Screen.PrimaryScreen!.Bounds.Width), random.Next(PINEAPPLE_HEIGHT, Screen.PrimaryScreen!.Bounds.Height)),
                        };

                        CurrentPineapple.MouseMove += async (object? sender, MouseEventArgs e) => {
                            if (Program.MouseButtons != MouseButtons.Left) return;
                            CurrentPineapple.Location = new(MousePosition.X - CurrentPineapple.Size.Width / 2, MousePosition.Y - CurrentPineapple.Size.Height / 2);

                            if (!this.Bounds.Contains(CurrentPineapple.Location)) return;
                            CurrentPineapple.Close();
                            CurrentPineapple.Dispose();
                            CurrentPineapple = null;

                            isDragging = true;
                            hunger += 50;

                            eat.Play();
                            idle.Stop();
                            move.Stop();

                            await Task.Delay(1000);
                            eat.Stop();
                            isDragging = false;

                            AI();
                        };
                        CurrentPineapple.Show();
                        break;
                }

                await Task.Delay(1000);
            }

            if (idle.isPlaying) idle.Stop();
            if (move.isPlaying) move.Stop();

            move.Dispose();
            idle.Dispose();

            die.Play();
            await Task.Delay(1500);
            die.Dispose();

            Application.Exit();
        }
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            Application.Run(new Program());
        }
    }
}