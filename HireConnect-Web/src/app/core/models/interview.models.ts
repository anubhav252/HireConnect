export interface Interview {
  interviewId: number;
  jobId: number;
  applicationId: number;
  candidateId: number;
  recruiterId: number;
  scheduledAt: string;
  status: string; // 'Scheduled', 'Completed', 'Canceled'
  mode: string;
  meetLink: string;
  notes: string;
  createdAt: string;
}

export interface InterviewRequestDto {
  jobId: number;
  applicationId: number;
  candidateId: number;
  recruiterId: number;
  scheduledAt: string;
  mode: string;
  meetLink: string;
  notes: string;
}
