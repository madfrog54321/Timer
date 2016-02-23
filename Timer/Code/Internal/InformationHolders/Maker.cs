namespace Timer
{
    class Maker
    {
        private string _name;
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        private string _imageUri;
        public string ImageUri
        {
            get { return _imageUri; }
            set { _imageUri = value; }
        }

        public Maker()
        {
            _name = string.Empty;
            _imageUri = string.Empty;
        }
        public Maker(string name)
        {
            _name = name;
            _imageUri = string.Empty;
        }
        public Maker(string name, string imageUri)
        {
            _name = name;
            _imageUri = imageUri;
        }
    }
}
