export interface CarePlanCreateRequest {
  patientId: number;
  goals: string[];
  instructions: string;
  status: boolean; // false = Active, true = Completed
}

export interface CarePlanResponse {
  carePlanId: number;
  patientId: number;
  goals: string[];
  instructions: string;
  status: string;
}

export interface CarePlanDetails {
  carePlanId: number;
  patientId: number;
  patientName: string;
  goalsJSON: string;
  instructions: string;
  status: boolean;
}

export interface CarePlanFilter {
  patientId?: number | null;
  patientName?: string | null;
  status?: boolean | null;
}
