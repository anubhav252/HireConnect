export interface Job {
  jobId: number;
  title: string;
  category: string;
  type: string;
  location: string;
  salaryMin: number;
  salaryMax: number;
  description: string;
  skills: string[];
  experienceRequired: number;
  postedBy: number;
  status: string;
  postedAt: string;
}

export interface JobRequestDto {
  title: string;
  category: string;
  type: string;
  location: string;
  salaryMin: number;
  salaryMax: number;
  description: string;
  skills: string[];
  experienceRequired: number;
  postedBy: number;
}
