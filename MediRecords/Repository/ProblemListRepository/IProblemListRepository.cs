using System;
using MediRecords.Domain.Entities;

namespace MediRecords.Repository.ProblemListRepository;

public interface IProblemListRepository
{
    Task AddAsync(ProblemList problem);
}
