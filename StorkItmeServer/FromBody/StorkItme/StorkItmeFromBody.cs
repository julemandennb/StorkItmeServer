namespace StorkItmeServer.FromBody.StorkItme
{
    public class StorkItmeFromBody
    {
        public string Name { get; set; }

        public string Description { get; set; } = "";

        public string Type { get; set; } = "";

        public DateTime BestBy { get; set; }

        public int Stork { get; set; }

        public string ImgUrl { get; set; } = "";

        public int UserGroupId { get; set; }
    }
}
