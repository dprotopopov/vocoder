namespace vocoder.DatabaseLibrary
{
    public class AudioFile : Record
    {
        public object Id { get; set; }
        public object FileName { get; set; }
        public object ContactId { get; set; }

        public override string ToString()
        {
            return string.Join(" - ",
                new[]
                {
                    Id.ToString(),
                    FileName.ToString()
                });
        }
    }
}