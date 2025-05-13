using ServerSide.Model;

namespace SearchTool_ServerSide.Models
{
    public class DrugClassV2: IEntity
    {
        public int Id { get; set; }
        public required string Name { get; set; }
    }
}