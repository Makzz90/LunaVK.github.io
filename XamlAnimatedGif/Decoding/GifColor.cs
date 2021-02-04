namespace XamlAnimatedGif.Decoding
{
    internal struct GifColor
    {
        internal GifColor(byte r, byte g, byte b)
        {
            R = r;
            G = g;
            B = b;
        }

        public byte R;// { get; private set; }
        public byte G;// { get; private set; }
        public byte B;// { get; private set; }

        public override string ToString()
        {
            //return $"#{R:x2}{G:x2}{B:x2}";
            return string.Format("#{0:x2}{1:x2}{2:x2}", this.R, this.G, this.B);
        }
    }
}
