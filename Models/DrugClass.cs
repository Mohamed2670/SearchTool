using ServerSide.Model;

namespace SearchTool_ServerSide.Models
{
    public class DrugClass: IEntity
    {
        public int Id { get; set; }
        public required string Name { get; set; }
    }
}