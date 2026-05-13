using Microsoft.EntityFrameworkCore; 
using MediRecords.Domain.Entities;   
using MediRecords.Services.AuthServices;
using MediRecords.Services.UserServices;
using MediRecords.Repository;
using MediRecords.Repositories;
using MediRecords.Repository.UserRepo;
using MediRecords.Repository.MedicationRepository;
using MediRecords.Services.MedicationServices;
using Microsoft.OpenApi;
using MediRecords.Repository.UserRoleRepository;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text;
using MediRecords.Repository.PatientRepo;
using MediRecords.Services.PatientServices;
using MediRecords.Repository.EncounterRepo;
using MediRecords.Services.EncounterServices;
using MediRecords.MappingProfiles;
using MediRecords.Services.AppointmentsServices;
using MediRecords.Repository.VitalSignRepository;
using MediRecords.Services.VitalSignServices;
using MediRecords.Repository.NursingNoteRepository;
using MediRecords.Services.NursingNoteServices;
using MediRecords.Repository.ImagingOrderRepository;
using MediRecords.Services.ImagingOrderServices;
using MediRecords.Repository.ImagingReportRepository;
using MediRecords.Services.ImagingReportServices;
using MediRecords.Services.ProblemListServices;
using MediRecords.Repository.ProblemListRepository;
using MediRecords.Services.AllergyServices;
using MediRecords.Repository.AllergyRepository;
using MediRecords.Services.MedicalHistoryServices;
using MediRecords.Repository.MedicalHistoryRepository;
using MediRecords.Services.PrescriptionWithItemsServices;
using MediRecords.Repository.PrescriptionWithItemsRepository;
using MediRecords.Repository.DocumentRepository;
using MediRecords.Services.DocumentServices;

using MediRecords.Repository.ImagingRepo;
using MediRecords.Services.ImagingServices;
using MediRecords.Repository.BillingRepo;
using MediRecords.Services.BillingServices;
using MediRecords.Repository.LabOrderRepository;
using MediRecords.Services.LabOrderServices;
using MediRecords.Services.LabResultServices;
using MediRecords.Repository.ProcedureCodeRepository;
using MediRecords.Services.ProcedureCodeServices;
using MediRecords.Repository.LabResultRepository;
using MediRecords.Repository.CarePlanRepo;
using MediRecords.Services.CarePlanServices;
using MediRecords.Repository.SOAPNoteRepo;
using MediRecords.Services.SOAPNoteService;
using MediRecords.Repository.ImmunizationRepository;
using MediRecords.Services.ImmunizationService;
using MediRecords.Repository.FollowUpRepository;
using MediRecords.Services.FollowUpService;

using MediRecords.Repository.PrescriptionRepository;
using MediRecords.Services.ReportsServices;
using MediRecords.Repository.ProviderProductivity;
using MediRecords.Services.ProviderProductivityServices;

var builder = WebApplication.CreateBuilder(args);
 
 
// Add DbContext
builder.Services.AddDbContext<MediRecordsDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly("MediRecords")
    )
);
 
// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name ="Authorization",
        Type=SecuritySchemeType.Http,
        Scheme="Bearer",
        BearerFormat="JWT",
        In=ParameterLocation.Header,
        Description="JWT Authentication using Bearer scheme"
    });
    options.AddSecurityRequirement(doc => new OpenApiSecurityRequirement
    {
        {new OpenApiSecuritySchemeReference("Bearer",doc),new List<string>()}
    });
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],

        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)
        )
    };
});

builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
                  "http://localhost:4200",
                  "https://localhost:4200")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAuthRepository,AuthRepository>();
builder.Services.AddScoped<IUserRoleRepository,UserRoleRepository>();
builder.Services.AddScoped<IMedicationRepository, MedicationRepository>();
builder.Services.AddScoped<IMedicationService, MedicationService>();
builder.Services.AddScoped<IPatientRepository, PatientRepository>();
builder.Services.AddScoped<IPatientService, PatientService>();
builder.Services.AddScoped<IEncounterRepository, EncounterRepository>();
builder.Services.AddScoped<IEncounterService, EncounterService>();
builder.Services.AddScoped<IVitalSignRepository, VitalSignRepository>();
builder.Services.AddScoped<IVitalSignService, VitalSignService>();
builder.Services.AddScoped<INursingNoteRepository, NursingNoteRepository>();
builder.Services.AddScoped<INursingNoteService, NursingNoteService>();
builder.Services.AddAutoMapper(typeof(EncounterMappingProfile),typeof(ImagingMappingProfile),typeof(ImagingMappingProfile));

builder.Services.AddScoped<ISOAPNoteRepository, SOAPNoteRepository>();
builder.Services.AddScoped<ISOAPNoteService, SOAPNoteService>();
builder.Services.AddAutoMapper(typeof(EncounterMappingProfile),typeof(ImagingMappingProfile),typeof(BillingMappingProfile));
builder.Services.AddScoped<IAppointmentsService, AppointmentsService>();
builder.Services.AddScoped<IAppointmentsRepository, AppointmentsRepository>();
builder.Services.AddScoped<IImagingOrderRepository,ImagingOrderRepository>();
builder.Services.AddScoped<IImagingOrderServices,ImagingOrderServices>();
builder.Services.AddScoped<IImagingReportRepository, ImagingReportRepository>();
builder.Services.AddScoped<IImagingReportServices, ImagingReportServices>();
builder.Services.AddScoped<IProblemListService, ProblemListService>();
builder.Services.AddScoped<IProblemListRepository, ProblemListRepository>();
builder.Services.AddScoped<IAllergyService, AllergyService>();
builder.Services.AddScoped<IAllergyRepository, AllergyRepository>();
builder.Services.AddScoped<IMedicalHistoryService, MedicalHistoryService>();
builder.Services.AddScoped<IMedicalHistoryRepository, MedicalHistoryRepository>();
builder.Services.AddScoped<IImagingRepository, ImagingRepository>();
builder.Services.AddScoped<IImagingService, ImagingService>();
builder.Services.AddScoped<IBillingRepository, BillingRepository>();
builder.Services.AddScoped<IBillingService, BillingService>();
builder.Services.AddScoped<ILabOrderRepository, LabOrderRepository>();
builder.Services.AddScoped<ILabOrderService, LabOrderService>();
builder.Services.AddScoped<ILabResultRepository, LabResultRepository>();
builder.Services.AddScoped<ILabResultService, LabResultService>();
builder.Services.AddScoped<IProcedureCodeRepository,ProcedureCodeRepository>();
builder.Services.AddScoped<IProcedureCodeService,ProcedureCodeService>();
builder.Services.AddScoped<IImagingRepository, ImagingRepository>();
builder.Services.AddScoped<IImagingService, ImagingService>();
builder.Services.AddScoped<IPrescriptionWithItemsService, PrescriptionWithItemsService>();
builder.Services.AddScoped<IPrescriptionWithItemsRepository, PrescriptionWithItemsRepository>();
builder.Services.AddScoped<IDocumentService, DocumentService>();
builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();
 
builder.Services.AddScoped<ICarePlanRepository, CarePlanRepository>();
builder.Services.AddScoped<ICarePlanService, CarePlanService>();
builder.Services.AddScoped<IImmunizationRepository, ImmunizationRepository>();
builder.Services.AddScoped<IImmunizationService, ImmunizationService>(); 
builder.Services.AddScoped<IFollowUpRepository, FollowUpRepository>();
builder.Services.AddScoped<IFollowUpService, FollowUpService>();

builder.Services.AddScoped<IPrescriptionRepository, PrescriptionRepository>();
builder.Services.AddScoped<IReportsService, ReportsService>();
builder.Services.AddScoped<IProviderProductivity, ProviderProductivity>();
builder.Services.AddScoped<IProviderProductivityService, ProviderProductivityService>();
var app = builder.Build();
app.UseStaticFiles();
 
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapControllers();
}
  
app.UseHttpsRedirection();

app.UseCors("AllowFrontend");

// Authentication and Authorization
app.UseAuthentication();
app.UseAuthorization();
 
app.Run();