using LloydWarningSystem.Net.Context;

namespace LloydWarningSystem.Net.Services;

public class DatabaseService
{
    private readonly LloydContext _dbContext;

    public DatabaseService(LloydContext dbContext)
    {
        _dbContext = dbContext;
    }

}
