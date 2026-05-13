export interface AddressDto {
  addressId: number;
  houseNo: string;
  street: string;
  city: string;
  state: string;
  pincode: string;
}

export interface CandidateProfileResponse {
  profileId: number;
  userId: number;
  fullName: string;
  email: string;
  mobile: string;
  dob: string;
  bio: string;
  skills: string[];
  experience: number;
  resumeUrl: string;
  createdAt: string;
  updatedAt: string;
  addresses: AddressDto[];
}

export interface RecruiterProfileResponse {
  profileId: number;
  userId: number;
  fullName: string;
  email: string;
  mobile: string;
  companyName: string;
  companySize: string;
  industry: string;
  website: string;
  bio: string;
  logoUrl: string;
  createdAt: string;
  updatedAt: string;
  addresses: AddressDto[];
}
