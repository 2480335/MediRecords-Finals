using System;
using MediRecords.Domain.Entities;

namespace MediRecords.Repository.AllergyRepository;

public interface IAllergyRepository
{
    Task AddAsync(Allergy allergy);
}
