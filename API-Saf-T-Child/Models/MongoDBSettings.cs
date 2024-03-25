namespace API_Saf_T_Child.Models
{
    public class MongoDBSettings
    {
        public string ConnectionURI { get; set; } = null!;
        public string DatabaseName { get; set; } = null!;
        public string UsersCollection { get; set; } = null!;
        public string GroupsCollection { get; set; } = null!;
        public string DevicesCollection { get; set; } = null!;
        public string VehiclesCollection { get; set; } = null!;

        public string TempUsersCollection { get; set; } = null!;
    }
}
