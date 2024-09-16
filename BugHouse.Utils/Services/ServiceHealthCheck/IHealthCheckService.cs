using BugHouse.Utils.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BugHouse.Utils.Services.ServiceHealthCheck
{
    public interface IHealthCheckService
    {
        Task<List<HealthCheck>> Execute();
    }
}
