namespace SlimeCharacterForDutch
{
    internal class ImageBuffer : IDisposable
    {
        private readonly Lazy<Image>[] images;
        private readonly Control target;

        private int index = 0;
        private readonly string[] paths;
        private bool isdisposed = false;

        public ImageBuffer(Control target, params string[] paths)
        {
            this.target = target;
            this.paths = paths;
            this.images = new Lazy<Image>[paths.Length];

            int i = 0;
            foreach (string s in paths){
                images[i] = new Lazy<Image>(() => {
                    try
                    {
                        return Image.FromFile(s).GetThumbnailImage(target.Width, target.Height, null, nint.Zero);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.StackTrace);
                        return new Bitmap(target.Width, target.Height);
                    }
                });
                i++;
            }
        }

        public void Dispose()
        {
            foreach (Lazy<Image> lazy in images) {
                if (!lazy.IsValueCreated) continue;
                lazy.Value.Dispose();
            }
            Array.Clear(images);
            Array.Clear(paths);

            isdisposed = true;
        }

        public bool IsAtEnd()
        {
            return index >= images.Length;
        }

        public Image Advance()
        {
            ObjectDisposedException.ThrowIf(isdisposed, this);
            if (index > 0){ //deload the last image
                if (images[index - 1].IsValueCreated) {
                    images[index - 1].Value.Dispose();//reset the lazy<T>
                    images[index - 1] = new Lazy<Image>(() => {
                        try {
                            return Image.FromFile(paths[index - 1]).GetThumbnailImage(target.Width, target.Height, null, nint.Zero);
                        }
                        catch (Exception e) {
                            Console.WriteLine(e.Message);
                            return new Bitmap(target.Width, target.Height);
                        }
                    });
                }
            }

            if (index >= images.Length) index = 0;
            return images[index++].Value;
        }
    }
}
