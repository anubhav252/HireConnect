export interface Application {
  applicationId: number;
  jobId: number;
  recruiterId: number;
  candidateId: number;
  status: string; // 'Applied', 'Shortlisted', 'Rejected'
  appliedAt: string;
  resumeUrl: string;
  coverLetter: string;
  jobTitle: string;
  candidateEmail: string;
}

export interface ApplicationRequestDto {
  jobId: number;
  jobTitle: string;
  recruiterId: number;
  candidateId: number;
  candidateEmail: string;
  appliedAt: string;
  resumeUrl: string;
  coverLetter: string;
}
