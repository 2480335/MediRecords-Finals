using MediRecords.Domain.Entities;
using MediRecords.Domain.Enums;
using MediRecords.Dto.PrescriptionWithItemsDtos;
using Microsoft.EntityFrameworkCore;

namespace MediRecords.Repository.PrescriptionWithItemsRepository;

public class PrescriptionWithItemsRepository : IPrescriptionWithItemsRepository
{
    private readonly MediRecordsDbContext _context;

    public PrescriptionWithItemsRepository(MediRecordsDbContext context)
    {
        _context = context;
    }

    public async Task<PrescriptionWithItemsResponseDto> CreatePrescriptionWithItemsAsync(CreatePrescriptionWithItemsRequestDto request)
    {
        try
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (request.EncounterId <= 0)
                throw new ArgumentException("EncounterId must be provided.", nameof(request.EncounterId));

            if (request.ProviderId <= 0)
                throw new ArgumentException("ProviderId must be provided.", nameof(request.ProviderId));

            if (request.PrescriptionItems == null || !request.PrescriptionItems.Any())
                throw new ArgumentException("At least one prescription item must be provided.", nameof(request.PrescriptionItems));

            var encounter = await _context.Encounters.FindAsync(request.EncounterId);
            if (encounter == null)
                throw new InvalidOperationException($"Encounter with ID {request.EncounterId} not found.");

            if (encounter.Status != Domain.Enums.EncounterStatus.Open)
                throw new InvalidOperationException($"Encounter status is {encounter.Status}, but must be Open to create prescriptions.");

            // Parse status string to enum
            var prescriptionStatus = Enum.Parse<PrescriptionStatus>(request.Status ?? "Draft", ignoreCase: true);
            var medicationStatus = prescriptionStatus == PrescriptionStatus.Issued ? MedicationStatus.Active : MedicationStatus.Inactive;

            // Get patient ID from encounter
            var patient = await _context.Patients.FindAsync(encounter.PatientId);
            if (patient == null)
                throw new InvalidOperationException($"Patient with ID {encounter.PatientId} not found.");

        var prescription = new Prescription
        {
            EncounterId = request.EncounterId,
            ProviderId = request.ProviderId,
            CreatedDate = DateTime.Now,
            Status = prescriptionStatus
        };

        foreach (var item in request.PrescriptionItems)
        {
            prescription.PrescriptionItems.Add(new PrescriptionItem
            {
                DrugName = item.DrugName,
                Dose = item.Dose ?? string.Empty,
                Frequency = item.Frequency ?? string.Empty,
                Route = item.Route ?? string.Empty,
                DurationDays = item.DurationDays,
                Instructions = item.Instructions ?? string.Empty
            });

            // Sync to MedicationList
            var medicationListEntry = new MedicationList
            {
                PatientId = patient.PatientId,
                DrugName = item.DrugName,
                Dose = item.Dose ?? string.Empty,
                Frequency = item.Frequency ?? string.Empty,
                Route = null,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(item.DurationDays),
                Status = medicationStatus
            };
            _context.MedicationLists.Add(medicationListEntry);
        }

        _context.Prescriptions.Add(prescription);
        await _context.SaveChangesAsync();

        return new PrescriptionWithItemsResponseDto
        {
            PrescriptionId = prescription.PrescriptionId,
            EncounterId = prescription.EncounterId,
            ProviderId = prescription.ProviderId,
            CreatedDate = DateOnly.FromDateTime(prescription.CreatedDate),
            Status = prescription.Status.ToString(),
            PrescriptionItems = prescription.PrescriptionItems.Select(pi => new PrescriptionItemWithPrescriptionResponseDto
            {
                ItemId = pi.ItemId,
                PrescriptionId = pi.PrescriptionId,
                DrugName = pi.DrugName,
                Dose = pi.Dose,
                Frequency = pi.Frequency,
                DurationDays = pi.DurationDays,
                Instructions = pi.Instructions
            }).ToList()
        };
        }
        catch (DbUpdateException dbEx)
        {
            var innerException = dbEx.InnerException?.Message ?? dbEx.Message;
            throw new InvalidOperationException($"Database error creating prescription: {innerException}", dbEx);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error creating prescription: {ex.Message}", ex);
        }
    }

    public async Task<PrescriptionWithItemsResponseDto?> GetPrescriptionWithItemsByIdAsync(int prescriptionId)
    {
        var prescription = await _context.Prescriptions
            .Include(p => p.PrescriptionItems)
            .FirstOrDefaultAsync(p => p.PrescriptionId == prescriptionId);

        if (prescription == null)
            return null;

        return new PrescriptionWithItemsResponseDto
        {
            PrescriptionId = prescription.PrescriptionId,
            EncounterId = prescription.EncounterId,
            ProviderId = prescription.ProviderId,
            CreatedDate = DateOnly.FromDateTime(prescription.CreatedDate),
            Status = prescription.Status.ToString(),
            PrescriptionItems = prescription.PrescriptionItems.Select(pi => new PrescriptionItemWithPrescriptionResponseDto
            {
                ItemId = pi.ItemId,
                PrescriptionId = pi.PrescriptionId,
                DrugName = pi.DrugName,
                Dose = pi.Dose,
                Frequency = pi.Frequency,
                Route = pi.Route,
                DurationDays = pi.DurationDays,
                Instructions = pi.Instructions
            }).ToList()
        };
    }

    public async Task<Encounter?> GetEncounterByIdAsync(int encounterId)
    {
        return await _context.Encounters.FindAsync(encounterId);
    }

    public async Task<IEnumerable<PrescriptionWithItemsResponseDto>> GetAllPrescriptionsWithItemsAsync()
    {
        var prescriptions = await _context.Prescriptions
            .Include(p => p.PrescriptionItems)
            .ToListAsync();

        return prescriptions.Select(p => new PrescriptionWithItemsResponseDto
        {
            PrescriptionId = p.PrescriptionId,
            EncounterId = p.EncounterId,
            ProviderId = p.ProviderId,
            CreatedDate = DateOnly.FromDateTime(p.CreatedDate),
            Status = p.Status.ToString(),
            PrescriptionItems = p.PrescriptionItems.Select(pi => new PrescriptionItemWithPrescriptionResponseDto
            {
                ItemId = pi.ItemId,
                PrescriptionId = pi.PrescriptionId,
                DrugName = pi.DrugName,
                Dose = pi.Dose,
                Frequency = pi.Frequency,
                Route = pi.Route,
                DurationDays = pi.DurationDays,
                Instructions = pi.Instructions
            }).ToList()
        }).ToList();
    }

    public async Task<PrescriptionWithItemsResponseDto> UpdatePrescriptionWithItemsAsync(int prescriptionId, UpdatePrescriptionWithItemsRequestDto request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        if (prescriptionId <= 0)
            throw new ArgumentException("PrescriptionId must be provided.", nameof(prescriptionId));

        if (request.ProviderId <= 0)
            throw new ArgumentException("ProviderId must be provided.", nameof(request.ProviderId));

        if (request.PrescriptionItems == null || !request.PrescriptionItems.Any())
            throw new ArgumentException("At least one prescription item must be provided.", nameof(request.PrescriptionItems));

        try
        {
            var prescription = await _context.Prescriptions
                .Include(p => p.PrescriptionItems)
                .FirstOrDefaultAsync(p => p.PrescriptionId == prescriptionId);

            if (prescription == null)
                throw new InvalidOperationException("Prescription not found.");

            // Get the encounter to access patient info
            var encounter = await _context.Encounters.FindAsync(prescription.EncounterId);
            if (encounter == null)
                throw new InvalidOperationException("Encounter not found.");

            var patient = await _context.Patients.FindAsync(encounter.PatientId);
            if (patient == null)
                throw new InvalidOperationException("Patient not found.");

            // Parse status string to enum
            var prescriptionStatus = Enum.Parse<PrescriptionStatus>(request.Status ?? "Draft", ignoreCase: true);
            var medicationStatus = prescriptionStatus == PrescriptionStatus.Issued ? MedicationStatus.Active : MedicationStatus.Inactive;

            prescription.ProviderId = request.ProviderId;
            prescription.Status = prescriptionStatus;

            // Remove old prescription items
            _context.PrescriptionItems.RemoveRange(prescription.PrescriptionItems);

            // Remove old medication list entries for this prescription
            var oldMedicationListEntries = await _context.MedicationLists
                .Where(m => m.PatientId == patient.PatientId)
                .ToListAsync();
            _context.MedicationLists.RemoveRange(oldMedicationListEntries);

            // Add new prescription items and sync to medication list
            foreach (var item in request.PrescriptionItems)
            {
                prescription.PrescriptionItems.Add(new PrescriptionItem
                {
                    DrugName = item.DrugName,
                    Dose = item.Dose ?? string.Empty,
                    Frequency = item.Frequency ?? string.Empty,
                    Route = item.Route ?? string.Empty,
                    DurationDays = item.DurationDays,
                    Instructions = item.Instructions ?? string.Empty
                });

                // Sync to MedicationList
                var medicationListEntry = new MedicationList
                {
                    PatientId = patient.PatientId,
                    DrugName = item.DrugName,
                    Dose = item.Dose ?? string.Empty,
                    Frequency = item.Frequency ?? string.Empty,
                    Route = item.Route ?? string.Empty,
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Now.AddDays(item.DurationDays),
                    Status = medicationStatus
                };
                _context.MedicationLists.Add(medicationListEntry);
            }

            _context.Prescriptions.Update(prescription);
            await _context.SaveChangesAsync();

            return new PrescriptionWithItemsResponseDto
            {
                PrescriptionId = prescription.PrescriptionId,
                EncounterId = prescription.EncounterId,
                ProviderId = prescription.ProviderId,
                CreatedDate = DateOnly.FromDateTime(prescription.CreatedDate),
                Status = prescription.Status.ToString(),
                PrescriptionItems = prescription.PrescriptionItems.Select(pi => new PrescriptionItemWithPrescriptionResponseDto
                {
                    ItemId = pi.ItemId,
                    PrescriptionId = pi.PrescriptionId,
                    DrugName = pi.DrugName,
                    Dose = pi.Dose,
                    Frequency = pi.Frequency,
                    Route = pi.Route,
                    DurationDays = pi.DurationDays,
                    Instructions = pi.Instructions
                }).ToList()
            };
        }
        catch (DbUpdateException ex)
        {
            var message = ex.InnerException?.Message ?? ex.Message;
            throw new InvalidOperationException($"Database error while updating prescription: {message}", ex);
        }
    }

    public async Task<bool> DeletePrescriptionWithItemsAsync(int prescriptionId)
    {
        if (prescriptionId <= 0)
            throw new ArgumentException("PrescriptionId must be provided.", nameof(prescriptionId));

        var prescription = await _context.Prescriptions
            .Include(p => p.PrescriptionItems)
            .FirstOrDefaultAsync(p => p.PrescriptionId == prescriptionId);

        if (prescription == null)
            throw new InvalidOperationException("Prescription not found.");

        _context.PrescriptionItems.RemoveRange(prescription.PrescriptionItems);
        _context.Prescriptions.Remove(prescription);
        await _context.SaveChangesAsync();

        return true;
    }
}