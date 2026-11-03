using Daycare.Domain.Entities;

namespace Daycare.Domain.Repositories;

public interface IClassroomRepository
{
    Task<Classroom?> GetByIdAsync(ClassroomId id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Classroom>> GetAllAsync(int page = 1, int pageSize = 10, CancellationToken cancellationToken = default);
    Task<IEnumerable<Classroom>> SearchAsync(string term, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default);
    Task<IEnumerable<Classroom>> GetByStatusAsync(ClassroomStatus status, CancellationToken cancellationToken = default);
    Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Classroom classroom, CancellationToken cancellationToken = default);
    Task UpdateAsync(Classroom classroom, CancellationToken cancellationToken = default);
    Task DeleteAsync(Classroom classroom, CancellationToken cancellationToken = default);
}
