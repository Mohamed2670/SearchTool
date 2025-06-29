using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SearchTool_ServerSide.Data;
using SearchTool_ServerSide.Models;

namespace SearchTool_ServerSide.Repository
{
    public class MainCompanyRepository : GenericRepository<MainCompany>
    {
        private readonly SearchToolDBContext _context;
        private readonly IMapper _mapper;

        // ⚠️ Test-only key
        private readonly string encryptionKey = "test_key";

        public MainCompanyRepository(SearchToolDBContext context, IMapper mapper) : base(context)
        {
            _context = context;
            _mapper = mapper;
        }

        internal async Task<MainCompany?> AddMainCompanyAsync(MainCompany mainCompany)
        {
            await using var conn = _context.Database.GetDbConnection();
            await conn.OpenAsync();

            await using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
        INSERT INTO ""MainCompanies"" (""Name"", ""SpecialtyId"")
        VALUES (pgp_sym_encrypt(@name, @key), @specialtyId)
        RETURNING ""Id"", pgp_sym_decrypt(""Name"", @key) AS ""Name"", ""SpecialtyId"";
    ";

            cmd.Parameters.Add(new Npgsql.NpgsqlParameter("@name", mainCompany.Name));
            cmd.Parameters.Add(new Npgsql.NpgsqlParameter("@key", encryptionKey));
            cmd.Parameters.Add(new Npgsql.NpgsqlParameter("@specialtyId", mainCompany.SpecialtyId));

            await using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new MainCompany
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    SpecialtyId = reader.GetInt32(2)
                };
            }

            return null;
        }


        internal async Task<IEnumerable<MainCompany>> GetAllMainCompaniesAsync()
        {
            var companies = await _context.MainCompanies
                .FromSqlRaw(@"
                    SELECT ""Id"",
                           pgp_sym_decrypt(""Name"", {0}) AS ""Name"",
                           ""SpecialtyId""
                    FROM ""MainCompanies""
                ", encryptionKey)
                .ToListAsync();

            return companies;
        }

        internal async Task<MainCompany?> GetMainCompanyByIdAsync(int id)
        {
            var company = await _context.MainCompanies
                .FromSqlRaw(@"
                    SELECT ""Id"",
                           pgp_sym_decrypt(""Name"", {0}) AS ""Name"",
                           ""SpecialtyId""
                    FROM ""MainCompanies""
                    WHERE ""Id"" = {1}
                ", encryptionKey, id)
                .FirstOrDefaultAsync();

            return company;
        }
    }

}
