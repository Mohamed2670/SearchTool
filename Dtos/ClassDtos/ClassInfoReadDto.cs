namespace SearchTool_ServerSide.Dtos.ClassDtos
{
    public class ClassInfoReadDto
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public int ClassTypeId { get; set; }
        public string ClassTypeName { get; set; } = string.Empty;
    }
}