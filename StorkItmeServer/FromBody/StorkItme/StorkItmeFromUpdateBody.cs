namespace StorkItmeServer.FromBody.StorkItme
{
    public class StorkItmeFromUpdateBody
    {
        public string ?Name { get; set; }

        public string ?Description { get; set; }

        public string ?Type { get; set; } 

        public DateTime ?BestBy { get; set; }

        public int ?Stork { get; set; }

        public int ?UserGroupId { get; set; }

        public string ? StoreLocation { get; set; }

        public string ?ItemNumber { get; set; }

        public string ?EAN { get; set; }
    }
}
