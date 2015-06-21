namespace vocoder.DatabaseLibrary
{
    public class Contact : Record
    {
        public object Id { get; set; }
        public object FirstName { get; set; }
        public object LastName { get; set; }
        public object Phone { get; set; }
        public object Email { get; set; }

        public override string ToString()
        {
            return string.Join(" - ",
                new[]
                {
                    Id.ToString(),
                    FirstName.ToString(),
                    LastName.ToString(),
                    Phone.ToString(),
                    Email.ToString()
                });
        }
    }
}