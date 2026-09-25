namespace CyberUnderground.Simulation
{
    public sealed class OrganizationSystem
    {
        readonly CompanyDef[] _companies;

        public OrganizationSystem(WorldData world)
        {
            _companies = world.Companies ?? new CompanyDef[0];
        }

        public CompanyDef[] All()
        {
            return _companies;
        }

        public CompanyDef Find(string query)
        {
            if (string.IsNullOrEmpty(query))
                return null;
            query = query.ToLowerInvariant();
            if (query.StartsWith("$"))
                query = query.Substring(1);

            for (int i = 0; i < _companies.Length; i++)
            {
                var company = _companies[i];
                if (company.Id == query)
                    return company;
                if (company.Aliases == null)
                    continue;
                for (int a = 0; a < company.Aliases.Length; a++)
                {
                    if (company.Aliases[a] == query)
                        return company;
                }
            }
            return null;
        }
    }
}
