namespace PhotoBackup.Logic
{
    public class Album
    {
        public string Title { get; set; }

        public override bool Equals(object obj)
        {
            var item = obj as Album;

            return item != null && item.Title.Equals(Title);
        }

        public override int GetHashCode()
        {
            return Title.GetHashCode();
        }
    }
}