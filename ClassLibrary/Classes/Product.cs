using System.Data.SqlClient;

namespace ClassLibrary.Classes
{
    public sealed class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public double Price { get; set; }
        public int Count { get; set; }
        public int SubcategoryId { get; set; }
        public int StorageId { get; set; }

        public static Product GetProduct(SqlDataReader reader)
        {
            return new Product
            {
                Id            = int.Parse(reader["ID"].ToString()),
                Name          = reader["Name"].ToString(),
                Description   = reader["Description"].ToString(),
                Count         = int.Parse(reader["Count"].ToString()),
                Price         = double.Parse(reader["Price"].ToString()),
                SubcategoryId = int.Parse(reader["SubcategoryID"].ToString()),
                StorageId     = int.Parse(reader["StorageID"].ToString())
            };
        }
    }
}