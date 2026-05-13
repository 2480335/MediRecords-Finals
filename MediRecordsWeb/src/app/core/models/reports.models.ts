export interface ClinicKpiReport {
  visitCount: number;
  labsOrdered: number;
  rxIssued: number;
}

export interface IncompleteEncounter {
  encounterId: number;
  providerId: number;
  providerName: string;
  encounterDate: string;
  encounterStatus: string;
  hasNoSoapNote: boolean;
  hasUnsignedSoapNote: boolean;
  missingSections: string[];
}

export interface DocumentationCompletenessReport {
  startDate: string;
  endDate: string;
  providerId?: number | null;
  totalEncounters: number;
  incompleteCount: number;
  incompleteEncounters: IncompleteEncounter[];
}

export interface NoShowCancellationReport {
  providerId?: number | null;
  providerName: string;
  startDate: string;
  endDate: string;
  totalAppointments: number;
  noShows: number;
  cancellations: number;
  noShowPercentage: number;
  cancellationPercentage: number;
}

export interface ProviderUtilizationReport {
  providerId: number;
  providerName: string;
  scheduledAppointments: number;
  completedEncounters: number;
  cancelledEncounters: number;
  noShowCount: number;
  utilizationRate: number;
}
